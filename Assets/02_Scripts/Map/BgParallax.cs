
using UnityEngine;

public class BgParallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    [SerializeField]
    private Vector2 Vector2_ParallaxEffect =
        new Vector2(0.2f, 0.15f);

    private Transform _transformCamera;
    private Vector3 _vector3StartPosition;
    private Vector3 _vector3CameraStartPosition;

    private void Awake()
    {
        _vector3StartPosition = transform.position;

        Camera cameraMain = Camera.main;

        if (cameraMain == null)
        {
            Logger.LogError("BgParallax: Main Camera를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        _transformCamera = cameraMain.transform;
        _vector3CameraStartPosition = _transformCamera.position;
    }

    private void LateUpdate()
    {
        if (_transformCamera == null)
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
