using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomMessage : MonoBehaviour
{

    public string[] messages;
    private int numOfMessages;
    private int messagenum;

    public Text messagUI;

    void Start()
    {
        numOfMessages = messages.Length;
        messagenum = Random.Range(0, numOfMessages);

        messagUI.text = messages[messagenum];
    }
}
