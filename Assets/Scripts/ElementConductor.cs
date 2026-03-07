using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementConductor : MonoBehaviour
{
    public string elementToConduct = "none";

    public AudioSource globalAudio;
    public AudioClip grabsfx;
    private bool played = false;

    public ParticleSystem grabparticles;

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

            conductor.CurrentElement = elementToConduct;
            conductor.UpdateElement(elementToConduct);

            if (!played)
            {
                if (grabparticles != null)
                    grabparticles.Play();

                globalAudio.PlayOneShot(grabsfx, 3.0f);
                played = true;
            }
        }
        else
        {
            played = false;
        }
    }
}