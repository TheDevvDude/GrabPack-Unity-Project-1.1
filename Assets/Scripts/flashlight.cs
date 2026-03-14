using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flashlight : MonoBehaviour
{
    public Animator flashlightanimator;
    public bool isOn = false;

    public HandManager handmanager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (handmanager.hasGrabPack == true)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                isOn = !isOn;
            }

            if (isOn)
            {
                flashlightanimator.SetBool("on", true);
            }
            if (!isOn)
            {
                flashlightanimator.SetBool("on", false);
            }
        }

    }

    public void toggleflashlight()
    {
        if (handmanager.hasGrabPack == true)
        {
            isOn = !isOn;

        }

    }
}
