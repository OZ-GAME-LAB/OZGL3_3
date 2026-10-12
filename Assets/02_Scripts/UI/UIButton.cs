using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    [SerializeField] private Button Button_Base;
    [SerializeField] private TextMeshProUGUI Text_Base;
    [SerializeField] private Image Image_Base;
    [SerializeField] private Image Image_Select;

    private bool _isManualUnbindEvent;

    private readonly Dictionary<Action, UnityAction> _boundActions = new Dictionary<Action, UnityAction>();

    private void Awake()
    {
        InitUIButton();
        SetDefaultUI();
    }

    private void OnDisable()
    {
        if (Button_Base == null)
        {
            return;
        }

        if (_isManualUnbindEvent == false)
        {
            foreach (var unityAction in _boundActions.Values)
            {
                Button_Base.onClick.RemoveListener(unityAction);
            }

            _boundActions.Clear();
        }
    }

    private void SetDefaultUI()
    {
        if (Image_Select != null)
        {
            Image_Select.gameObject.SetActive(false);
        }
    }

    private void InitUIButton()
    {
        if (Button_Base != null)
        {
            return;
        }

        var button = this.gameObject.GetComponentInChildren<Button>();

        if (button != null)
        {
            this.Button_Base = button;
        }
    }

    public void BindOnClickButtonEvent(Action onClickCallback, bool isManualUnbindEvent = false)
    {
        if (Button_Base == null || onClickCallback == null)
        {
            return;
        }

        if (_boundActions.ContainsKey(onClickCallback) == true)
        {
            return;
        }

        UnityAction unityAction = onClickCallback.Invoke;
        _boundActions.Add(onClickCallback, unityAction);
        Button_Base.onClick.AddListener(unityAction);
        _isManualUnbindEvent = isManualUnbindEvent;
    }

    public void UnBindOnClickButtonEvent(Action onClickCallback)
    {
        if (Button_Base == null || onClickCallback == null)
        {
            return;
        }

        if (_boundActions.TryGetValue(onClickCallback, out var unityAction) == false)
        {
            return;
        }

        Button_Base.onClick.RemoveListener(unityAction);
        _boundActions.Remove(onClickCallback);
    }

    public void UnBindAllOnClickButtonEvent()
    {
        if (Button_Base == null)
        {
            return;
        }

        Button_Base.onClick.RemoveAllListeners();
        _boundActions.Clear();
    }

    public void ChangeButtonText(string buttonStr)
    {
        if (Text_Base == null)
        {
            return;
        }

        Text_Base.text = buttonStr;
    }

    public void SetSelectedUI(bool isSelected)
    {
        if (Image_Select == null)
        {
            return;
        }

        Image_Select.gameObject.SetActive(isSelected);
    }

    public void SetInteractable(bool isInteractable)
    {
        if (Button_Base == null)
        {
            return;
        }

        Button_Base.interactable = isInteractable;
    }
}