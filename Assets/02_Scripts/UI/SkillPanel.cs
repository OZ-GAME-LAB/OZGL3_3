using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillPanel : MonoBehaviour
{
    [SerializeField] private SkillSlot _slotPrefab;       // Project 창의 프리팹
    [SerializeField] private Transform _slotRoot;
    [SerializeField] private Sprite _iconGuardedAttack;   // S
    [SerializeField] private Sprite _iconHeal;            // D
    [SerializeField] private string _keyGuardedAttack = "Q";   // 슬롯 아래에 표시할 단축키 글자 (표시만 바뀐다)
    [SerializeField] private string _keyHeal = "E";

    private readonly List<SkillSlot> _slots = new List<SkillSlot>();
    private Coroutine _waitRoutine;
    private PlayerController _player;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        // 스킬을 얻기 전에는 상자 전체가 보이지 않는다.
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (_player != null)
        {
            RefreshUnlockState();
        }
    }

    // MainHUD의 OnEnable에서 호출한다. 플레이어가 준비될 때까지 기다렸다가 초기화한다.
    public void Begin()
    {
        if (_waitRoutine != null)
        {
            StopCoroutine(_waitRoutine);
        }

        _waitRoutine = StartCoroutine(WaitAndInit());
    }

    public void Init(PlayerController player, PlayerStatus status)
    {
        _player = player;
        if (_slots.Count == 0)
        {
            CreateSlot(Const.GuardedAttackSkillId, _iconGuardedAttack, _keyGuardedAttack);
            CreateSlot(Const.HealSkillId, _iconHeal, _keyHeal);
        }

        foreach (SkillSlot slot in _slots)
        {
            slot.Init(slot.SkillId, player, status);
        }

        RefreshUnlockState();
    }

    // 해금된 스킬의 슬롯만 보여 주고, 하나도 없으면 상자 전체를 숨긴다. (S: 방어 공격, D: 힐)
    private void RefreshUnlockState()
    {
        bool isAnyUnlocked = false;

        foreach (SkillSlot slot in _slots)
        {
            bool isUnlocked = IsSkillUnlocked(slot.SkillId);

            if (slot.gameObject.activeSelf != isUnlocked)
            {
                slot.gameObject.SetActive(isUnlocked);
            }

            if (isUnlocked == true)
            {
                isAnyUnlocked = true;
            }
        }

        _canvasGroup.alpha = isAnyUnlocked == true ? 1f : 0f;
    }

    private bool IsSkillUnlocked(string skillId)
    {
        if (skillId == Const.HealSkillId)
        {
            return _player.IsSkillDUnlocked;
        }

        return _player.IsSkillSUnlocked;
    }

    public void Release()
    {
        if (_waitRoutine != null)
        {
            StopCoroutine(_waitRoutine);
            _waitRoutine = null;
        }

        _player = null;

        foreach (SkillSlot slot in _slots)
        {
            slot.Release();
        }
    }

    private IEnumerator WaitAndInit()
    {
        PlayerController player = null;

        while (player == null || PlayerStatus.Instance == null)
        {
            player = FindFirstObjectByType<PlayerController>();
            yield return null;
        }

        _waitRoutine = null;
        Init(player, PlayerStatus.Instance);
    }

    private void CreateSlot(string skillId, Sprite icon, string key)
    {
        SkillSlot slot = Instantiate(_slotPrefab, _slotRoot);
        slot.name = $"SkillSlot_{skillId}";
        slot.SetIcon(icon);
        slot.SetKeyText(key);
        slot.SetSkillId(skillId);
        _slots.Add(slot);
    }
}