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
        if (_slots.Count == 0)
        {
            CreateSlot(Const.GuardedAttackSkillId, _iconGuardedAttack, _keyGuardedAttack);
            CreateSlot(Const.HealSkillId, _iconHeal, _keyHeal);
        }

        foreach (SkillSlot slot in _slots)
        {
            slot.Init(slot.SkillId, player, status);
        }
    }

    public void Release()
    {
        if (_waitRoutine != null)
        {
            StopCoroutine(_waitRoutine);
            _waitRoutine = null;
        }

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