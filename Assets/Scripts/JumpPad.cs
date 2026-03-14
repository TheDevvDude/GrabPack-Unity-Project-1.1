using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class JumpPad : MonoBehaviour
{
    public float jumpForce = 10f;
    public float maxBoostDistance = 10f;
    public float minBoostMultiplier = 0.2f;
    public float cooldownTime = 2f;

    [Header("Power (Optional)")]
    public ElectricalReciever powerSource;

    public Material poweredMaterial;
    public Material offMaterial;
    public GameObject lightObject;

    public AudioClip boostSFX;

    private AudioSource audioSource;
    private MeshRenderer meshRenderer;

    private float cooldownTimer;
    private bool launched;
    private bool Powered;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        Powered = powerSource == null || powerSource.CircuitComplete;

        ApplyVisualState();
    }

    void Update()
    {
        HandlePowerState();
        cooldownTimer -= Time.deltaTime;
    }

    private void HandlePowerState()
    {
        bool shouldBePowered = powerSource == null || powerSource.CircuitComplete;

        if (shouldBePowered == Powered)
            return;

        Powered = shouldBePowered;
        ApplyVisualState();
    }

    private void ApplyVisualState()
    {


        if (lightObject != null)
            lightObject.SetActive(Powered);
    }

    void OnTransformChildrenChanged()
    {

        Debug.Log("Children changed!");
    

        if (!Powered || cooldownTimer > 0)
            return;

        LaunchHand hand = GetComponentInChildren<LaunchHand>();
        if (hand == null)
            return;

        if (hand.isRocketHand == false)
            return;

        Rigidbody playerRb = hand.ownerRigidbody;
        RigidboyPlayerController player = hand.ownerController;

        if (playerRb == null)
            return;

        float baseForce = player != null && player.isGrounded ? jumpForce : jumpForce / 2f;

        float distance = Vector3.Distance(playerRb.position, transform.position);
        float distanceMultiplier = Mathf.Clamp01(1 - (distance / maxBoostDistance));
        distanceMultiplier = Mathf.Lerp(minBoostMultiplier, 1f, distanceMultiplier);

        float finalForce = baseForce * distanceMultiplier;

        playerRb.velocity = Vector3.zero;
        playerRb.AddForce(transform.up * finalForce, ForceMode.Impulse);

        hand.return1();

        if (boostSFX != null)
            audioSource.PlayOneShot(boostSFX);

        cooldownTimer = cooldownTime;
    }
}