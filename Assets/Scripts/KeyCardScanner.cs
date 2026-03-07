using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCardScanner : MonoBehaviour
{
    public Animator animator;
    public Animator doorAnimator;

    public KeyCard connectedCard;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (connectedCard.PICKED == true)
            {
                Camera cam = Camera.main;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, 2f))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        KeyCardScanner scanner = GetComponent<KeyCardScanner>();
                        scanner.Insert();
                    }
                }
            }


        }

    }

    public void Insert()
    {
        animator.SetBool("insert", true);
        doorAnimator.SetBool("open", true);

    }
}
