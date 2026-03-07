using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryPoweredScanner : MonoBehaviour
{
    public GameObject ONLINEscanner;
    public BatterySOcket connectedBatterySocket;

    public HandScanner workingScanner;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (connectedBatterySocket.full == true)
        {
            ONLINEscanner.SetActive(true);
            workingScanner.enabled = true;
        }
        if (connectedBatterySocket.full == false)
        {
            ONLINEscanner.SetActive(false); 
            workingScanner.enabled = false;

        }
    }
}
