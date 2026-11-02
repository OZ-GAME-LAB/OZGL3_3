using System;
using UnityEngine;

public class PlayerDashCharge
{
    private readonly float _coolDuration;

    private bool _initialized;
    private float _rechargeTime;
    private int _lastMax;

    public int Current { get; private set; }
    public int Max => Mathf.Max(1, PlayerStatSet.Instance.MaxDashChance);
    public bool HasCharge => Current > 0;

    public event Action<int, int, bool> Changed;

    public PlayerDashCharge(float coolDuration)
    {
        _coolDuration = coolDuration;
    }

    public void Consume()
    {
        if (Current == _lastMax)
        {
            _rechargeTime = Time.time + _coolDuration;
        }

        Current--;
        Notify(true);
    }

    public void Tick()
    {
        int previous = Current;
        int previousMax = _lastMax;
        int max = Max;

        if (!_initialized)
        {
            Current = max;
            _lastMax = max;
            _initialized = true;
            Notify(false);
            return;
        }

        if (max > _lastMax)
        {
            Current = Mathf.Min(max, Current + max - _lastMax);
        }
        else if (max < _lastMax)
        {
            Current = Mathf.Min(Current, max);
        }

        _lastMax = max;

        if (Current >= max)
        {
            _rechargeTime = 0f;
        }
        else if (_rechargeTime <= 0f)
        {
            _rechargeTime = Time.time + _coolDuration;
        }
        else if (Time.time >= _rechargeTime)
        {
            Current++;
            _rechargeTime = Current < max
                ? Time.time + _coolDuration
                : 0f;
        }

        if (Current != previous || max != previousMax)
        {
            Notify(false);
        }
    }

    private void Notify(bool wasUsed)
    {
        Changed?.Invoke(Current, Max, wasUsed);
    }
}