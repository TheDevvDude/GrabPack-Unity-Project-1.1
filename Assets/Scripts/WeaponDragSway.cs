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
    public RigidboyPlayerController playerController;

    public Vector3 restPosition;

    void Start()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<RigidboyPlayerController>();
        }
    }

    void Update()
    {
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

        transform.localPosition = restPosition + currentOffset;

        Vector2 lookDelta = playerController != null ? playerController.CurrentLookDelta : Vector2.zero;
        float mouseX = lookDelta.x;
        float mouseY = lookDelta.y;

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

        transform.localRotation = currentRotation;
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