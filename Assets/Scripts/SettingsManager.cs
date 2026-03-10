using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    private const string DebugSpawnInEditorPrefKey = "MobileUIBootstrap_DebugSpawnInEditor";
    public Slider sensitivitySlider;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle vSyncToggle;
    public Toggle fullScreenToggle;

    public RigidboyPlayerController playerController;

    private const string SensitivityKey = "Sensitivity";
    private const string QualityKey = "QualityLevel";
    private const string VSyncKey = "VSync";
    private const string FullScreenKey = "FullScreen";
    private const string ResolutionKey = "Resolution"; 

    Resolution[] resolutions; 

    public Animator animator;

    public bool open = false;
    public bool useMobileMenuMode = false;

    public LaunchHand[] hands;

    public Animator playerAnimator;

    public GameObject Dragsource1;
    public GameObject Dragsource2;
    public bool debugMobileInEditor = false;

    void Start()
    {
        SetupQualityDropdown();
        SetupResolutionDropdown();
        LoadSettings();

        if (fullScreenToggle != null)
            fullScreenToggle.onValueChanged.AddListener(SetFullscreen);
        if (vSyncToggle != null)
            vSyncToggle.onValueChanged.AddListener(SetVSync);
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution); 
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        SetMenuState(!open, !useMobileMenuMode);
    }

    public void SetMenuState(bool state, bool animateDesktopMenu = true)
    {
        open = state;
        updateOpenStatus(state, animateDesktopMenu);
    }

    void updateOpenStatus(bool state, bool animateDesktopMenu = true)
    {
        if (animator != null)
        {
            animator.SetBool("open", animateDesktopMenu && state);
        }

        if (playerController != null)
        {
            playerController.enabled = !state;
        }

        if (playerAnimator != null)
        {
            playerAnimator.enabled = !state;
        }

        foreach (LaunchHand hand in hands)
        {
            if (hand != null)
                hand.enabled = !state;
        }

        if (state)
        {
            Rigidbody rb = playerController != null ? playerController.GetComponent<Rigidbody>() : null;
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            UnlockCursor();

            if (playerController != null)
            {
                playerController.StopFootsteps();
            }

            if (Dragsource1 != null)
                Dragsource1.SetActive(false);
            if (Dragsource2 != null)
                Dragsource2.SetActive(false);
        }
        else
        {
            LockCursor();
        }
    }

    void SetupQualityDropdown()
    {
        if (qualityDropdown == null)
        {
            return;
        }

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }

    void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null)
        {
            return;
        }

        if (Application.isMobilePlatform)
        {
            resolutionDropdown.gameObject.SetActive(false);
            return;
        }

        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        PlayerPrefs.SetInt(VSyncKey, enabled ? 1 : 0);
    }

    public void SetFullscreen(bool enabled)
    {
        Screen.fullScreen = enabled;
        PlayerPrefs.SetInt(FullScreenKey, enabled ? 1 : 0);
    }

    public void SetSensitivity(float value)
    {
        if (playerController != null)
        {
            playerController.lookSpeedX = value;
            playerController.lookSpeedY = value;
        }

        PlayerPrefs.SetFloat(SensitivityKey, value);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt(QualityKey, index);
    }

    public void SetResolution(int index)
    {
        if (Application.isMobilePlatform || resolutions == null || resolutions.Length == 0)
        {
            return;
        }

        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt(ResolutionKey, index);
    }

    void LoadSettings()
    {
        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1.5f);
        if (sensitivitySlider != null)
            sensitivitySlider.value = savedSensitivity;
        SetSensitivity(savedSensitivity);

        int savedQuality = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
        if (qualityDropdown != null)
            qualityDropdown.value = savedQuality;
        SetQuality(savedQuality);

        int savedVSync = PlayerPrefs.GetInt(VSyncKey, 1);
        if (vSyncToggle != null)
            vSyncToggle.isOn = savedVSync == 1;
        SetVSync(savedVSync == 1);

        int savedFullScreen = PlayerPrefs.GetInt(FullScreenKey, 1);
        if (fullScreenToggle != null)
            fullScreenToggle.isOn = savedFullScreen == 1;
        SetFullscreen(savedFullScreen == 1);

        if (!Application.isMobilePlatform && resolutionDropdown != null && resolutionDropdown.gameObject.activeSelf)
        {
            int savedResolution = PlayerPrefs.GetInt(ResolutionKey, resolutionDropdown.value);
            resolutionDropdown.value = savedResolution;
            SetResolution(savedResolution);
        }
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LockCursor()
    {
        if (Application.isMobilePlatform || IsEditorDebugMobileActive())
        {
            UnlockCursor();
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool IsEditorDebugMobileActive()
    {
        return Application.isEditor && (debugMobileInEditor || PlayerPrefs.GetInt(DebugSpawnInEditorPrefKey, 0) == 1);
    }
}