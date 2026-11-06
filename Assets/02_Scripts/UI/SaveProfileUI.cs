using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SaveProfileUI : UIBase
{
    [SerializeField] private SaveSlot _slotPrefab;     // Project 창의 SaveSlot 프리팹
    [SerializeField] private Transform _slotRoot;      // SaveSlotRoot
    [FormerlySerializedAs("Button_Close")]
    [SerializeField] private UIButton _closeButton;

    private readonly List<SaveSlot> _slots = new();
    private int _pendingDeleteSlot;

    private void OnEnable()
    {
        _closeButton.BindOnClickButtonEvent(OnClickClose);
        CreateSlots();
        Refresh();
    }

    private void OnDisable()
    {
        _closeButton.UnBindAllOnClickButtonEvent();
    }

    // 처음 한 번만 만든다. UI는 파괴되지 않고 껐다 켜지므로 이후에는 재사용한다.
    private void CreateSlots()
    {
        if (_slots.Count > 0)
        {
            return;
        }

        if (_slotPrefab == null || _slotRoot == null)
        {
            Logger.LogError("SaveSlotUI: _slotPrefab 또는 _slotRoot가 연결되지 않았습니다.");
            return;
        }

        for (int i = 0; i < UserDataManager.SlotCount; i++)
        {
            SaveSlot slot = Instantiate(_slotPrefab, _slotRoot, false);
            slot.name = $"SaveSlot_{i}";
            _slots.Add(slot);
        }
    }

    private void Refresh()
    {
        UserDataManager userData = GameManager.UserData;

        for (int i = 0; i < _slots.Count; i++)
        {
            bool hasSave = userData.HasSave(i);
            SaveSlotInfo info = hasSave ? GetSlotInfo(i) : default;
            _slots[i].Setup(i, hasSave, info, OnClickSlotStart, OnClickSlotDelete);
        }
    }

    private SaveSlotInfo GetSlotInfo(int slot)
    {
        UserSlotMetaData.Slot meta = GameManager.UserData.GetSlotMeta(slot);
        if (meta == null)
        {
            return new SaveSlotInfo { Currency = "-", PlayTime = "-", CreatedAt = "읽기 실패" };
        }

        TimeSpan playTime = TimeSpan.FromSeconds(meta.PlayTimeSeconds);
        return new SaveSlotInfo
        {
            Currency = meta.Currency.ToString("N0"),
            PlayTime = $"{(long)playTime.TotalHours:D2}:{playTime.Minutes:D2}:{playTime.Seconds:D2}",
            CreatedAt = meta.CreatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
        };
    }

    private void OnClickSlotStart(int slot)
    {
        UserDataManager userData = GameManager.UserData;
        bool success = userData.HasSave(slot) ? userData.SelectSlot(slot) : userData.CreateSlot(slot);
        if (!success)
        {
            ShowError("슬롯을 불러오거나 생성하지 못했습니다.");
            return;
        }

        StartGame();
    }

    private void OnClickSlotDelete(int slot)
    {
        _pendingDeleteSlot = slot;
        UIManager.Instance.OpenConfirmPopup(
            $"슬롯 {slot + 1}의 저장 데이터를 삭제할까요?\n삭제하면 되돌릴 수 없습니다.",
            DeleteSelectedSlot);
    }

    private void DeleteSelectedSlot()
    {
        if (!GameManager.UserData.DeleteSlot(_pendingDeleteSlot))
        {
            ShowError("슬롯을 삭제하지 못했습니다.");
        }

        Refresh();
    }

    private void StartGame()
    {
        GameManager.UserData.BeginPlaySession();
        UIManager.Instance.CloseSaveSlotUI();
        UIManager.Instance.CloseGameStartUI();
        UIManager.Instance.OpenMainHUD();

        // TODO(강혜창): 시작 직후 구간 로고(GameManager.Area.CurrentRegionId)는 플레이어 동적 생성 때 재검토
    }

    private void OnClickClose()
    {
        UIManager.Instance.CloseSaveSlotUI();
    }

    private void ShowError(string message)
    {
        SimplePopup popup = UIManager.Instance.OpenPopupUI(UIType.SimplePopupUI) as SimplePopup;
        popup?.Init(message);
    }
}
