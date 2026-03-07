using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSway : MonoBehaviour
{
    public float swayAmount;
    public float maxSwayAmount;
    public float swaySmoothness;

    private Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = -Input.GetAxis("Mouse X") * swayAmount;
        float moveY = -Input.GetAxis("Mouse Y") * swayAmount;


        moveX = Mathf.Clamp(moveX, -maxSwayAmount, maxSwayAmount);
        moveY = Mathf.Clamp(moveY, -maxSwayAmount, maxSwayAmount);


        Vector3 swayPosition = new Vector3(moveX, moveY, 0);


        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + swayPosition, Time.deltaTime * swaySmoothness);
    }
}
