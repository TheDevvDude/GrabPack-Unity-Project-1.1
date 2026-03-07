using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CableManager : MonoBehaviour
{
    public float maxCableLength = 30f;
    public List<CablePhysics> cables = new List<CablePhysics>();

    public Image cableMeterImage;


    void LateUpdate()
    {
        float totalLength = GetTotalCableLength();
        UpdateCableUI();

        if (totalLength <= maxCableLength)
            return;

        float excess = totalLength - maxCableLength;

        EnforceAllCables(excess);
    }

    public float GetTotalCableLength()
    {
        float total = 0f;

        foreach (var cable in cables)
        {
            if (cable != null && cable.isActive)
            {
                total += cable.GetCableLength();
            }
        }

        return total;
    }

    void EnforceAllCables(float excess)
    {
        foreach (var cable in cables)
        {
            if (cable != null && cable.isActive)
            {
                cable.ApplySharedTension(excess);
            }
        }
    }

    public float GetRemainingLength()
    {
        float total = GetTotalCableLength();
        float remaining = maxCableLength - total;

        return Mathf.Max(0f, remaining);
    }

    void UpdateCableUI()
    {
        float remaining = GetRemainingLength();

        float normalized = remaining / maxCableLength;

        normalized = Mathf.Clamp01(normalized);

        cableMeterImage.fillAmount = normalized;
    }
}
