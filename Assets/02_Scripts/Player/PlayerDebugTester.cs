using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDebugTester : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] private int _testDamage = 1;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        PlayerStatus status = PlayerStatus.Instance;

        if (keyboard == null || status == null)
        {
            return;
        }
        // K: 자살치트키, J: 자해치트키, H: 자힐치트키
        if (keyboard.kKey.wasPressedThisFrame)
        {
            status.TakeDamage(status.MaxHealth * 2);
            Logger.Log($"[Debug] 플레이어사망 치트키 (HP: {status.CurrentHealth}/{status.MaxHealth})");
        }

        if (keyboard.jKey.wasPressedThisFrame)
        {
            status.TakeDamage(_testDamage);
            Logger.Log($"[Debug] 셀프 데미지 치트키 {_testDamage} (HP: {status.CurrentHealth}/{status.MaxHealth})");
        }

        if (keyboard.hKey.wasPressedThisFrame)
        {
            status.Heal(1);
            Logger.Log($"[Debug] 회복 치트키 (HP: {status.CurrentHealth}/{status.MaxHealth})");
        }

        // 1: 최대 체력 +1, 2: 이동 속도 +50%, 3: 대시 횟수 +1, 4: 공격력 +1
        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            PlayerStatSet.Instance.ApplyArtifactBonus(PlayerStatType.MaxHealth, 1f);
            Logger.Log($"[Debug] MaxHealth = {PlayerStatSet.Instance.MaxHealth}");
        }

        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            PlayerStatSet.Instance.ApplyArtifactBonus(PlayerStatType.WalkSpeed, 0.5f);
            Logger.Log($"[Debug] WalkSpeed = {PlayerStatSet.Instance.WalkSpeed}");
        }

        if (keyboard.digit3Key.wasPressedThisFrame)
        {
            PlayerStatSet.Instance.ApplyArtifactBonus(PlayerStatType.MaxDashChance, 1f);
            Logger.Log($"[Debug] MaxDashChance = {PlayerStatSet.Instance.MaxDashChance}");
        }

        if (keyboard.digit4Key.wasPressedThisFrame)
        {
            PlayerStatSet.Instance.ApplyArtifactBonus($"{PlayerStatType.AttackDamage}", 1f);
            Logger.Log($"[Debug] AttackDamage = {PlayerStatSet.Instance.AttackDamage}");
        }
    }
#endif
}