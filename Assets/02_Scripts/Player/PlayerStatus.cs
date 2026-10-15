using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerStatus : MonoBehaviour
{
    private PlayerStats _playerStats;
    private int _lastMaxHealth;
    private int _lastMaxSkillGauge;

    public int MaxHealth => _playerStats.MaxHealth;
    public int CurrentHealth { get; private set; }
    public int MaxSkillGauge => _playerStats.MaxSkillGauge;
    public int CurrentSkillGauge { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnSkillGaugeChanged;
    public event Action OnDied;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _lastMaxHealth = MaxHealth;
        _lastMaxSkillGauge = MaxSkillGauge;
        CurrentHealth = MaxHealth;
    }

    private void OnEnable()
    {
        if (_playerStats != null)
        {
            _playerStats.OnStatsChanged += HandleStatsChanged;
        }
    }

    private void OnDisable()
    {
        if (_playerStats != null)
        {
            _playerStats.OnStatsChanged -= HandleStatsChanged;
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

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead)
        {
            return;
        }

        SetHealth(CurrentHealth - damage);

        if (IsDead)
        {
            OnDied?.Invoke();
        }
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
