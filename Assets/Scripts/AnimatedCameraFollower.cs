using UnityEngine;

public class AnimatedCameraFollower : MonoBehaviour
{
    public Transform animatedBone;
    public WeaponDragSway swayScript;

    void LateUpdate()
    {
        Vector3 worldOffset = animatedBone.TransformDirection(
            swayScript.GetTotalPositionOffset()
        );

        transform.position = animatedBone.position - worldOffset;

        Quaternion rotationOffset = swayScript.GetCurrentRotationOffset();
        transform.rotation = animatedBone.rotation * Quaternion.Inverse(rotationOffset);
    }
}