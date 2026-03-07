using UnityEngine;
using System.Collections;

public class RigidboyPlayerController : MonoBehaviour
{
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


    private bool isPlayingFootsteps = false;



    public HandManager handmanager;

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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


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
        if (Input.GetKeyDown(KeyCode.Alpha1) && handmanager.hasRedHand)
        {
            if (redcable.isActive == false && purplecable.isActive == false && pressurecable.isActive == false && conductivecable.isActive == false)
            {
                if (redlaunch.holdingbattery == false && purplelaunch.holdingbattery == false && pressurelaunch.holdingbattery == false && conductivelaunch.holdingbattery == false)
                {
                    playeranimations.SetBool("switch", true);
                    handtoSwitch = "red";
                    playeranimations.SetTrigger("Switch");

                }

            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && handmanager.hasPurpleHand)
        {
            if (redcable.isActive == false && purplecable.isActive == false && pressurecable.isActive == false && conductivecable.isActive == false)
            {
                if (redlaunch.holdingbattery == false && purplelaunch.holdingbattery == false && pressurelaunch.holdingbattery == false && conductivelaunch.holdingbattery == false)
                {
                    playeranimations.SetBool("switch", true);
                    handtoSwitch = "purple";
                    playeranimations.SetTrigger("Switch");

                }

            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && handmanager.hasPressureHand)
        {
            if (redcable.isActive == false && purplecable.isActive == false && pressurecable.isActive == false && conductivecable.isActive == false)
            {
                if (redlaunch.holdingbattery == false && purplelaunch.holdingbattery == false && pressurelaunch.holdingbattery == false && conductivelaunch.holdingbattery == false)
                {
                    playeranimations.SetBool("switch", true);
                    handtoSwitch = "flare";
                    playeranimations.SetTrigger("Switch");

                }

            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && handmanager.hasConductiveHand)
        {
            if (redcable.isActive == false && purplecable.isActive == false && pressurecable.isActive == false && conductivecable.isActive == false)
            {
                if (redlaunch.holdingbattery == false && purplelaunch.holdingbattery == false && pressurelaunch.holdingbattery == false && conductivelaunch.holdingbattery == false)
                {
                    playeranimations.SetBool("switch", true);
                    playeranimations.SetTrigger("Switch");

                    handtoSwitch = "conductive";
                }

            }
        }

        isGrounded = Physics.SphereCast(groundCheck.position, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckRadius + 0.3f, groundLayer);

        float mouseX = Input.GetAxis("Mouse X") * lookSpeedX;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeedY;

        targetX -= mouseY;
        targetX = Mathf.Clamp(targetX, -90f, 90f);

        targetY += mouseX;

        rotationX = Mathf.Lerp(rotationX, targetX, 30f * Time.deltaTime);
        currentY = Mathf.Lerp(currentY, targetY, 30f * Time.deltaTime);

        playerCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, currentY, 0f);

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

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
            if (Input.GetKey(KeyCode.LeftShift))
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
                targetMoveSpeed = moveSpeed * crouchMultiplier;
                playeranimations.speed = 1f;

                IsCrouched = true;
                croucher.SetBool("Crouched", true);
                playeranimations.SetBool("crouch", true);
                playerstandingCollider.enabled = false;

                footstepSource.PlayOneShot(crouchsfx, 1.0f); 
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) && IsCrouched)
            {
                if (CanStandUp())
                {
                    croucher.SetBool("Crouched", false);
                    playeranimations.SetBool("crouch", false);
                    IsCrouched = false;
                    playerstandingCollider.enabled = true;

                    footstepSource.PlayOneShot(uncrouchsfx, 1.0f); 
                }
            }
        }

        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, sprintAcceleration * Time.deltaTime);

        if (!IsCrouched && Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            footstepSource.PlayOneShot(jumpsfx, 3.0f);

        }
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

    public void SwitchHand()
    {
        playeranimations.speed = 1f;
        targetMoveSpeed = moveSpeed;

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
}
