using System;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [SerializeField, Min(1)] private int _maxHealth = 5;
    [SerializeField, Min(1)] private int _maxSkillGauge = 100;

    public int MaxHealth => _maxHealth;
    public int CurrentHealth { get; private set; }
    public int MaxSkillGauge => _maxSkillGauge;
    public int CurrentSkillGauge { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnSkillGaugeChanged;
    public event Action OnDied;

    private void Awake()
    {
        CurrentHealth = _maxHealth;
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
        int nextHealth = Mathf.Clamp(health, 0, _maxHealth);
        if (CurrentHealth == nextHealth)
        {
            return;
        }

        CurrentHealth = nextHealth;
        OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
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
        int nextSkillGauge = Mathf.Clamp(SkillGauge, 0, _maxSkillGauge);
        if (CurrentHealth == nextSkillGauge)
        {
            return;
        }

        CurrentSkillGauge = nextSkillGauge;
        OnSkillGaugeChanged?.Invoke(CurrentSkillGauge, _maxSkillGauge);
    }

}
