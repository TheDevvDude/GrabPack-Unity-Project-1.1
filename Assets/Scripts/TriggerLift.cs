using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLift : MonoBehaviour
{
    public Animator Liftanimator;
    public Lift lift;
    public bool triggered = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == ("Player"))
        {
            if (!triggered)
            {
                Liftanimator.SetBool("open", false);
                Invoke(nameof(startLift), 1f);
                triggered = true;
            }


        }
    }

    void startLift()
    {
        lift.StartMoving();
    }
}
