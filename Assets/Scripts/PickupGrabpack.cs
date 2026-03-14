using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupGrabpack : MonoBehaviour
{
    public bool hasRedHand = true;
    public bool hasBlueHand = true;
    public bool hasPurpleHand = true;
    public bool hasPressureHand = true;
    public bool hasConductiveHand = true;


    public GameObject RedHand;
    public GameObject PurpleHand;
    public GameObject FlareHand;
    public GameObject conductiveHand;
    public GameObject BlueHand;


    public GameObject MockRedHand;
    public GameObject MockPurpleHand;
    public GameObject MockFlareHand;
    public GameObject MockconductiveHand;
    public GameObject MockBlueHand;

    public HandManager handmanager;

    public RigidboyPlayerController player;
    public MobileIcons mobileicons;


    public void Pickup()
    {
        gameObject.SetActive(false);

        BlueHand.SetActive(hasBlueHand);

        RedHand.SetActive(false);
        PurpleHand.SetActive(false);
        FlareHand.SetActive(false);
        conductiveHand.SetActive(false);

        if (hasRedHand)
            RedHand.SetActive(true);
        else if (hasPurpleHand)
            PurpleHand.SetActive(true);
        else if (hasPressureHand)
            FlareHand.SetActive(true);
        else if (hasConductiveHand)
            conductiveHand.SetActive(true);

        handmanager.hasGrabPack = true;

        handmanager.hasRedHand = hasRedHand;
        handmanager.hasBlueHand = hasBlueHand;
        handmanager.hasPressureHand = hasPressureHand;
        handmanager.hasPurpleHand = hasPurpleHand;
        handmanager.hasConductiveHand = hasConductiveHand;

        player.UpdateHandButtons();

        if (mobileicons.isMobile)
        {
            mobileicons.UpdateMobileIcons();

        }

    }

    void Start()
    {
        if (handmanager.hasGrabPack == true)
        {
            gameObject.SetActive(false);
        }


        if (hasBlueHand)
        {
            MockBlueHand.SetActive(true);
        }


        if (hasRedHand)
            MockRedHand.SetActive(true);
        else if (hasPurpleHand)
            MockPurpleHand.SetActive(true);
        else if (hasPressureHand)
            MockFlareHand.SetActive(true);
        else if (hasConductiveHand)
            MockconductiveHand.SetActive(true);
    }
}
