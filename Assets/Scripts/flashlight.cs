using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flashlight : MonoBehaviour
{
    public Animator flashlightanimator;
    public bool isOn = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
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

    public void ToggleFlashlight()
    {
        isOn = !isOn;
    }
}
