using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveProfileUI : UIBase
{
    private const int _slotCount = 3;   // TODO: UserDataManager.SlotCount가 생기면 교체

    [SerializeField] private SaveSlot _slotPrefab;     // Project 창의 SaveSlot 프리팹
    [SerializeField] private Transform _slotRoot;      // SaveSlotRoot
    [SerializeField] private UIButton Button_Close;

    private readonly List<SaveSlot> _slots = new();
    private int _pendingDeleteSlot;

    private void OnEnable()
    {
        Button_Close.BindOnClickButtonEvent(OnClickClose);
        CreateSlots();
        Refresh();
    }

    private void OnDisable()
    {
        Button_Close.UnBindAllOnClickButtonEvent();
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

        for (int i = 0; i < _slotCount; i++)
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
        DateTime? time = GameManager.UserData.GetSaveTime(slot);

        // TODO(강혜창): GetSlotSummary(slot)가 생기면 Stage/Currency를 실제 값으로 교체
        //               Stage = GetStageName(summary.StoryStage), Currency = summary.Currency.ToString("N0")
        return new SaveSlotInfo
        {
            Stage = "-",
            Currency = "-",
            SavedTime = time.HasValue ? time.Value.ToString("yyyy-MM-dd HH:mm") : "-"
        };
    }

    // StoryStage 값 → 표시 이름 (실제 표기는 기획과 확인, 데이터화는 나중에)
    private static string GetStageName(int storyStage)
    {
        return storyStage switch
        {
            0 => "프롤로그",
            1 => "챕터 1: 메마른 숲",
            2 => "챕터 2: 오래된 유적",
            3 => "플레이 구간 완료",
            _ => "-"
        };
    }

    private void OnClickSlotStart(int slot)
    {
        UserDataManager userData = GameManager.UserData;
        bool hasSave = userData.HasSave(slot);

        userData.SelectSlot(slot);

        if (hasSave == true)
        {
            if (userData.LoadAllUserData() == false)
            {
                // TODO(강혜창): 불러오기 실패 알림(SimplePopup)
                return;
            }
        }
        else
        {
            userData.SetAllDefaultUserData();
            userData.SaveAllUserData();
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
        GameManager.UserData.DeleteSlot(_pendingDeleteSlot);
        Refresh();
    }

    private void StartGame()
    {
        UIManager.Instance.CloseSaveSlotUI();
        UIManager.Instance.CloseGameStartUI();
        UIManager.Instance.OpenMainHUD();

        // TODO(강혜창): 시작 직후 구간 로고(GameManager.Area.CurrentRegionId)는 플레이어 동적 생성 때 재검토
    }

    private void OnClickClose()
    {
        UIManager.Instance.CloseSaveSlotUI();
    }
}