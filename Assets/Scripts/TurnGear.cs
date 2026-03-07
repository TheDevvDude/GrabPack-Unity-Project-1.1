using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnGear : MonoBehaviour
{
    public float rotationSpeed = 30f;

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f);
    }
}

