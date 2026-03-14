using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileIcons : MonoBehaviour
{

    public bool isMobile;

    public GameObject MobileControls;

    public HandManager handmanager;

    // Start is called before the first frame update
    void Start()
    {
        if (isMobile)
        {
            if (handmanager.hasGrabPack == true)
            {
                MobileControls.SetActive(true);

            }
            else
            {
                MobileControls.SetActive(false);

            }
        }
        else
        {
            MobileControls.SetActive(false);
        }
    }

    public void UpdateMobileIcons()
    {
        MobileControls.SetActive(true);

    }
}
