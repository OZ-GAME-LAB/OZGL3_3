using System;
using UnityEngine;

public class PlayerStatus : SingletonBehaviour<PlayerStatus>
{
    public int MaxHealth => PlayerStats.Instance.MaxHealth;
    public int MaxSkillGauge => PlayerStats.Instance.MaxSkillGauge;
    
    public int CurrentHealth { get; private set; }
    public int CurrentSkillGauge { get; private set; }

    public bool IsDead => CurrentHealth <= 0;
    public bool IsBlocking { get; private set; }

    private int _lastMaxHealth;
    private int _lastMaxSkillGauge;

    private float _blockDamageReduction = 1f;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnSkillGaugeChanged;
    public event Action OnDied;
    public event Action OnBlock;

    private void Awake()
    {
        Init();
        _lastMaxHealth = MaxHealth;
        _lastMaxSkillGauge = MaxSkillGauge;
        CurrentHealth = MaxHealth;
    }

    private void OnEnable()
    {
        PlayerStats.Instance.OnStatsChanged += HandleStatsChanged;
    }

    private void OnDisable()
    {
        PlayerStats.Instance.OnStatsChanged -= HandleStatsChanged;
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

    // TODO(이주헌): Vcetor2 attackPosition 인자를 넣어서 방향에 따라 block판정을 넣을 예정
    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead)
        {
            return;
        }

        int finalDamage = GetChaosGaugeOverDamage(damage);

        if (IsBlocking)
        {
            finalDamage = Mathf.CeilToInt(finalDamage * (1f - _blockDamageReduction));
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
    }

    public void SetBlocking(bool isBlocking)
    {
        IsBlocking = isBlocking;
    }

    public int GetChaosGaugeOverDamage(int damage)
    {
        return PlayerStats.Instance.ChaosGaugeOver ? damage * 2 : damage;
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        SetHealth(CurrentHealth + amount);
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

    // TODO(이주헌): 스킬 구현 후 재개
    private void UseSkillGauge()
    {

    }

    private void GetSkillGauge()
    {

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
