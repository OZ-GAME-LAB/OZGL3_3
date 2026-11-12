using UnityEngine;

/// <summary>
/// PauseUI 프리팹의 "조작 방법" 버튼을 누르면 가이드 프리팹을 연다.
/// (PauseUI 코드는 건드리지 않고 프리팹에서만 연결한다.)
/// </summary>
public class GuideOpener : MonoBehaviour
{
    [SerializeField] private UIButton _button;
    [SerializeField] private GameObject _guidePrefab;
    [SerializeField] private Transform _parent;

    private void OnEnable()
    {
        if (_button != null)
        {
            _button.BindOnClickButtonEvent(Open);
        }
    }

    private void OnDisable()
    {
        if (_button != null)
        {
            _button.UnBindAllOnClickButtonEvent();
        }
    }

    private void Open()
    {
        if (_guidePrefab == null || GuideUI.IsOpen == true)
        {
            return;
        }

        Transform parent = _parent != null ? _parent : transform.parent;
        Instantiate(_guidePrefab, parent, false);
    }
}
