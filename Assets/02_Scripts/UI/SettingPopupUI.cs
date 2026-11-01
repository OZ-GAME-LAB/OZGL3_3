using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 설정 팝업: 사운드 볼륨과 화면(해상도, 전체화면/창모드)을 바꾼다.
public class SettingPopupUI : UIBase
{
    private const string KeyResWidth = "Setting_ResWidth";
    private const string KeyResHeight = "Setting_ResHeight";
    private const string KeyWindowMode = "Setting_WindowMode";    // 0: 전체화면, 1: 창모드

    private static readonly Vector2Int[] ResolutionCandidates =
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
    };

    [SerializeField] private Slider Slider_Bgm;    // Min 0, Max 1
    [SerializeField] private Slider Slider_Sfx;
    [SerializeField] private TMP_Dropdown Dropdown_Resolution;
    [SerializeField] private TMP_Dropdown Dropdown_WindowMode;    // 옵션 2개: 전체화면, 창모드
    [SerializeField] private UIButton Button_Close;

    private readonly List<Vector2Int> _resolutions = new List<Vector2Int>();
    private SoundManager _soundManager;

    private void OnEnable()
    {
        DoInitSound();
        DoInitResolutionDropdown();
        DoInitWindowModeDropdown();

        Dropdown_Resolution.onValueChanged.AddListener(OnChangedDisplay);
        Dropdown_WindowMode.onValueChanged.AddListener(OnChangedDisplay);
        Button_Close.BindOnClickButtonEvent(OnClickClose);
    }

    private void OnDisable()
    {
        // UIManager는 UI를 파괴하지 않고 끄기만 하므로, 꺼질 때 반드시 해제한다.
        Slider_Bgm.onValueChanged.RemoveListener(OnChangedBgm);
        Slider_Sfx.onValueChanged.RemoveListener(OnChangedSfx);
        Dropdown_Resolution.onValueChanged.RemoveListener(OnChangedDisplay);
        Dropdown_WindowMode.onValueChanged.RemoveListener(OnChangedDisplay);
        Button_Close.UnBindAllOnClickButtonEvent();
        _soundManager = null;
    }

    private void DoInitSound()
    {
        if (GameManager.Instance == null)
        {
            Logger.LogError("SettingPopupUI: GameManager가 씬에 없습니다.");
            return;
        }

        _soundManager = GameManager.Sound;

        Slider_Bgm.SetValueWithoutNotify(_soundManager.BGMVolume);
        Slider_Sfx.SetValueWithoutNotify(_soundManager.SFXVolume);

        Slider_Bgm.onValueChanged.AddListener(OnChangedBgm);
        Slider_Sfx.onValueChanged.AddListener(OnChangedSfx);
    }

    private void DoInitResolutionDropdown()
    {
        _resolutions.Clear();
        Dropdown_Resolution.ClearOptions();

        List<string> labels = new List<string>();
        int currentIndex = 0;
        Resolution display = Screen.currentResolution;

        for (int i = 0; i < ResolutionCandidates.Length; i++)
        {
            Vector2Int candidate = ResolutionCandidates[i];

            // 모니터보다 큰 해상도는 목록에서 뺀다.
            if (candidate.x > display.width || candidate.y > display.height)
            {
                continue;
            }

            _resolutions.Add(candidate);
            labels.Add($"{candidate.x} x {candidate.y}");

            if (candidate.x == Screen.width && candidate.y == Screen.height)
            {
                currentIndex = _resolutions.Count - 1;
            }
        }

        Dropdown_Resolution.AddOptions(labels);
        Dropdown_Resolution.SetValueWithoutNotify(currentIndex);
    }

    private void DoInitWindowModeDropdown()
    {
        Dropdown_WindowMode.SetValueWithoutNotify(ToModeIndex(Screen.fullScreenMode));
    }

    private void OnChangedBgm(float value)
    {
        _soundManager.SetBGMVolume(value);
    }

    private void OnChangedSfx(float value)
    {
        _soundManager.SetSFXVolume(value);
    }

    private void OnChangedDisplay(int _)
    {
        if (_resolutions.Count == 0)
        {
            return;
        }

        Vector2Int resolution = _resolutions[Dropdown_Resolution.value];
        int modeIndex = Dropdown_WindowMode.value;

        DoApplyDisplay(resolution.x, resolution.y, modeIndex);

        PlayerPrefs.SetInt(KeyResWidth, resolution.x);
        PlayerPrefs.SetInt(KeyResHeight, resolution.y);
        PlayerPrefs.SetInt(KeyWindowMode, modeIndex);
        PlayerPrefs.Save();
    }

    private static void DoApplyDisplay(int width, int height, int modeIndex)
    {
        Screen.SetResolution(width, height, ToFullScreenMode(modeIndex));
    }

    private static FullScreenMode ToFullScreenMode(int modeIndex)
    {
        switch (modeIndex)
        {
            case 0:
                return FullScreenMode.ExclusiveFullScreen;
            case 1:
                return FullScreenMode.FullScreenWindow;
            default:
                return FullScreenMode.Windowed;
        }
    }

    private static int ToModeIndex(FullScreenMode mode)
    {
        switch (mode)
        {
            case FullScreenMode.ExclusiveFullScreen:
                return 0;
            case FullScreenMode.FullScreenWindow:
            case FullScreenMode.MaximizedWindow:
                return 1;
            default:
                return 2;
        }
    }

    // 게임이 켜질 때 한 번 호출해 저장된 화면 설정을 적용한다.
    public static void ApplySavedDisplay()
    {
        if (PlayerPrefs.HasKey(KeyResWidth) == false)
        {
            return;
        }

        DoApplyDisplay(
            PlayerPrefs.GetInt(KeyResWidth),
            PlayerPrefs.GetInt(KeyResHeight),
            PlayerPrefs.GetInt(KeyWindowMode));
    }

    private void OnClickClose()
    {
        UIManager.Instance.CloseSettingPopupUI();
    }
}