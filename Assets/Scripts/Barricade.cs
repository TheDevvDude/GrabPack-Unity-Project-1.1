using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : MonoBehaviour
{
    Rigidbody rb;

    public float pullFactor = 1f;
    public bool isgrabbingRight;

    public bool Pulled = false;
    public float rotationSpeed = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
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

        foreach (Transform child in transform)
        {
            if (!child.name.StartsWith("Hand")) continue;

            bool rightHand =
                child.name == "Hand_Rocket" ||
                child.name == "Hand_Red" ||
                child.name == "Hand_Pressure" ||
                child.name == "Hand_Conductive";

            bool leftHand = child.name == "Hand_Blue";

            if (rightHand && Input.GetMouseButton(1))
            {
                PullBarricade();
                isgrabbingRight = true;
                rightHandFound = true;
            }

            if (leftHand && Input.GetMouseButton(0) && !isgrabbingRight)
            {
                PullBarricade();
            }
        }

        if (!rightHandFound)
        {
            isgrabbingRight = false;
        }
    }

    void PullBarricade()
    {
        pullFactor -= Time.deltaTime;

        if (!Pulled)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
}