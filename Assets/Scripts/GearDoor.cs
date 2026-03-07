using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GearDoor : MonoBehaviour
{

    public List<DetectGear> gears;
    public Animator door;

    // Start is called before the first frame update
    void Update()
    {
        if (gears.All(g => g.complete))
        {
            AllGearsComplete();
            enabled = false; 
        }
    }

    private void AllGearsComplete()
    {
        door.SetBool("open", true);
    }
}
