using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnObject : MonoBehaviour
{

    public AudioSource globalAudio;
    public AudioClip firesfx;
    private bool played = false;

    public ParticleSystem grabparticles;


    public BoxCollider collider;
    public SkinnedMeshRenderer renderer;

    public Door door;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Transform child1 = transform.Find("Hand_Conductive");
        

        if (child1 != null )
        {

            Conductor conductor = child1.GetComponent<Conductor>();
            LaunchHand launchhand = child1.GetComponent<LaunchHand>();

            if (played == false && conductor.CurrentElement == "fire")
            {


                if (grabparticles != null)
                {
                    grabparticles.Play();
                }
                globalAudio.PlayOneShot(firesfx, 3.0f);
                played = true;

                renderer.enabled = false;
                collider.enabled = false;
                launchhand.return1();
                door.Locked = false;
            }
        }
        else
        {
            played = false;

        }
    }
}
