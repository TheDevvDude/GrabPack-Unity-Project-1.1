using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticPull : MonoBehaviour
{
    public RigidboyPlayerController controller;
    public Rigidbody Player;
    public float pullSpeed = 20f;
    private bool wasBeingPulled = false;
    public bool isgrabbingRight;
    public bool grabbed = false;

    public bool isBeingPulled = false;

    public AudioSource globalaudio;
    public AudioClip pullsfx;

    private bool virtualHeld;
    bool isMobile = false;

    public HandManager handmanager;

    public void UIButtonDown()
    {
        virtualHeld = true;
    }

    public void UIButtonUp()
    {
        virtualHeld = false;
    }

    void FixedUpdate()
    {
        if (handmanager.HasEMUCuffs == false) return;

        bool rightHandFound = false;
        bool anyHandFound = false;

        isBeingPulled = false;

        foreach (Transform child in transform)
        {
            if (!child.name.StartsWith("Hand"))
                continue;

            anyHandFound = true;

            LaunchHand hand = child.GetComponent<LaunchHand>();
            if (hand == null)
                continue;

            bool isHeld = hand.IsHeld();

            bool rightHand =
                child.name == "Hand_Rocket" ||
                child.name == "Hand_Red" ||
                child.name == "Hand_Pressure" ||
                child.name == "Hand_Conductive";

            bool leftHand = child.name == "Hand_Blue";

            if (rightHand && isHeld)
            {
                ApplyPull();
                isgrabbingRight = true;
                rightHandFound = true;
                isBeingPulled = true;
            }

            if (leftHand && isHeld && !isgrabbingRight)
            {
                ApplyPull();
                isBeingPulled = true;
            }
        }

        if (!rightHandFound)
            isgrabbingRight = false;

        if (isBeingPulled && !wasBeingPulled)
        {
            controller.CanMove = false;
        }

        if (!isBeingPulled && wasBeingPulled)
        {
            controller.CanMove = true;
            grabbed = false;
            Player.useGravity = true;
            globalaudio.Stop();

        }

        wasBeingPulled = isBeingPulled;
    }

    void ApplyPull()
    {
        Player.useGravity = false;

        Vector3 targetPosition = transform.position;

        Vector3 newPosition = Vector3.MoveTowards(
            Player.position,
            targetPosition,
            pullSpeed * Time.fixedDeltaTime
        );

        Player.MovePosition(newPosition);

        if (!grabbed)
        {
            globalaudio.PlayOneShot(pullsfx, 0.3f);
            grabbed = true;
        }
    }
}