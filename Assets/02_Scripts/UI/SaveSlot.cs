using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct SaveSlotInfo
{
    public string Stage;
    public string Currency;
    public string SavedTime;
}

public class SaveSlot : MonoBehaviour
{
    [SerializeField] private Image Image_Character;
    [SerializeField] private GameObject Obj_Info;              // Set_Text
    [SerializeField] private TextMeshProUGUI Text_Stage;
    [SerializeField] private TextMeshProUGUI Text_Currency;
    [SerializeField] private TextMeshProUGUI Text_Time;
    [SerializeField] private UIButton Button_Start;
    [SerializeField] private UIButton Button_Delete;

    private int _slot;
    private Action<int> _onStart;
    private Action<int> _onDelete;

    private void OnEnable()
    {
        Button_Start.BindOnClickButtonEvent(OnClickStart);
        Button_Delete.BindOnClickButtonEvent(OnClickDelete);
    }

    private void OnDisable()
    {
        Button_Start.UnBindAllOnClickButtonEvent();
        Button_Delete.UnBindAllOnClickButtonEvent();
    }

    public void Setup(int slot, bool hasSave, SaveSlotInfo info, Action<int> onStart, Action<int> onDelete)
    {
        _slot = slot;
        _onStart = onStart;
        _onDelete = onDelete;

        // 빈 슬롯이면 캐릭터, 글자, 삭제 버튼을 모두 숨기고 게임 시작 버튼만 남긴다.
        Image_Character.gameObject.SetActive(hasSave);
        Obj_Info.SetActive(hasSave);
        Button_Delete.gameObject.SetActive(hasSave);

        if (hasSave == true)
        {
            Text_Stage.text = info.Stage;
            Text_Currency.text = info.Currency;
            Text_Time.text = info.SavedTime;
        }
    }

    private void OnClickStart()
    {
        _onStart?.Invoke(_slot);
    }

    private void OnClickDelete()
    {
        _onDelete?.Invoke(_slot);
    }
}