using UnityEngine;
using UnityEngine.XR;

public class camstart : MonoBehaviour
{
    [Header("Références")]
    public Transform cameraTransform;
    public Transform camOrigin1st;
    public Transform camOrigin3rd;

    [Header("Input")]
    public XRNode inputSource = XRNode.LeftHand;

    private InputDevice device;

    private bool isThirdPerson = false;
    private bool buttonPressedLastFrame = false;

    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(inputSource);
        RecenterTo(camOrigin1st);
    }

    void Update()
    {
        if (!device.isValid)
            device = InputDevices.GetDeviceAtXRNode(inputSource);

        bool buttonPressed;
        device.TryGetFeatureValue(CommonUsages.primaryButton, out buttonPressed);
        if (buttonPressed && !buttonPressedLastFrame)
        {
            isThirdPerson = !isThirdPerson;

            if (isThirdPerson)
                RecenterTo(camOrigin3rd);
            else
                RecenterTo(camOrigin1st);
        }

        buttonPressedLastFrame = buttonPressed;
    }

    void RecenterTo(Transform target)
    {
        if (cameraTransform == null || target == null)
            return;
        Vector3 offset = target.position - cameraTransform.position;
        transform.position += offset;
        Vector3 forwardCamera = cameraTransform.forward;
        Vector3 forwardTarget = target.forward;

        forwardCamera.y = 0f;
        forwardTarget.y = 0f;

        if (forwardCamera.sqrMagnitude > 0.001f && forwardTarget.sqrMagnitude > 0.001f)
        {
            float angle = Vector3.SignedAngle(forwardCamera, forwardTarget, Vector3.up);
            transform.RotateAround(cameraTransform.position, Vector3.up, angle);
        }
    }
}