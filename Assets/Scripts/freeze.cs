using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class freeze : MonoBehaviour
{
    public AudioSource globalAudio;
    public AudioClip icesfx;
    public AudioClip firesfx;

    private bool played = false;

    public ParticleSystem grabparticles;
    public ParticleSystem heatedparticles;

    public MeshRenderer[] renderers;

    public Material frozenmat;
    public Material unfrozen;

    public bool frozen = false;

    public Breakable breakable;

    void Update()
    {
        Transform conductiveHand = null;

        foreach (Transform child in transform)
        {
            if (child.name == "Hand_Conductive")
            {
                conductiveHand = child;
                break;
            }
        }

        if (conductiveHand != null)
        {
            Conductor conductor = conductiveHand.GetComponent<Conductor>();
            LaunchHand launchhand = conductiveHand.GetComponent<LaunchHand>();

            if (!played && conductor.CurrentElement == "ice")
            {
                if (!frozen)
                {
                    if (grabparticles != null)
                        grabparticles.Play();

                    foreach (MeshRenderer render in renderers)
                        render.material = frozenmat;

                    globalAudio.PlayOneShot(icesfx, 3.0f);

                    played = true;
                    frozen = true;
                    breakable.isbreakable = true;

                    launchhand.return1();
                }
            }

            if (!played && conductor.CurrentElement == "fire")
            {
                if (frozen)
                {
                    if (heatedparticles != null)
                        heatedparticles.Play();

                    foreach (MeshRenderer render in renderers)
                        render.material = unfrozen;

                    globalAudio.PlayOneShot(firesfx, 3.0f);

                    played = true;
                    frozen = false;
                    breakable.isbreakable = false;

                    launchhand.return1();
                }
            }
        }
        else
        {
            played = false;
        }
    }
}