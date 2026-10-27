using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
public class LadderZone : MonoBehaviour
{
    [Header("Drop Through")]
    [SerializeField, Min(0.01f)]
    private float _dropThroughDuration = 0.35f;

    private Collider2D _collider2D_ExitPlatform;
    private Coroutine _coroutineDropThrough;

    private void Awake()
    {
        TilemapCollider2D Collider2D_Ladder = GetComponent<TilemapCollider2D>();
        Collider2D_Ladder.isTrigger = true;

        Transform Transform_LadderExitPlatform =
            transform.Find("LadderExitPlatform");

        if (Transform_LadderExitPlatform == null)
        {
            Logger.LogError(
                "LadderZone: 자식 LadderExitPlatform을 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        _collider2D_ExitPlatform =
            Transform_LadderExitPlatform.GetComponent<Collider2D>();

        if (_collider2D_ExitPlatform == null)
        {
            Logger.LogError(
                "LadderZone: LadderExitPlatform에 Collider2D가 없습니다.");
            enabled = false;
        }
    }

    /// <summary>
    /// 호출하면 출구 발판 Collider를 잠시 끈 뒤 자동으로 다시 켠다.
    /// PlayerController에서는 DropThrough() 한 줄만 호출하면 된다.
    /// </summary>
    public void DropThrough()
    {
        if (_collider2D_ExitPlatform == null)
        {
            Logger.LogError("LadderZone: 출구 발판 Collider가 없습니다.");
            return;
        }

        if (_coroutineDropThrough != null)
        {
            StopCoroutine(_coroutineDropThrough);
        }

        _coroutineDropThrough = StartCoroutine(DropThroughRoutine());
    }

    private IEnumerator DropThroughRoutine()
    {
        _collider2D_ExitPlatform.enabled = false;

        yield return new WaitForSeconds(_dropThroughDuration);

        if (_collider2D_ExitPlatform != null)
        {
            _collider2D_ExitPlatform.enabled = true;
        }

        _coroutineDropThrough = null;
    }

    private void OnDisable()
    {
        if (_coroutineDropThrough != null)
        {
            StopCoroutine(_coroutineDropThrough);
            _coroutineDropThrough = null;
        }

        if (_collider2D_ExitPlatform != null)
        {
            _collider2D_ExitPlatform.enabled = true;
        }
    }
}
