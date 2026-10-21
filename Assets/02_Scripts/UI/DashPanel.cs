using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 대쉬 횟수: 플레이어 아래를 따라다니며, 최대 횟수만큼 칸을 만들고 남은 횟수만큼 채운다.
public class DashPanel : MonoBehaviour
{
    [SerializeField] private RectTransform Transform_DashRoot;
    [SerializeField] private Image _dashPrefab;
    [SerializeField] private Sprite _sprite_DashFull;
    [SerializeField] private Sprite _sprite_DashEmpty;
    [SerializeField] private Vector3 _worldOffset = new Vector3(0f, -0.7f, 0f);

    private readonly List<Image> _dashes = new List<Image>();
    private PlayerController _playerController;
    private RectTransform _parentRect;
    private Canvas _canvas;
    private Camera _camera;

    private void OnDisable()
    {
        DoUnsubscribe();
        _playerController = null;
    }

    public void Init(PlayerController playerController)
    {
        DoUnsubscribe();

        if (playerController == null)
        {
            Logger.LogError("DashPanel: PlayerController가 null입니다.");
            return;
        }

        _camera = playerController.GetComponentInChildren<Camera>();

        if (_camera == null)
        {
            _camera = Camera.main;
        }

        _canvas = GetComponentInParent<Canvas>().rootCanvas;
        _parentRect = Transform_DashRoot.parent as RectTransform;

        if (_camera == null || _canvas == null || _parentRect == null)
        {
            Logger.LogError("DashPanel: 카메라/캔버스/부모 RectTransform을 찾지 못했습니다.");
        }

        _playerController = playerController;
        _playerController.OnDashChargesChanged += OnChangedDashCharges;

        DoRefresh(_playerController.CurrentDashCharges, _playerController.MaxDashCharges);
    }

    // 플레이어가 움직인 뒤에 따라가도록 LateUpdate에서 위치를 맞춘다.
    private void LateUpdate()
    {
        if (_playerController == null || _camera == null || _canvas == null || _parentRect == null)
        {
            return;
        }

        Vector3 screenPoint =
            _camera.WorldToScreenPoint(_playerController.transform.position + _worldOffset);

        // Overlay면 카메라가 필요 없고, Camera 모드면 캔버스 카메라를 넘긴다.
        Camera canvasCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRect, screenPoint, canvasCamera, out Vector2 localPoint))
        {
            Transform_DashRoot.localPosition = localPoint;
            // LateUpdate 안, localPoint를 적용한 직후에 임시로 추가 (확인 후 삭제)
            if (Time.frameCount % 60 == 0)
            {
                Logger.Log($"DashPanel: player={_playerController.transform.position}, " +
                           $"screen={screenPoint}, local={localPoint}, camera={_camera.name}, " +
                           $"canvasMode={_canvas.renderMode}");
            }
        }
    }

    private void DoUnsubscribe()
    {
        if (_playerController != null)
        {
            _playerController.OnDashChargesChanged -= OnChangedDashCharges;
        }
    }

    private void OnChangedDashCharges(int current, int max, bool wasDashUsed)
    {
        DoRefresh(current, max);
    }

    private void DoRefresh(int current, int max)
    {
        while (_dashes.Count < max)
        {
            _dashes.Add(Instantiate(_dashPrefab, Transform_DashRoot));
        }

        for (int i = 0; i < _dashes.Count; i++)
        {
            bool isVisible = i < max;
            _dashes[i].gameObject.SetActive(isVisible);

            if (isVisible == true)
            {
                _dashes[i].sprite = i < current ? _sprite_DashFull : _sprite_DashEmpty;
            }
        }

        Transform_DashRoot.gameObject.SetActive(current < max);
    }
}