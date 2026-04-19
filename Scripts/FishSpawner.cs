using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;
    public float fishscale = 35f;

    [Header("Spawn Area")]
    public Vector2 areaSize = new Vector2(50f, 50f);
    public Vector3 areaCenter = Vector3.zero;
    public float waterY = 0f;

    [Header("Spawn Distance From Player")]
    [Tooltip("Minimum horizontal distance from the player where fish can appear.")]
    public float minSpawnDistance = 5f;
    [Tooltip("Maximum horizontal distance from the player where fish can appear.")]
    public float maxSpawnDistance = 25f;

    [Header("Spawn Timing - Normal")]
    public float minDelay = 1f;
    public float maxDelay = 5f;

    [Header("Spawn Frenzy Permanent")]
    [Tooltip("Multiplie la fréquence de spawn une fois le bouton activé.")]
    public float frenzySpawnRateMultiplier = 4f;

    [Tooltip("Nombre de poissons spawnés à chaque vague en mode frenzy.")]
    public int frenzyFishPerWave = 50;

    [Header("Frenzy Formation")]
    public float frenzySideSpacing = 0.12f;
    public float frenzyForwardSpacing = 0.10f;
    public float frenzyRandomJitter = 0.02f;
    public float frenzySpeedVariation = 0.03f;
    public int frenzyColumns = 10;

    [Header("Jump Shape")]
    public float minAirTime = 5f;
    public float maxAirTime = 15f;

    [Tooltip("Tous les poissons partent avec cette même vitesse totale.")]
    public float launchSpeed = 18f;

    [Tooltip("Gravité spéciale pour les poissons. Plus petite = sauts plus longs et plus doux.")]
    public float fishGravity = 2.5f;

    [Header("Direction Control")]
    public bool biasTowardPlayer = false;
    public Transform playerHead;

    [Header("Direction Randomness")]
    [Range(0f, 180f)]
    public float randomYawOffset = 35f;

    [Header("Obstacle Check")]
    public LayerMask obstacleMask = ~0;
    public float obstacleCheckHeight = 200f;
    public float minClearanceAboveWater = 0.2f;
    public int maxSpawnValidationAttempts = 50;

    [Header("Debug")]
    public bool debugLogs = false; 
    public bool debugDrawRay = false;

    private int spawnCount = 0;
    private bool frenzyActive = false;

    private void Start()
    {
        if (fishPrefab == null)
            return;

        ScheduleNext();
    }

    void ScheduleNext()
    {
        float minCurrent = minDelay;
        float maxCurrent = maxDelay;

        if (frenzyActive)
        {
            minCurrent = Mathf.Max(0.02f, minDelay / frenzySpawnRateMultiplier);
            maxCurrent = Mathf.Max(0.02f, maxDelay / frenzySpawnRateMultiplier);
        }

        float delay = Random.Range(minCurrent, maxCurrent);

        CancelInvoke(nameof(SpawnFish));
        Invoke(nameof(SpawnFish), delay);
    }

    public void ActivatePermanentFrenzy()
    {
        if (frenzyActive)
            return;

        frenzyActive = true;

        if (debugLogs)
            Debug.Log("[FishSpawner] Frenzy permanent activé.");

        ScheduleNext();
    }

    void SpawnFish()
    {
        if (fishPrefab == null)
            return;

        if (frenzyActive)
            SpawnFrenzySchool();
        else
            SpawnSingleFish();

        ScheduleNext();
    }

    void SpawnSingleFish()
    {
        Vector3 spawnPos = GetBaseSpawnPosition();
        Vector3 initialVelocity = BuildInitialVelocity(spawnPos);

        SpawnFishInstance(spawnPos, initialVelocity);
    }

    void SpawnFrenzySchool()
    {
        int count = frenzyFishPerWave;

        Vector3 baseSpawnPos = GetBaseSpawnPosition();
        Vector3 baseVelocity = BuildInitialVelocity(baseSpawnPos);

        Vector3 horizontalDir = new Vector3(baseVelocity.x, 0f, baseVelocity.z).normalized;
        if (horizontalDir.sqrMagnitude < 0.001f)
            horizontalDir = Vector3.forward;

        Vector3 sideDir = Vector3.Cross(Vector3.up, horizontalDir).normalized;
        if (sideDir.sqrMagnitude < 0.001f)
            sideDir = Vector3.right;

        int columns = Mathf.Max(1, frenzyColumns);
        int rows = Mathf.CeilToInt((float)count / columns);

        for (int i = 0; i < count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            float centeredCol = col - (columns - 1) * 0.5f;
            float centeredRow = row - (rows - 1) * 0.5f;

            Vector3 offset =
                sideDir * centeredCol * frenzySideSpacing +
                horizontalDir * centeredRow * frenzyForwardSpacing;

            offset += sideDir * Random.Range(-frenzyRandomJitter, frenzyRandomJitter);
            offset += horizontalDir * Random.Range(-frenzyRandomJitter, frenzyRandomJitter);

            Vector3 spawnPos = baseSpawnPos + offset;
            spawnPos.y = waterY;

            if (!IsSpawnPointValid(spawnPos))
                continue;

            Vector3 velocity = baseVelocity;
            velocity += sideDir * Random.Range(-frenzySpeedVariation, frenzySpeedVariation);
            velocity += horizontalDir * Random.Range(-frenzySpeedVariation, frenzySpeedVariation);

            SpawnFishInstance(spawnPos, velocity);
        }
    }

    Vector3 GetBaseSpawnPosition()
    {
        if (playerHead != null)
            areaCenter = new Vector3(playerHead.position.x, waterY, playerHead.position.z);
        else
            areaCenter = new Vector3(transform.position.x, waterY, transform.position.z);

        float halfWidth = areaSize.x * 0.5f;
        float halfDepth = areaSize.y * 0.5f;

        for (int i = 0; i < maxSpawnValidationAttempts; i++)
        {
            Vector3 candidate;

            if (playerHead != null)
                candidate = GetSpawnPositionNearPlayer(areaCenter, halfWidth, halfDepth);
            else
            {
                float x = areaCenter.x + Random.Range(-halfWidth, halfWidth);
                float z = areaCenter.z + Random.Range(-halfDepth, halfDepth);
                candidate = new Vector3(x, waterY, z);
            }

            if (IsSpawnPointValid(candidate))
                return candidate;
        }

        if (debugLogs)
            Debug.LogWarning("[FishSpawner] Aucun point de spawn valide trouvé, fallback au centre.");

        return new Vector3(areaCenter.x, waterY, areaCenter.z);
    }

    bool IsSpawnPointValid(Vector3 point)
    {
        Vector3 rayStart = new Vector3(point.x, waterY + obstacleCheckHeight, point.z);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, obstacleCheckHeight * 2f, obstacleMask))
        {
            if (debugDrawRay)
                Debug.DrawLine(rayStart, hit.point, Color.red, 1f);

            if (hit.point.y > waterY + minClearanceAboveWater)
            {
                if (debugLogs)
                    Debug.Log("[FishSpawner] Point refusé : " + hit.collider.name + " à y=" + hit.point.y);

                return false;
            }
        }
        else if (debugDrawRay)
        {
            Debug.DrawRay(rayStart, Vector3.down * (obstacleCheckHeight * 2f), Color.green, 1f);
        }

        return true;
    }

    Vector3 BuildInitialVelocity(Vector3 spawnPos)
    {
        float airTime = Random.Range(minAirTime, maxAirTime);

        float verticalSpeed = fishGravity * airTime * 0.5f;

        float speedSquared = launchSpeed * launchSpeed;
        float verticalSquared = verticalSpeed * verticalSpeed;

        if (verticalSquared >= speedSquared)
        {
            verticalSpeed = launchSpeed * 0.8f;
            verticalSquared = verticalSpeed * verticalSpeed;
        }

        float horizontalSpeed = Mathf.Sqrt(Mathf.Max(0.01f, speedSquared - verticalSquared));

        Vector3 horizontalDir;

        if (biasTowardPlayer && playerHead != null)
        {
            Vector3 toPlayer = playerHead.position - spawnPos;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude < 0.001f)
                horizontalDir = Random.insideUnitSphere;
            else
                horizontalDir = toPlayer.normalized;
        }
        else
        {
            horizontalDir = Random.insideUnitSphere;
        }

        horizontalDir.y = 0f;

        if (horizontalDir.sqrMagnitude < 0.001f)
            horizontalDir = Vector3.forward;

        horizontalDir.Normalize();

        float yaw = Random.Range(-randomYawOffset, randomYawOffset);
        horizontalDir = Quaternion.Euler(0f, yaw, 0f) * horizontalDir;
        horizontalDir.Normalize();

        return horizontalDir * horizontalSpeed + Vector3.up * verticalSpeed;
    }

    void SpawnFishInstance(Vector3 spawnPos, Vector3 initialVelocity)
    {
        GameObject fish = Instantiate(fishPrefab, spawnPos, Quaternion.identity);
        fish.transform.localScale = Vector3.one * fishscale;
        spawnCount++;

        FishProjectile proj = fish.GetComponent<FishProjectile>();
        if (proj == null)
        {
            Destroy(fish);
            return;
        }

        proj.Launch(initialVelocity, waterY, fishGravity);

        if (debugDrawRay)
        {
            Debug.DrawRay(spawnPos, initialVelocity, Color.green, 2f);
        }

        if (debugLogs)
        {
            Debug.Log("[FishSpawner] Spawn #" + spawnCount + (frenzyActive ? " [FRENZY]" : ""));
        }
    }

    private Vector3 GetSpawnPositionNearPlayer(Vector3 center, float halfWidth, float halfDepth)
    {
        int attempts = 0;
        Vector2 center2D = new Vector2(center.x, center.z);

        while (attempts < 40)
        {
            float angle = Random.Range(0f, 360f);
            float radius = Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector2 offset = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * radius;

            float x = center.x + offset.x;
            float z = center.z + offset.y;

            if (Mathf.Abs(x - center.x) <= halfWidth && Mathf.Abs(z - center.z) <= halfDepth)
                return new Vector3(x, waterY, z);

            attempts++;
        }

        Vector3 fallback;
        int fallbackAttempts = 0;

        do
        {
            float x = center.x + Random.Range(-halfWidth, halfWidth);
            float z = center.z + Random.Range(-halfDepth, halfDepth);
            fallback = new Vector3(x, waterY, z);
            fallbackAttempts++;
        }
        while (
            fallbackAttempts < 30 &&
            Vector2.Distance(new Vector2(fallback.x, fallback.z), center2D) < minSpawnDistance
        );

        return fallback;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            new Vector3(areaCenter.x, waterY, areaCenter.z),
            new Vector3(areaSize.x, 0.1f, areaSize.y)
        );
    }
}