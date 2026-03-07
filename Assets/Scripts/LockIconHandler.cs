using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockIconHandler : MonoBehaviour
{
    public GameObject LockIcon;


    public MeshRenderer lockRenderer;
    public Material lockedMat;
    public Material unlockedMat;

    public Door connecteddoor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.tag == ("Player"))
        {
            if (connecteddoor.Locked == true)
            {
                lockRenderer.material = lockedMat;
            }
            if (connecteddoor.Locked == false)
            {
                lockRenderer.material = unlockedMat;

            }
            LockIcon.SetActive(true);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == ("Player"))
        {

            LockIcon.SetActive(false);
        }
    }
}
