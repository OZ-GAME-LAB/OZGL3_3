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
        // TODO(강혜창): SoundManager 접근 경로 확인 (GameManager.Instance.Sound는 추측)
        _soundManager = GameManager.Sound;

        if (_soundManager == null)
        {
            Logger.LogError("SettingUI: SoundManager가 없습니다.");
            return;
        }

        // 저장된 값을 보여 주되, 이벤트는 울리지 않게 넣는다.
        Slider_Bgm.SetValueWithoutNotify(_soundManager.BGMVolume);
        Slider_Sfx.SetValueWithoutNotify(_soundManager.SFXVolume);

        Slider_Bgm.onValueChanged.AddListener(OnChangedBgm);
        Slider_Sfx.onValueChanged.AddListener(OnChangedSfx);
        Button_Close.BindOnClickButtonEvent(OnClickClose);
    }

    private void OnDisable()
    {
        // UIManager는 UI를 파괴하지 않고 끄기만 하므로, 꺼질 때 반드시 해제한다.
        Slider_Bgm.onValueChanged.RemoveListener(OnChangedBgm);
        Slider_Sfx.onValueChanged.RemoveListener(OnChangedSfx);
        Button_Close.UnBindAllOnClickButtonEvent();
        _soundManager = null;
    }

    private void OnChangedBgm(float value)
    {
        //_soundManager.SetBGMVolume(value);
    }

    private void OnChangedSfx(float value)
    {
        //_soundManager.SetSFXVolume(value);
    }

    private void OnClickClose()
    {
        UIManager.Instance.CloseSettingPopupUI();
    }
}