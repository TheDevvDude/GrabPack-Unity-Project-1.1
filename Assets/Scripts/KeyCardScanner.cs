using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCardScanner : MonoBehaviour
{
    public Animator animator;
    public Animator doorAnimator;

    public KeyCard connectedCard;
    public float interactionRange = 2f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && connectedCard != null && connectedCard.PICKED)
        {
            TryInsertFromScreenPosition(Input.mousePosition);
        }
    }

    private void TryInsertFromScreenPosition(Vector3 screenPosition)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Ray ray = cam.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange) && hit.collider.gameObject == gameObject)
        {
            Insert();
        }
    }

    public void Insert()
    {
        animator.SetBool("insert", true);
        doorAnimator.SetBool("open", true);
    }
}
