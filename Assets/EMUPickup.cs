using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMUPickup : MonoBehaviour
{

    public AudioSource globalaudio;
    public AudioClip pickupsx;
    public HandManager handmanager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Pickup()
    {
        gameObject.SetActive(false);
        globalaudio.PlayOneShot(pickupsx, 1.0f);
        handmanager.HasEMUCuffs = true;
    }
}
