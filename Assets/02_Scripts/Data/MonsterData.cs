using System;
using UnityEngine;

[Serializable]
public class MonsterData : BaseData
{
    public string Name;
    public int MaxHp;
    public int MoveSpeed;
    public int AttackDamage;
    public int AttackPatternCount;
    public string AttackBoxSize;
    public string AttackBoxOffset;
    public int IsOnlyUseSkill;
    public int IsAssassin;
    public string MonsterType;
    public float AttackCoolDown;
    public float AttackDistance;
    public float PushbackAmount;

    public bool GetIsOnlyUseSkill()
    {
        TryParseBool(IsOnlyUseSkill, out bool value);
        return value;
    }

    public bool GetIsAssassin()
    {
        TryParseBool(IsAssassin, out bool value);
        return value;
    }

    public Vector2 GetAttackBoxSize()
    {
        TryParseVector2(AttackBoxSize, out Vector2 value);

        return value;
    }

    public Vector2 GetAttackBoxOffset()
    {
        TryParseVector2(AttackBoxOffset, out Vector2 value);
        return value;
    }

    public MonsterType GetMonsterType()
    {
        TryParseMonsterType(MonsterType, out MonsterType value);
        return value;
    }

    private Vector2 TryParseVector2(string text, out Vector2 value)
    {
        value = Vector2.zero;
        if (string.IsNullOrWhiteSpace(text))
        {
            return value;
        }

        text = text.Replace("(", "").Replace(")", "").Trim();
        string[] splitValues = text.Split(',');

        if (splitValues.Length == 2)
        {
            if (float.TryParse(splitValues[0], out float x) && float.TryParse(splitValues[1], out float y))
            {
                value = new Vector2(x, y);
                return value;
            }
        }

        Logger.LogError($"[MonsterData]: Vector2 파싱 실패: 올바르지 않은 형식입니다. {text}");
        return value;
    }

    private bool TryParseBool(int key, out bool value)
    {
        if (key == 0)
        {
            value = false;
        }

        else if (key == 1)
        {
            value = true;
        }

        else
        {
            Logger.LogError($"[MonsterData]: Bool 파싱 실패: 올바르지 않은 형식입니다. {key}");
            value = false;
        }

        return value;
    }

    private MonsterType TryParseMonsterType(string text, out MonsterType value)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            value = global::MonsterType.None;
            return value;
        }

        if (Enum.TryParse(MonsterType, true, out MonsterType result))
        {
            value = result;
        }

        else
        {
            Logger.LogError($"[MonsterData]: Enum 파싱 실패: 올바르지 않은 형식입니다.{text}");
            value = global::MonsterType.None;
        }

        return value;
    }
}
