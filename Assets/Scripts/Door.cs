using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool Locked = false;
    public Animator animator;

    public HandScanner ConnectedHandScanner;

    public bool hasopened = false;
    private bool JustUnlocked = false;

    public AudioSource audiosource;
    public AudioClip opensfx;
    public AudioClip closesfx;

    void Update()
    {
        if (!Locked)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Camera cam = Camera.main;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, 2f))
                {
                    Door door = hit.collider.GetComponent<Door>();
                    if (door != null)
                    {
                        door.Open();
                    }
                }
            }

            bool handAttached = false;

            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("Hand"))
                {
                    handAttached = true;
                    break;
                }
            }

            if (handAttached)
            {
                if (!JustUnlocked && !hasopened)
                {
                    Open();
                    hasopened = true;
                }
            }
            else
            {
                hasopened = false;
                JustUnlocked = false;
            }
        }
        else
        {
            if (ConnectedHandScanner.SCANNED)
            {
                Locked = false;
                JustUnlocked = true;
            }
        }
    }

    public void Open()
    {
        if (Locked) return;

        bool open = animator.GetBool("open");

        animator.SetBool("open", !open);

        if (open)
            audiosource.PlayOneShot(closesfx, 1.0f);
        else
            audiosource.PlayOneShot(opensfx, 1.0f);
    }
}