using UnityEngine;
using System.Collections;

public class RigidboyPlayerController : MonoBehaviour
{
    private const string DebugSpawnInEditorPrefKey = "MobileUIBootstrap_DebugSpawnInEditor";
    private Coroutine footstepCoroutine;

    public Transform headCheck; 
    public float standUpCheckHeight = 1f;

    private float targetX;
    private float targetY;
    private float currentY;

    public float moveSpeed = 5f;
    public float sprintMultiplier = 2f;
    public float crouchMultiplier = 0.5f;

    public float sprintAcceleration = 2f;
    public float jumpForce = 10f;
    public float lookSpeedX = 2f;
    public float lookSpeedY = 2f;
    public Transform playerCamera;

    private Rigidbody rb;
    private float currentMoveSpeed;
    private float targetMoveSpeed;
    private float rotationX = 0f;

    public Animator playeranimations;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;
    public bool isGrounded;

    public bool IsCrouched = false;

    public Animator croucher;
    private Vector3 moveDirection;

    public GameObject RedHand;
    public GameObject PurpleHand;
    public GameObject FlareHand;
    public GameObject conductiveHand;
    public GameObject BlueHand;

    public string handtoSwitch;

    public LaunchHand purplelauncher;
    public LaunchHand redLauncher;

    public float groundCheckRadius;
    public Transform groundCheck;

    public CapsuleCollider playerstandingCollider;

    private Vector3 currentMoveDirection;
    public float airInputSmooth = 6f;

    public CablePhysics redcable;
    public CablePhysics purplecable;
    public CablePhysics pressurecable;
    public CablePhysics conductivecable;

    public LaunchHand redlaunch;
    public LaunchHand purplelaunch;
    public LaunchHand pressurelaunch;
    public LaunchHand conductivelaunch;

    public AudioSource footstepSource;
    public AudioClip[] grassFootsteps;
    public AudioClip[] woodFootsteps;
    public AudioClip[] concreteFootsteps;
    public AudioClip[] MetalFootsteps;

    public AudioClip crouchsfx;
    public AudioClip jumpsfx;
    public AudioClip uncrouchsfx;

    public bool mobileControlsEnabled = true;
    public bool useMobilePlatformDetection = true;
    public bool debugMobileInEditor = false;

    private Vector2 mobileMoveInput;
    private Vector2 mobileLookInput;
    private bool mobileJumpQueued;
    private bool mobileSprintHeld;
    private bool mobileCrouchQueued;
    private bool mobileSelectRedQueued;
    private bool mobileSelectPurpleQueued;
    private bool mobileSelectFlareQueued;
    private bool mobileSelectConductiveQueued;
    private Vector2 currentLookDelta;
    private bool currentLookUsesMouse;

    private bool isPlayingFootsteps = false;

    public HandManager handmanager;

    public bool MobileControlsActive => mobileControlsEnabled && (!useMobilePlatformDetection || IsMobileControlsRuntimeActive());
    public Vector2 CurrentLookDelta => currentLookDelta;
    public bool CurrentLookUsesMouse => currentLookUsesMouse;
    public bool DesktopMouseLookAllowed => ShouldUseDesktopMouseLook();
    public string CurrentLookSource => currentLookUsesMouse ? "Mouse" : (currentLookDelta.sqrMagnitude > 0.0001f ? "Touch" : "None");

    private bool CanStandUp()
    {
        return !Physics.SphereCast(headCheck.position, 0.3f, Vector3.up, out _, standUpCheckHeight, groundLayer);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentMoveSpeed = moveSpeed;
        targetMoveSpeed = moveSpeed;

        playeranimations.SetBool("walk", false);
        playeranimations.SetBool("switch", false);
        playeranimations.SetBool("jump", false);
        playeranimations.SetBool("crouch", false);

        if (MobileControlsActive)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }

        if (handmanager.hasRedHand == false)
        {
            RedHand.SetActive(false);
        }
        if (handmanager.hasBlueHand == false)
        {
            BlueHand.SetActive(false);
        }
        if (handmanager.hasPurpleHand == false)
        {
            PurpleHand.SetActive(false);
        }
        if (handmanager.hasPressureHand == false)
        {
            FlareHand.SetActive(false);
        }
        if (handmanager.hasConductiveHand == false)
        {
            conductiveHand.SetActive(false);
        }

        InitializeStartingHand();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || mobileSelectRedQueued)
        {
            Debug.Log($"[RigidboyPlayerController] Red hand input received keyboard={Input.GetKeyDown(KeyCode.Alpha1)} queued={mobileSelectRedQueued}");
            SelectRedHand();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || mobileSelectPurpleQueued)
        {
            Debug.Log($"[RigidboyPlayerController] Purple hand input received keyboard={Input.GetKeyDown(KeyCode.Alpha2)} queued={mobileSelectPurpleQueued}");
            SelectPurpleHand();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) || mobileSelectFlareQueued)
        {
            Debug.Log($"[RigidboyPlayerController] Flare hand input received keyboard={Input.GetKeyDown(KeyCode.Alpha3)} queued={mobileSelectFlareQueued}");
            SelectFlareHand();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) || mobileSelectConductiveQueued)
        {
            Debug.Log($"[RigidboyPlayerController] Conductive hand input received keyboard={Input.GetKeyDown(KeyCode.Alpha4)} queued={mobileSelectConductiveQueued}");
            SelectConductiveHand();
        }

        mobileSelectRedQueued = false;
        mobileSelectPurpleQueued = false;
        mobileSelectFlareQueued = false;
        mobileSelectConductiveQueued = false;

        isGrounded = Physics.SphereCast(groundCheck.position, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckRadius + 0.3f, groundLayer);

        float desktopLookX = ShouldUseDesktopMouseLook() ? Input.GetAxis("Mouse X") : 0f;
        float desktopLookY = ShouldUseDesktopMouseLook() ? Input.GetAxis("Mouse Y") : 0f;
        currentLookUsesMouse = Mathf.Abs(desktopLookX) > 0.0001f || Mathf.Abs(desktopLookY) > 0.0001f;
        currentLookDelta = currentLookUsesMouse ? new Vector2(desktopLookX, desktopLookY) : mobileLookInput;

        float lookX = currentLookDelta.x * lookSpeedX;
        float lookY = currentLookDelta.y * lookSpeedY;
        mobileLookInput = Vector2.zero;

        targetX -= lookY;
        targetX = Mathf.Clamp(targetX, -90f, 90f);

        targetY += lookX;

        rotationX = Mathf.Lerp(rotationX, targetX, 30f * Time.deltaTime);
        currentY = Mathf.Lerp(currentY, targetY, 30f * Time.deltaTime);

        playerCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, currentY, 0f);

        float moveX = Mathf.Clamp(Input.GetAxisRaw("Horizontal") + mobileMoveInput.x, -1f, 1f);
        float moveZ = Mathf.Clamp(Input.GetAxisRaw("Vertical") + mobileMoveInput.y, -1f, 1f);

        Vector3 rawDirection = transform.right * moveX + transform.forward * moveZ;

        currentMoveDirection = Vector3.MoveTowards(
            currentMoveDirection,
            rawDirection,
            (isGrounded ? 20f : airInputSmooth) * Time.deltaTime
        );

        moveDirection = currentMoveDirection.normalized;
        if (moveX != 0 || moveZ != 0)
        {
            playeranimations.SetBool("walk", true);
            if (footstepCoroutine == null)
            {
                footstepCoroutine = StartCoroutine(PlayFootsteps());
            }
        }
        else
        {
            playeranimations.SetBool("walk", false);
            if (footstepCoroutine != null)
            {
                StopCoroutine(footstepCoroutine);
                footstepCoroutine = null;
            }
        }

        if (isGrounded)
        {
            playeranimations.SetBool("jump", false);
        }
        else
        {
            playeranimations.SetBool("jump", true);
        }

        if (!IsCrouched)
        {
            if (Input.GetKey(KeyCode.LeftShift) || mobileSprintHeld)
            {
                targetMoveSpeed = moveSpeed * sprintMultiplier;
                playeranimations.speed = 1.6f;
            }
            else
            {
                targetMoveSpeed = moveSpeed;
                playeranimations.speed = 1f;
            }
        }

        if (isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) && !IsCrouched)
            {
                Crouch();
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) && IsCrouched)
            {
                StandUp();
            }

            if (mobileCrouchQueued)
            {
                if (IsCrouched)
                {
                    StandUp();
                }
                else
                {
                    Crouch();
                }
            }
        }

        mobileCrouchQueued = false;

        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, sprintAcceleration * Time.deltaTime);

        if (!IsCrouched && (Input.GetButtonDown("Jump") || mobileJumpQueued) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            footstepSource.PlayOneShot(jumpsfx, 3.0f);
        }

        mobileJumpQueued = false;
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector3(
            moveDirection.x * currentMoveSpeed,
            rb.velocity.y,
            moveDirection.z * currentMoveSpeed
        );
    }

    private IEnumerator PlayFootsteps()
    {
        isPlayingFootsteps = true;

        while (moveDirection.magnitude > 0)
        {
            if (isGrounded)
            {
                float footstepInterval = IsCrouched ? 0.8f : (targetMoveSpeed > moveSpeed ? 0.3f : 0.5f);
                float volume = IsCrouched ? 0.5f : (targetMoveSpeed > moveSpeed ? 0.8f : 0.5f);

                AudioClip footstepClip = GetFootstepSound();
                if (footstepClip != null)
                {
                    footstepSource.PlayOneShot(footstepClip, volume);
                }

                yield return new WaitForSeconds(footstepInterval);
            }
            else
            {
                yield return null;
            }
        }

        isPlayingFootsteps = false;
    }

    private AudioClip GetFootstepSound()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            switch (hit.collider.tag)
            {
                case "Grass": return grassFootsteps[Random.Range(0, grassFootsteps.Length)];
                case "Wood": return woodFootsteps[Random.Range(0, woodFootsteps.Length)];
                case "Concrete": return concreteFootsteps[Random.Range(0, concreteFootsteps.Length)];
                case "Grab-able": return concreteFootsteps[Random.Range(0, concreteFootsteps.Length)];
                case "Metal": return MetalFootsteps[Random.Range(0, MetalFootsteps.Length)];
            }
        }
        return null;
    }

    public void SelectRedHand()
    {
        Debug.Log($"[RigidboyPlayerController] SelectRedHand unlocked={handmanager != null && handmanager.hasRedHand}");
        TrySwitchHand("red", handmanager.hasRedHand);
    }

    public void SelectPurpleHand()
    {
        Debug.Log($"[RigidboyPlayerController] SelectPurpleHand unlocked={handmanager != null && handmanager.hasPurpleHand}");
        TrySwitchHand("purple", handmanager.hasPurpleHand);
    }

    public void SelectFlareHand()
    {
        Debug.Log($"[RigidboyPlayerController] SelectFlareHand unlocked={handmanager != null && handmanager.hasPressureHand}");
        TrySwitchHand("flare", handmanager.hasPressureHand);
    }

    public void SelectConductiveHand()
    {
        Debug.Log($"[RigidboyPlayerController] SelectConductiveHand unlocked={handmanager != null && handmanager.hasConductiveHand}");
        TrySwitchHand("conductive", handmanager.hasConductiveHand);
    }

    public void QueueSelectRedHand()
    {
        mobileSelectRedQueued = true;
        Debug.Log("[RigidboyPlayerController] QueueSelectRedHand");
    }

    public void QueueSelectPurpleHand()
    {
        mobileSelectPurpleQueued = true;
        Debug.Log("[RigidboyPlayerController] QueueSelectPurpleHand");
    }

    public void QueueSelectFlareHand()
    {
        mobileSelectFlareQueued = true;
        Debug.Log("[RigidboyPlayerController] QueueSelectFlareHand");
    }

    public void QueueSelectConductiveHand()
    {
        mobileSelectConductiveQueued = true;
        Debug.Log("[RigidboyPlayerController] QueueSelectConductiveHand");
    }

    public void EmulateDesktopHandKey1()
    {
        Debug.Log("[RigidboyPlayerController] EmulateDesktopHandKey1");
        QueueSelectRedHand();
    }

    public void EmulateDesktopHandKey2()
    {
        Debug.Log("[RigidboyPlayerController] EmulateDesktopHandKey2");
        QueueSelectPurpleHand();
    }

    public void EmulateDesktopHandKey3()
    {
        Debug.Log("[RigidboyPlayerController] EmulateDesktopHandKey3");
        QueueSelectFlareHand();
    }

    public void EmulateDesktopHandKey4()
    {
        Debug.Log("[RigidboyPlayerController] EmulateDesktopHandKey4");
        QueueSelectConductiveHand();
    }

    private void TrySwitchHand(string targetHand, bool unlocked)
    {
        if (!unlocked)
        {
            Debug.Log($"[RigidboyPlayerController] TrySwitchHand blocked target={targetHand} reason=locked");
            return;
        }

        if (redcable.isActive || purplecable.isActive || pressurecable.isActive || conductivecable.isActive)
        {
            Debug.Log($"[RigidboyPlayerController] TrySwitchHand blocked target={targetHand} reason=cable_active red={redcable.isActive} purple={purplecable.isActive} flare={pressurecable.isActive} conduct={conductivecable.isActive}");
            return;
        }

        if (redlaunch.holdingbattery || purplelaunch.holdingbattery || pressurelaunch.holdingbattery || conductivelaunch.holdingbattery)
        {
            Debug.Log($"[RigidboyPlayerController] TrySwitchHand blocked target={targetHand} reason=holding_battery red={redlaunch.holdingbattery} purple={purplelaunch.holdingbattery} flare={pressurelaunch.holdingbattery} conduct={conductivelaunch.holdingbattery}");
            return;
        }

        Debug.Log($"[RigidboyPlayerController] TrySwitchHand trigger target={targetHand}");
        playeranimations.SetBool("switch", true);
        handtoSwitch = targetHand;
        playeranimations.SetTrigger("Switch");
    }

    public void SwitchHand()
    {
        playeranimations.speed = 1f;
        targetMoveSpeed = moveSpeed;
        Debug.Log($"[RigidboyPlayerController] SwitchHand handtoSwitch={handtoSwitch}");

        if (handtoSwitch == "red")
        {
            redhand();
        }
        if (handtoSwitch == "purple")
        {
            purplehand();
        }
        if (handtoSwitch == "flare")
        {
            flarehand();
        }
        if (handtoSwitch == "conductive")
        {
            conductivehand();
        }

        Debug.Log($"[RigidboyPlayerController] ActiveHands Red={RedHand.activeSelf} Purple={PurpleHand.activeSelf} Flare={FlareHand.activeSelf} Conduct={conductiveHand.activeSelf} Blue={BlueHand.activeSelf}");
    }

    public void redhand()
    {
        RedHand.SetActive(true);
        PurpleHand.SetActive(false);
        FlareHand.SetActive(false);
        conductiveHand.SetActive(false);
    }

    public void purplehand()
    {
        RedHand.SetActive(false);
        PurpleHand.SetActive(true);
        FlareHand.SetActive(false);
        conductiveHand.SetActive(false);
    }

    public void flarehand()
    {
        RedHand.SetActive(false);
        PurpleHand.SetActive(false);
        FlareHand.SetActive(true);
        conductiveHand.SetActive(false);
    }

    public void conductivehand()
    {
        RedHand.SetActive(false);
        PurpleHand.SetActive(false);
        FlareHand.SetActive(false);
        conductiveHand.SetActive(true);
    }

    private void InitializeStartingHand()
    {
        RedHand.SetActive(false);
        PurpleHand.SetActive(false);
        FlareHand.SetActive(false);
        conductiveHand.SetActive(false);

        BlueHand.SetActive(handmanager.hasBlueHand);

        if (handmanager.hasRedHand)
        {
            redhand();
        }
        else if (handmanager.hasPurpleHand)
        {
            purplehand();
        }
        else if (handmanager.hasPressureHand)
        {
            flarehand();
        }
        else if (handmanager.hasConductiveHand)
        {
            conductivehand();
        }
    }

    public void StopFootsteps()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }

        isPlayingFootsteps = false;

        if (footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }
    }

    public void SetMoveInput(Vector2 input)
    {
        mobileMoveInput = Vector2.ClampMagnitude(input, 1f);
    }

    public void SetLookInput(Vector2 input)
    {
        mobileLookInput = input;
    }

    public void AddLookInput(Vector2 input)
    {
        mobileLookInput += input;
    }

    public void QueueJump()
    {
        mobileJumpQueued = true;
    }

    public void SetSprint(bool sprinting)
    {
        mobileSprintHeld = sprinting;
    }

    public void ToggleCrouch()
    {
        mobileCrouchQueued = true;
    }

    private void Crouch()
    {
        targetMoveSpeed = moveSpeed * crouchMultiplier;
        playeranimations.speed = 1f;

        IsCrouched = true;
        croucher.SetBool("Crouched", true);
        playeranimations.SetBool("crouch", true);
        playerstandingCollider.enabled = false;

        footstepSource.PlayOneShot(crouchsfx, 1.0f);
    }

    private void StandUp()
    {
        if (!CanStandUp())
        {
            return;
        }

        croucher.SetBool("Crouched", false);
        playeranimations.SetBool("crouch", false);
        IsCrouched = false;
        playerstandingCollider.enabled = true;

        footstepSource.PlayOneShot(uncrouchsfx, 1.0f);
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockCursor()
    {
        if (IsEditorDebugMobileActive())
        {
            UnlockCursor();
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool IsMobileControlsRuntimeActive()
    {
        if (Application.isMobilePlatform)
        {
            return true;
        }

        return IsEditorDebugMobileActive();
    }

    private bool ShouldUseDesktopMouseLook()
    {
        if (Application.isMobilePlatform)
        {
            return false;
        }

        return !IsEditorDebugMobileActive();
    }

    private bool IsEditorDebugMobileActive()
    {
        return Application.isEditor && (debugMobileInEditor || PlayerPrefs.GetInt(DebugSpawnInEditorPrefKey, 0) == 1);
    }
}
