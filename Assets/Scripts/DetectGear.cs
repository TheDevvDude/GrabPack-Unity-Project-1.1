using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectGear : MonoBehaviour
{
    public GameObject GearToDetect;

    public GameObject GearVisual;

    public bool complete = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GearToDetect)
        {

            Destroy(GearToDetect);
            GearVisual.SetActive(true);
            complete = true;
        }
    }


}
