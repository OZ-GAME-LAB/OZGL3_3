
using UnityEngine;

public class BgParallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    [SerializeField] private Transform Transform_TargetCamera;
    [SerializeField] private Vector2 Vector2_ParallaxEffect = new Vector2(0.2f, 0f);

    private Vector3 _startPosition;
    private Vector3 _cameraStartPosition;

    private void Awake()
    {
        _startPosition = transform.position;

        if (Transform_TargetCamera == null && Camera.main != null)
        {
            Transform_TargetCamera = Camera.main.transform;
        }

        if (Transform_TargetCamera != null)
        {
            _cameraStartPosition = Transform_TargetCamera.position;
        }
    }

    private void LateUpdate()
    {
        if (Transform_TargetCamera == null)
        {
            return;
        }

        Vector3 cameraOffset =
            Transform_TargetCamera.position - _cameraStartPosition;

        transform.position = _startPosition + new Vector3(
            cameraOffset.x * Vector2_ParallaxEffect.x,
            cameraOffset.y * Vector2_ParallaxEffect.y,
            0f
        );
    }
}
