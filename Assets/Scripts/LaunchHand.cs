using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class LaunchHand : MonoBehaviour
{
    private const string DebugSpawnInEditorPrefKey = "MobileUIBootstrap_DebugSpawnInEditor";
    private float nextDebugLogTime;
    private bool lastLoggedCanReturn;
    private bool lastLoggedCanDrag;
    private bool lastLoggedReturned = true;
    private bool lastLoggedIsReturning;
    private bool lastLoggedMobileHeld;
    private bool lastLoggedCurrentHeld;
    private string lastLoggedHitName;
    private bool lastLoggedHitObjWasNull = true;
    private bool lastLoggedAttachedToParentWasNull = true;
    private string lastLoggedAttachedParentName;
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
    public AudioClip dragsfx;
    public GameObject dragsounds;
    public GameObject pressurebuild;
    public AudioClip pressureRelease;

    private bool lockReturn = false;

    public RotateArms aimOverride;

    public bool isPressureHand = false;
    public float pressure = 0;

    public GameObject SMOKE;

    public GameObject gauge;
    public Image guageUI;
    public ParticleSystem impact;

    public GameObject Crosshair;

    public bool mobileControlsEnabled = true;
    public bool useScreenCenterAimOnMobile = true;
    public bool debugMobileInEditor = false;

    private bool mobileButtonHeld;
    private bool mobileButtonDown;
    private bool mobileButtonUp;
    private bool currentButtonHeld;
    private bool currentButtonDown;
    private bool currentButtonUp;
    private Vector2 mobileAimScreenPosition;

    public bool MobileControlsActive => mobileControlsEnabled && IsMobileControlsRuntimeActive();
    public bool IsActionHeld => currentButtonHeld || mobileButtonHeld;

    void Start()
    {
        originalParent = handTransform.parent;
        selftransform = gameObject.transform.localScale;
        lastLoggedCanReturn = CanReturn;
        lastLoggedCanDrag = canDrag;
        lastLoggedReturned = returned;
        lastLoggedIsReturning = isReturning;
        lastLoggedMobileHeld = mobileButtonHeld;
        lastLoggedCurrentHeld = currentButtonHeld;
        LogDebug($"Start MobileControlsActive={MobileControlsActive} GrabableLayer={GrabableLayer}");
    }

    void Update()
    {
        if (isReturning)
        {
            UpdateInputState();
            LogStateChanges();
            return;
        }

        UpdateInputState();
        LogStateChanges();

        if (currentButtonDown)
        {
            LogDebug($"ButtonDown returned={returned} awaitingSecondInput={awaitingSecondInput} canDrag={canDrag} canReturn={CanReturn} held={IsActionHeld}");
            mouseHeld = true;

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
                LogDebug("Second input armed for attached hand state.");
                return;
            }
        }

        if (currentButtonUp)
        {
            LogDebug($"ButtonUp returned={returned} awaitingSecondInput={awaitingSecondInput} canDrag={canDrag} canReturn={CanReturn}");
            mouseHeld = false;

            if (!returned && !awaitingSecondInput)
            {
                awaitingSecondInput = true;
                LogDebug("ButtonUp armed second input because hand was still out.");
                return;
            }

            if (!returned && awaitingSecondInput)
            {
                if (!isPressureHand && canDrag == true || isPressureHand && canDrag == false || !isPressureHand && canDrag == false)
                {
                    LogDebug("Starting ReturnHand from button release path.");
                    CanReturn = true;
                    StartCoroutine(ReturnHand());
                }

                awaitingSecondInput = false;
            }
        }
    }

    public void EnableDrag()
    {
        canDrag = true;
        LogDebug($"EnableDrag called. hitOBJ={(hitOBJ != null ? hitOBJ.name : "null")}");
    }

    public void FireHand()
    {
        LogDebug($"FireHand start MobileControlsActive={MobileControlsActive} aim={GetAimScreenPosition()} holdingBattery={holdingbattery}");
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
            LogDebug("FireHand is throwing carried battery.");
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

        if (isReturning)
        {
            LogDebug("FireHand aborted because hand is returning.");
            return;
        }
        playeranimations.SetTrigger("shoot");
        handTransform.parent = null;
        returned = false;
        Vector3 targetPoint;
        Camera cam = Camera.main;
        if (cam == null)
        {
            LogDebug("FireHand aborted because Camera.main is null.");
            return;
        }

        Ray ray = cam.ScreenPointToRay(GetAimScreenPosition());
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
        {
            LogDebug($"Raycast hit {hit.collider.gameObject.name} tag={hit.collider.gameObject.tag} layer={LayerMask.LayerToName(hit.collider.gameObject.layer)} distance={hit.distance:0.00}");
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
                LogDebug($"Hit matches grab layer/tag. Preparing attach to {hit.collider.gameObject.name}");
                CanReturn = false;

                hitOBJ = hit.collider.gameObject;
                if (LayerMask.LayerToName(hitOBJ.layer) == "Battery")
                {
                    holdingbattery = true;
                    handgrabbing.SetBool("grabbing", true);
                    LogDebug("Hit object is battery. holdingbattery=true");
                }
                if (hitOBJ.GetComponent<HandScanner>() == true)
                {
                    handgrabbing.SetBool("grabbing", false);
                    LogDebug("Hit object has HandScanner. grabbing animation disabled.");
                }
                if (hitOBJ.GetComponent<Rigidbody>() != null)
                {
                    LogDebug($"Hit object has Rigidbody. Scheduling EnableDrag for {hitOBJ.name}");
                    if (hitOBJ.GetComponent<Barricade>() != null)
                    {
                        br = hitOBJ.GetComponent<Barricade>();
                        LogDebug("Hit object is Barricade.");
                    }
                    if (!isPressureHand)
                    {
                        handgrabbing.SetBool("grabbing", true);
                    }

                    Invoke("EnableDrag", 0.5f);
                }
                else
                {
                    LogDebug($"Hit object {hitOBJ.name} has no Rigidbody. Drag will not activate.");
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
            else
            {
                LogDebug($"Raycast hit {hit.collider.gameObject.name} but tag '{hit.collider.gameObject.tag}' does not match GrabableLayer '{GrabableLayer}'");
            }
        }
        else
        {
            targetPoint = ray.origin + ray.direction * maxRange;
            LogDebug($"Raycast missed. Sending hand to fallback target {targetPoint}");
        }
        Vector3 impactPoint = hit.point;

        StartCoroutine(MoveHand(targetPoint, impactPoint));
    }

    private IEnumerator MoveHand(Vector3 target, Vector3 impactPoint)
    {
        LogDebug($"MoveHand start target={target} impact={impactPoint} hitOBJ={(hitOBJ != null ? hitOBJ.name : "null")}");
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
            LogDebug($"MoveHand attaching to {hitOBJ.name}");
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
            }

            lockReturn = true;
            StartCoroutine(UnlockReturn());
        }
        else
        {
            LogDebug("MoveHand finished with no hitOBJ attached.");
        }

        yield return new WaitForSeconds(0.1f);

        if (CanReturn)
        {
            LogDebug("MoveHand auto-returning because CanReturn is true.");
            StartCoroutine(ReturnHand());
        }
    }

    private IEnumerator UnlockReturn()
    {
        yield return new WaitForSeconds(0.1f);
        lockReturn = false;
        LogDebug("UnlockReturn completed.");
    }

    public void return1()
    {
        if (lockReturn)
        {
            LogDebug("return1 ignored because lockReturn is true.");
            return;
        }

        LogDebug("return1 called.");
        CanReturn = true;
        StartCoroutine(ReturnHand());
    }

    private IEnumerator ReturnHand()
    {
        LogDebug($"ReturnHand start battery={(battery != null ? battery.name : "null")}");
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
        canDrag = false;
        hitOBJ = null;

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
        canDrag = false;
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

        LogDebug("ReturnHand complete.");
    }

    void LateUpdate()
    {
        if (CanReturn || !canDrag || isReturning)
        {
            if (ShouldEmitPeriodicDragLog())
            {
                LogDebug($"LateUpdate blocked CanReturn={CanReturn} canDrag={canDrag} isReturning={isReturning} held={IsActionHeld} parent={(handTransform.parent != null ? handTransform.parent.name : "null")}");
            }
            dragsounds.SetActive(false);
            ResetTransientInput();
            return;
        }

        bool leftReleased = Hand == "Left" && currentButtonUp;
        bool rightReleased = Hand == "Right" && currentButtonUp;

        if ((leftReleased || rightReleased) && isPressureHand)
        {
            LogDebug("Pressure hand release detected in LateUpdate.");
            canDrag = false;
            CanReturn = true;
            GameObject grabbedObj = handTransform.parent?.gameObject;

            pressureHoldTimer = 0f;
            pressureBuilding = false;

            if (grabbedObj != null)
            {
                Rigidbody rb = grabbedObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    if (!grabbedObj.GetComponent<Breakable>())
                    {
                        if (pressure >= 1f)
                        {
                            Vector3 launchDir = (grabbedObj.transform.position - handOrigin.position).normalized;

                            float launchForce = pressure * 100f;

                            handgrabbing.SetBool("grabbing", true);
                            rb.isKinematic = false;
                            rb.AddForce(launchDir * launchForce, ForceMode.Impulse);

                            impact.Play();
                        }
                    }
                    else
                    {
                        if (pressure > 9f)
                        {
                            Breakable breakable = grabbedObj.GetComponent<Breakable>();

                            breakable.breakObject();
                            impact.Play();
                        }
                    }
                }
            }

            SMOKE.SetActive(false);
            gauge.SetActive(false);
            if (pressure >= 1f)
            {
                globalAudio.PlayOneShot(pressureRelease, 2.0f);
            }
            pressure = 0f;
            pressurebuild.SetActive(false);

            Crosshair.SetActive(true);

            return1();
            ResetTransientInput();
            return; 
        }

        bool isHandActive = IsActionHeld;

        if (!CanReturn && canDrag && isHandActive)
        {
            GameObject grabbed = handTransform.parent?.gameObject;
            if (grabbed == null)
            {
                LogDebug("LateUpdate drag aborted because grabbed parent is null.");
                ResetTransientInput();
                return;
            }

            Rigidbody rb = grabbed.GetComponent<Rigidbody>();
            if (rb == null)
            {
                LogDebug($"LateUpdate drag aborted because {grabbed.name} has no Rigidbody.");
                ResetTransientInput();
                return;
            }

            if (ShouldEmitPeriodicDragLog())
            {
                LogDebug($"LateUpdate drag armed for {grabbed.name} velocity={rb.velocity} handPos={handTransform.position} origin={handOrigin.position} kinematic={rb.isKinematic} constraints={rb.constraints}");
            }

            if (!isPressureHand)
            {
                dragsounds.SetActive(true);
            }
            else
            {
                pressureHoldTimer += Time.deltaTime;

                if (pressureHoldTimer >= pressureStartDelay)
                {
                    pressureBuilding = true;

                    SMOKE.SetActive(true);
                    gauge.SetActive(true);
                    pressurebuild.SetActive(true);
                    Crosshair.SetActive(false);

                    if (pressure < 10f)
                    {
                        pressure += Time.deltaTime * 4f;
                        pressure = Mathf.Min(pressure, 10f);
                        guageUI.fillAmount = pressure / 10f;
                    }
                }

                Breakable breakable = grabbed.GetComponent<Breakable>();
                if (breakable != null && breakable.SwitchMaterials)
                {
                    if (pressure < 5f)
                        breakable.renderer.material = breakable.pristine;
                    else if (pressure <= 9f)
                        breakable.renderer.material = breakable.damaged;
                    else
                        breakable.renderer.material = breakable.broken;
                }
            }
        }
        else
        {
            if (ShouldEmitPeriodicDragLog())
            {
                LogDebug($"LateUpdate not dragging CanReturn={CanReturn} canDrag={canDrag} held={isHandActive} parent={(handTransform.parent != null ? handTransform.parent.name : "null")}");
            }
            dragsounds.SetActive(false);
        }

        ResetTransientInput();
    }

    void FixedUpdate()
    {
        if (CanReturn || !canDrag || isReturning || isPressureHand || !IsActionHeld)
        {
            return;
        }

        GameObject grabbed = handTransform.parent?.gameObject;
        if (grabbed == null)
        {
            return;
        }

        Rigidbody rb = grabbed.GetComponent<Rigidbody>();
        if (rb == null)
        {
            return;
        }

        rb.WakeUp();

        Vector3 targetPos = handOrigin.position;
        Vector3 direction = targetPos - rb.position;
        Vector3 velocityCompensation = -rb.velocity * 12f;
        Vector3 acceleration = direction * (pullSpeed * 45f) + velocityCompensation;

        rb.AddForce(acceleration, ForceMode.Acceleration);

        if (Time.frameCount % 15 == 0)
        {
            LogDebug($"FixedUpdate force on {grabbed.name} dir={direction} vel={rb.velocity} kinematic={rb.isKinematic} constraints={rb.constraints}");
        }
    }

    public void ForceImmediateReturn()
    {
        LogDebug("ForceImmediateReturn called.");
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

    public void MobilePress()
    {
        if (!mobileControlsEnabled)
        {
            LogDebug("MobilePress ignored because mobileControlsEnabled is false.");
            return;
        }

        mobileButtonHeld = true;
        mobileButtonDown = true;
        LogDebug($"MobilePress received. held={mobileButtonHeld} down={mobileButtonDown}");
    }

    public void MobileRelease()
    {
        if (!mobileControlsEnabled)
        {
            LogDebug("MobileRelease ignored because mobileControlsEnabled is false.");
            return;
        }

        mobileButtonHeld = false;
        mobileButtonUp = true;
        LogDebug($"MobileRelease received. held={mobileButtonHeld} up={mobileButtonUp}");
    }

    public void SetAimScreenPosition(Vector2 screenPosition)
    {
        mobileAimScreenPosition = screenPosition;
    }

    private void UpdateInputState()
    {
        bool isLeft = Hand == "Left";
        bool isRight = Hand == "Right";

        bool allowMouseButtons = ShouldUseDesktopMouseButtons();

        bool mouseDown = allowMouseButtons && ((isLeft && Input.GetMouseButtonDown(0)) ||
                         (isRight && Input.GetMouseButtonDown(1)));

        bool mouseHeldNow = allowMouseButtons && ((isLeft && Input.GetMouseButton(0)) ||
                            (isRight && Input.GetMouseButton(1)));

        bool mouseUp = allowMouseButtons && ((isLeft && Input.GetMouseButtonUp(0)) ||
                       (isRight && Input.GetMouseButtonUp(1)));

        currentButtonDown = mouseDown || mobileButtonDown;
        currentButtonHeld = mouseHeldNow || mobileButtonHeld;
        currentButtonUp = mouseUp || mobileButtonUp;
    }

    private Vector2 GetAimScreenPosition()
    {
        if (MobileControlsActive)
        {
            if (useScreenCenterAimOnMobile || mobileAimScreenPosition == Vector2.zero)
            {
                return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }

            return mobileAimScreenPosition;
        }

        return Input.mousePosition;
    }

    private void ResetTransientInput()
    {
        mobileButtonDown = false;
        mobileButtonUp = false;
        currentButtonDown = false;
        currentButtonUp = false;
    }

    private bool IsMobileControlsRuntimeActive()
    {
        if (Application.isMobilePlatform)
        {
            return true;
        }

        return IsEditorDebugMobileActive();
    }

    private bool ShouldUseDesktopMouseButtons()
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

    private bool ShouldEmitPeriodicDragLog()
    {
        if (Time.unscaledTime < nextDebugLogTime)
        {
            return false;
        }

        nextDebugLogTime = Time.unscaledTime + 0.25f;
        return true;
    }

    private void LogStateChanges()
    {
        if (lastLoggedCanReturn != CanReturn)
        {
            lastLoggedCanReturn = CanReturn;
            LogDebug($"CanReturn -> {CanReturn}");
        }

        if (lastLoggedCanDrag != canDrag)
        {
            lastLoggedCanDrag = canDrag;
            LogDebug($"canDrag -> {canDrag}");
        }

        if (lastLoggedReturned != returned)
        {
            lastLoggedReturned = returned;
            LogDebug($"returned -> {returned}");
        }

        if (lastLoggedIsReturning != isReturning)
        {
            lastLoggedIsReturning = isReturning;
            LogDebug($"isReturning -> {isReturning}");
        }

        if (lastLoggedMobileHeld != mobileButtonHeld)
        {
            lastLoggedMobileHeld = mobileButtonHeld;
            LogDebug($"mobileButtonHeld -> {mobileButtonHeld}");
        }

        if (lastLoggedCurrentHeld != currentButtonHeld)
        {
            lastLoggedCurrentHeld = currentButtonHeld;
            LogDebug($"currentButtonHeld -> {currentButtonHeld}");
        }

        bool hitObjIsNull = hitOBJ == null;
        if (lastLoggedHitObjWasNull != hitObjIsNull || (!hitObjIsNull && lastLoggedHitName != hitOBJ.name))
        {
            lastLoggedHitObjWasNull = hitObjIsNull;
            lastLoggedHitName = hitObjIsNull ? null : hitOBJ.name;
            LogDebug($"hitOBJ -> {(hitObjIsNull ? "null" : hitOBJ.name)}");
        }

        Transform attachedParent = handTransform != null ? handTransform.parent : null;
        bool attachedParentIsNull = attachedParent == null;
        string attachedParentName = attachedParentIsNull ? null : attachedParent.name;
        if (lastLoggedAttachedToParentWasNull != attachedParentIsNull || lastLoggedAttachedParentName != attachedParentName)
        {
            lastLoggedAttachedToParentWasNull = attachedParentIsNull;
            lastLoggedAttachedParentName = attachedParentName;
            LogDebug($"handTransform.parent -> {(attachedParentIsNull ? "null" : attachedParentName)}");
        }
    }

    private void LogDebug(string message)
    {
        Debug.Log($"[LaunchHand:{Hand}] {message}");
    }
}


