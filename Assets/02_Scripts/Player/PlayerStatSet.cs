using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatSet : SingletonBehaviour<PlayerStatSet>
{
    [Header("Base Stats")]
    [SerializeField, Min(0f)] private float _baseJumpValue = 15f;
    [SerializeField, Min(0f)] private float _baseWalkSpeed = 4f;
    [SerializeField, Min(0f)] private float _baseRunSpeed = 6f;
    [SerializeField, Min(0f)] private float _baseCrouchSpeed = 2f;
    [SerializeField, Min(0f)] private float _baseLadderSpeed = 3f;
    [SerializeField, Min(1)] private int _baseMaxDashChance = 1;
    [SerializeField, Min(1)] private int _baseAttackDamage = 1;
    [SerializeField, Min(1)] private int _baseMaxHealth = 5;
    [SerializeField, Min(1)] private int _baseMaxSkillGauge = 100;
    [SerializeField, Min(1)] private int _baseMaxChaosGauge = 10;

    private Dictionary<PlayerStatType, PlayerStat> _stats;

    public bool ChaosGaugeOver { get; private set; } = false;

    public float JumpValue => GetFloat(PlayerStatType.JumpValue);
    public float WalkSpeed => GetFloat(PlayerStatType.WalkSpeed);
    public float RunSpeed => GetFloat(PlayerStatType.RunSpeed);
    public float CrouchSpeed => GetFloat(PlayerStatType.CrouchSpeed);
    public float LadderSpeed => GetFloat(PlayerStatType.LadderSpeed);

    public int MaxDashChance => GetInt(PlayerStatType.MaxDashChance);
    public int AttackDamage => GetInt(PlayerStatType.AttackDamage);
    public int MaxHealth => GetInt(PlayerStatType.MaxHealth);
    public int MaxSkillGauge => GetInt(PlayerStatType.MaxSkillGauge);
    public int MaxChaosGauge => GetInt(PlayerStatType.MaxChaosGauge);

    public event Action OnStatsChanged;

    private Dictionary<PlayerStatType, PlayerStat> Stats
    {
        get
        {
            if (_stats == null)
            {
                _stats = BuildStats();
            }

            return _stats;
        }
    }

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
        OnStatsChanged?.Invoke();
    }

    public void ApplyArtifactBonus(PlayerStatType statType, float bonusValue)
    {
        Stats[statType].AddBonus(bonusValue);
        OnStatsChanged?.Invoke();
    }

    public void ApplyArtifactBonus(string artifactId, float bonusValue)
    {
        if (!TryParseStatType(artifactId, out PlayerStatType statType))
        {
            Logger.LogWarning($"ApplyArtifactBonus: 알 수 없는 ID입니다. ({artifactId})");
            return;
        }

        ApplyArtifactBonus(statType, bonusValue);
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
        Stats[PlayerStatType.JumpValue].SetBonus(jumpValueBonusPercent);
        Stats[PlayerStatType.WalkSpeed].SetBonus(walkSpeedBonusPercent);
        Stats[PlayerStatType.RunSpeed].SetBonus(runSpeedBonusPercent);
        Stats[PlayerStatType.CrouchSpeed].SetBonus(crouchSpeedBonusPercent);
        Stats[PlayerStatType.LadderSpeed].SetBonus(ladderSpeedBonusPercent);
        Stats[PlayerStatType.MaxDashChance].SetBonus(maxDashChanceBonus);
        Stats[PlayerStatType.AttackDamage].SetBonus(attackDamageBonus);
        Stats[PlayerStatType.MaxHealth].SetBonus(maxHealthBonus);
        Stats[PlayerStatType.MaxSkillGauge].SetBonus(maxSkillGaugeBonus);
        Stats[PlayerStatType.MaxChaosGauge].SetBonus(maxChaosGaugeBonus);

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

    public static bool TryParseStatType(string id, out PlayerStatType statType)
    {
        return Enum.TryParse(id, false, out statType) && statType.ToString() == id;
    }

    private float GetFloat(PlayerStatType statType)
    {
        return Stats[statType].Value;
    }

    private int GetInt(PlayerStatType statType)
    {
        return Mathf.RoundToInt(Stats[statType].Value);
    }

    private Dictionary<PlayerStatType, PlayerStat> BuildStats()
    {
        return new Dictionary<PlayerStatType, PlayerStat>
        {
            { PlayerStatType.JumpValue,     new PlayerStat(StatMode.Multiplier, () => _baseJumpValue) },
            { PlayerStatType.WalkSpeed,     new PlayerStat(StatMode.Multiplier, () => _baseWalkSpeed) },
            { PlayerStatType.RunSpeed,      new PlayerStat(StatMode.Multiplier, () => _baseRunSpeed) },
            { PlayerStatType.CrouchSpeed,   new PlayerStat(StatMode.Multiplier, () => _baseCrouchSpeed) },
            { PlayerStatType.LadderSpeed,   new PlayerStat(StatMode.Multiplier, () => _baseLadderSpeed) },
            { PlayerStatType.MaxDashChance, new PlayerStat(StatMode.Flat, () => _baseMaxDashChance) },
            { PlayerStatType.AttackDamage,  new PlayerStat(StatMode.Flat, () => _baseAttackDamage) },
            { PlayerStatType.MaxHealth,     new PlayerStat(StatMode.Flat, () => _baseMaxHealth) },
            { PlayerStatType.MaxSkillGauge, new PlayerStat(StatMode.Flat, () => _baseMaxSkillGauge) },
            { PlayerStatType.MaxChaosGauge, new PlayerStat(StatMode.Flat, () => _baseMaxChaosGauge) },
        };
    }
}