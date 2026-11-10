using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : SingletonBehaviour<PlayerStatus>
{
    public int MaxHealth => _playerStatSet.MaxHealth;
    public int MaxSkillGauge => _playerStatSet.MaxSkillGauge;
    
    public int CurrentHealth { get; private set; }
    public int CurrentSkillGauge { get; private set; }

    public bool IsDead => CurrentHealth <= 0;
    public bool IsBlocking { get; private set; }

    private int _lastMaxHealth;
    private int _lastMaxSkillGauge;

    private float _blockDamageReduction = 1f;
    private SpriteRenderer _spriteRenderer_Player;
    private PlayerStatSet _playerStatSet;
    private readonly Dictionary<string, float> _skillGaugeCostDecrease =
        new Dictionary<string, float>();

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnSkillGaugeChanged;
    public event Action<Vector2?, Vector2?> OnDamaged;
    public event Action OnDied;
    public event Action OnBlock;

    private void Awake()
    {
        Init();

        _playerStatSet = GetComponent<PlayerStatSet>();
        if (_playerStatSet == null)
        {
            Logger.LogError("PlayerStatus: 같은 Player 오브젝트에서 PlayerStatSet을 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        _spriteRenderer_Player = GetComponentInChildren<SpriteRenderer>();
        _lastMaxHealth = MaxHealth;
        _lastMaxSkillGauge = MaxSkillGauge;
        CurrentHealth = MaxHealth;
    }

    private void OnEnable()
    {
        if (_playerStatSet == null)
        {
            Logger.LogError("PlayerStatus: PlayerStatSet을 찾을 수 없어 스탯 변경 이벤트를 구독하지 못했습니다.");
            return;
        }

        _playerStatSet.OnStatsChanged += HandleStatsChanged;
    }

    private void OnDisable()
    {
        if (_playerStatSet != null)
        {
            _playerStatSet.OnStatsChanged -= HandleStatsChanged;
        }
    }

    private void HandleStatsChanged()
    {
        int maxHealth = MaxHealth;
        int maxSkillGauge = MaxSkillGauge;
        int nextHealth = maxHealth > _lastMaxHealth
            ? maxHealth
            : Mathf.Clamp(CurrentHealth, 0, maxHealth);
        int nextSkillGauge = Mathf.Clamp(CurrentSkillGauge, 0, maxSkillGauge);
        bool healthChanged = CurrentHealth != nextHealth || _lastMaxHealth != maxHealth;
        bool skillGaugeChanged =
            CurrentSkillGauge != nextSkillGauge ||
            _lastMaxSkillGauge != maxSkillGauge;

        CurrentHealth = nextHealth;
        CurrentSkillGauge = nextSkillGauge;
        _lastMaxHealth = maxHealth;
        _lastMaxSkillGauge = maxSkillGauge;

        if (healthChanged)
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        if (skillGaugeChanged)
        {
            OnSkillGaugeChanged?.Invoke(CurrentSkillGauge, maxSkillGauge);
        }
    }

    public void TakeDamage(
        int damage,
        Vector2? attackPosition = null,
        Vector2? knockbackVelocity = null)
    {
        if (damage <= 0 || IsDead)
        {
            return;
        }

        int finalDamage = GetChaosGaugeOverDamage(damage);

        if (IsBlocking && IsAttackInFront(attackPosition))
        {
            finalDamage = Mathf.CeilToInt(finalDamage * (1f - _blockDamageReduction));
            OnBlock?.Invoke();

            if (finalDamage <= 0)
            {
                return;
            }
        }

        SetHealth(CurrentHealth - finalDamage);

        if (IsDead)
        {
            OnDied?.Invoke();
        }
        else
        {
            OnDamaged?.Invoke(attackPosition, knockbackVelocity);
        }
    }

    private bool IsAttackInFront(Vector2? attackPosition)
    {
        if (!attackPosition.HasValue)
        {
            return true;
        }

        if (_spriteRenderer_Player == null)
        {
            Logger.LogError("SpriteRenderer가 null입니다.");
            return false;
        }

        float directionToAttacker =
            attackPosition.Value.x - transform.position.x;

        if (Mathf.Approximately(directionToAttacker, 0f))
        {
            return true;
        }

        bool isFacingRight = !_spriteRenderer_Player.flipX;
        return isFacingRight
            ? directionToAttacker > 0f
            : directionToAttacker < 0f;
    }

    public void SetBlocking(bool isBlocking)
    {
        IsBlocking = isBlocking;
    }

    public int GetChaosGaugeOverDamage(int damage)
    {
        return _playerStatSet.ChaosGaugeOver ? damage * 2 : damage;
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        SetHealth(CurrentHealth + amount);
    }

    internal void RestoreForRevive()
    {
        IsBlocking = false;
        SetHealth(MaxHealth);
        SetSkillGauge(0);
    }

    public bool TryUseSkillGauge(int amount)
    {
        if (amount < 0)
        {
            Logger.LogError($"스킬 게이지 사용량이 음수입니다. ({amount})");
            return false;
        }

        float totalReduction = 0f;
        foreach (float reduction in _skillGaugeCostDecrease.Values)
        {
            totalReduction += reduction;
        }

        totalReduction = Mathf.Clamp01(totalReduction);
        int adjustedAmount = Mathf.CeilToInt(amount * (1f - totalReduction));

        if (CurrentSkillGauge < adjustedAmount)
        {
            return false;
        }

        SetSkillGauge(CurrentSkillGauge - adjustedAmount);
        return true;
    }

    public void SetSkillGaugeCostDecrease(string artifactId, float DecreasePercent)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            Logger.LogError("스킬 게이지 비용 감소의 artifactId가 비어 있습니다.");
            return;
        }

        if (DecreasePercent <= 0f)
        {
            _skillGaugeCostDecrease.Remove(artifactId);
            return;
        }

        _skillGaugeCostDecrease[artifactId] =
            Mathf.Clamp(DecreasePercent, 0f, 1f);
    }

    public void RemoveSkillGaugeCostReduction(string artifactId)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            Logger.LogError("스킬 게이지 비용 감소의 artifactId가 비어 있습니다.");
            return;
        }

        _skillGaugeCostDecrease.Remove(artifactId);
    }

    public void AddSkillGauge(int amount)
    {
        if (amount <= 0)
        {
            Logger.LogError($"스킬 게이지 획득량은 0보다 커야 합니다. ({amount})");
            return;
        }

        SetSkillGauge(CurrentSkillGauge + amount);
    }

    private void SetHealth(int health)
    {
        int nextHealth = Mathf.Clamp(health, 0, MaxHealth);
        if (CurrentHealth == nextHealth)
        {
            return;
        }

        CurrentHealth = nextHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private void SetSkillGauge(int SkillGauge)
    {
        int nextSkillGauge = Mathf.Clamp(SkillGauge, 0, MaxSkillGauge);
        if (CurrentSkillGauge == nextSkillGauge)
        {
            return;
        }

        CurrentSkillGauge = nextSkillGauge;
        OnSkillGaugeChanged?.Invoke(CurrentSkillGauge, MaxSkillGauge);
    }

}
