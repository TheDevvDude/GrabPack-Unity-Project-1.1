using UnityEngine;

public class AnimatedCameraFollower : MonoBehaviour
{
    public Transform animatedBone;
    public Vector3 localOffset;

    void LateUpdate()
    {
        transform.position = animatedBone.TransformPoint(localOffset);

        transform.rotation = animatedBone.rotation;
    }
}