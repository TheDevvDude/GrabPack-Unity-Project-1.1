using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineCartAdjuster : MonoBehaviour
{
    public float rot1;
    public float rot2;

    public bool position = false;

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
        if (other.gameObject.layer == LayerMask.NameToLayer("Minecart"))
        {
            position = !position;

            if (position == true)
            {

                other.transform.rotation = Quaternion.Euler(0, rot1, 0);
            }
            if (position == false)
            {
   
                other.transform.rotation = Quaternion.Euler(0, rot2, 0);
            }
        }
    }
}
