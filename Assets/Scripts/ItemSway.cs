using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSway : MonoBehaviour
{
    public float swayAmount;
    public float maxSwayAmount;
    public float swaySmoothness;
    public RigidboyPlayerController playerController;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;

        if (playerController == null)
        {
            playerController = FindObjectOfType<RigidboyPlayerController>();
        }
    }

    void Update()
    {
        Vector2 lookDelta = playerController != null ? playerController.CurrentLookDelta : Vector2.zero;

        float moveX = -lookDelta.x * swayAmount;
        float moveY = -lookDelta.y * swayAmount;

        moveX = Mathf.Clamp(moveX, -maxSwayAmount, maxSwayAmount);
        moveY = Mathf.Clamp(moveY, -maxSwayAmount, maxSwayAmount);

        Vector3 swayPosition = new Vector3(moveX, moveY, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + swayPosition, Time.deltaTime * swaySmoothness);
    }
}
