using UnityEngine;

public class Conductor : MonoBehaviour
{
    public string CurrentElement = "none";

    public SkinnedMeshRenderer meshRenderer;

    public Material iceMaterial;
    public Material fireMaterial;
    public Material greenMaterial;
    public Material noneMaterial;

    public int materialIndex = 1;

    private float elementcounter = 0;
    public float ElementTime = 15;

    public AudioSource globalAudio;
    public AudioClip fireRelease;
    public AudioClip greenRelease;
    public AudioClip iceRelease;

    public GameObject sparks;
    public GameObject freezing;
    public GameObject heat;

    void OnEnable()
    {
        CurrentElement = "none";
        UpdateElement(CurrentElement);

        elementcounter = ElementTime;
    }

    public void UpdateElement(string element)
    {
        CurrentElement = element;
        elementcounter = ElementTime;

        Material newMat = noneMaterial;

        if (element == "ice")
        {
            newMat = iceMaterial;
            freezing.SetActive(true);
        }
        else if (element == "fire")
        {
            newMat = fireMaterial;
            heat.SetActive(true);
        }
        else if (element == "green")
            newMat = greenMaterial;
            sparks.SetActive(true);


        if (element != "green")
        {
            sparks.SetActive(false);
        }
        if (element != "ice")
        {
            freezing.SetActive(false);

        }
        if (element != "fire")
        {
            heat.SetActive(false);

        }

        ChangeMaterial(newMat);
    }

    void ChangeMaterial(Material newMat)
    {
        Material[] mats = meshRenderer.materials;

        if (materialIndex < mats.Length)
        {
            mats[materialIndex] = newMat;
            meshRenderer.materials = mats;
        }
    }

    public void Update()
    {
        if (CurrentElement != "none")
        {
            elementcounter -= Time.deltaTime;

        }
        if (elementcounter < 0)
        {
            if (CurrentElement == "fire")
            {
                globalAudio.PlayOneShot(fireRelease, 2.0f);
            }
            if (CurrentElement == "ice")
            {
                globalAudio.PlayOneShot(iceRelease, 2.0f);
            }
            if (CurrentElement == "green")
            {
                globalAudio.PlayOneShot(greenRelease, 2.0f);
            }

            CurrentElement = "none";
            UpdateElement(CurrentElement);

            elementcounter = ElementTime;
        }
    }
}