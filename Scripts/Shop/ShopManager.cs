using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Danh sách hạt giống")]
    public List<SeedData> availableSeeds = new List<SeedData>();
    
    [Header("Tham chiếu")]
    public ShopUI shopUI;
    public PlayerGoldManager playerGoldManager;
    
    [Header("Sự kiện")]
    public System.Action<SeedData> OnSeedPurchased;
    public System.Action<string> OnPurchaseFailed;
    
    private void Start()
    {
        InitializeShop();
    }
    
    private void InitializeShop()
    {
        // Khởi tạo shop với danh sách hạt giống có sẵn
        if (shopUI != null)
        {
            shopUI.SetupShop(this);
        }
    }
    
    public bool CanPurchaseSeed(SeedData seed)
    {
        if (seed == null || !seed.isUnlocked)
            return false;
            
        // Kiểm tra tiền có đủ không
        switch (seed.currencyType)
        {
            case SeedData.CurrencyType.Gold:
                return playerGoldManager != null && playerGoldManager.GetGold() >= seed.price;
            case SeedData.CurrencyType.Gems:
                // TODO: Implement gem system
                return true;
            case SeedData.CurrencyType.Coins:
                // TODO: Implement coin system
                return true;
            default:
                return false;
        }
    }
    
    public bool PurchaseSeed(SeedData seed)
    {
        if (!CanPurchaseSeed(seed))
        {
            OnPurchaseFailed?.Invoke("Không đủ tiền hoặc hạt giống chưa mở khóa!");
            return false;
        }
        
        // Trừ tiền
        switch (seed.currencyType)
        {
            case SeedData.CurrencyType.Gold:
                if (playerGoldManager != null)
                {
                    playerGoldManager.SpendGold(seed.price);
                }
                break;
            case SeedData.CurrencyType.Gems:
                // TODO: Implement gem spending
                break;
            case SeedData.CurrencyType.Coins:
                // TODO: Implement coin spending
                break;
        }
        
        // Thêm hạt giống vào túi
        AddSeedToBag(seed);
        
        // Thông báo mua thành công
        OnSeedPurchased?.Invoke(seed);
        Debug.Log($"Đã mua hạt giống: {seed.seedName} với giá {seed.price}");
        
        return true;
    }
    
    private void AddSeedToBag(SeedData seed)
    {
        if (BagManager.Instance != null)
        {
            // ✅ THÊM TIỀN TỐ "Hạt " để phân biệt hạt giống vs sản phẩm thu hoạch
            string seedItemName;
            
            // Kiểm tra xem tên đã có "Hạt" chưa để tránh duplicate
            string lowerName = seed.seedName.ToLower();
            if (lowerName.StartsWith("hạt ") || lowerName.Contains("hạt giống"))
            {
                // Đã có "Hạt" rồi → giữ nguyên
                seedItemName = seed.seedName; // Ví dụ: "Hạt Giống Cơ Bản" → giữ nguyên
            }
            else
            {
                // Chưa có "Hạt" → thêm vào
                seedItemName = "Hạt " + seed.seedName; // Ví dụ: "Bí Ngô" → "Hạt Bí Ngô"
            }
            
            // Thêm vào túi (KHÔNG cần tạo BagItem trước, AddItem sẽ tự tạo)
            bool success = BagManager.Instance.AddItem(
                seedItemName, // ✅ Dùng tên mới có "Hạt " ở đầu
                seed.seedIcon,
                1, // Số lượng
                seed.sellPrice // Giá bán
            );
            
            if (success)
            {
                Debug.Log($"✅ Đã thêm '{seedItemName}' vào túi!");
            }
            else
            {
                Debug.LogWarning($"Không thể thêm {seed.seedName} vào túi (túi đầy?)");
            }
        }
        else
        {
            Debug.LogWarning("BagManager.Instance is null! Không thể thêm hạt giống vào túi.");
        }
    }
    
    public List<SeedData> GetAvailableSeeds()
    {
        return availableSeeds.FindAll(seed => seed.isUnlocked);
    }
    
    public void UnlockSeed(string seedName)
    {
        SeedData seed = availableSeeds.Find(s => s.seedName == seedName);
        if (seed != null)
        {
            seed.isUnlocked = true;
            Debug.Log($"Đã mở khóa hạt giống: {seedName}");
        }
    }
    
    public void LockSeed(string seedName)
    {
        SeedData seed = availableSeeds.Find(s => s.seedName == seedName);
        if (seed != null)
        {
            seed.isUnlocked = false;
            Debug.Log($"Đã khóa hạt giống: {seedName}");
        }
    }
}
