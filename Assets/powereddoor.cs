using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powereddoor : MonoBehaviour
{
    public Animator animator;
    public ElectricalReciever electricalreciever;

    private bool done = false;

    void Update()
    {
        if (done) return;

        if (electricalreciever.CircuitComplete)
        {
            Powered();
            done = true;
        }
    }

    public void Powered()
    {
        animator.SetBool("open", true);
    }
}
