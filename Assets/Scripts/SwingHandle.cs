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

    void FixedUpdate()
    {
        bool rightHandFound = false;
        bool anyHandFound = false;

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Hand"))
            {
                anyHandFound = true;

                bool rightHand =
                    child.name == "Hand_Rocket" ||
                    child.name == "Hand_Red" ||
                    child.name == "Hand_Pressure" ||
                    child.name == "Hand_Conductive";

                bool leftHand = child.name == "Hand_Blue";

                if (rightHand && Input.GetMouseButton(1))
                {
                    ApplySwingForce();
                    isgrabbingRight = true;
                    rightHandFound = true;
                }

                if (leftHand && Input.GetMouseButton(0) && !isgrabbingRight)
                {
                    ApplySwingForce();
                }
            }
        }

        if (!rightHandFound)
        {
            isgrabbingRight = false;
        }

        if (!anyHandFound)
        {
            grabbed = false;
        }
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