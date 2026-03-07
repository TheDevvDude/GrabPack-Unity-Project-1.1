using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flashlight : MonoBehaviour
{
    public Animator flashlightanimator;
    public bool isOn = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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
