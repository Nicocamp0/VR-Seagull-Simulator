using UnityEngine;

public class Suivi : MonoBehaviour
{
    public Transform head;
    public float distance = 1.8f;
    public float heightOffset = -0.05f;

    public float positionSmooth = 6f;
    public float rotationSmooth = 6f;

    public float moveDeadzone = 0.03f;
    public float rotateDeadzone = 2f;

    private bool initialized = false;

    void Start()
    {
        SnapNow();
    }

    void LateUpdate()
    {
        if (head == null) return;

        Vector3 flatForward = head.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = Vector3.forward;

        flatForward.Normalize();

        Vector3 targetPosition = head.position + flatForward * distance + Vector3.up * heightOffset;

        Vector3 toHead = head.position - transform.position;
        toHead.y = 0f;

        if (toHead.sqrMagnitude < 0.001f)
            toHead = -flatForward;

        Quaternion targetRotation = Quaternion.LookRotation(-toHead.normalized, Vector3.up);

        if (!initialized)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
            initialized = true;
            return;
        }

        float posDelta = Vector3.Distance(transform.position, targetPosition);
        float rotDelta = Quaternion.Angle(transform.rotation, targetRotation);

        if (posDelta > moveDeadzone)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.unscaledDeltaTime * positionSmooth
            );
        }

        if (rotDelta > rotateDeadzone)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.unscaledDeltaTime * rotationSmooth
            );
        }
    }

    public void SnapNow()
    {
        if (head == null) return;

        Vector3 flatForward = head.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = Vector3.forward;

        flatForward.Normalize();

        Vector3 targetPosition = head.position + flatForward * distance + Vector3.up * heightOffset;
        Vector3 toHead = head.position - targetPosition;
        toHead.y = 0f;

        if (toHead.sqrMagnitude < 0.001f)
            toHead = -flatForward;

        transform.position = targetPosition;
        transform.rotation = Quaternion.LookRotation(-toHead.normalized, Vector3.up);
        initialized = true;
    }
}