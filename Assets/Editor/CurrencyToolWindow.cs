using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CurrencyToolWindow : EditorWindow
{
    [SerializeField] private int _amount = 1000;

    private Label _currencyLabel;
    private Button _addButton;

    [MenuItem("Tools/Currency")]
    public static void Open()
    {
        CurrencyToolWindow window = GetWindow<CurrencyToolWindow>("재화 추가");
        window.minSize = new Vector2(360, 140);
    }

    public void CreateGUI()
    {
        rootVisualElement.Clear();

        _currencyLabel = new Label();
        IntegerField amountField = new("추가 수량") { value = _amount };
        amountField.RegisterValueChangedCallback(change =>
        {
            _amount = change.newValue;
            Refresh();
        });

        _addButton = new Button(AddCurrency) { text = "재화 추가" };
        _addButton.tooltip = "1 이상의 수량만 추가할 수 있으며, 보유량은 int 최댓값을 넘을 수 없습니다.";

        rootVisualElement.Add(_currencyLabel);
        rootVisualElement.Add(amountField);
        rootVisualElement.Add(_addButton);
        rootVisualElement.Add(new HelpBox(
            "플레이 모드에서 GameManager가 준비되면 사용할 수 있습니다.\n추가 즉시 모든 유저 데이터를 저장합니다.",
            HelpBoxMessageType.Info));

        rootVisualElement.schedule.Execute(Refresh).Every(100);
        Refresh();
    }

    private void Refresh()
    {
        if (!EditorApplication.isPlaying || GameManager.Instance == null)
        {
            _currencyLabel.text = "보유 재화: -";
            _addButton.SetEnabled(false);
            return;
        }

        UserInventoryData inventory = GameManager.UserData.GetUserData<UserInventoryData>();
        _currencyLabel.text = $"보유 재화: {inventory.Currency:N0}";
        _addButton.SetEnabled(_amount > 0 && (long)inventory.Currency + _amount <= int.MaxValue);
    }

    private void AddCurrency()
    {
        if (!EditorApplication.isPlaying || GameManager.Instance == null || _amount <= 0)
        {
            return;
        }

        UserDataManager userData = GameManager.UserData;
        UserInventoryData inventory = userData.GetUserData<UserInventoryData>();
        if ((long)inventory.Currency + _amount > int.MaxValue)
        {
            return;
        }

        inventory.Currency += _amount;
        bool saved = userData.SaveAllUserData();
        ShowNotification(new GUIContent(saved
            ? $"재화 {_amount:N0} 추가 및 저장 완료"
            : "재화는 추가되었지만 저장에 실패했습니다. Console을 확인하세요."));
        Refresh();
    }
}
