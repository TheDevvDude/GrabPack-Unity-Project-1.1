using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCard : MonoBehaviour
{
    private const string DebugSpawnInEditorPrefKey = "MobileUIBootstrap_DebugSpawnInEditor";
    private MeshRenderer renderer;
    private BoxCollider collider;

    public LaunchHand redhand;
    public LaunchHand PurpleHand;
    public LaunchHand BlueHand;
    public LaunchHand PressureHand;
    public LaunchHand ConductiveHand;

    public Transform child1;
    public Transform child2;
    public Transform child3;
    public Transform child4;
    public Transform child5;

    public bool PICKED = false;
    public bool autoPickUpOnMobile = true;
    public bool debugMobileInEditor = false;

    void Start()
    {
        renderer = gameObject.GetComponent<MeshRenderer>();
        collider = gameObject.GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (PICKED)
        {
            return;
        }

        child1 = transform.Find("Hand_Rocket");
        child2 = transform.Find("Hand_Red");
        child3 = transform.Find("Hand_Blue");
        child4 = transform.Find("Hand_Pressure");
        child5 = transform.Find("Hand_Conductive");

        if (autoPickUpOnMobile && IsMobileControlsRuntimeActive())
        {
            if (TryPickUpAttachedHand())
            {
                return;
            }
        }

        if (child1 != null || child2 != null || child3 != null || child4 != null || child5 != null)
        {
            bool allowMouseButtons = ShouldUseDesktopMouseButtons();

            if (child1 != null && allowMouseButtons && Input.GetMouseButtonDown(1))
            {
                PickUp();
                PurpleHand.return1();
            }

            if (child2 != null && allowMouseButtons && Input.GetMouseButtonDown(1))
            {
                PickUp();
                redhand.return1();
            }
            if (child3 != null && allowMouseButtons && Input.GetMouseButtonDown(0))
            {
                PickUp();
                BlueHand.return1();
            }

            if (child4 != null && allowMouseButtons && Input.GetMouseButtonDown(1))
            {
                PickUp();
                PressureHand.return1();
            }
            if (child5 != null && allowMouseButtons && Input.GetMouseButtonDown(1))
            {
                PickUp();
                ConductiveHand.return1();
            }
        }
    }

    public void PickUp()
    {
        renderer.enabled = false;
        collider.enabled = false;
        PICKED = true;
        if (child1 != null)
        {
            PurpleHand.return1();
        }

        if (child2 != null)
        {
            redhand.return1();
        }
        if (child3 != null)
        {
            BlueHand.return1();
        }
        if (child4 != null)
        {
            PressureHand.return1();
        }
        if (child5 != null)
        {
            ConductiveHand.return1();
        }
    }

    private bool TryPickUpAttachedHand()
    {
        if (child1 != null)
        {
            PickUp();
            PurpleHand.return1();
            return true;
        }

        if (child2 != null)
        {
            PickUp();
            redhand.return1();
            return true;
        }

        if (child3 != null)
        {
            PickUp();
            BlueHand.return1();
            return true;
        }

        if (child4 != null)
        {
            PickUp();
            PressureHand.return1();
            return true;
        }

        if (child5 != null)
        {
            PickUp();
            ConductiveHand.return1();
            return true;
        }

        return false;
    }

    private bool IsMobileControlsRuntimeActive()
    {
        if (Application.isMobilePlatform)
        {
            return true;
        }

        return IsEditorDebugMobileActive();
    }

    private bool ShouldUseDesktopMouseButtons()
    {
        if (Application.isMobilePlatform)
        {
            return false;
        }

        return !IsEditorDebugMobileActive();
    }

    private bool IsEditorDebugMobileActive()
    {
        return Application.isEditor && (debugMobileInEditor || PlayerPrefs.GetInt(DebugSpawnInEditorPrefKey, 0) == 1);
    }
}
