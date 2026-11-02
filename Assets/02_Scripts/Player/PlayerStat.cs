using System;
using UnityEngine;

public class PlayerStat
{
    private const float FlatMinValue = 1f;

    private readonly StatMode _mode;
    private readonly Func<float> _baseGetter;
    private float _bonus;

    public PlayerStat(StatMode mode, Func<float> baseGetter)
    {
        _mode = mode;
        _baseGetter = baseGetter;
    }

    public float Value
    {
        get
        {
            float baseValue = _baseGetter();

            return _mode == StatMode.Multiplier
                ? baseValue * Mathf.Max(0f, 1f + _bonus)
                : Mathf.Max(FlatMinValue, baseValue + _bonus);
        }
    }

    public void AddBonus(float bonus)
    {
        _bonus += _mode == StatMode.Flat ? Mathf.RoundToInt(bonus) : bonus;
    }

    public void SetBonus(float bonus)
    {
        _bonus = bonus;
    }
}