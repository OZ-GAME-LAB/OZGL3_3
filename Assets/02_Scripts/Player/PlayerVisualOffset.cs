using UnityEngine;

public class PlayerVisualOffset : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _moveSpeed = 0f;

    private Vector3 _baseLocalPosition;
    private float _currentOffsetY;
    private float _targetOffsetY;

    private void Awake()
    {
        _baseLocalPosition = transform.localPosition;
    }

    public void SetOffsetY(float offsetY)
    {
        _targetOffsetY = offsetY;

        if (_moveSpeed <= 0f)
        {
            _currentOffsetY = offsetY;
            Apply();
        }
    }

    private void LateUpdate()
    {
        if (_moveSpeed <= 0f || Mathf.Approximately(_currentOffsetY, _targetOffsetY))
        {
            return;
        }

        _currentOffsetY = Mathf.MoveTowards(
            _currentOffsetY,
            _targetOffsetY,
            _moveSpeed * Time.deltaTime
            );

        Apply();
    }

    private void Apply()
    {
        transform.localPosition = _baseLocalPosition + Vector3.up * _currentOffsetY;
    }
}
