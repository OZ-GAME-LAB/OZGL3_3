using UnityEngine;

public class PlayerHealSkill : PlayerSkillBase
{
    public PlayerHealSkill(PlayerStatus playerStatus, Animator animator)
        : base(Const.HealSkillId, playerStatus, animator)
    {
    }

    protected override bool CanUseSkill(PlayerSkillData skillData)
    {
        if (PlayerStatus.CurrentHealth >= PlayerStatus.MaxHealth)
        {
            return false;
        }

        if (Mathf.RoundToInt(skillData.Amount1) <= 0)
        {
            Logger.LogError($"힐 스킬 Amount1은 0보다 커야 합니다. ({skillData.Amount1})");
            return false;
        }

        return true;
    }

    protected override void ApplyEffect(PlayerSkillData skillData)
    {
        PlayerStatus.Heal(Mathf.RoundToInt(skillData.Amount1));
    }
}
