using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{
    private const string DebugSpawnInEditorPrefKey = "MobileUIBootstrap_DebugSpawnInEditor";
    public float interactionRange = 2f;
    public LayerMask buttonLayer; 
    public LayerMask CodeCheckerLayer;
    public LayerMask grabpackinteractable;
    public LayerMask handpickups;

    public Animator grabpack;
    public bool useScreenCenterAimOnMobile = true;
    public bool debugMobileInEditor = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TriggerInteraction();
        }
    }

    public void TriggerInteraction()
    {
        RaycastInteraction();
    }

    void RaycastInteraction()
    {
        Ray ray = Camera.main.ScreenPointToRay(GetInteractionScreenPosition());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, buttonLayer))
        {
            keyButton KeyButton = hit.transform.gameObject.GetComponent<keyButton>();
            KeyButton.pushed();
        }
        if (Physics.Raycast(ray, out hit, interactionRange, CodeCheckerLayer))
        {
            CheckCode checkcode = hit.transform.gameObject.GetComponent<CheckCode>();
            checkcode.Check();
        }
        if (Physics.Raycast(ray, out hit, interactionRange, grabpackinteractable))
        {
            PickupGrabpack pickup = hit.transform.gameObject.GetComponent<PickupGrabpack>();
            pickup.Pickup();
            grabpack.SetTrigger("pickup");
        }
        if (Physics.Raycast(ray, out hit, interactionRange, handpickups))
        {
            HandPickUp pickup = hit.transform.gameObject.GetComponent<HandPickUp>();
            pickup.PickupHand();
        }
        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            KeyCardScanner scanner = hit.transform.GetComponent<KeyCardScanner>();
            if (scanner != null && scanner.connectedCard != null && scanner.connectedCard.PICKED)
            {
                scanner.Insert();
            }
        }
    }

    private Vector3 GetInteractionScreenPosition()
    {
        if (IsMobileControlsRuntimeActive() && useScreenCenterAimOnMobile)
        {
            return new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        }

        return Input.mousePosition;
    }

    private bool IsMobileControlsRuntimeActive()
    {
        if (Application.isMobilePlatform)
        {
            return true;
        }

        return Application.isEditor && (debugMobileInEditor || PlayerPrefs.GetInt(DebugSpawnInEditorPrefKey, 0) == 1);
    }
}