using UnityEngine;
using System.Collections.Generic;

public class ElectricalReciever : MonoBehaviour
{
    [SerializeField] private List<PowerPole> polesInPuzzle;
    [SerializeField] private AudioSource globalAudio;
    [SerializeField] private AudioClip puzzleCompleteSFX;

    public bool CircuitComplete;

    private bool complete;

    void Update()
    {
        if (complete) return;

        if (AllPolesPowered() && HasHandAttached())
        {
            CompleteCircuit();
        }
    }

    private bool AllPolesPowered()
    {
        foreach (PowerPole pole in polesInPuzzle)
        {
            if (!pole.powered)
                return false;
        }
        return true;
    }

    private bool HasHandAttached()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<LaunchHand>() != null)
                return true;
        }

        return false;
    }

    private void CompleteCircuit()
    {
        CircuitComplete = true;
        complete = true;

        if (globalAudio != null && puzzleCompleteSFX != null)
            globalAudio.PlayOneShot(puzzleCompleteSFX);

        ReturnAllHands();
    }

    private void ReturnAllHands()
    {
        LaunchHand[] hands = FindObjectsOfType<LaunchHand>();

        foreach (LaunchHand hand in hands)
        {
            hand.return1();
        }
    }
}