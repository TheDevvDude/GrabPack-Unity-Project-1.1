using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
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

    public LaunchHand[] hands;

    public Animator playerAnimator;

    public GameObject Dragsource1;
    public GameObject Dragsource2;

    public MobileIcons mobileIcons;


    void Start()
    {
        SetupQualityDropdown();
        SetupResolutionDropdown();
        LoadSettings();

        fullScreenToggle.onValueChanged.AddListener(SetFullscreen);
        vSyncToggle.onValueChanged.AddListener(SetVSync);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        resolutionDropdown.onValueChanged.AddListener(SetResolution); 
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            open = !open;
            updateOpenStatus(open);
        }
    }

    public void ToggleOpen()
    {
        open = !open;
        updateOpenStatus(open);
    }

    void updateOpenStatus(bool state)
    {
        animator.SetBool("open", state);
        playerController.enabled = !state;

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
            Rigidbody rb = playerController.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            UnlockCursor();

            playerController.StopFootsteps();
            Dragsource1.SetActive(false);
            Dragsource2.SetActive(false);
        }
        else
        {
            LockCursor();
        }
    }

    void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }

    
    void SetupResolutionDropdown()
    {
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
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt(ResolutionKey, index);
    }

    void LoadSettings()
    {
        // Sensitivity
        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1.5f);
        sensitivitySlider.value = savedSensitivity;
        SetSensitivity(savedSensitivity);

        //Quality (Mobile Override)
        bool hasSavedQuality = PlayerPrefs.HasKey(QualityKey);

        int savedQuality;

        if (Application.isMobilePlatform && !hasSavedQuality)
        {
            savedQuality = 0;
            PlayerPrefs.SetInt(QualityKey, 0);
        }
        else
        {
            savedQuality = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
        }

        qualityDropdown.value = savedQuality;
        SetQuality(savedQuality);

        // VSync
        int savedVSync = PlayerPrefs.GetInt(VSyncKey, 0);
        vSyncToggle.isOn = savedVSync == 1;
        SetVSync(vSyncToggle.isOn);

        // Fullscreen (Skip On Mobile)
        if (!Application.isMobilePlatform)
        {
            int savedFullScreen = PlayerPrefs.GetInt(FullScreenKey, 1);
            fullScreenToggle.isOn = savedFullScreen == 1;
            SetFullscreen(fullScreenToggle.isOn);
        }
        else
        {
            fullScreenToggle.gameObject.SetActive(false);
            resolutionDropdown.gameObject.SetActive(false);
        }

        //Resolution (PC Only)
        if (!Application.isMobilePlatform)
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
        if (mobileIcons.isMobile == false)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }
}