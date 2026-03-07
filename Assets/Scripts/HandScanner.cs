using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandScanner : MonoBehaviour
{
    public string HandToDectect = "Hand_Blue";

    public MeshRenderer screen;
    public Material screenidle;
    public Material screenScanning;

    public bool hasStarted = false;

    public float minOffset = 0f;
    public float maxOffset = 1f;
    public float speed = 1f;

    public MeshRenderer scanningLabel;
    public Material ready;
    public Material scanning;

    public Material greenbackground;
    public Material verified;


    public bool SCANNED = false;
    private float scanningduration;

    public float ScanDuration;


    public MeshRenderer handprint;
    public Material handprintmaterial;
    public Material smile;


    public Color Green;
    public Color scannercolour;


    public Light scannerLight;

    public GameObject scanningAudio;

    // Start is called before the first frame update
    void Start()
    {
        scanningduration = 0f;


        scannerLight.color = scannercolour;

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 offset3 = ready.mainTextureOffset;
        offset3.x += speed / 2 * Time.deltaTime; 
        ready.mainTextureOffset = offset3;

        if (!SCANNED)
        {
            Transform child = transform.Find(HandToDectect);
            if (child != null)
            {
                if (hasStarted == false)
                {
                    Vector2 offset2 = scanning.mainTextureOffset;
                    offset2.x = 0;
                    startScanning();


                }

            }
            else
            {
                screen.material = screenidle;
                hasStarted = false;
                scanningLabel.material = ready;
                Vector2 offset2 = scanning.mainTextureOffset;
                offset2.x = 0;

                scanningAudio.SetActive(false);


            }
            if (hasStarted)
            {
                scanningduration += Time.deltaTime;


                Vector2 offset2 = scanning.mainTextureOffset;
                offset2.x += speed / 2 * Time.deltaTime; 
                scanning.mainTextureOffset = offset2;



                float offset = Mathf.Lerp(minOffset, maxOffset, Mathf.PingPong(Time.time * speed, 1f));
                screenScanning.mainTextureOffset = new Vector2(screenScanning.mainTextureOffset.x, offset);



                if (scanningduration > ScanDuration)
                {
                    SCANNED = true;
                }
            }
        }
        if (SCANNED)
        {
            handprint.material = smile;
            scannerLight.color = Green;


            screen.material = greenbackground;
            hasStarted = false;
            scanningLabel.material = verified;

            Vector2 offset4 = verified.mainTextureOffset;
            offset4.x += speed / 2 * Time.deltaTime; 
            verified.mainTextureOffset = offset4;
        }




    }

    public void startScanning()
    {
        scanningduration = 0f;
        scanningAudio.SetActive(true);
        Vector2 offset2 = scanning.mainTextureOffset;
        offset2.x = 0;
        scanningLabel.material = scanning;
        hasStarted = true;

        screen.material = screenScanning;

    }
}