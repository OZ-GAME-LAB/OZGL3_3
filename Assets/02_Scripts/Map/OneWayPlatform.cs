
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    [Header("Sprite")]
    [SerializeField] private Transform Transform_Sprite;

    [Header("Move")]
    [SerializeField] private Vector2 Vector2_MoveOffset = new Vector2(0f, 2f);
    [SerializeField] private float Float_MoveSpeed = 2f;

    private Vector3 _vector3StartPosition;
    private Vector3 _vector3TargetPosition;
    private bool _isMoving;
    private bool _isReturning;

    private void Awake()
    {
        _vector3StartPosition = Transform_Sprite.localPosition;
        _vector3TargetPosition = _vector3StartPosition + (Vector3)Vector2_MoveOffset;
    }

    public void DoOpen()
    {
        _isReturning = false;
        _isMoving = true;
    }

    public void DoClose()
    {
        _isReturning = true;
        _isMoving = true;
    }

    private void Update()
    {
        if (_isMoving == false)
        {
            return;
        }
        Logger.Log($"OneWayPlatform: Update: _isMoving={_isMoving}, _isReturning={_isReturning}");
        Vector3 Vector3_TargetPosition = _isReturning
            ? _vector3StartPosition
            : _vector3TargetPosition;

        Transform_Sprite.localPosition = Vector3.MoveTowards(
            Transform_Sprite.localPosition,
            Vector3_TargetPosition,
            Float_MoveSpeed * Time.deltaTime
        );

        if (Transform_Sprite.localPosition == Vector3_TargetPosition)
        {
            _isMoving = false;
        }
    }
}
