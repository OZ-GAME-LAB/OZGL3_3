using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    [SerializeField] private PlayerCombatHandler _combatHandler;
    [SerializeField] private PlayerController _playerController;
    private PlayerVisualOffset _visualOffset;

    private void Awake()
    {
        _visualOffset = GetComponent<PlayerVisualOffset>();

        if (_combatHandler == null)
        {
            _combatHandler = GetComponentInParent<PlayerCombatHandler>();
        }

        if (_playerController == null)
        {
            _playerController = GetComponentInParent<PlayerController>();
        }
    }

    public void AnimationFixOffset(float offsetY)
    {
        if (_visualOffset == null)
        {
            Logger.LogError("AnimationFixOffset: PlayerVisualOffset를 찾을 수 없습니다.");
            return;
        }

        _visualOffset.SetOffsetY(offsetY);
    }

    public void AnimationCrouchEnd()
    {
        AnimationFixOffset(0f);

        if (_playerController == null)
        {
            Logger.LogError("AnimationCrouchEnd: PlayerController를 찾을 수 없습니다.");
            return;
        }

        _playerController.AnimationCrouchEnd();
    }

    public void AnimationAttackHit()
    {
        if (_combatHandler == null)
        {
            Logger.LogError("AnimationHeal: PlayerCombatHandler 찾을 수 없습니다.");
            return;
        }

        _combatHandler.AnimationAttackHit();
    }

    public void AnimationHeal()
    {
        if (_playerController == null)
        {
            Logger.LogError("AnimationHeal: PlayerController를 찾을 수 없습니다.");
            return;
        }

        _playerController.AnimationHeal();
    }

    public void AnimationHealEnd()
    {
        if (_playerController == null)
        {
            Logger.LogError("AnimationHealEnd: PlayerController를 찾을 수 없습니다.");
            return;
        }

        _playerController.AnimationHealEnd();
    }

    public void AnimationGuardedAttack()
    {
        if (_combatHandler == null)
        {
            Logger.LogError("AnimationHeal: PlayerCombatHandler 찾을 수 없습니다.");
            return;
        }

        _playerController.AnimationGuardedAttackHit();
    }

    public void AnimationGuardedAttackEnd()
    {
        if (_playerController == null)
        {
            Logger.LogError("AnimationHealEnd: PlayerController를 찾을 수 없습니다.");
            return;
        }

        _playerController.AnimationGuardedAttackHitEnd();
    }
}
