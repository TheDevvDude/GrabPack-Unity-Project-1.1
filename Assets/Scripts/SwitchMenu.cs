using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchMenu : MonoBehaviour
{
    public GameObject switcher;
    public bool open = false;
    
    public void clicked()
    {
        open = !open;

        switcher.SetActive(open);
    }

    public void closed()
    {
        open = false;

        switcher.SetActive(false);
    }
}
