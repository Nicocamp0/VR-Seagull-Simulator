using UnityEngine;

public enum CollectibleType
{
    consomable,
    dansBeque
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Nourriture : MonoBehaviour
{
    [Header("Type de collectible")]
    public CollectibleType type;

    [Header("Score")]
    public int scoreValue = 1;

    [Header("Audio")]
    public AudioClip consommablePickupSound;
    public AudioClip beakPickupSound;
    public AudioClip beakDropSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    [Header("Cassage à l'impact")]
    public bool canBreakOnImpact = false;
    public float minDropHeightToBreak = 3f;
    public GameObject brokenPiecePrefab1;
    public GameObject brokenPiecePrefab2;

    [Header("Spawn des morceaux")]
    public Vector3 piece1Offset = new Vector3(-0.15f, 0.05f, 0f);
    public Vector3 piece2Offset = new Vector3(0.15f, 0.05f, 0f);

    private Rigidbody rb;
    private Collider col;

    [HideInInspector] public bool isCollected = false;
    [HideInInspector] public float lastDropTime = -999f;

    private float dropStartY;
    private bool wasDropped = false;
    private bool hasBroken = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Collect(Transform beakHoldPoint)
    {
        if (isCollected) return;
        if (hasBroken) return;

        isCollected = true;
        wasDropped = false;

        if (type == CollectibleType.consomable)
        {
            if (Score.instance != null)
            {
                Score.instance.AddScore(scoreValue);
                Score.instance.RegisterCoconutEaten();
            }

            if (consommablePickupSound != null)
            {
                AudioSource.PlayClipAtPoint(consommablePickupSound, transform.position, soundVolume);
            }

            Destroy(gameObject);
        }
        else if (type == CollectibleType.dansBeque)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            col.enabled = false;

            transform.SetParent(beakHoldPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (beakPickupSound != null)
            {
                AudioSource.PlayClipAtPoint(beakPickupSound, transform.position, soundVolume);
            }
        }
    }

    public void Drop()
    {
        if (!isCollected) return;
        if (type != CollectibleType.dansBeque) return;

        transform.SetParent(null);

        col.enabled = true;
        rb.isKinematic = false;
        rb.useGravity = true;

        lastDropTime = Time.time;
        dropStartY = transform.position.y;
        wasDropped = true;
        isCollected = false;

        if (beakDropSound != null)
        {
            AudioSource.PlayClipAtPoint(beakDropSound, transform.position, soundVolume);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canBreakOnImpact) return;
        if (!wasDropped) return;
        if (hasBroken) return;

        if (!collision.collider.CompareTag("Sol")) return;

        float dropHeight = dropStartY - transform.position.y;

        if (dropHeight >= minDropHeightToBreak)
        {
            BreakObject();
        }
    }

    private void BreakObject()
    {
        hasBroken = true;
        Score.instance.RegisterBrokenCoconut();

        if (brokenPiecePrefab1 != null)
        {
            Instantiate(
                brokenPiecePrefab1,
                transform.position + transform.TransformDirection(piece1Offset),
                transform.rotation
            );
        }

        if (brokenPiecePrefab2 != null)
        {
            Instantiate(
                brokenPiecePrefab2,
                transform.position + transform.TransformDirection(piece2Offset),
                transform.rotation
            );
        }

        Destroy(gameObject);
    }
}