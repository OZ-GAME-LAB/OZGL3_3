using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    [SerializeField] private PlayerCombatHandler _combatHandler;
    [SerializeField] private PlayerController _playerController;

    private void Awake()
    {
        if (_combatHandler == null)
        {
            _combatHandler = GetComponentInParent<PlayerCombatHandler>();
        }

        if (_playerController == null)
        {
            _playerController = GetComponentInParent<PlayerController>();
        }
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
}
