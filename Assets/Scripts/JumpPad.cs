using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public Rigidbody Player;
    public float jumpForce = 10f;
    public LaunchHand rockethand;

    public bool launched = false;

    public bool Powered = true;
    public ElectricalReciever powerSource;

    public Material poweredmatieral;
    public Material offMaterial;
    public MeshRenderer renderer;

    public GameObject light;

    public RigidboyPlayerController player;

    public AudioSource GlobalAudio;
    public AudioClip boostsfx;

    public GameObject rocketHand;

    public float maxBoostDistance = 10f;
    public float minBoostMultiplier = 0.2f;

    public float cooldownTime = 2f;
    private float cooldownTimer = 0f;

    void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        bool handAttached = rocketHand != null &&
                            rocketHand.activeInHierarchy &&
                            rocketHand.transform.IsChildOf(transform);

        if (handAttached)
        {
            if (cooldownTimer > 0)
            {
                rockethand.return1();
                return;
            }

            if (!launched && Powered)
            {
                float baseForce = player.isGrounded ? jumpForce : jumpForce / 2f;

                float distance = Vector3.Distance(Player.transform.position, transform.position);

                float distanceMultiplier = Mathf.Clamp01(1 - (distance / maxBoostDistance));
                distanceMultiplier = Mathf.Lerp(minBoostMultiplier, 1f, distanceMultiplier);

                float finalForce = baseForce * distanceMultiplier;

                Player.velocity = Vector3.zero;
                Player.AddForce(transform.up * finalForce, ForceMode.Impulse);

                launched = true;
                cooldownTimer = cooldownTime;

                rockethand.return1();
                GlobalAudio.PlayOneShot(boostsfx, 1.0f);
            }
        }
        else
        {
            launched = false;
        }

        if (powerSource == null)
            return;

        if (!Powered && powerSource.CircuitComplete)
        {
            Powered = true;
            renderer.material = poweredmatieral;
            light.SetActive(true);
        }
        else if (Powered && !powerSource.CircuitComplete)
        {
            Powered = false;
            renderer.material = offMaterial;
            light.SetActive(false);
        }
    }
}