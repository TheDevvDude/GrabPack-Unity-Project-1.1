using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLean : MonoBehaviour
{
    public float balanceIntensity = 0.5f;
    public float inclineIntensity = 0.5f;
    public float balanceSpeed = 10f;
    public float tiltSpeed = 5f; 
    public float leanAngle = 15f;
    public GameObject mainCamera;
    public float cameraLeanFactor = 0.5f; 

    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private Quaternion currentBalanceRotation;
    private Quaternion currentTiltRotation;
    private Quaternion cameraOriginalRotation;

    void Start()
    {
        originalRotation = transform.localRotation;
        originalPosition = transform.localPosition;
        currentBalanceRotation = originalRotation;
        currentTiltRotation = originalRotation;

        if (mainCamera != null)
        {
            cameraOriginalRotation = mainCamera.transform.localRotation;
        }
    }

    void Update()
    {
        SwingObject();
        InclineObject();
        LeanCamera();
    }

    void SwingObject()
    {
        float mouseX = Input.GetAxis("Mouse X") * balanceIntensity;
        float mouseY = Input.GetAxis("Mouse Y") * balanceIntensity;
        Quaternion targetRotation = Quaternion.Euler(-mouseY, mouseX, 0);

        currentBalanceRotation = Quaternion.Lerp(currentBalanceRotation, targetRotation, Time.deltaTime * balanceSpeed);
        transform.localRotation = currentBalanceRotation;
    }

    void InclineObject()
    {
        float moveX = Input.GetAxis("Horizontal") * inclineIntensity;

        float lean = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            lean = leanAngle; 
        }
        else if (Input.GetKey(KeyCode.D))
        {
            lean = -leanAngle;
        }

        Quaternion tiltRotation = Quaternion.Euler(0, 0, moveX * 30f + lean);

        currentTiltRotation = Quaternion.Lerp(currentTiltRotation, tiltRotation, Time.deltaTime * tiltSpeed);

        transform.localRotation = Quaternion.Euler(
            currentBalanceRotation.eulerAngles.x,
            currentBalanceRotation.eulerAngles.y,
            currentTiltRotation.eulerAngles.z
        );

        float moveZ = Input.GetAxis("Vertical") * inclineIntensity;
        Vector3 targetPosition = originalPosition + new Vector3(0, 0, moveZ);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 10);
    }

    void LeanCamera()
    {
        if (mainCamera != null)
        {
            float cameraLean = 0f;
            if (Input.GetKey(KeyCode.A))
            {
                cameraLean = leanAngle * cameraLeanFactor; 
            }
            else if (Input.GetKey(KeyCode.D))
            {
                cameraLean = -leanAngle * cameraLeanFactor; 
            }

            Quaternion cameraTiltRotation = Quaternion.Euler(0, 0, cameraLean);

            mainCamera.transform.localRotation = Quaternion.Lerp(
                mainCamera.transform.localRotation,
                cameraOriginalRotation * cameraTiltRotation,
                Time.deltaTime * tiltSpeed
            );
        }
    }
}
