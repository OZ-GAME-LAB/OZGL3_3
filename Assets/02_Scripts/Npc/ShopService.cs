using System;
using System.Collections.Generic;
using System.Linq;

public enum ShopPurchaseResult
{
    Purchased,
    UnknownItem,
    AlreadyPurchased,
    AlreadyOwned,
    InsufficientCurrency
}

public class ShopService
{
    private readonly DataTableManager _tables;
    private readonly UserShopData _shopData;
    private readonly UserInventoryData _inventoryData;
    private readonly UserArtifactData _artifactData;

    public int Currency => _inventoryData.Currency;
    public ShopData SelectedItem { get; private set; }
    public IEnumerable<ShopData> AvailableItems => _tables.Shop.Values
        .Where(item => !_shopData.PurchasedShopIds.Contains(item.Id))
        .OrderBy(item => item.Id, StringComparer.Ordinal);

    public event Action OnChanged;

    public ShopService(DataTableManager tables, UserDataManager userData)
    {
        _tables = tables;
        _shopData = userData.GetUserData<UserShopData>();
        _inventoryData = userData.GetUserData<UserInventoryData>();
        _artifactData = userData.GetUserData<UserArtifactData>();
    }

    public bool Select(string shopId)
    {
        if (!_tables.Shop.TryGetValue(shopId, out ShopData item) || _shopData.PurchasedShopIds.Contains(shopId))
        {
            return false;
        }

        SelectedItem = item;
        OnChanged?.Invoke();
        return true;
    }

    public ShopPurchaseResult TryPurchase(string shopId)
    {
        if (!_tables.Shop.TryGetValue(shopId, out ShopData item))
        {
            return ShopPurchaseResult.UnknownItem;
        }

        if (_shopData.PurchasedShopIds.Contains(shopId))
        {
            return ShopPurchaseResult.AlreadyPurchased;
        }

        bool isArtifact = _tables.Artifact.ContainsKey(item.ItemId);
        if (isArtifact && _artifactData.OwnedArtifactIds.Contains(item.ItemId))
        {
            return ShopPurchaseResult.AlreadyOwned;
        }

        if (item.Price < 0)
        {
            throw new InvalidOperationException($"상점 가격이 음수입니다: {shopId}");
        }

        if (Currency < item.Price)
        {
            return ShopPurchaseResult.InsufficientCurrency;
        }

        _inventoryData.Currency -= item.Price;
        _shopData.PurchasedShopIds.Add(shopId);
        if (isArtifact)
        {
            _artifactData.OwnedArtifactIds.Add(item.ItemId);
        }
        else
        {
            _inventoryData.AddItem(item.ItemId, 1);
        }

        if (SelectedItem?.Id == shopId)
        {
            SelectedItem = null;
        }

        _shopData.SaveData();
        if (item.Price > 0 || !isArtifact)
        {
            _inventoryData.SaveData();
        }

        if (isArtifact)
        {
            _artifactData.SaveData();
        }

        OnChanged?.Invoke();
        GameManager.Sound.PlaySFX(Const.Audio.PURCHASE);
        return ShopPurchaseResult.Purchased;
    }
}
