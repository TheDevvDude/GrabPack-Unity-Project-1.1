using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MobileUIBootstrap : MonoBehaviour
{
    private static MobileUIBootstrap instance;

    private const string DebugSpawnInEditorPrefKey = "MobileUIBootstrap_DebugSpawnInEditor";
    private const int MaxLogLines = 300;

    public bool spawnOnMobile = true;
    public bool debugSpawnInEditor = true;
    public bool forceLandscapeOnMobile = true;
    public KeyCode debugToggleKey = KeyCode.F8;
    public int canvasSortOrder = 5000;
    public float controlAlpha = 0.55f;
    public float lookSensitivityMultiplier = 0.02f;

    private GameObject currentUi;
    private GameObject gameplayHudRoot;
    private GameObject mobileSettingsRoot;
    private Font runtimeFont;
    private RigidboyPlayerController debugPlayer;
    private Text debugStatusLabel;
    private SettingsManager trackedSettings;
    private Text mobileLogText;
    private Text mobileSensitivityValue;
    private Text mobileQualityValue;
    private Text mobileVsyncValue;
    private Text mobileFullscreenValue;
    private Slider mobileSensitivitySlider;
    private readonly Queue<string> recentLogs = new Queue<string>();
    private int lastKnownHandUiStateHash;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (instance != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject("MobileUIBootstrap");
        instance = bootstrap.AddComponent<MobileUIBootstrap>();
        DontDestroyOnLoad(bootstrap);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        runtimeFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        debugSpawnInEditor = PlayerPrefs.GetInt(DebugSpawnInEditorPrefKey, debugSpawnInEditor ? 1 : 0) == 1;
        Application.logMessageReceived += HandleLogMessage;
        ApplyMobileScreenSettings();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        RebuildUi();
    }

    private void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(debugToggleKey))
        {
            ToggleEditorDebugMode();
        }

        if (HasHandUiStateChanged())
        {
            RebuildUi();
        }

        UpdateRuntimeUiVisibility();
        UpdateDebugStatus();
        UpdateMobileSettingsValues();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            Application.logMessageReceived -= HandleLogMessage;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMobileScreenSettings();
        RebuildUi();
    }

    private void RebuildUi()
    {
        if (currentUi != null)
        {
            Destroy(currentUi);
            currentUi = null;
        }

        gameplayHudRoot = null;
        mobileSettingsRoot = null;
        debugStatusLabel = null;
        debugPlayer = null;
        trackedSettings = null;
        mobileLogText = null;
        mobileSensitivityValue = null;
        mobileQualityValue = null;
        mobileVsyncValue = null;
        mobileFullscreenValue = null;
        mobileSensitivitySlider = null;

        RigidboyPlayerController player = FindObjectOfType<RigidboyPlayerController>();
        Interact interact = FindObjectOfType<Interact>();
        SettingsManager settings = FindObjectOfType<SettingsManager>();
        flashlight flashlightController = FindObjectOfType<flashlight>();
        List<LaunchHand> allHands = FindSceneObjectsOfType<LaunchHand>();
        List<KeyCard> keyCards = FindSceneObjectsOfType<KeyCard>();

        trackedSettings = settings;

        bool editorDebugActive = debugSpawnInEditor && Application.isEditor;
        bool mobileUiActive = ShouldSpawnUi();

        if (player != null)
        {
            player.debugMobileInEditor = editorDebugActive;
            debugPlayer = player;
        }

        if (interact != null)
        {
            interact.debugMobileInEditor = editorDebugActive;
        }

        if (settings != null)
        {
            settings.debugMobileInEditor = editorDebugActive;
            settings.useMobileMenuMode = mobileUiActive;
            if (mobileUiActive && settings.open)
            {
                settings.SetMenuState(true, false);
            }
        }

        foreach (LaunchHand hand in allHands)
        {
            if (hand != null)
            {
                hand.debugMobileInEditor = editorDebugActive;
            }
        }

        foreach (KeyCard keyCard in keyCards)
        {
            if (keyCard != null)
            {
                keyCard.debugMobileInEditor = editorDebugActive;
            }
        }

        UpdateCursorMode(editorDebugActive, settings);
        lastKnownHandUiStateHash = ComputeHandUiStateHash(player);

        if (!mobileUiActive || player == null)
        {
            return;
        }

        EnsureEventSystem();

        currentUi = new GameObject("MobileUIRuntimeCanvas");
        currentUi.transform.SetParent(transform, false);

        Canvas canvas = currentUi.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = canvasSortOrder;

        CanvasScaler scaler = currentUi.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        currentUi.AddComponent<GraphicRaycaster>();

        gameplayHudRoot = CreateFullscreenUiLayer("GameplayHudRoot", currentUi.transform);
        mobileSettingsRoot = CreateFullscreenUiLayer("MobileSettingsRoot", currentUi.transform);

        CreateLookPanel(gameplayHudRoot.transform, player, allHands);
        CreateMoveJoystick(gameplayHudRoot.transform, player);
        CreateActionButtons(gameplayHudRoot.transform, player, interact, flashlightController, allHands);
        CreateMobileSettingsPanel(mobileSettingsRoot.transform, settings);

        if (editorDebugActive)
        {
            CreateDebugLabel(currentUi.transform);
            CreateDebugStatusLabel(currentUi.transform);
            CreateDebugToggleButton(currentUi.transform);
        }

        RefreshLogText();
        UpdateRuntimeUiVisibility();
        UpdateMobileSettingsValues();
    }

    public void SetEditorDebugMode(bool enabled)
    {
        debugSpawnInEditor = enabled;
        PlayerPrefs.SetInt(DebugSpawnInEditorPrefKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
        RebuildUi();
    }

    public void ToggleEditorDebugMode()
    {
        SetEditorDebugMode(!debugSpawnInEditor);
    }

    public void ToggleMobileSettingsMenu()
    {
        if (trackedSettings == null)
        {
            return;
        }

        trackedSettings.SetMenuState(!trackedSettings.open, false);
        UpdateRuntimeUiVisibility();
        UpdateMobileSettingsValues();
    }

    public void SelectMobileRedHand()
    {
        Debug.Log("[MobileUIBootstrap] SelectMobileRedHand pressed");
        RigidboyPlayerController player = FindObjectOfType<RigidboyPlayerController>();
        if (player != null)
        {
            player.EmulateDesktopHandKey1();
        }
        else
        {
            Debug.Log("[MobileUIBootstrap] SelectMobileRedHand could not find player");
        }
    }

    public void SelectMobilePurpleHand()
    {
        Debug.Log("[MobileUIBootstrap] SelectMobilePurpleHand pressed");
        RigidboyPlayerController player = FindObjectOfType<RigidboyPlayerController>();
        if (player != null)
        {
            player.EmulateDesktopHandKey2();
        }
        else
        {
            Debug.Log("[MobileUIBootstrap] SelectMobilePurpleHand could not find player");
        }
    }

    public void SelectMobileFlareHand()
    {
        Debug.Log("[MobileUIBootstrap] SelectMobileFlareHand pressed");
        RigidboyPlayerController player = FindObjectOfType<RigidboyPlayerController>();
        if (player != null)
        {
            player.EmulateDesktopHandKey3();
        }
        else
        {
            Debug.Log("[MobileUIBootstrap] SelectMobileFlareHand could not find player");
        }
    }

    public void SelectMobileConductiveHand()
    {
        Debug.Log("[MobileUIBootstrap] SelectMobileConductiveHand pressed");
        RigidboyPlayerController player = FindObjectOfType<RigidboyPlayerController>();
        if (player != null)
        {
            player.EmulateDesktopHandKey4();
        }
        else
        {
            Debug.Log("[MobileUIBootstrap] SelectMobileConductiveHand could not find player");
        }
    }

    public void SetMobileQualityOffset(int offset)
    {
        if (trackedSettings == null)
        {
            return;
        }

        int qualityCount = QualitySettings.names.Length;
        int newIndex = Mathf.Clamp(QualitySettings.GetQualityLevel() + offset, 0, qualityCount - 1);
        trackedSettings.SetQuality(newIndex);
        UpdateMobileSettingsValues();
    }

    public void ToggleMobileVsync()
    {
        if (trackedSettings == null)
        {
            return;
        }

        trackedSettings.SetVSync(QualitySettings.vSyncCount == 0);
        UpdateMobileSettingsValues();
    }

    public void ToggleMobileFullscreen()
    {
        if (trackedSettings == null)
        {
            return;
        }

        trackedSettings.SetFullscreen(!Screen.fullScreen);
        UpdateMobileSettingsValues();
    }

    public void ClearMobileLogs()
    {
        recentLogs.Clear();
        RefreshLogText();
    }

    public void CopyMobileLogs()
    {
        StringBuilder builder = new StringBuilder();
        foreach (string line in recentLogs)
        {
            builder.AppendLine(line);
        }

        GUIUtility.systemCopyBuffer = builder.ToString();
        Debug.Log($"[MobileUIBootstrap] Copied {recentLogs.Count} log lines to clipboard.");
    }

    private void UpdateCursorMode(bool editorDebugActive, SettingsManager settings)
    {
        if (editorDebugActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (!Application.isMobilePlatform && (settings == null || !settings.open))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private bool ShouldSpawnUi()
    {
        if (spawnOnMobile && Application.isMobilePlatform)
        {
            return true;
        }

        return debugSpawnInEditor && Application.isEditor;
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private void CreateLookPanel(Transform parent, RigidboyPlayerController player, List<LaunchHand> allHands)
    {
        GameObject panel = CreateUiObject("LookPanel", parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.001f);
        image.raycastTarget = true;

        MobileLookInput lookInput = panel.AddComponent<MobileLookInput>();
        lookInput.player = player;
        lookInput.lookMultiplier = lookSensitivityMultiplier;
        lookInput.hands = allHands.ToArray();
    }

    private void CreateMoveJoystick(Transform parent, RigidboyPlayerController player)
    {
        GameObject joystickRoot = CreateUiObject("MoveJoystick", parent);
        RectTransform rootRect = joystickRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 0f);
        rootRect.anchorMax = new Vector2(0f, 0f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(220f, 220f);
        rootRect.anchoredPosition = new Vector2(170f, 170f);

        Image backgroundImage = joystickRoot.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, controlAlpha * 0.4f);
        backgroundImage.raycastTarget = true;

        MobileMoveInput moveInput = joystickRoot.AddComponent<MobileMoveInput>();
        moveInput.player = player;

        GameObject handleObject = CreateUiObject("Handle", joystickRoot.transform);
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(90f, 90f);
        handleRect.anchoredPosition = Vector2.zero;

        Image handleImage = handleObject.AddComponent<Image>();
        handleImage.color = new Color(1f, 1f, 1f, controlAlpha);
        handleImage.raycastTarget = false;

        MobileJoystick joystick = joystickRoot.AddComponent<MobileJoystick>();
        joystick.background = rootRect;
        joystick.handle = handleRect;
        joystick.moveInput = moveInput;
        joystick.handleRange = 75f;
    }

    private void CreateActionButtons(Transform parent, RigidboyPlayerController player, Interact interact, flashlight flashlightController, List<LaunchHand> allHands)
    {
        List<LaunchHand> leftHands = new List<LaunchHand>();
        List<LaunchHand> rightHands = new List<LaunchHand>();

        foreach (LaunchHand hand in allHands)
        {
            if (hand == null)
            {
                continue;
            }

            if (hand.Hand == "Left")
            {
                leftHands.Add(hand);
            }
            else if (hand.Hand == "Right")
            {
                rightHands.Add(hand);
            }
        }

        CreateHoldButton(parent, "Left Fire", new Vector2(340f, 150f), () => InvokeHands(leftHands, true), () => InvokeHands(leftHands, false));
        CreateHoldButton(parent, "Right Fire", new Vector2(-350f, 150f), () => InvokeHands(rightHands, true), () => InvokeHands(rightHands, false), true);
        CreateButton(parent, "Jump", new Vector2(-140f, 150f), () => player.QueueJump(), true);
        CreateButton(parent, "Interact", new Vector2(-550f, 150f), interact != null ? (UnityAction)interact.TriggerInteraction : null, true);
        CreateButton(parent, "Flash", new Vector2(-420f, 280f), flashlightController != null ? (UnityAction)flashlightController.ToggleFlashlight : null, true, new Vector2(120f, 80f));
        CreateButton(parent, "Crouch", new Vector2(-140f, 280f), () => player.ToggleCrouch(), true);
        CreateHoldButton(parent, "Sprint", new Vector2(-280f, 280f), () => player.SetSprint(true), () => player.SetSprint(false), true);
        CreateButton(parent, "Menu", new Vector2(-90f, -70f), ToggleMobileSettingsMenu, true, new Vector2(140f, 70f), new Vector2(1f, 1f));

        float handButtonSpacing = 130f;
        float handButtonY = 95f;
        List<(string label, UnityAction action, Vector2 size)> handButtons = new List<(string, UnityAction, Vector2)>();

        if (player.handmanager != null && player.handmanager.hasRedHand)
        {
            handButtons.Add(("Red", SelectMobileRedHand, new Vector2(100f, 60f)));
        }
        if (player.handmanager != null && player.handmanager.hasPurpleHand)
        {
            handButtons.Add(("Purple", SelectMobilePurpleHand, new Vector2(100f, 60f)));
        }
        if (player.handmanager != null && player.handmanager.hasPressureHand)
        {
            handButtons.Add(("Flare", SelectMobileFlareHand, new Vector2(100f, 60f)));
        }
        if (player.handmanager != null && player.handmanager.hasConductiveHand)
        {
            handButtons.Add(("Conduct", SelectMobileConductiveHand, new Vector2(120f, 60f)));
        }

        float startX = -((handButtons.Count - 1) * handButtonSpacing) * 0.5f;
        for (int i = 0; i < handButtons.Count; i++)
        {
            var handButton = handButtons[i];
            CreateButton(parent, handButton.label, new Vector2(startX + i * handButtonSpacing, handButtonY), handButton.action, false, handButton.size, new Vector2(0.5f, 0f));
        }
    }

    private void CreateMobileSettingsPanel(Transform parent, SettingsManager settings)
    {
        RectTransform rootRect = parent.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image overlay = parent.gameObject.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.82f);
        overlay.raycastTarget = true;

        GameObject panel = CreateUiObject("Panel", parent);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.04f, 0.06f);
        panelRect.anchorMax = new Vector2(0.96f, 0.94f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.96f);

        CreateText(panel.transform, "Title", "Mobile Settings", 34, TextAnchor.UpperLeft, new Vector2(20f, -20f), new Vector2(500f, 50f), new Vector2(0f, 1f));
        CreatePanelButton(panel.transform, "Close", new Vector2(-20f, -20f), ToggleMobileSettingsMenu, new Vector2(140f, 60f), new Vector2(1f, 1f));

        GameObject leftColumn = CreateUiObject("LeftColumn", panel.transform);
        RectTransform leftRect = leftColumn.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0f, 0f);
        leftRect.anchorMax = new Vector2(0.42f, 1f);
        leftRect.offsetMin = new Vector2(20f, 20f);
        leftRect.offsetMax = new Vector2(-10f, -90f);

        GameObject rightColumn = CreateUiObject("RightColumn", panel.transform);
        RectTransform rightRect = rightColumn.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.44f, 0f);
        rightRect.anchorMax = new Vector2(1f, 1f);
        rightRect.offsetMin = new Vector2(10f, 20f);
        rightRect.offsetMax = new Vector2(-20f, -90f);

        CreateText(leftColumn.transform, "SettingsHeader", "Settings", 28, TextAnchor.UpperLeft, new Vector2(0f, 0f), new Vector2(300f, 40f), new Vector2(0f, 1f));
        CreateText(leftColumn.transform, "SensitivityLabel", "Sensitivity", 24, TextAnchor.MiddleLeft, new Vector2(0f, -70f), new Vector2(220f, 40f), new Vector2(0f, 1f));
        mobileSensitivityValue = CreateText(leftColumn.transform, "SensitivityValue", "0.00", 24, TextAnchor.MiddleRight, new Vector2(0f, -70f), new Vector2(220f, 40f), new Vector2(1f, 1f));
        mobileSensitivitySlider = CreateSlider(leftColumn.transform, new Vector2(0f, -125f), new Vector2(0f, -85f));
        mobileSensitivitySlider.minValue = 0.1f;
        mobileSensitivitySlider.maxValue = 5f;
        mobileSensitivitySlider.wholeNumbers = false;
        mobileSensitivitySlider.onValueChanged.AddListener(value =>
        {
            if (settings != null)
            {
                settings.SetSensitivity(value);
                UpdateMobileSettingsValues();
            }
        });

        CreateText(leftColumn.transform, "QualityLabel", "Quality", 24, TextAnchor.MiddleLeft, new Vector2(0f, -180f), new Vector2(200f, 40f), new Vector2(0f, 1f));
        mobileQualityValue = CreateText(leftColumn.transform, "QualityValue", string.Empty, 22, TextAnchor.MiddleCenter, new Vector2(0f, -180f), new Vector2(180f, 40f), new Vector2(0.5f, 1f));
        CreatePanelButton(leftColumn.transform, "-", new Vector2(210f, -180f), () => SetMobileQualityOffset(-1), new Vector2(55f, 45f), new Vector2(0f, 1f));
        CreatePanelButton(leftColumn.transform, "+", new Vector2(275f, -180f), () => SetMobileQualityOffset(1), new Vector2(55f, 45f), new Vector2(0f, 1f));

        mobileVsyncValue = CreateText(leftColumn.transform, "VsyncValue", string.Empty, 24, TextAnchor.MiddleLeft, new Vector2(0f, -245f), new Vector2(220f, 40f), new Vector2(0f, 1f));
        CreatePanelButton(leftColumn.transform, "Toggle", new Vector2(230f, -245f), ToggleMobileVsync, new Vector2(120f, 45f), new Vector2(0f, 1f));

        mobileFullscreenValue = CreateText(leftColumn.transform, "FullscreenValue", string.Empty, 24, TextAnchor.MiddleLeft, new Vector2(0f, -310f), new Vector2(260f, 40f), new Vector2(0f, 1f));
        CreatePanelButton(leftColumn.transform, "Toggle", new Vector2(230f, -310f), ToggleMobileFullscreen, new Vector2(120f, 45f), new Vector2(0f, 1f));

        CreateText(rightColumn.transform, "LogsHeader", "Logs", 28, TextAnchor.UpperLeft, new Vector2(0f, 0f), new Vector2(200f, 40f), new Vector2(0f, 1f));
        CreatePanelButton(rightColumn.transform, "Clear", new Vector2(-10f, 0f), ClearMobileLogs, new Vector2(120f, 45f), new Vector2(1f, 1f));
        CreatePanelButton(rightColumn.transform, "Copy All", new Vector2(-140f, 0f), CopyMobileLogs, new Vector2(120f, 45f), new Vector2(1f, 1f));
        CreateLogScrollView(rightColumn.transform);

        mobileSensitivitySlider.SetValueWithoutNotify(settings != null && settings.playerController != null ? settings.playerController.lookSpeedX : PlayerPrefs.GetFloat("Sensitivity", 1.5f));
    }

    private void CreateLogScrollView(Transform parent)
    {
        GameObject scrollObject = CreateUiObject("LogScroll", parent);
        RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0f, 0f);
        scrollRect.anchorMax = new Vector2(1f, 1f);
        scrollRect.offsetMin = new Vector2(0f, 0f);
        scrollRect.offsetMax = new Vector2(0f, -55f);

        Image scrollImage = scrollObject.AddComponent<Image>();
        scrollImage.color = new Color(0f, 0f, 0f, 0.35f);

        ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;

        GameObject viewport = CreateUiObject("Viewport", scrollObject.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        GameObject content = CreateUiObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = new Vector2(10f, 0f);
        contentRect.offsetMax = new Vector2(-10f, 0f);

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        mobileLogText = content.AddComponent<Text>();
        mobileLogText.font = runtimeFont;
        mobileLogText.fontSize = 18;
        mobileLogText.alignment = TextAnchor.UpperLeft;
        mobileLogText.horizontalOverflow = HorizontalWrapMode.Wrap;
        mobileLogText.verticalOverflow = VerticalWrapMode.Overflow;
        mobileLogText.color = Color.white;
        mobileLogText.supportRichText = false;

        scroll.viewport = viewportRect;
        scroll.content = contentRect;
    }

    private void InvokeHands(List<LaunchHand> hands, bool pressed)
    {
        foreach (LaunchHand hand in hands)
        {
            if (hand == null)
            {
                continue;
            }

            if (pressed)
            {
                hand.MobilePress();
            }
            else
            {
                hand.MobileRelease();
            }
        }
    }

    private void CreateButton(Transform parent, string text, Vector2 anchoredPosition, UnityAction onClick, bool rightSide = false, Vector2? sizeOverride = null, Vector2? anchorOverride = null)
    {
        GameObject buttonObject = CreateUiObject(text + "Button", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        Vector2 anchor = anchorOverride ?? (rightSide ? new Vector2(1f, 0f) : new Vector2(0f, 0f));
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = sizeOverride ?? new Vector2(120f, 120f);
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, controlAlpha);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => Debug.Log($"[MobileUIBootstrap] Button clicked: {text}"));
        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }

        CreateLabel(buttonObject.transform, text);
    }

    private void CreateHoldButton(Transform parent, string text, Vector2 anchoredPosition, UnityAction onPress, UnityAction onRelease, bool rightSide = false)
    {
        GameObject buttonObject = CreateUiObject(text + "Button", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        Vector2 anchor = rightSide ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(140f, 140f);
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, controlAlpha);

        MobilePressEvents pressEvents = buttonObject.AddComponent<MobilePressEvents>();
        if (pressEvents.onPress == null)
        {
            pressEvents.onPress = new UnityEvent();
        }
        if (pressEvents.onRelease == null)
        {
            pressEvents.onRelease = new UnityEvent();
        }
        if (onPress != null)
        {
            pressEvents.onPress.AddListener(onPress);
        }
        if (onRelease != null)
        {
            pressEvents.onRelease.AddListener(onRelease);
        }

        CreateLabel(buttonObject.transform, text);
    }

    private Slider CreateSlider(Transform parent, Vector2 topLeft, Vector2 topRight)
    {
        GameObject sliderObject = CreateUiObject("Slider", parent);
        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(topLeft.x, topLeft.y);
        rect.offsetMax = new Vector2(topRight.x, topRight.y);

        Slider slider = sliderObject.AddComponent<Slider>();

        GameObject background = CreateUiObject("Background", sliderObject.transform);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        GameObject fillArea = CreateUiObject("Fill Area", sliderObject.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0f);
        fillAreaRect.anchorMax = new Vector2(1f, 1f);
        fillAreaRect.offsetMin = new Vector2(15f, 0f);
        fillAreaRect.offsetMax = new Vector2(-15f, 0f);

        GameObject fill = CreateUiObject("Fill", fillArea.transform);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0.25f);
        fillRect.anchorMax = new Vector2(1f, 0.75f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.25f, 0.7f, 1f, 1f);

        GameObject handleArea = CreateUiObject("Handle Area", sliderObject.transform);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = new Vector2(0f, 0f);
        handleAreaRect.anchorMax = new Vector2(1f, 1f);
        handleAreaRect.offsetMin = new Vector2(15f, 0f);
        handleAreaRect.offsetMax = new Vector2(-15f, 0f);

        GameObject handle = CreateUiObject("Handle", handleArea.transform);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(30f, 50f);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        return slider;
    }

    private Text CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 anchoredPosition, Vector2 size, Vector2 anchor)
    {
        GameObject labelObject = CreateUiObject(name, parent);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(anchor.x, anchor.y);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Text label = labelObject.AddComponent<Text>();
        label.font = runtimeFont;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.white;
        label.text = text;
        label.raycastTarget = false;
        return label;
    }

    private void CreatePanelButton(Transform parent, string text, Vector2 anchoredPosition, UnityAction onClick, Vector2 size, Vector2 anchor)
    {
        GameObject buttonObject = CreateUiObject(text + "PanelButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(anchor.x, anchor.y);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.18f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }

        GameObject labelObject = CreateUiObject("Label", buttonObject.transform);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Text label = labelObject.AddComponent<Text>();
        label.font = runtimeFont;
        label.fontSize = 22;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.text = text;
        label.raycastTarget = false;
    }

    private void CreateDebugLabel(Transform parent)
    {
        GameObject labelObject = CreateUiObject("EditorDebugLabel", parent);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(420f, 50f);
        rect.anchoredPosition = new Vector2(20f, -20f);

        Text label = labelObject.AddComponent<Text>();
        label.font = runtimeFont;
        label.fontSize = 24;
        label.alignment = TextAnchor.UpperLeft;
        label.color = Color.white;
        label.text = $"EDITOR MOBILE DEBUG ({debugToggleKey} to toggle)";
    }

    private void CreateDebugStatusLabel(Transform parent)
    {
        GameObject labelObject = CreateUiObject("EditorDebugStatus", parent);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(700f, 80f);
        rect.anchoredPosition = new Vector2(20f, -70f);

        debugStatusLabel = labelObject.AddComponent<Text>();
        debugStatusLabel.font = runtimeFont;
        debugStatusLabel.fontSize = 20;
        debugStatusLabel.alignment = TextAnchor.UpperLeft;
        debugStatusLabel.color = Color.white;
        debugStatusLabel.text = "Look Debug";
    }

    private void CreateDebugToggleButton(Transform parent)
    {
        CreateButton(parent, "Debug Off", new Vector2(110f, -120f), () => SetEditorDebugMode(false), false, new Vector2(180f, 60f), new Vector2(0f, 1f));
    }

    private void CreateLabel(Transform parent, string text)
    {
        GameObject labelObject = CreateUiObject("Label", parent);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text label = labelObject.AddComponent<Text>();
        label.font = runtimeFont;
        label.fontSize = 24;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.text = text;
        label.raycastTarget = false;
    }

    private GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private GameObject CreateFullscreenUiLayer(string name, Transform parent)
    {
        GameObject layer = CreateUiObject(name, parent);
        RectTransform rect = layer.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        return layer;
    }

    private List<T> FindSceneObjectsOfType<T>() where T : Component
    {
        T[] allObjects = Resources.FindObjectsOfTypeAll<T>();
        List<T> sceneObjects = new List<T>();

        foreach (T item in allObjects)
        {
            if (item == null)
            {
                continue;
            }

            if (!item.gameObject.scene.IsValid())
            {
                continue;
            }

            if ((item.hideFlags & HideFlags.HideAndDontSave) != 0)
            {
                continue;
            }

            sceneObjects.Add(item);
        }

        return sceneObjects;
    }

    private void ApplyMobileScreenSettings()
    {
        if (!Application.isMobilePlatform || !forceLandscapeOnMobile)
        {
            return;
        }

        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void UpdateDebugStatus()
    {
        if (debugStatusLabel == null || debugPlayer == null)
        {
            return;
        }

        debugStatusLabel.text = $"Mouse look allowed: {debugPlayer.DesktopMouseLookAllowed}\nLook source: {debugPlayer.CurrentLookSource}\nLook delta: {debugPlayer.CurrentLookDelta}";
    }

    private void UpdateRuntimeUiVisibility()
    {
        if (currentUi == null)
        {
            return;
        }

        bool menuOpen = trackedSettings != null && trackedSettings.open;

        if (gameplayHudRoot != null && gameplayHudRoot.activeSelf == menuOpen)
        {
            gameplayHudRoot.SetActive(!menuOpen);
        }

        if (mobileSettingsRoot != null && mobileSettingsRoot.activeSelf != menuOpen)
        {
            mobileSettingsRoot.SetActive(menuOpen);
        }
    }

    private void UpdateMobileSettingsValues()
    {
        if (trackedSettings == null)
        {
            return;
        }

        if (mobileSensitivityValue != null)
        {
            float sensitivity = trackedSettings.playerController != null ? trackedSettings.playerController.lookSpeedX : PlayerPrefs.GetFloat("Sensitivity", 1.5f);
            mobileSensitivityValue.text = sensitivity.ToString("0.00");
            if (mobileSensitivitySlider != null && !Mathf.Approximately(mobileSensitivitySlider.value, sensitivity))
            {
                mobileSensitivitySlider.SetValueWithoutNotify(sensitivity);
            }
        }

        if (mobileQualityValue != null)
        {
            mobileQualityValue.text = QualitySettings.names[QualitySettings.GetQualityLevel()];
        }

        if (mobileVsyncValue != null)
        {
            mobileVsyncValue.text = $"VSync: {(QualitySettings.vSyncCount > 0 ? "On" : "Off")}";
        }

        if (mobileFullscreenValue != null)
        {
            mobileFullscreenValue.text = $"Fullscreen: {(Screen.fullScreen ? "On" : "Off")}";
        }
    }

    private void HandleLogMessage(string condition, string stackTrace, LogType type)
    {
        string line = $"[{type}] {condition}";
        recentLogs.Enqueue(line);

        while (recentLogs.Count > MaxLogLines)
        {
            recentLogs.Dequeue();
        }

        RefreshLogText();
    }

    private void RefreshLogText()
    {
        if (mobileLogText == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        foreach (string line in recentLogs)
        {
            builder.AppendLine(line);
        }

        mobileLogText.text = builder.ToString();
    }

    private bool HasHandUiStateChanged()
    {
        RigidboyPlayerController player = FindObjectOfType<RigidboyPlayerController>();
        int currentHash = ComputeHandUiStateHash(player);
        return currentHash != lastKnownHandUiStateHash;
    }

    private int ComputeHandUiStateHash(RigidboyPlayerController player)
    {
        if (player == null || player.handmanager == null)
        {
            return 0;
        }

        HandManager handManager = player.handmanager;
        int hash = 17;
        hash = hash * 31 + (handManager.hasGrabPack ? 1 : 0);
        hash = hash * 31 + (handManager.hasRedHand ? 1 : 0);
        hash = hash * 31 + (handManager.hasBlueHand ? 1 : 0);
        hash = hash * 31 + (handManager.hasPurpleHand ? 1 : 0);
        hash = hash * 31 + (handManager.hasPressureHand ? 1 : 0);
        hash = hash * 31 + (handManager.hasConductiveHand ? 1 : 0);
        return hash;
    }
}
