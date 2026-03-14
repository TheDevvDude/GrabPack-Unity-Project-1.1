using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Valve : MonoBehaviour
{
    private Animator valveanimator;

    public ParticleSystem GasMainParticles;
    public ParticleSystem GasValveParticles;

    public BoxCollider gasCollider;

    public RedSmokeEffects RedSmoke;

    void Start()
    {
        valveanimator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        bool shouldAnimate = false;

        foreach (Transform child in transform)
        {
            if (!child.name.StartsWith("Hand"))
                continue;

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
                shouldAnimate = true;
                break;
            }

            if (leftHand && isHeld)
            {
                shouldAnimate = true;
                break;
            }
        }

        valveanimator.speed = shouldAnimate ? 0.3f : 0f;
    }

    public void TurnOffGas()
    {
        GasMainParticles.Stop();
        GasValveParticles.Stop();
        gasCollider.enabled = false;

        RedSmoke.RemoveEffects();
    }
}