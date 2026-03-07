using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPole : MonoBehaviour
{
    public GameObject glow;
    public float glowcounter = 0.1f;
    public ElectricalSource source;

    public AudioSource GlobalAudio;
    public AudioClip connect;
    public AudioClip disconnect;


    public bool powered = false;
    private bool touchedThisFrame = false;


    private bool wasPowered = false;

    public void Startglow()
    {
        if (source.powering)
        {
            glow.SetActive(true);
            powered = true;
            touchedThisFrame = true;
            glowcounter = 0.1f;
        }
    }



    void Update()
    {
        if (!touchedThisFrame)
            glowcounter -= Time.deltaTime;

        if (glowcounter <= 0)
        {
            glow.SetActive(false);
            powered = false;
        }

        if (powered && !wasPowered)
        {
            GlobalAudio.PlayOneShot(connect, 0.7f);
        }
        else if (!powered && wasPowered)
        {
            GlobalAudio.PlayOneShot(disconnect, 0.7f);
        }

        wasPowered = powered;

        touchedThisFrame = false;
    }
}

