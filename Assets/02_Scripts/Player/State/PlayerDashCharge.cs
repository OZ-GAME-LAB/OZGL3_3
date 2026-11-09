using System;
using UnityEngine;

public class PlayerDashCharge
{
    private float _coolDuration;

    private bool _initialized;
    private float _rechargeTime;
    private int _lastMax;

    public int CurrentDashCharge { get; private set; }
    public int MaxDashCharge => Mathf.Max(1, PlayerStatSet.Instance.MaxDashChance);
    public bool HasDashCharge => CurrentDashCharge > 0;

    public event Action<int, int, bool> DashChanceChanged;

    public PlayerDashCharge(float coolDuration)
    {
        _coolDuration = coolDuration;
    }

    public void SetCoolDuration(float coolDuration)
    {
        float nextCoolDuration = Mathf.Max(0.01f, coolDuration);
        if (_rechargeTime > Time.time && _coolDuration > 0f)
        {
            float remainingRatio =
                (_rechargeTime - Time.time) / _coolDuration;
            _rechargeTime =
                Time.time + remainingRatio * nextCoolDuration;
        }

        _coolDuration = nextCoolDuration;
    }

    public void ConsumeDashCharge()
    {
        if (CurrentDashCharge == _lastMax)
        {
            _rechargeTime = Time.time + _coolDuration;
        }

        CurrentDashCharge--;
        NotifyDashCharge(true);
    }

    public void UpdateDashCharge()
    {
        int previous = CurrentDashCharge;
        int previousMax = _lastMax;
        int max = MaxDashCharge;

        if (!_initialized)
        {
            CurrentDashCharge = max;
            _lastMax = max;
            _initialized = true;
            NotifyDashCharge(false);
            return;
        }

        if (max > _lastMax)
        {
            CurrentDashCharge = Mathf.Min(max, CurrentDashCharge + max - _lastMax);
        }
        else if (max < _lastMax)
        {
            CurrentDashCharge = Mathf.Min(CurrentDashCharge, max);
        }

        _lastMax = max;

        if (CurrentDashCharge >= max)
        {
            _rechargeTime = 0f;
        }
        else if (_rechargeTime <= 0f)
        {
            _rechargeTime = Time.time + _coolDuration;
        }
        else if (Time.time >= _rechargeTime)
        {
            CurrentDashCharge++;
            _rechargeTime = CurrentDashCharge < max
                ? Time.time + _coolDuration
                : 0f;
        }

        if (CurrentDashCharge != previous || max != previousMax)
        {
            NotifyDashCharge(false);
        }
    }

    private void NotifyDashCharge(bool wasUsed)
    {
        DashChanceChanged?.Invoke(CurrentDashCharge, MaxDashCharge, wasUsed);
    }
}