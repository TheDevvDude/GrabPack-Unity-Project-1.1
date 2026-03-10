using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : MonoBehaviour
{
    Rigidbody rb;
    private float nextDebugLogTime;
    private bool loggedNoHandChildren;

    public float pullFactor = 1f;
    public bool isgrabbingRight;

    public bool Pulled = false;
    public float rotationSpeed = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        LogDebug($"Start pullFactor={pullFactor} isKinematic={rb.isKinematic}");
    }

    void Update()
    {
        if (pullFactor < 0)
        {
            rb.isKinematic = false;
            Pulled = true;
        }
    }

    void FixedUpdate()
    {
        bool rightHandFound = false;
        bool anyHandFound = false;

        foreach (Transform child in transform)
        {
            if (!child.name.StartsWith("Hand")) continue;

            anyHandFound = true;
            loggedNoHandChildren = false;

            bool rightHand =
                child.name == "Hand_Rocket" ||
                child.name == "Hand_Red" ||
                child.name == "Hand_Pressure" ||
                child.name == "Hand_Conductive";

            bool leftHand = child.name == "Hand_Blue";
            LaunchHand attachedHand = GetAttachedLaunchHand(child);
            bool handHeld = attachedHand != null && attachedHand.IsActionHeld;

            if (ShouldEmitPeriodicLog())
            {
                LogDebug($"Child={child.name} attachedHand={(attachedHand != null ? attachedHand.Hand : "null")} handHeld={handHeld} rightHand={rightHand} leftHand={leftHand} pullFactor={pullFactor:0.000}");
            }

            if (rightHand && handHeld)
            {
                PullBarricade();
                isgrabbingRight = true;
                rightHandFound = true;
            }

            if (leftHand && handHeld && !isgrabbingRight)
            {
                PullBarricade();
            }
        }

        if (!rightHandFound)
        {
            isgrabbingRight = false;
        }

        if (!anyHandFound && !loggedNoHandChildren)
        {
            loggedNoHandChildren = true;
            LogDebug("No hand children attached.");
        }
    }

    void PullBarricade()
    {
        pullFactor -= Time.deltaTime;
        LogDebug($"PullBarricade called. New pullFactor={pullFactor:0.000}");

        if (!Pulled)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    private LaunchHand GetAttachedLaunchHand(Transform handTransform)
    {
        LaunchHand[] allHands = FindObjectsOfType<LaunchHand>();
        foreach (LaunchHand hand in allHands)
        {
            if (hand != null && hand.handTransform == handTransform)
            {
                return hand;
            }
        }

        return null;
    }

    private bool ShouldEmitPeriodicLog()
    {
        if (Time.unscaledTime < nextDebugLogTime)
        {
            return false;
        }

        nextDebugLogTime = Time.unscaledTime + 0.25f;
        return true;
    }

    private void LogDebug(string message)
    {
        Debug.Log($"[Barricade:{name}] {message}");
    }
}