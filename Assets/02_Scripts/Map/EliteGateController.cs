using UnityEngine;
using Cysharp.Threading.Tasks;

public class EliteGateController : MonoBehaviour, IBreakable
{
    private const string STRING_PLAYER_TAG = "Player";

    [Header("Collapse Animation")]
    [SerializeField, Min(0.01f)]
    private float Float_CollapseDuration = 0.8f;

    [SerializeField, Min(0f)]
    private float Float_ShakePositionAmount = 0.025f;

    [SerializeField, Min(0f)]
    private float Float_ShakeFrequency = 22f;

    [SerializeField, Min(0f)]
    private float Float_DropDistance = 0.4f;

    private bool _isBreaking;
    private bool _isBroken;

    private Vector3 _vector3StartLocalPosition;

    private void Awake()
    {
        _vector3StartLocalPosition = transform.localPosition;
    }

    private void OnCollisionEnter2D(Collision2D Collision2D_Other)
    {
        if (IsPlayer(Collision2D_Other.collider))
        {
            Break();
        }
    }

    private void OnTriggerEnter2D(Collider2D Collider2D_Other)
    {
        if (IsPlayer(Collider2D_Other))
        {
            Break();
        }
    }

    public void Break()
    {
        if (_isBreaking || _isBroken)
        {
            return;
        }

        _isBreaking = true;

        DoCollapseAsync().Forget();
    }

    private async UniTaskVoid DoCollapseAsync()
    {
        float Float_ElapsedTime = 0f;

        while (Float_ElapsedTime < Float_CollapseDuration)
        {
            if (this == null)
            {
                return;
            }

            float Float_NormalizedTime =
                Mathf.Clamp01(
                    Float_ElapsedTime / Float_CollapseDuration);

            // 시간이 지날수록 아래로 가라앉는 거리 증가
            float Float_DropProgress =
                Float_NormalizedTime * Float_NormalizedTime;

            float Float_DropOffset =
                Float_DropDistance * Float_DropProgress;

            // 가라앉을수록 흔들림을 약하게 조정
            float Float_ShakeStrength =
                1f - Float_NormalizedTime * 0.3f;

            float Float_ShakeOffset =
                Mathf.Sin(
                    Float_ElapsedTime * Float_ShakeFrequency)
                * Float_ShakePositionAmount
                * Float_ShakeStrength;

            // 좌우 흔들림과 아래 방향 이동 적용
            transform.localPosition =
                _vector3StartLocalPosition +
                new Vector3(
                    Float_ShakeOffset,
                    -Float_DropOffset,
                    0f);

            Float_ElapsedTime += Time.deltaTime;

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        // 최종 위치를 아래쪽으로 확정
        transform.localPosition =
            _vector3StartLocalPosition +
            Vector3.down * Float_DropDistance;

        _isBreaking = false;
        _isBroken = true;

        // 무너지는 연출이 끝난 뒤 비활성화
        gameObject.SetActive(false);
    }

    private bool IsPlayer(Collider2D Collider2D_Other)
    {
        if (Collider2D_Other == null)
        {
            return false;
        }

        Transform Transform_Current =
            Collider2D_Other.transform;

        // 부모 계층까지 Player 태그 확인
        while (Transform_Current != null)
        {
            if (Transform_Current.CompareTag(STRING_PLAYER_TAG))
            {
                return true;
            }

            Transform_Current = Transform_Current.parent;
        }

        return false;
    }
}