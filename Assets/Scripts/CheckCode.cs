using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCode : MonoBehaviour
{
    public Keypad keypad;

    public MeshRenderer screenrenderer;
    public Material incorrect;
    public Material correct;
    public Material normal;

    public float resetcount = 2f;
    public bool resetting;
    public int materialIndex = 2;    

    public bool editable = true;

    public Animator anim;

    public Animator DoorAnimator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (resetting)
        {
            resetcount -= Time.deltaTime;
            if (resetcount < 0)
            {
                resetcount = 3;
                resetting = false;
                Material[] materials = screenrenderer.materials;
                materials[materialIndex] = normal;  
                screenrenderer.materials = materials;  

                keypad.Code = "";

                editable = true;
            }
        }
    }

    public void Check()
    {
        anim.SetTrigger("push");

        if (keypad.Code == keypad.CorrectCode)
        {
            editable = false;

            Material[] materials = screenrenderer.materials;
            materials[materialIndex] = correct;  
            screenrenderer.materials = materials;  

            DoorAnimator.SetBool("open", true);
        }
        if (keypad.Code != keypad.CorrectCode)
        {
            editable = false;

            Material[] materials = screenrenderer.materials;
            materials[materialIndex] = incorrect;  
            screenrenderer.materials = materials;  
            resetting = true;
        }
    }
}
