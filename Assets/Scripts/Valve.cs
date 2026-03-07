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
            if (!child.name.StartsWith("Hand")) continue;

            bool rightHand =
                child.name == "Hand_Rocket" ||
                child.name == "Hand_Red" ||
                child.name == "Hand_Pressure" ||
                child.name == "Hand_Conductive";

            bool leftHand = child.name == "Hand_Blue";

            if (rightHand && Input.GetMouseButton(1))
            {
                shouldAnimate = true;
                break;
            }

            if (leftHand && Input.GetMouseButton(0))
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