using UnityEngine;

public class seagullbone : MonoBehaviour
{
    [Header("Trackers (XR)")]
    public Transform headTracker;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Bones (Seagull)")]
    public Transform headBone;
    public Transform leftWingBone;
    public Transform rightWingBone;

    [Header("Wing detection")]
    public float speedThreshold = 0.08f;
    public float maxSpeed = 0.8f;
    public float smoothing = 10f;

    [Header("Head")]
    public float headFollowSpeed = 12f;
    public bool yawOnly = false;

    [Header("Pose cible aile gauche")]
    public Vector3 leftTargetLocalPosition = new Vector3(0.09f, 0.157f, 0.02f);
    public Vector3 leftTargetLocalEuler = new Vector3(75f, -90f, -45f);

    [Header("Pose cible aile droite")]
    public Vector3 rightTargetLocalPosition = new Vector3(0.09f, 0.15f, 0.02f);
    public Vector3 rightTargetLocalEuler = new Vector3(75f, 90f, 45f);

    private Vector3 leftStartLocalPos;
    private Quaternion leftStartLocalRot;
    private Vector3 rightStartLocalPos;
    private Quaternion rightStartLocalRot;

    private Vector3 prevLeftHandPos;
    private Vector3 prevRightHandPos;

    private float leftBlend = 0f;
    private float rightBlend = 0f;

    private Quaternion headOffset;

    void Start()
    {
        if (headTracker == null && Camera.main != null)
        {
            headTracker = Camera.main.transform;
            Debug.Log("[SeagullVRBones] headTracker auto-assigné : " + headTracker.name);
        }

        if (headBone == null || leftWingBone == null || rightWingBone == null || leftHand == null || rightHand == null)
        {
            Debug.LogError("[SeagullVRBones] Références manquantes dans l'inspecteur.");
            enabled = false;
            return;
        }

        leftStartLocalPos = leftWingBone.localPosition;
        leftStartLocalRot = leftWingBone.localRotation;
        rightStartLocalPos = rightWingBone.localPosition;
        rightStartLocalRot = rightWingBone.localRotation;

        prevLeftHandPos = leftHand.position;
        prevRightHandPos = rightHand.position;
        headOffset = Quaternion.Inverse(headTracker.rotation) * headBone.rotation;
    }

    void Update()
    {
        UpdateHeadBone();
        UpdateWings();
    }

    void UpdateHeadBone()
    {
        if (headBone == null || headTracker == null) return;

        Quaternion trackerRotation = headTracker.rotation;

        if (yawOnly)
        {
            float yaw = trackerRotation.eulerAngles.y;
            trackerRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        Quaternion targetRotation = trackerRotation * headOffset;

        headBone.rotation = Quaternion.Slerp(
            headBone.rotation,
            targetRotation,
            Time.deltaTime * headFollowSpeed
        );
    }

    void UpdateWings()
    {
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);

        float leftVerticalSpeed = Mathf.Abs(leftHand.position.y - prevLeftHandPos.y) / dt;
        float rightVerticalSpeed = Mathf.Abs(rightHand.position.y - prevRightHandPos.y) / dt;

        prevLeftHandPos = leftHand.position;
        prevRightHandPos = rightHand.position;

        float avgSpeed = (leftVerticalSpeed + rightVerticalSpeed) * 0.5f;
        float targetBlend = Mathf.InverseLerp(speedThreshold, maxSpeed, avgSpeed);

        leftBlend = Mathf.Lerp(leftBlend, targetBlend, Time.deltaTime * smoothing);
        rightBlend = Mathf.Lerp(rightBlend, targetBlend, Time.deltaTime * smoothing);

        leftWingBone.localPosition = Vector3.Lerp(leftStartLocalPos, leftTargetLocalPosition, leftBlend);
        leftWingBone.localRotation = Quaternion.Slerp(
            leftStartLocalRot,
            Quaternion.Euler(leftTargetLocalEuler),
            leftBlend
        );

        rightWingBone.localPosition = Vector3.Lerp(rightStartLocalPos, rightTargetLocalPosition, rightBlend);
        rightWingBone.localRotation = Quaternion.Slerp(
            rightStartLocalRot,
            Quaternion.Euler(rightTargetLocalEuler),
            rightBlend
        );
    }

    public float GetFlapStrength() => (leftBlend + rightBlend) * 0.5f;
}