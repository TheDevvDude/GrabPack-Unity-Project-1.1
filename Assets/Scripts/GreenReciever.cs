using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenReciever : MonoBehaviour
{

    public AudioSource globalAudio;
    public AudioClip greensfx;
    private bool played = false;

    public ParticleSystem grabparticles;

    public Door door;

    public float livetime = 15;
    private float livecount = 0;

    public bool powered = false;

    public GameObject poweredLight;
    private bool wasPowered = false;
    // Start is called before the first frame update
    void Start()
    {
        livecount = livetime;

    }

    // Update is called once per frame
    void Update()
    {
        Transform child1 = transform.Find("Hand_Conductive");


        if (child1 != null)
        {

            Conductor conductor = child1.GetComponent<Conductor>();
            LaunchHand launchhand = child1.GetComponent<LaunchHand>();

            if (played == false && conductor.CurrentElement == "green")
            {


                if (grabparticles != null)
                {
                    grabparticles.Play();
                }
                globalAudio.PlayOneShot(greensfx, 3.0f);
                played = true;


                launchhand.return1();

                powered = true;



            }






        }
        else
        {
            played = false;

        }




        if (powered)
        {
            poweredLight.SetActive(true);
            livecount -= Time.deltaTime;
            if (livecount <= 0)
            {
                powered = false;
                livecount = livetime;
            }

            if (!wasPowered)
            {
                door.Locked = false;
                door.Open();
                wasPowered = true;
            }
        }
        else
        {
            if (wasPowered)
            {
                door.Locked = true;
                door.animator.SetBool("open", false);
                wasPowered = false;
            }
            poweredLight.SetActive(false);

        }
    }
}
