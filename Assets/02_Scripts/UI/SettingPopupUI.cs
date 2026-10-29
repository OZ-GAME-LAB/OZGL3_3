using UnityEngine;
using UnityEngine.UI;

// 설정 팝업: 사운드 매니저의 현재 볼륨을 보여 주고, 슬라이더로 바꾼다.
public class SettingPopupUI : UIBase
{
    [SerializeField] private Slider Slider_Bgm;    // Min 0, Max 1
    [SerializeField] private Slider Slider_Sfx;
    [SerializeField] private UIButton Button_Close;

    private SoundManager _soundManager;

    private void OnEnable()
    {
        if (GameManager.Instance == null)
        {
            Logger.LogError("SettingUI: GameManager가 씬에 없습니다.");
            return;
        }

        _soundManager = GameManager.Sound;

        Slider_Bgm.SetValueWithoutNotify(_soundManager.BGMVolume);
        Slider_Sfx.SetValueWithoutNotify(_soundManager.SFXVolume);

        Slider_Bgm.onValueChanged.AddListener(OnChangedBgm);
        Slider_Sfx.onValueChanged.AddListener(OnChangedSfx);
        Button_Close.BindOnClickButtonEvent(OnClickClose);
    }

    private void OnDisable()
    {
        Slider_Bgm.onValueChanged.RemoveListener(OnChangedBgm);
        Slider_Sfx.onValueChanged.RemoveListener(OnChangedSfx);
        Button_Close.UnBindAllOnClickButtonEvent();
        _soundManager = null;
    }

    private void OnChangedBgm(float value)
    {
        _soundManager.SetBGMVolume(value);
    }

    private void OnChangedSfx(float value)
    {
        _soundManager.SetSFXVolume(value);
    }

    private void OnClickClose()
    {
        UIManager.Instance.CloseSettingPopupUI();
    }
}