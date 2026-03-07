using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricalReciever : MonoBehaviour
{
    public List<PowerPole> polesInPuzzle;
    public bool CircuitComplete = false;

    public AudioSource GlobalAudio;
    public AudioClip puzzlecomplete;

    private bool complete = false;

    public GameObject[] hands;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (AllPolesPowered())
        {

            foreach (GameObject hand in hands)
            {
                if (hand != null && hand.activeInHierarchy && hand.transform.parent == transform)
                {
                    CircuitComplete = true;

                    if (!complete)
                    {
                        GlobalAudio.PlayOneShot(puzzlecomplete, 1.0f);
                        complete = true;
                        ReturnAllHands();
                    }
                }
            }

        }
    }

    bool AllPolesPowered()
    {
        foreach (PowerPole pole in polesInPuzzle)
        {
            if (!pole.powered) return false;
        }
        return true;
    }

    void ReturnAllHands()
    {
        LaunchHand[] hands = FindObjectsOfType<LaunchHand>();

        foreach (LaunchHand hand in hands)
        {
            hand.return1();
        }
    }
}
