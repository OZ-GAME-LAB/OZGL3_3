using UnityEngine;

[DefaultExecutionOrder(10000)]
public class BgParallax : MonoBehaviour
{
    private const float FLOAT_PARALLAX_START_DELAY = 0.1f;

    [Header("Parallax Settings")]
    [SerializeField]
    private Vector2 Vector2_ParallaxEffect =
        new Vector2(0.2f, 0.15f);

    private Transform _cameraTransform;
    private MapRoot _mapRootOwner;

    private Vector3 _previousCameraPosition;
    private float _parallaxStartTime;

    private void Awake()
    {
        // 부모 계층에서 MapRoot 자동 검색
        _mapRootOwner = GetComponentInParent<MapRoot>(true);

        if (_mapRootOwner == null)
        {
            Logger.LogError(
                "BgParallax: 부모 계층에서 MapRoot를 찾을 수 없습니다.");

            enabled = false;
            return;
        }

        // Main Camera 참조
        Camera Camera_Main = Camera.main;

        if (Camera_Main == null)
        {
            Logger.LogError(
                "BgParallax: Main Camera를 찾을 수 없습니다.");

            enabled = false;
            return;
        }

        _cameraTransform = Camera_Main.transform;
    }

    private void OnEnable()
    {
        // 활성화 후 지정한 시간 동안 패럴랙스 시작 대기
        _parallaxStartTime =
            Time.time + FLOAT_PARALLAX_START_DELAY;

        if (_cameraTransform != null)
        {
            _previousCameraPosition = _cameraTransform.position;
        }
    }

    private void LateUpdate()
    {
        if (_cameraTransform == null ||
            _mapRootOwner == null ||
            !_mapRootOwner.gameObject.activeInHierarchy)
        {
            return;
        }

        Vector3 Vector3_CameraCurrentPosition =
            _cameraTransform.position;

        // 대기 중에는 배경을 이동하지 않고 카메라 좌표만 갱신
        if (Time.time < _parallaxStartTime)
        {
            _previousCameraPosition =
                Vector3_CameraCurrentPosition;

            return;
        }

        // 이전 프레임 대비 카메라 이동량 계산
        Vector3 Vector3_CameraOffset =
            Vector3_CameraCurrentPosition -
            _previousCameraPosition;

        // 카메라 이동량에 패럴랙스 비율 적용
        transform.position += new Vector3(
            Vector3_CameraOffset.x * Vector2_ParallaxEffect.x,
            Vector3_CameraOffset.y * Vector2_ParallaxEffect.y,
            0f
        );

        // 다음 프레임 계산을 위해 카메라 위치 갱신
        _previousCameraPosition =
            Vector3_CameraCurrentPosition;
    }
}