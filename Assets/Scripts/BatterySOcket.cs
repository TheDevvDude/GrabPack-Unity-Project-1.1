using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterySOcket : MonoBehaviour
{

    public LayerMask batteryLayer;
    public GameObject batteryPOS;

    public bool full = false;

    public Collider batterycollider;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerStay(Collider other)
    {

        if (full == false)
        {
            if ((batteryLayer & (1 << other.gameObject.layer)) != 0 && other.gameObject.name != "Gear")
            {


                Rigidbody rb = other.GetComponent<Rigidbody>();
                rb.isKinematic = true;
                other.gameObject.transform.position = batteryPOS.transform.position;
                other.gameObject.transform.rotation = batteryPOS.transform.rotation;
                full = true;

                batterycollider = other;
            }
        }



    }

    public void OnTriggerExit(Collider other)
    {

        if ((batteryLayer & (1 << other.gameObject.layer)) != 0)
        {
 
            full = false;
        }

    }

    void LateUpdate()
    {
        if (batterycollider.gameObject.transform.position != batteryPOS.transform.position)
        {
            full = false;
            batterycollider = null;
        }
    }
}
