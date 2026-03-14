using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingHandle : MonoBehaviour
{
    public Rigidbody Player;
    public float forceStrength = 10f;
    public float yForceMultiplier = 0.5f;

    public bool isgrabbingRight;
    public bool grabbed = false;

    public AudioSource globalaudio;
    public AudioClip swingsfx;

    private bool virtualHeld;

    //bool isMobile = Application.isMobilePlatform;
    bool isMobile = false;

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
        bool rightHandFound = false;
        bool anyHandFound = false;

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
                ApplySwingForce();
                isgrabbingRight = true;
                rightHandFound = true;
            }

            if (leftHand && isHeld && !isgrabbingRight)
            {
                ApplySwingForce();
            }
        }

        if (!rightHandFound)
            isgrabbingRight = false;

        if (!anyHandFound)
            grabbed = false;
    }

    void ApplySwingForce()
    {
        Vector3 direction = (transform.position - Player.position).normalized;
        direction.y *= yForceMultiplier;

        Player.AddForce(direction * forceStrength, ForceMode.Force);

        if (!grabbed)
        {
            globalaudio.PlayOneShot(swingsfx, 1.0f);
            grabbed = true;
        }
    }
}