using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyButton : MonoBehaviour
{
    public Animator selfanimator;

    public int number;

    public Keypad keypad;

    public CheckCode checkcode;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void pushed()
    {
        selfanimator.SetTrigger("push");

        if (checkcode.editable == true)
        {
     
            if (keypad.Code.Length >= 4)
            {
                keypad.Code = ""; 
                Debug.Log("Code reset to null");
            }
            else
            {
                
                keypad.Code += number.ToString();
                Debug.Log(keypad.Code); 
            }
        }

    }
}
