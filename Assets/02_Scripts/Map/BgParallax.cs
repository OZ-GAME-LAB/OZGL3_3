using UnityEngine;
using Cysharp.Threading.Tasks;

public class BgParallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    [SerializeField]
    private Vector2 Vector2_ParallaxEffect =
        new Vector2(0.2f, 0.15f);

    private Transform _transformCamera;
    private Vector3 _vector3StartPosition;
    private Vector3 _vector3CameraStartPosition;

    private bool _isParallaxReady;

    private void Awake()
    {
        // 배경의 원래 위치를 보존
        _vector3StartPosition = transform.position;

        Camera cameraMain = Camera.main;

        if (cameraMain == null)
        {
            Logger.LogError(
                "BgParallax: Main Camera를 찾을 수 없습니다.");

            enabled = false;
            return;
        }

        _transformCamera = cameraMain.transform;
    }

    private void OnEnable()
    {
        if (_transformCamera == null)
        {
            return;
        }

        // 이전 패럴랙스 이동으로 밀린 배경을 원래 위치로 복구
        transform.position = _vector3StartPosition;

        // 카메라 기준점을 다시 설정할 때까지 이동 계산 중지
        _isParallaxReady = false;

        DoResetCameraStartPositionAsync().Forget();
    }

    private async UniTaskVoid DoResetCameraStartPositionAsync()
    {
        // 플레이어 이동 및 카메라 갱신이 진행된 프레임의 끝까지 대기
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        if (this == null ||
            !isActiveAndEnabled ||
            _transformCamera == null)
        {
            return;
        }

        // 새 맵 기준 카메라 위치 저장
        _vector3CameraStartPosition = _transformCamera.position;

        _isParallaxReady = true;
    }

    private void OnDisable()
    {
        _isParallaxReady = false;

        // 다음 활성화를 위해 배경 위치 복구
        transform.position = _vector3StartPosition;
    }

    private void LateUpdate()
    {
        if (!_isParallaxReady || _transformCamera == null)
        {
            return;
        }

        Vector3 vector3CameraOffset =
            _transformCamera.position - _vector3CameraStartPosition;

        transform.position = _vector3StartPosition + new Vector3(
            vector3CameraOffset.x * Vector2_ParallaxEffect.x,
            vector3CameraOffset.y * Vector2_ParallaxEffect.y,
            0f
        );
    }
}