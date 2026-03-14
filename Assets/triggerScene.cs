using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class triggerScene : MonoBehaviour
{

    public int sceneindex = 0;
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb == null) return;
            SceneManager.LoadScene(sceneindex);

        }
    }
}
