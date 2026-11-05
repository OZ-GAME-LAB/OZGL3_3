using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public struct SaveSlotInfo
{
    public string Currency { get; set; }
    public string PlayTime { get; set; }
    public string CreatedAt { get; set; }
}

public class SaveSlot : MonoBehaviour
{
    [FormerlySerializedAs("Image_Character")]
    [SerializeField] private UnityEngine.UI.Image _characterImage;
    [FormerlySerializedAs("Obj_Info")]
    [SerializeField] private GameObject _infoRoot;
    [FormerlySerializedAs("Text_Stage")]
    [SerializeField] private TextMeshProUGUI _playTimeText;
    [FormerlySerializedAs("Text_Currency")]
    [SerializeField] private TextMeshProUGUI _currencyText;
    [FormerlySerializedAs("Text_Time")]
    [SerializeField] private TextMeshProUGUI _createdAtText;
    [FormerlySerializedAs("Button_Start")]
    [SerializeField] private UIButton _startButton;
    [FormerlySerializedAs("Button_Delete")]
    [SerializeField] private UIButton _deleteButton;

    private int _slot;
    private Action<int> _onStart;
    private Action<int> _onDelete;

    private void OnEnable()
    {
        _startButton.BindOnClickButtonEvent(OnClickStart);
        _deleteButton.BindOnClickButtonEvent(OnClickDelete);
    }

    private void OnDisable()
    {
        _startButton.UnBindAllOnClickButtonEvent();
        _deleteButton.UnBindAllOnClickButtonEvent();
    }

    public void Setup(int slot, bool hasSave, SaveSlotInfo info, Action<int> onStart, Action<int> onDelete)
    {
        _slot = slot;
        _onStart = onStart;
        _onDelete = onDelete;

        // 빈 슬롯이면 캐릭터, 글자, 삭제 버튼을 모두 숨기고 게임 시작 버튼만 남긴다.
        _characterImage.gameObject.SetActive(hasSave);
        _infoRoot.SetActive(hasSave);
        _deleteButton.gameObject.SetActive(hasSave);
        // UIButton은 비활성화될 때 이벤트를 해제하므로 삭제 후 재생성에도 다시 연결한다.
        _deleteButton.BindOnClickButtonEvent(OnClickDelete);
        _startButton.ChangeButtonText(hasSave ? "이어 하기" : "새로 시작");

        if (hasSave == true)
        {
            _playTimeText.text = info.PlayTime;
            _currencyText.text = info.Currency;
            _createdAtText.text = info.CreatedAt;
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
