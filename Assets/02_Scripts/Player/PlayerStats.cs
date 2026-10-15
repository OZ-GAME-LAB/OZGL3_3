using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField, Min(0f)] private float _baseJumpValue = 5f;
    [SerializeField, Min(0f)] private float _baseWalkSpeed = 4f;
    [SerializeField, Min(0f)] private float _baseRunSpeed = 6f;
    [SerializeField, Min(0f)] private float _baseCrouchSpeed = 2f;
    [SerializeField, Min(1)] private int _baseAttackDamage = 1;
    [SerializeField, Min(1)] private int _baseMaxHealth = 5;
    [SerializeField, Min(1)] private int _baseMaxSkillGauge = 100;
    [SerializeField, Min(1)] private int _baseMaxChaosGauge = 10;

    private bool _isCalculatedStats;
    private float _jumpValueBaseBonus = 1f;
    private float _walkSpeedBaseBonus = 1f;
    private float _runSpeedBaseBonus = 1f;
    private float _crouchSpeedBaseBonus = 1f;
    private float _jumpValueBonusValue;
    private float _walkSpeedBonusValue;
    private float _runSpeedBonusValue;
    private float _crouchSpeedBonusValue;
    private int _attackDamage;
    private int _maxHealth;
    private int _maxSkillGauge;
    private int _maxChaosGauge;
    private int _attackDamageBonusValue;
    private int _maxHealthBonusValue;
    private int _maxSkillGaugeBonusValue;
    private int _maxChaosGaugeBonusValue;

    public float JumpValue { get { EnsureStatsCalculated(); return _baseJumpValue * _jumpValueBaseBonus; } }
    public float WalkSpeed { get { EnsureStatsCalculated(); return _baseWalkSpeed * _walkSpeedBaseBonus; } }
    public float RunSpeed { get { EnsureStatsCalculated(); return _baseRunSpeed * _runSpeedBaseBonus; } }
    public float CrouchSpeed { get { EnsureStatsCalculated(); return _baseCrouchSpeed * _crouchSpeedBaseBonus; } }
    public int AttackDamage { get { EnsureStatsCalculated(); return _attackDamage; } }
    public int MaxHealth { get { EnsureStatsCalculated(); return _maxHealth; } }
    public int MaxSkillGauge { get { EnsureStatsCalculated(); return _maxSkillGauge; } }
    public int MaxChaosGauge { get { EnsureStatsCalculated(); return _maxChaosGauge; } }
    public event Action OnStatsChanged;

    private void Awake()
    {
        RecalculateStats();
    }

    private void OnEnable()
    {
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        _jumpValueBaseBonus = Mathf.Max(0f, 1f + _jumpValueBonusValue);
        _walkSpeedBaseBonus = Mathf.Max(0f, 1f + _walkSpeedBonusValue);
        _runSpeedBaseBonus = Mathf.Max(0f, 1f + _runSpeedBonusValue);
        _crouchSpeedBaseBonus = Mathf.Max(0f, 1f + _crouchSpeedBonusValue);
        _attackDamage = Mathf.Max(1, _baseAttackDamage + _attackDamageBonusValue);
        _maxHealth = Mathf.Max(1, _baseMaxHealth + _maxHealthBonusValue);
        _maxSkillGauge = Mathf.Max(1, _baseMaxSkillGauge + _maxSkillGaugeBonusValue);
        _maxChaosGauge = Mathf.Max(1, _baseMaxChaosGauge + _maxChaosGaugeBonusValue);
        _isCalculatedStats = true;
        OnStatsChanged?.Invoke();
    }

    public void SetArtifactBonus(
        float jumpValueBonusPercent,
        float walkSpeedBonusPercent,
        float runSpeedBonusPercent,
        float crouchSpeedBonusPercent,
        int attackDamageBonus,
        int maxHealthBonus,
        int maxSkillGaugeBonus,
        int maxChaosGaugeBonus)
    {
        _jumpValueBonusValue = jumpValueBonusPercent;
        _walkSpeedBonusValue = walkSpeedBonusPercent;
        _runSpeedBonusValue = runSpeedBonusPercent;
        _crouchSpeedBonusValue = crouchSpeedBonusPercent;
        _attackDamageBonusValue = attackDamageBonus;
        _maxHealthBonusValue = maxHealthBonus;
        _maxSkillGaugeBonusValue = maxSkillGaugeBonus;
        _maxChaosGaugeBonusValue = maxChaosGaugeBonus;
        RecalculateStats();
    }

    private void EnsureStatsCalculated()
    {
        if (!_isCalculatedStats)
        {
            RecalculateStats();
        }
    }
}
