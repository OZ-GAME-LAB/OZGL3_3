using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    [SerializeField]private PlayerCombatHandler _combatHandler;

    private void Awake()
    {
        if (_combatHandler == null)
        {
            _combatHandler = GetComponentInParent<PlayerCombatHandler>();
        }
    }

    public void AnimationAttackHit()
    {
        _combatHandler.AnimationAttackHit();
    }
}
