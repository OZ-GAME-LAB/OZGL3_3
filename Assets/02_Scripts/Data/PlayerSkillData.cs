using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public class PlayerSkillData : BaseData
{
    public string Name;
    public string Type;
    public string TriggerName;
    public string StateName;
    public int needGauge;
    public float Amount1;
    public string RangeOffset;
    public string RangeSize;

    public bool TryGetRangeOffset(out Vector2 value)
    {
        return TryParseVector2(RangeOffset, out value);
    }

    public bool TryGetRangeSize(out Vector2 value)
    {
        return TryParseVector2(RangeSize, out value);
    }

    private static bool TryParseVector2(string text, out Vector2 value)
    {
        value = Vector2.zero;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string[] components = text.Split(',');
        if (components.Length != 2 ||
            !float.TryParse(components[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float x) ||
            !float.TryParse(components[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
        {
            return false;
        }

        value = new Vector2(x, y);
        return true;
    }
}
