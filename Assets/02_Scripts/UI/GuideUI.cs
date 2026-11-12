using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 조작 방법(가이드) 화면. 프리팹에서 디자인하고, 클릭하거나 ESC 를 누르면 닫힌다.
/// 일시정지(timeScale 0) 중에도 동작하도록 입력만 사용한다.
/// </summary>
public class GuideUI : MonoBehaviour
{
    [SerializeField] private Button _closeButton;

    /// <summary>가이드가 열려 있는지. MainHUD 의 ESC 처리에서 확인한다.</summary>
    public static bool IsOpen { get; private set; }

    private bool _closeRequested;

    private void OnEnable()
    {
        IsOpen = true;
        _closeRequested = false;

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(RequestClose);
        }
    }

    private void OnDisable()
    {
        IsOpen = false;

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(RequestClose);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) == true)
        {
            RequestClose();
        }
    }

    private void LateUpdate()
    {
        // 같은 프레임의 MainHUD ESC 처리가 끝난 뒤에 닫는다.
        if (_closeRequested == true)
        {
            Destroy(gameObject);
        }
    }

    private void RequestClose()
    {
        _closeRequested = true;
    }
}
