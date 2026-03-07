using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryDoor : MonoBehaviour
{
    public string BoolName;
    public Animator animator;

    public BatterySOcket ConnectedBattery;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ConnectedBattery.full == true)
        {
            animator.SetBool(BoolName, true);
        }
        if (ConnectedBattery.full == false)
        {
            animator.SetBool(BoolName, false);
        }
    }
}
