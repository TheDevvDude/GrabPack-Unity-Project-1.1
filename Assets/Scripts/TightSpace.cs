using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TightSpace : MonoBehaviour
{

    public RigidboyPlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb == null) return;

            RetractAndDisableAllHands();  

            player.playeranimations.SetBool("squeeze", true);
            player.squeeze = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb == null) return;

            EnableAllHands();  

            player.squeeze = false;
            player.playeranimations.SetBool("squeeze", false);
        }
    }

    void RetractAndDisableAllHands()
    {
        LaunchHand[] hands = FindObjectsOfType<LaunchHand>();

        foreach (LaunchHand hand in hands)
        {
            if (hand != null)
            {
                hand.ForceImmediateReturn();
                hand.enabled = false;
            }
        }
    }

    void EnableAllHands()
    {
        LaunchHand[] hands = FindObjectsOfType<LaunchHand>();

        foreach (LaunchHand hand in hands)
        {
            if (hand != null)
            {
                hand.enabled = true;
            }
        }
    }
}
