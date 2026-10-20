
using System;
using UnityEngine;

public class PlayerStats : SingletonBehaviour<PlayerStats>
{
    [Header("Base Stats")]
    [SerializeField, Min(0f)] private float _baseJumpValue = 5f;
    [SerializeField, Min(0f)] private float _baseWalkSpeed = 4f;
    [SerializeField, Min(0f)] private float _baseRunSpeed = 6f;
    [SerializeField, Min(0f)] private float _baseCrouchSpeed = 2f;
    [SerializeField, Min(0f)] private float _baseLadderSpeed = 3f;
    [SerializeField, Min(1)] private int _baseMaxDashChance = 1;
    [SerializeField, Min(1)] private int _baseAttackDamage = 1;
    [SerializeField, Min(1)] private int _baseMaxHealth = 5;
    [SerializeField, Min(1)] private int _baseMaxSkillGauge = 100;
    [SerializeField, Min(1)] private int _baseMaxChaosGauge = 10;

    private bool _isCalculatedStats;

    private float _jumpValueBaseBonus = 1f;
    private float _walkSpeedBaseBonus = 1f;
    private float _runSpeedBaseBonus = 1f;
    private float _crouchSpeedBaseBonus = 1f;
    private float _ladderSpeedBaseBonus = 1f;

    private float _jumpValueBonusValue;
    private float _walkSpeedBonusValue;
    private float _runSpeedBonusValue;
    private float _crouchSpeedBonusValue;
    private float _ladderSpeedBonusValue;

    private int _maxDashChance;
    private int _attackDamage;
    private int _maxHealth;
    private int _maxSkillGauge;
    private int _maxChaosGauge;

    private int _maxDashChanceBonusValue;
    private int _attackDamageBonusValue;
    private int _maxHealthBonusValue;
    private int _maxSkillGaugeBonusValue;
    private int _maxChaosGaugeBonusValue;

    public bool ChaosGaugeOver { get; private set; } = false;

    public float JumpValue{get { EnsureStatsCalculated(); return _baseJumpValue * _jumpValueBaseBonus; }}
    public float WalkSpeed{get { EnsureStatsCalculated(); return _baseWalkSpeed * _walkSpeedBaseBonus; }}
    public float RunSpeed{get { EnsureStatsCalculated(); return _baseRunSpeed * _runSpeedBaseBonus; }}
    public float CrouchSpeed{get { EnsureStatsCalculated(); return _baseCrouchSpeed * _crouchSpeedBaseBonus; }}
    public float LadderSpeed{get { EnsureStatsCalculated(); return _baseLadderSpeed * _ladderSpeedBaseBonus; }}

    public int MaxDashChance{get { EnsureStatsCalculated(); return _maxDashChance; }}
    public int AttackDamage{get { EnsureStatsCalculated(); return _attackDamage; }}
    public int MaxHealth{get { EnsureStatsCalculated(); return _maxHealth; }}
    public int MaxSkillGauge{get { EnsureStatsCalculated(); return _maxSkillGauge; }}
    public int MaxChaosGauge{get { EnsureStatsCalculated(); return _maxChaosGauge; }}
    
    public event Action OnStatsChanged;

    private void Awake()
    {
        Init();
        RecalculateStats();
    }

    private void OnEnable()
    {
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        RecalculateStat("JumpValue");
        RecalculateStat("WalkSpeed");
        RecalculateStat("RunSpeed");
        RecalculateStat("CrouchSpeed");
        RecalculateStat("LadderSpeed");
        RecalculateStat("MaxDashChance");
        RecalculateStat("AttackDamage");
        RecalculateStat("MaxHealth");
        RecalculateStat("MaxSkillGauge");
        RecalculateStat("MaxChaosGauge");

        _isCalculatedStats = true;
        OnStatsChanged?.Invoke();
    }

    private void RecalculateStat(string artifactId)
    {
        switch (artifactId)
        {
            case "JumpValue":
                _jumpValueBaseBonus = Mathf.Max(0f, 1f + _jumpValueBonusValue);
                break;

            case "WalkSpeed":
                _walkSpeedBaseBonus = Mathf.Max(0f, 1f + _walkSpeedBonusValue);
                break;

            case "RunSpeed":
                _runSpeedBaseBonus = Mathf.Max(0f, 1f + _runSpeedBonusValue);
                break;

            case "CrouchSpeed":
                _crouchSpeedBaseBonus = Mathf.Max(0f, 1f + _crouchSpeedBonusValue);
                break;

            case "LadderSpeed":
                _ladderSpeedBaseBonus = Mathf.Max(0f, 1f + _ladderSpeedBonusValue);
                break;

            case "MaxDashChance":
                _maxDashChance = Mathf.Max(1, _baseMaxDashChance + _maxDashChanceBonusValue);
                break;

            case "AttackDamage":
                _attackDamage = Mathf.Max(1, _baseAttackDamage + _attackDamageBonusValue);
                break;

            case "MaxHealth":
                _maxHealth = Mathf.Max(1, _baseMaxHealth + _maxHealthBonusValue);
                break;

            case "MaxSkillGauge":
                _maxSkillGauge = Mathf.Max(1, _baseMaxSkillGauge + _maxSkillGaugeBonusValue);
                break;

            case "MaxChaosGauge":
                _maxChaosGauge = Mathf.Max(1, _baseMaxChaosGauge + _maxChaosGaugeBonusValue);
                break;

            default:
                Logger.LogWarning($"RecalculateStats: 알 수 없는 능력치 ID입니다. ({artifactId})");
                return;
        }

        _isCalculatedStats = true;
    }

    public void ApplyArtifactBonus(string artifactId, float bonusValue)
    {
        switch (artifactId)
        {
            case "JumpValue":
                _jumpValueBonusValue += bonusValue;
                break;

            case "WalkSpeed":
                _walkSpeedBonusValue += bonusValue;
                break;

            case "RunSpeed":
                _runSpeedBonusValue += bonusValue;
                break;

            case "CrouchSpeed":
                _crouchSpeedBonusValue += bonusValue;
                break;

            case "LadderSpeed":
                _ladderSpeedBonusValue += bonusValue;
                break;

            case "MaxDashChance":
                _maxDashChanceBonusValue += Mathf.RoundToInt(bonusValue);
                break;

            case "AttackDamage":
                _attackDamageBonusValue += Mathf.RoundToInt(bonusValue);
                break;

            case "MaxHealth":
                _maxHealthBonusValue += Mathf.RoundToInt(bonusValue);
                break;

            case "MaxSkillGauge":
                _maxSkillGaugeBonusValue += Mathf.RoundToInt(bonusValue);
                break;

            case "MaxChaosGauge":
                _maxChaosGaugeBonusValue += Mathf.RoundToInt(bonusValue);
                break;

            default:
                Logger.LogWarning($"ApplyArtifactBonus: 알 수 없는 ID입니다. ({artifactId})");
                return;
        }

        RecalculateStat(artifactId);
        OnStatsChanged?.Invoke();
    }

    public void SetArtifactBonus(
        float jumpValueBonusPercent,
        float walkSpeedBonusPercent,
        float runSpeedBonusPercent,
        float crouchSpeedBonusPercent,
        float ladderSpeedBonusPercent,
        int maxDashChanceBonus,
        int attackDamageBonus,
        int maxHealthBonus,
        int maxSkillGaugeBonus,
        int maxChaosGaugeBonus)
    {
        _jumpValueBonusValue = jumpValueBonusPercent;
        _walkSpeedBonusValue = walkSpeedBonusPercent;
        _runSpeedBonusValue = runSpeedBonusPercent;
        _crouchSpeedBonusValue = crouchSpeedBonusPercent;
        _ladderSpeedBonusValue = ladderSpeedBonusPercent;
        _maxDashChanceBonusValue = maxDashChanceBonus;
        _attackDamageBonusValue = attackDamageBonus;
        _maxHealthBonusValue = maxHealthBonus;
        _maxSkillGaugeBonusValue = maxSkillGaugeBonus;
        _maxChaosGaugeBonusValue = maxChaosGaugeBonus;

        RecalculateStats();
    }

    public void SetChaosGaugeOver(bool isOver)
    {
        if (ChaosGaugeOver == isOver)
        {
            return;
        }

        ChaosGaugeOver = isOver;
        OnStatsChanged?.Invoke();
    }

    private void EnsureStatsCalculated()
    {
        if (!_isCalculatedStats)
        {
            RecalculateStats();
        }
    }
}
