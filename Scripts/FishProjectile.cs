using UnityEngine;

public class FishProjectile : MonoBehaviour
{
    [Header("Runtime")]
    public float waterY = 0f;
    public float customGravity = 2.5f;
    public bool rotateToVelocity = true;

    private Vector3 velocity;
    private float t = 0f;
    private bool launched = false;

    public void Launch(Vector3 initialVelocity, float targetWaterY, float gravityValue)
    {
        velocity = initialVelocity;
        waterY = targetWaterY;
        customGravity = gravityValue;
        launched = true;
    }

    void Update()
    {
        if (!launched)
            return;

        float dt = Time.deltaTime;
        t += dt;

  
        velocity += Vector3.down * customGravity * dt;

        transform.position += velocity * dt;

        if (rotateToVelocity && velocity.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
        }

        if (velocity.y <= 0f && transform.position.y <= waterY)
        {
            Destroy(gameObject);
            return;
        }
    }
}