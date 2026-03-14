using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class RedSmokeEffects : MonoBehaviour
{
    public PostProcessVolume postProcessVolume;
    public float maxIntensity = 1.0f;
    public float increaseRate = 0.5f;
    public float decreaseRate = 1.0f;
    public float delayTime = 13f;

    private ChromaticAberration chromaticAberration;
    private bool isInZone = false;
    private float currentIntensity = 0f;
    private float timeInZone = 0f;


    public Animator redcamera;

    void Start()
    {
        if (postProcessVolume.profile.TryGetSettings(out chromaticAberration))
        {
            chromaticAberration.intensity.Override(0f);
        }
    }

    void Update()
    {
        if (chromaticAberration == null) return;

        if (isInZone)
        {
            timeInZone += Time.deltaTime;
            if (timeInZone >= delayTime)
            {
                currentIntensity = Mathf.Min(currentIntensity + increaseRate * Time.deltaTime, maxIntensity);
            }
        }
        else
        {
            timeInZone = 0f;
            currentIntensity = Mathf.Max(currentIntensity - decreaseRate * Time.deltaTime, 0f);
        }

        chromaticAberration.intensity.Override(currentIntensity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInZone = true;
            timeInZone = 0f;

            redcamera.SetBool("in", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInZone = false;
            redcamera.SetBool("in", false);

        }
    }

    public void RemoveEffects()
    {
        isInZone = false;
        redcamera.SetBool("in", false);
    }
}
