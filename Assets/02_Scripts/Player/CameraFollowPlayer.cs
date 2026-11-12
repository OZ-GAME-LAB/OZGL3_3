using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    private const float BoundsRetryInterval = 0.25f;

    [SerializeField] private Transform _target;
    [SerializeField] private Collider2D _cameraTargetBounds;

    private readonly List<Collider2D> _overlapResults =
        new List<Collider2D>(10);

    private Camera _camera;
    private ContactFilter2D _overlapFilter;
    private float _boundsRetryTimer;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        _overlapFilter = new ContactFilter2D();
        _overlapFilter.useTriggers = true;
        _overlapFilter.SetLayerMask(Physics2D.AllLayers);
    }

    private void OnEnable()
    {
        MapChange.OnAfterMapChange += RefreshBoundsAfterMapChange;
        RefreshBounds();
    }

    private void OnDisable()
    {
        MapChange.OnAfterMapChange -= RefreshBoundsAfterMapChange;
    }

    private void RefreshBoundsAfterMapChange()
    {
        MapRoot mapRoot = FindAnyObjectByType<MapRoot>();
        if (mapRoot == null)
        {
            Logger.LogError("CameraFollowPlayer: 활성화된 MapRoot를 찾을 수 없습니다.");
            _cameraTargetBounds = null;
            return;
        }

        AreaZone areaZone = mapRoot.GetComponentInChildren<AreaZone>();
        if (areaZone == null || areaZone.ColliderBounds == null)
        {
            Logger.LogError(
                $"CameraFollowPlayer: {mapRoot.name}에서 CameraBounds의 AreaZone Collider를 찾을 수 없습니다.");
            _cameraTargetBounds = null;
            return;
        }

        _cameraTargetBounds = areaZone.ColliderBounds;
    }

    private void RefreshBounds()
    {
        _cameraTargetBounds = null;
        if (_target == null)
        {
            return;
        }

        Physics2D.SyncTransforms();

        int count = Physics2D.OverlapPoint(
            _target.position,
            _overlapFilter,
            _overlapResults);

        for (int i = 0; i < count; i++)
        {
            Collider2D collider = _overlapResults[i];

            if (collider.TryGetComponent<AreaZone>(out _))
            {
                _cameraTargetBounds = collider;
                break;
            }
        }
    }

    private void LateUpdate()
    {
        if (_target == null || _camera == null)
        {
            return;
        }

        if (_cameraTargetBounds == null)
        {
            _boundsRetryTimer -= Time.unscaledDeltaTime;
            if (_boundsRetryTimer <= 0f)
            {
                _boundsRetryTimer = BoundsRetryInterval;
                RefreshBounds();
            }
        }

        Vector3 pos = _target.position;
        pos.z = transform.position.z;

        if (_cameraTargetBounds != null)
        {
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            Bounds bounds = _cameraTargetBounds.bounds;

            pos.x = ClampCamera(
                pos.x,
                bounds.min.x + halfWidth,
                bounds.max.x - halfWidth,
                bounds.center.x);
            pos.y = ClampCamera(
                pos.y,
                bounds.min.y + halfHeight,
                bounds.max.y - halfHeight,
                bounds.center.y);
        }

        transform.position = pos;
    }

    private static float ClampCamera(float value, float min, float max, float center)
    {
        if (min > max)
        {
            return center;
        }

        return Mathf.Clamp(value, min, max);
    }

    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
        RefreshBounds();
    }
}
