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
    [Header("잠금 표시 (잠긴 슬롯 위에 덮이는 오브젝트)")]
    [SerializeField] private GameObject _lockRoot;
    [SerializeField] private UnityEngine.UI.Button _lockButton;

    private int _slot;
    private Action<int> _onStart;
    private Action<int> _onDelete;
    private Action<int> _onLockedClick;
    private bool _isLocked;

    private void OnEnable()
    {
        _startButton.BindOnClickButtonEvent(OnClickStart);
        _deleteButton.BindOnClickButtonEvent(OnClickDelete);

        if (_lockButton != null)
        {
            _lockButton.onClick.AddListener(OnClickLocked);
        }
    }

    private void OnDisable()
    {
        if (_lockButton != null)
        {
            _lockButton.onClick.RemoveListener(OnClickLocked);
        }

        _startButton.UnBindAllOnClickButtonEvent();
        _deleteButton.UnBindAllOnClickButtonEvent();
    }

    public void Setup(int slot, bool hasSave, SaveSlotInfo info, Action<int> onStart, Action<int> onDelete, bool isLocked = false, Action<int> onLockedClick = null)
    {
        _slot = slot;
        _onStart = onStart;
        _onDelete = onDelete;
        _onLockedClick = onLockedClick;
        _isLocked = isLocked;

        // 잠긴 슬롯은 저장 데이터가 있어도 빈 슬롯처럼 보여 주고, 자물쇠가 위에 덮여서 클릭을 막는다.
        if (isLocked == true)
        {
            hasSave = false;
        }

        if (_lockRoot != null)
        {
            _lockRoot.SetActive(isLocked);
        }

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
        if (_isLocked == true)
        {
            return;
        }

        _onStart?.Invoke(_slot);
    }

    private void OnClickDelete()
    {
        if (_isLocked == true)
        {
            return;
        }

        _onDelete?.Invoke(_slot);
    }

    private void OnClickLocked()
    {
        _onLockedClick?.Invoke(_slot);
    }
}
