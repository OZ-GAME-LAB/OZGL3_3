using System;
using System.Collections;
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
    private readonly Dictionary<string, int> _variableAttackDamageBonuses =
        new Dictionary<string, int>();
    private readonly Dictionary<string, Coroutine> _attackDamageBonusTimers =
        new Dictionary<string, Coroutine>();

    public bool ChaosGaugeOver { get; private set; } = false;

    public float JumpValue => GetFloat(PlayerStatType.JumpValue);
    public float WalkSpeed => GetFloat(PlayerStatType.WalkSpeed);
    public float RunSpeed => GetFloat(PlayerStatType.RunSpeed);
    public float CrouchSpeed => GetFloat(PlayerStatType.CrouchSpeed);
    public float LadderSpeed => GetFloat(PlayerStatType.LadderSpeed);

    public int MaxDashChance => GetInt(PlayerStatType.MaxDashChance);
    public int AttackDamage
    {
        get
        {
            int variableBonus = 0;
            foreach (int bonus in _variableAttackDamageBonuses.Values)
            {
                variableBonus += bonus;
            }

            return Mathf.Max(
                1,
                GetInt(PlayerStatType.AttackDamage) + variableBonus);
        }
    }
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
        if (!Stats.ContainsKey(statType))
        {
            Logger.LogWarning($"ApplyArtifactBonus: 지원하지 않는 스탯입니다. ({statType})");
            return;
        }

        Stats[statType].AddBonus(bonusValue);
        OnStatsChanged?.Invoke();
    }

    public void SetVariableAttackDamageBonus(string artifactId, int bonus)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            Logger.LogError("임시 공격력 보너스의 artifactId가 비어 있습니다.");
            return;
        }

        if (bonus == 0)
        {
            RemoveVariableAttackDamageBonus(artifactId);
        }
        else
        {
            SetVariableAttackDamageBonus(artifactId, bonus, float.PositiveInfinity);
        }
    }

    public void SetVariableAttackDamageBonus(
        string artifactId,
        int bonus,
        float durationSeconds)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            Logger.LogError("임시 공격력 보너스의 artifactId가 비어 있습니다.");
            return;
        }

        if (bonus == 0)
        {
            RemoveVariableAttackDamageBonus(artifactId);
            return;
        }

        if (durationSeconds <= 0f || float.IsNaN(durationSeconds))
        {
            Logger.LogError(
                $"임시 공격력 보너스의 지속시간은 0보다 커야 합니다. ({durationSeconds})");
            return;
        }

        StopAttackDamageBonusTimer(artifactId);
        _variableAttackDamageBonuses[artifactId] = bonus;
        OnStatsChanged?.Invoke();

        if (!float.IsPositiveInfinity(durationSeconds))
        {
            _attackDamageBonusTimers[artifactId] =
                StartCoroutine(RemoveVariableAttackDamageBonusAfter(
                    artifactId,
                    durationSeconds));
        }
    }

    public void RemoveVariableAttackDamageBonus(string artifactId)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            Logger.LogError("임시 공격력 보너스의 artifactId가 비어 있습니다.");
            return;
        }

        StopAttackDamageBonusTimer(artifactId);
        if (_variableAttackDamageBonuses.Remove(artifactId))
        {
            OnStatsChanged?.Invoke();
        }
    }

    private IEnumerator RemoveVariableAttackDamageBonusAfter(
        string artifactId,
        float durationSeconds)
    {
        yield return new WaitForSeconds(durationSeconds);

        _attackDamageBonusTimers.Remove(artifactId);
        if (_variableAttackDamageBonuses.Remove(artifactId))
        {
            OnStatsChanged?.Invoke();
        }
    }

    private void StopAttackDamageBonusTimer(string artifactId)
    {
        if (!_attackDamageBonusTimers.TryGetValue(
                artifactId,
                out Coroutine timer))
        {
            return;
        }

        StopCoroutine(timer);
        _attackDamageBonusTimers.Remove(artifactId);
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