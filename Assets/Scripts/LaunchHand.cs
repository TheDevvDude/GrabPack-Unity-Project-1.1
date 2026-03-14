
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class LaunchHand : MonoBehaviour
{
    private Coroutine returnRoutine;
    private Coroutine moveRoutine;

    public LayerMask raycastLayers;

    public Rigidbody ownerRigidbody;
    public RigidboyPlayerController ownerController;

    private bool mouseHeld = false;
    public Transform limonpos;
    private bool awaitingSecondInput = false;

    private float pressureHoldTimer = 0f;
    public float pressureStartDelay = 0.5f;
    private bool pressureBuilding = false;

    private float mouseDownTime;
    public CableManager cableManager;

    public string Hand = "Left";
    public Transform handTransform;
    public Transform handOrigin;
    public float maxRange = 10f;
    public float speed = 20f;

    private Transform originalParent;
    private bool isReturning = false;
    public bool returned = true;

    public Animator playeranimations;
    public string GrabableLayer;

    public bool CanReturn = true;
    public bool canDrag = false;
    public float pullSpeed = 5f;

    public CablePhysics CableSim;

    public GameObject hitOBJ;
    public Vector3 selftransform;

    public bool holdingbattery = false;
    public GameObject battery;
    public Transform batterypos;

    public Animator handgrabbing;
    public Barricade br;

    public AudioSource globalAudio;
    public AudioClip firesfx;
    public AudioClip grabsfx;
    public GameObject dragsounds;
    public GameObject pressurebuild;
    public AudioClip pressureRelease;

    private bool lockReturn = false;
    public RotateArms aimOverride;

    public bool isRocketHand = false;
    public bool isPressureHand = false;
    public float pressure = 0;

    public GameObject SMOKE;
    public GameObject gauge;
    public Image guageUI;
    public ParticleSystem impact;
    public GameObject Crosshair;

    private bool virtualPressed;
    private bool virtualHeld;
    private bool virtualReleased;

    private bool inputDown;
    private bool inputHeld;
    private bool inputUp;


    private Rigidbody currentDraggedRB;
    private bool applyDragForce;

    public MobileIcons mobileIcons;

    void Start()
    {
        originalParent = handTransform.parent;
        selftransform = transform.localScale;

        originalParent = handTransform.parent;
        selftransform = transform.localScale;


    }

    public bool IsHeld()
    {
        return inputHeld;
    }

    public void UIButtonDown()
    {
        virtualPressed = true;
        virtualHeld = true;
    }

    public void UIButtonUp()
    {
        virtualReleased = true;
        virtualHeld = false;
    }

    void Update()
    {
        if (isReturning) return;

        if (ownerController.canSwitch == false) return;

        inputDown = false;
        inputHeld = false;
        inputUp = false;

        bool isLeft = Hand == "Left";
        bool isRight = Hand == "Right";

        if (mobileIcons.isMobile)
        {
            inputDown = virtualPressed;
            inputHeld = virtualHeld;
            inputUp = virtualReleased;
        }
        else
        {
            inputDown = (isLeft && Input.GetMouseButtonDown(0)) ||
                        (isRight && Input.GetMouseButtonDown(1));

            inputHeld = (isLeft && Input.GetMouseButton(0)) ||
                        (isRight && Input.GetMouseButton(1));

            inputUp = (isLeft && Input.GetMouseButtonUp(0)) ||
                      (isRight && Input.GetMouseButtonUp(1));
        }

        virtualPressed = false;
        virtualReleased = false;

        if (inputDown)
        {
            if (returned)
            {
                FireHand();
                awaitingSecondInput = false;
                return;
            }

            if (!returned && !awaitingSecondInput)
            {
                awaitingSecondInput = true;
                mouseDownTime = Time.time;
                return;
            }
        }

        if (inputUp)
        {
            if (!returned && awaitingSecondInput)
            {
                CanReturn = true;
                if (!isPressureHand || isPressureHand && pressure == 0)
                {
                    if (returnRoutine == null)
                    {
                        returnRoutine = StartCoroutine(ReturnHand());
                    }
                }
                awaitingSecondInput = false;
            }
        }
        if (isPressureHand && inputUp && canDrag)
        {
            Debug.Log(canDrag);
            ReleasePressure();
        }
        HandleDraggingState();


    }

    void ReleasePressure()
    {
        GameObject grabbedObj = handTransform.parent?.gameObject;
        if (grabbedObj == null) return;

        Rigidbody rb = grabbedObj.GetComponent<Rigidbody>();
        if (rb == null) return;

        if (pressure >= 1f)
        {
            Vector3 launchDir = (grabbedObj.transform.position - handOrigin.position).normalized;
            float launchForce = pressure * 50f;

            rb.isKinematic = false;
            rb.AddForce(launchDir * launchForce, ForceMode.Impulse);

            impact.Play();
            globalAudio.PlayOneShot(pressureRelease, 2.0f);
        }
        if (pressure > 9)
        {
            Breakable breakable = grabbedObj.GetComponent<Breakable>();
            if (breakable != null)
            {
                breakable.breakObject();
            }
            
        }

        pressure = 0f;
        pressureHoldTimer = 0f;
        pressureBuilding = false;

        SMOKE.SetActive(false);
        gauge.SetActive(false);
        pressurebuild.SetActive(false);
        Crosshair.SetActive(true);

        CanReturn = true;
        canDrag = false;
        if (returnRoutine == null)
        {
            returnRoutine = StartCoroutine(ReturnHand());
        }
    }

    void HandleDraggingState()
    {
        if (CanReturn || !canDrag || isReturning)
        {
            applyDragForce = false;
            dragsounds.SetActive(false);
            return;
        }

        if (!inputHeld)
        {
            applyDragForce = false;
            dragsounds.SetActive(false);
            return;
        }

        GameObject grabbed = handTransform.parent?.gameObject;
        if (grabbed == null) return;

        Rigidbody rb = grabbed.GetComponent<Rigidbody>();
        if (rb == null) return;

        currentDraggedRB = rb;
        applyDragForce = true;

        if (!isPressureHand)
        {
            dragsounds.SetActive(true);
        }
        else
        {
            pressureHoldTimer += Time.deltaTime;

            if (pressureHoldTimer >= pressureStartDelay)
            {
                SMOKE.SetActive(true);
                gauge.SetActive(true);
                pressurebuild.SetActive(true);
                Crosshair.SetActive(false);

                pressure += Time.deltaTime * 4f;
                pressure = Mathf.Min(pressure, 10f);
                guageUI.fillAmount = pressure / 10f;
            }
        }
    }

    void FixedUpdate()
    {
        if (!applyDragForce || currentDraggedRB == null)
            return;

        if (!isPressureHand)
        {
            Vector3 targetPos = handOrigin.position;
            Vector3 direction = targetPos - currentDraggedRB.position;

            Vector3 force =
                direction.normalized * (pullSpeed * 600f)
                - currentDraggedRB.velocity * 8f;

            currentDraggedRB.AddForce(force, ForceMode.Force);
        }
    }

    public void EnableDrag()
    {
        canDrag = true;
    }

    public void FireHand()
    {
        globalAudio.PlayOneShot(firesfx, 0.7f);

        CableSim.isActive = true;
        float remaining = cableManager.GetRemainingLength();
        maxRange = remaining;

        if (Hand == "Right")
        {
            aimOverride.rightActive = true;
        }
        if (Hand == "Left")
        {
            aimOverride.leftActive = true;
        }

        if (holdingbattery)
        {
            holdingbattery = false;
            battery.transform.parent = null;
            Rigidbody batteryRB = battery.GetComponent<Rigidbody>();
            BoxCollider col = battery.GetComponent<BoxCollider>();

            batteryRB.isKinematic = false;

            if (Hand == "Left")
            {
                batteryRB.AddForce(-transform.up * 800f, ForceMode.Impulse);
                aimOverride.leftActive = false;

            }
            if (Hand == "Right")
            {
                batteryRB.AddForce(transform.up * 800f, ForceMode.Impulse);
                aimOverride.rightActive = false;

            }

            CableSim.isActive = false;

            col.enabled = true;
            battery = null;
            handgrabbing.SetBool("grabbing", false);

            return;



        }
        CanReturn = true;
        //cableRenderer.enabled = true;


        if (isReturning) return;
        playeranimations.SetTrigger("shoot");
        handTransform.parent = null;
        returned = false;
        Vector3 targetPoint;


        Camera cam = Camera.main;
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);


        Ray ray = cam.ScreenPointToRay(screenCenter); if (Physics.Raycast(ray, out RaycastHit hit, maxRange, raycastLayers))
        {
            targetPoint = hit.point;

            if (Hand == "Right")
            {
                aimOverride.rightHitPoint = targetPoint;
            }
            if (Hand == "Left")
            {
                aimOverride.leftHitPoint = targetPoint;
            }

            if (Hand == "Right")
            {
                Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, -hit.normal);
                handTransform.rotation = Quaternion.LookRotation(projectedForward, -hit.normal);
            }
            if (Hand == "Left")
            {
                Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
                handTransform.rotation = Quaternion.LookRotation(projectedForward, hit.normal);
            }

            if (hit.collider.gameObject.tag == (GrabableLayer))
            {
                CanReturn = false;
                //handTransform.parent = hit.collider.gameObject.transform;



                hitOBJ = hit.collider.gameObject;
                if (LayerMask.LayerToName(hitOBJ.layer) == "Battery")
                {
                    holdingbattery = true;
                    handgrabbing.SetBool("grabbing", true);



                }
                if (hitOBJ.GetComponent<HandScanner>() == true)
                {
                    handgrabbing.SetBool("grabbing", false);
                }
                if (hitOBJ.GetComponent<Rigidbody>() != null)
                {
                    if (hitOBJ.GetComponent<Barricade>() != null)
                    {
                        br = hitOBJ.GetComponent<Barricade>();

                    }
                    if (!isPressureHand)
                    {
                        handgrabbing.SetBool("grabbing", true);

                    }

                    Invoke("EnableDrag", 0.5f);
                    pressure = 0f;
                    pressureHoldTimer = 0f;
                    pressureBuilding = false;

                }
                if (LayerMask.LayerToName(hitOBJ.layer) == "Grabanimation" || LayerMask.LayerToName(hitOBJ.layer) == "Minecart" || LayerMask.LayerToName(hitOBJ.layer) == "KeyCard")
                {
                    handgrabbing.SetBool("grabbing", true);
                }


                if (Hand == "Right")
                {
                    Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, -hit.normal);
                    handTransform.rotation = Quaternion.LookRotation(projectedForward, -hit.normal);
                }
                if (Hand == "Left")
                {
                    Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
                    handTransform.rotation = Quaternion.LookRotation(projectedForward, hit.normal);
                }

            }
        }
        else
        {
            targetPoint = ray.origin + ray.direction * maxRange;
        }
        Vector3 impactPoint = hit.point;

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(MoveHand(targetPoint, impactPoint));
    }
    private IEnumerator MoveHand(Vector3 target, Vector3 impactPoint)
    {
        Vector3 start = handTransform.position;
        float distance = Vector3.Distance(start, target);
        float duration = distance / speed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            handTransform.position = Vector3.Lerp(start, target, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        handTransform.position = target;
        if (hitOBJ != null)
        {
            handTransform.position = impactPoint;
            handTransform.SetParent(hitOBJ.transform, true);
            if (LayerMask.LayerToName(hitOBJ.layer) == "Battery")
            {

                if (hitOBJ.GetComponent<gear>() != null)
                {
                    if (hitOBJ.transform.childCount == 2)
                    {
                        battery = hitOBJ;
                    }
                }
                else
                {
                    if (hitOBJ.transform.childCount == 1)
                    {
                        battery = hitOBJ;
                    }
                }


                //handTransform.parent = hitOBJ.transform;
                //hitOBJ.transform.parent = handTransform;




            }


            lockReturn = true;
            StartCoroutine(UnlockReturn());
        }

        yield return new WaitForSeconds(0.1f);

        if (CanReturn)
        {
            if (returnRoutine == null)
            {
                returnRoutine = StartCoroutine(ReturnHand());
            }
        }

        moveRoutine = null;
    }

    private IEnumerator UnlockReturn()
    {
        yield return new WaitForSeconds(0.1f);
        lockReturn = false;
    }

    public void return1()
    {
        if (lockReturn) return;

        CanReturn = true;
        if (returnRoutine == null)
        {
            returnRoutine = StartCoroutine(ReturnHand());
        }
    }

    private IEnumerator ReturnHand()
    {
        handTransform.parent = null;

        if (battery != null)
        {
            limon Limon = battery.GetComponent<limon>();
            if (Limon == null)
            {
                battery.transform.position = batterypos.position;
                battery.transform.rotation = batterypos.rotation;
            }
            else
            {
                battery.transform.position = limonpos.position;
                battery.transform.rotation = limonpos.rotation;
            }


            Rigidbody rb = battery.GetComponent<Rigidbody>();

            if (rb != null)
            {
                battery.transform.parent = null;

                rb.isKinematic = true;

                BoxCollider col1 = battery.GetComponent<BoxCollider>();
                col1.enabled = false;
                battery.transform.parent = handTransform;

            }
        }

        handgrabbing.SetBool("grabbing", true);

        //dragsounds.SetActive(false);
        pressure = 0;
        globalAudio.PlayOneShot(grabsfx, 0.7f);

        br = null;
        Transform batterychild = transform.Find("Battery");
        if (batterychild == null)
        {
            batterychild = transform.Find("Gear");

        }
        if (batterychild == null)
        {
            holdingbattery = false;
            battery = null;
        }


        isReturning = true;
        if (!isPressureHand)
        {
            canDrag = false;

        }
        hitOBJ = null;
        Vector3 startPosition = handTransform.position;
        Quaternion startRotation = handTransform.rotation;

        float duration = Vector3.Distance(startPosition, handOrigin.position) / speed;
        float elapsedTime = 0f;

        while (CableSim.GetCablePoints().Count > 1)
        {
            List<Vector3> points = CableSim.GetCablePoints();

            if (points.Count <= 2)
                break;

            Vector3 targetPoint = points[points.Count - 2];

            while (Vector3.Distance(handTransform.position, targetPoint) > 0.05f)
            {
                handTransform.position = Vector3.MoveTowards(
                    handTransform.position,
                    targetPoint,
                    speed * Time.deltaTime
                );

                yield return null;
            }

            CableSim.RemoveLastWrapPoint();
        }

        while (Vector3.Distance(handTransform.position, handOrigin.position) > 0.1f)
        {
            handTransform.position = Vector3.MoveTowards(
                handTransform.position,
                handOrigin.position,
                speed * Time.deltaTime
            );

            yield return null;
        }

        if (batterychild == null)
        {
            handgrabbing.SetBool("grabbing", false);

        }

        handTransform.position = handOrigin.position;
        handTransform.rotation = handOrigin.rotation;
        handTransform.parent = originalParent;
        isReturning = false;
        returned = true;
        // playeranimations.SetTrigger("return");
        canDrag = false;
        // cableRenderer.enabled = false;
        CableSim.InitializeCable();
        CableSim.isActive = false;
        gameObject.transform.localScale = selftransform;


        if (Hand == "Right")
        {
            aimOverride.rightActive = false;
        }
        if (Hand == "Left")
        {
            aimOverride.leftActive = false;
        }

        returnRoutine = null;
    }
    void LateUpdate()
    {
        if (CanReturn || !canDrag || isReturning)
        {
            dragsounds.SetActive(false);
        }
    }

    public void ForceImmediateReturn()
    {
        pressure = 0;
        StopAllCoroutines();
        CableSim.isActive = false;
        isReturning = false;
        returned = true;
        canDrag = false;
        hitOBJ = null;
        handgrabbing.SetBool("grabbing", false);
        CanReturn = true;
        handTransform.parent = originalParent;
        handTransform.position = handOrigin.position;
        handTransform.rotation = handOrigin.rotation;

        //cableRenderer.enabled = false;
        CableSim.InitializeCable();

        if (Hand == "Right")
        {
            aimOverride.rightActive = false;
        }
        if (Hand == "Left")
        {
            aimOverride.leftActive = false;
        }
    }



}


