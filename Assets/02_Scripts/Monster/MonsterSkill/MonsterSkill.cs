using UnityEngine;

public abstract class MonsterSkill : ScriptableObject
{
    protected string _skillName;
    public float CoolDown;
    public float Range;

    public abstract void UseSkill(MonsterBase caster, Transform target = null);
}