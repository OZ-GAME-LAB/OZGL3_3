
using UnityEngine;

public class DamagePlatform : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private int _damageAmount = 10;

    /// <summary>
    /// Trigger 영역에 진입했을 때 호출된다.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        LogDamage(other.gameObject);
    }

    /// <summary>
    /// 접촉 대상과 설정된 데미지를 로그로 출력한다.
    /// 실제 데미지는 아직 적용하지 않는다.
    /// </summary>
    private void LogDamage(GameObject Target)
    {
        if (Target == null)
        {
            return;
        }

        int Damage = Mathf.Max(0, _damageAmount);

        Logger.Log(
            $"[DamagePlatform] {Target.name} 접촉 감지 | 데미지: {Damage}"
        );
    }
}
