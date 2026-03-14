using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ElectricalSource : MonoBehaviour
{
    [SerializeField] private AudioClip powerOnSFX;

    public bool Powering { get; private set; }

    private AudioSource audioSource;
    private CableMaterials cableMaterials;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        cableMaterials = FindObjectOfType<CableMaterials>();
    }

    void Update()
    {
        bool shouldBePowered = transform.childCount > 0;

        if (shouldBePowered != Powering)
        {
            Powering = shouldBePowered;

            if (cableMaterials != null)
                cableMaterials.SetPowered(Powering);

            if (Powering && powerOnSFX != null)
                audioSource.PlayOneShot(powerOnSFX);
        }
    }
}