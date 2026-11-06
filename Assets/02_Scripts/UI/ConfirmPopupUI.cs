using System;
using TMPro;
using UnityEngine;

public class ConfirmPopupUI : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_Message;
    [SerializeField] private UIButton Button_Confirm;
    [SerializeField] private UIButton Button_Cancel;

    private Action _onConfirm;
    private Action _onCancel;

    public void Init(string message, Action onConfirm, Action onCancel = null)
    {
        Text_Message.text = message;
        _onConfirm = onConfirm;
        _onCancel = onCancel;
    }

    private void OnEnable()
    {
        Button_Confirm.BindOnClickButtonEvent(OnClickConfirm);
        Button_Cancel.BindOnClickButtonEvent(OnClickCancel);
    }

    private void OnDisable()
    {
        Button_Confirm.UnBindAllOnClickButtonEvent();
        Button_Cancel.UnBindAllOnClickButtonEvent();
    }

    private void OnClickConfirm()
    {
        Action callback = _onConfirm;
        Close();
        callback?.Invoke();
    }

    private void OnClickCancel()
    {
        Action callback = _onCancel;
        Close();
        callback?.Invoke();
    }

    private void Close()
    {
        _onConfirm = null;
        _onCancel = null;
        UIManager.Instance.CloseConfirmPopupUI();
    }
}