using UnityEngine;

public class AnimatedCameraFollower : MonoBehaviour
{
    public Transform animatedBone;
    public WeaponDragSway swayScript;

    void LateUpdate()
    {
       // bool isMobile = Application.isMobilePlatform;
        bool isMobile = false;

        Vector3 worldOffset = Vector3.zero;
        Quaternion rotationOffset = Quaternion.identity;

        if (!isMobile)
        {
            worldOffset = animatedBone.TransformDirection(
                swayScript.GetTotalPositionOffset()
            );

            rotationOffset = swayScript.GetCurrentRotationOffset();
        }

        transform.position = animatedBone.position - worldOffset;
        //transform.rotation = animatedBone.rotation * Quaternion.Inverse(rotationOffset);
    }
}