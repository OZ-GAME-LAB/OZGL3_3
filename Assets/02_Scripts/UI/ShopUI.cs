using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class ShopUI : NpcUIBase
{
    [SerializeField] private UIButton[] _itemButtons;
    [SerializeField] private UIButton _purchaseButton;
    [SerializeField] private TextMeshProUGUI _currencyText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    private ShopData[] _items = Array.Empty<ShopData>();

    protected override NpcType ScreenType => NpcType.Shop;

    protected override void OnEnable()
    {
        base.OnEnable();
        _purchaseButton.BindOnClickButtonEvent(Purchase);
        for (int index = 0; index < _itemButtons.Length; index++)
        {
            int slot = index;
            _itemButtons[index].BindOnClickButtonEvent(() => SelectItem(slot), true);
        }
    }

    protected override void OnDisable()
    {
        _purchaseButton.UnBindOnClickButtonEvent(Purchase);
        foreach (UIButton button in _itemButtons)
        {
            button.UnBindAllOnClickButtonEvent();
        }

        base.OnDisable();
    }

    protected override void Refresh()
    {
        _items = Npc.Shop.AvailableItems.ToArray();
        for (int index = 0; index < _itemButtons.Length; index++)
        {
            bool visible = index < _items.Length;
            _itemButtons[index].gameObject.SetActive(visible);
            if (!visible)
            {
                continue;
            }

            ShopData item = _items[index];
            string name = Npc.Tables.Artifact.TryGetValue(item.ItemId, out ArtifactData artifact)
                ? artifact.Name : item.ItemId;
            _itemButtons[index].ChangeButtonText($"{name} | {item.Price:N0}");
        }

        ShopData selected = Npc.Shop.SelectedItem;
        _currencyText.text = $"보유 재화: {Npc.Shop.Currency:N0}";
        _descriptionText.text = selected == null ? "상품을 선택하세요." : selected.Desc;
        _purchaseButton.SetInteractable(selected != null);
    }

    public void SelectItem(int index)
    {
        Npc.Shop.Select(_items[index].Id);
    }

    public void Purchase()
    {
        Npc.Shop.TryPurchase(Npc.Shop.SelectedItem.Id);
    }

    protected override void CloseWindow()
    {
        UIManager.Instance.CloseShopUI();
    }
}
