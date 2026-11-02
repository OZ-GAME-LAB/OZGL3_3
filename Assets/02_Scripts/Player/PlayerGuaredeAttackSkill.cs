using UnityEngine;

public class PlayerGuaredeAttackSkill : PlayerSkillBase
{
    private readonly PlayerCombatHandler _combatHandler;

    public PlayerGuaredeAttackSkill(
        PlayerStatus playerStatus,
        Animator animator,
        PlayerCombatHandler combatHandler)
        : base(Const.GuardedAttackSkillId, playerStatus, animator)
    {
        _combatHandler = combatHandler;
    }

    protected override bool CanUseSkill(PlayerSkillData skillData)
    {
        if ((skillData.Amount1) <= 0)
        {
            Logger.LogError($"가디드어택스킬 Amount1은 0보다 커야 합니다. ({skillData.Amount1})");
            return false;
        }

        return true;
    }

    protected override void ApplyEffect(PlayerSkillData skillData)
    {
        _combatHandler.ApplySkillAttackDamage((skillData.Amount1));
    }
}
