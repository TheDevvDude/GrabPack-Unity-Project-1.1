using UnityEngine;

public class RotateArms : MonoBehaviour
{
    public Transform rightHandBone;
    public Vector3 rightHitPoint;
    public bool rightActive = false;
    private Quaternion rightBaseLocalRotation;

    public Transform leftHandBone;
    public Vector3 leftHitPoint;
    public bool leftActive = false;
    private Quaternion leftBaseLocalRotation;

    public float maxPitch = 60f;
    public float maxYaw = 80f;

    private bool initialized = false;


    void Start()
    {

    }

    void LateUpdate()
    {
        if (!initialized)
        {
            rightBaseLocalRotation = rightHandBone.localRotation;
            leftBaseLocalRotation = leftHandBone.localRotation;
            initialized = true;
        }

        RotateRightArm();
        RotateLeftArm();
    }

    void RotateRightArm()
    {
        if (!rightActive) return;
        if (rightHitPoint == Vector3.zero) return;
        if (rightHandBone == null) return;

        Vector3 worldDir = (rightHitPoint - rightHandBone.position).normalized;

        Quaternion fromTo =
            Quaternion.FromToRotation(rightHandBone.forward, worldDir);

        Quaternion targetWorldRot =
            fromTo * rightHandBone.rotation;
        Quaternion targetLocalRot =
            Quaternion.Inverse(rightHandBone.parent.rotation) * targetWorldRot;

        Quaternion delta =
            Quaternion.Inverse(rightBaseLocalRotation) * targetLocalRot;

        Vector3 deltaEuler = delta.eulerAngles;

        deltaEuler.x = NormalizeAngle(deltaEuler.x);
        deltaEuler.y = NormalizeAngle(deltaEuler.y);
        deltaEuler.z = NormalizeAngle(deltaEuler.z);

        deltaEuler.x = Mathf.Clamp(deltaEuler.x, -maxPitch, maxPitch);
        deltaEuler.y = Mathf.Clamp(deltaEuler.y, -maxYaw, maxYaw);
        deltaEuler.z = 0f;

        Quaternion clampedLocal =
            rightBaseLocalRotation * Quaternion.Euler(deltaEuler);

        rightHandBone.localRotation = clampedLocal;

        Debug.DrawLine(rightHandBone.position, rightHitPoint, Color.red);
    }

    void RotateLeftArm()
    {
        if (!leftActive) return;
        if (leftHitPoint == Vector3.zero) return;
        if (leftHandBone == null) return;

        Vector3 worldDir = (leftHitPoint - leftHandBone.position).normalized;
        worldDir = -worldDir; 
        Quaternion targetWorldRot = Quaternion.LookRotation(worldDir);

        Quaternion targetLocalRot =
            Quaternion.Inverse(leftHandBone.parent.rotation) * targetWorldRot;

        Quaternion delta =
            Quaternion.Inverse(leftBaseLocalRotation) * targetLocalRot;

        Vector3 deltaEuler = delta.eulerAngles;

        deltaEuler.x = NormalizeAngle(deltaEuler.x);
        deltaEuler.y = NormalizeAngle(deltaEuler.y);
        deltaEuler.z = NormalizeAngle(deltaEuler.z);

        deltaEuler.x = Mathf.Clamp(deltaEuler.x, -maxPitch, maxPitch);
        deltaEuler.y = Mathf.Clamp(deltaEuler.y, -maxYaw, maxYaw);
        deltaEuler.z = 0f;

        Quaternion clampedLocal =
            leftBaseLocalRotation * Quaternion.Euler(deltaEuler);

        leftHandBone.localRotation = clampedLocal;
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}