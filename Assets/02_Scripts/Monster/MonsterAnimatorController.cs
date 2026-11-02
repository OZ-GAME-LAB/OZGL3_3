using UnityEngine;

public class MonsterAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator Animator_Monster;

    private bool _isAnimationPlaying { get; set; } = false;

    private MonsterState _currentAnimState;
    
    public void ChangeState(MonsterState newState)
    {
        if (newState == _currentAnimState)
        {
            return;
        }

        _currentAnimState = newState;

        ResetAllAnimParameters();

        ApplyStateParameters(_currentAnimState);
    }

    private bool HasParameter(Animator animator, string parameterName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == parameterName) return true;
        }
        return false;
    }

    private void SafeSetBool(string paramName, bool value)
    {
        if (HasParameter(Animator_Monster, paramName))
        {
            Animator_Monster.SetBool(paramName, value);
        }
    }

    private void ApplyStateParameters(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Idle:
                break;
            case MonsterState.Attack_1:
                SafeSetBool("isAttack_1", true);
                break;
            case MonsterState.Attack_2:
                SafeSetBool("isAttack_2", true);
                break;
            case MonsterState.Skill_1:
                SafeSetBool("isUseSkill_1", true);
                break;
            case MonsterState.Skill_2:
                SafeSetBool("isUseSkill_2", true);
                break;
            case MonsterState.Skill_3:
                SafeSetBool("isUseSkill_3", true);
                break;
            case MonsterState.Skill_4:
                SafeSetBool("isUseSkill_4", true);
                break;
            case MonsterState.Spawn:
                SafeSetBool("isSpawned", true);
                break;
            case MonsterState.Hit:
                SafeSetBool("isHit", true);
                break;
            case MonsterState.Forward:
                SafeSetBool("isMoved", true);
                break;
            case MonsterState.Dead:
                SafeSetBool("isDead", true);
                break;
            case MonsterState.Fall:
                SafeSetBool("isFalling", true);
                break;
        }
        _isAnimationPlaying = false;
    }

    private void ResetAllAnimParameters()
    {
        SafeSetBool("isAttack_1", false);
        SafeSetBool("isAttack_2", false);
        SafeSetBool("isUseSkill_1", false);
        SafeSetBool("isUseSkill_2", false);
        SafeSetBool("isUseSkill_3", false);
        SafeSetBool("isUseSkill_4", false);
        SafeSetBool("isSpawned", false);
        SafeSetBool("isHit", false);
        SafeSetBool("isMoved", false);
        SafeSetBool("isDead", false);
        SafeSetBool("isFalling", false);
    }
}
