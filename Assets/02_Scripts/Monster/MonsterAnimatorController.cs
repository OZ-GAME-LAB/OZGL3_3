using System;
using UnityEngine;

public class MonsterAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator Animator_Monster;

    private MonsterState _currentAnimState;
    public void ChangeState(MonsterState newState)
    {
        if (newState == MonsterState.Idle && _currentAnimState == MonsterState.Idle) return;

        _currentAnimState = newState;

        switch (_currentAnimState)
        {
            case MonsterState.Idle:
                {
                    ResetAllAnimParameters();
                }
                break;
            case MonsterState.Attack_1:
                {
                    Animator_Monster.SetBool("isAttack_1", true);
                }
                break;
            case MonsterState.Attack_2:
                {
                    Animator_Monster.SetBool("isAttack_2", true);
                }
                break;
            case MonsterState.Skill_1:
                {
                    Animator_Monster.SetBool("isUseSkill_1", true);
                }
                break;
            case MonsterState.Skill_2:
                {
                    Animator_Monster.SetBool("isUseSkill_2", true);
                }
                break;
            case MonsterState.Skill_3:
                {
                    Animator_Monster.SetBool("isUseSkill_3", true);
                }
                break;
            case MonsterState.Skill_4:
                {
                    Animator_Monster.SetBool("isUseSkill_4", true);
                }
                break;
            case MonsterState.Spawn:
                {
                    Animator_Monster.SetBool("isSpawned", true);
                }
                break;
            case MonsterState.Hit:
                {
                    Animator_Monster.SetBool("isHit", true);
                }
                break;
            case MonsterState.Forward:
                {
                    Animator_Monster.SetBool("isMoved", true);
                }
                break;
            case MonsterState.Dead:
                {
                    Animator_Monster.SetBool("isDead", true);
                }
                break;
            default:
                {
                    ResetAllAnimParameters();
                }
                break;
        }
    }

    private void ResetAllAnimParameters()
    {
        Animator_Monster.SetBool("isAttack_1", false);
        Animator_Monster.SetBool("isAttack_2", false);
        Animator_Monster.SetBool("isUseSkill_1", false);
        Animator_Monster.SetBool("isUseSkill_2", false);
        Animator_Monster.SetBool("isUseSkill_3", false);
        Animator_Monster.SetBool("isUseSkill_4", false);
        Animator_Monster.SetBool("isSpawned", false);
        Animator_Monster.SetBool("isHit", false);
        Animator_Monster.SetBool("isMoved", false);
        Animator_Monster.SetBool("isDead", false);
    }
}
