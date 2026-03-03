using UnityEngine;

public class WeaponDragSway : MonoBehaviour
{
    public float swayAmount = 0.05f;
    public float smoothSpeed = 8f;

    public float rotationSwayAmount = 4f;
    public float rotationSmoothSpeed = 10f;

    private Vector3 currentOffset;
    private Vector3 velocity;

    private Quaternion currentRotation;

    public Transform reference;
    public Rigidbody rb;

    public Vector3 restPosition;

    public Transform swayTarget;

    void Awake()
    {
        if (swayTarget == null)
        {
            swayTarget = transform;
        }

        if (reference == null)
        {
            reference = transform.root;
        }

        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody>();
        }

        currentRotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        if (swayTarget == null || reference == null || rb == null)
        {
            return;
        }

        Vector3 animatedLocalPosition = swayTarget.localPosition;
        Quaternion animatedLocalRotation = swayTarget.localRotation;

        Vector3 localVelocity = reference.InverseTransformDirection(rb.velocity);

        float forwardSpeed = Mathf.Max(0f, localVelocity.z);

        Vector3 targetOffset = new Vector3(
            0f,
            0f,
            -forwardSpeed * swayAmount
        );

        currentOffset = Vector3.SmoothDamp(
            currentOffset,
            targetOffset,
            ref velocity,
            1f / smoothSpeed
        );

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 rotationOffset = new Vector3(
            -mouseY,
            -mouseX,
            0f
        ) * rotationSwayAmount;

        Quaternion targetRotation = Quaternion.Euler(rotationOffset);

        currentRotation = Quaternion.Slerp(
            currentRotation,
            targetRotation,
            Time.deltaTime * rotationSmoothSpeed
        );

        swayTarget.localPosition = animatedLocalPosition + restPosition + currentOffset;
        swayTarget.localRotation = animatedLocalRotation * currentRotation;
    }

    public Vector3 GetCurrentOffset()
    {
        return currentOffset;
    }

    public Quaternion GetCurrentRotationOffset()
    {
        return currentRotation;
    }

    public Vector3 GetTotalPositionOffset()
    {
        return restPosition + currentOffset;
    }
}