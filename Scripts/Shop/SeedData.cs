using UnityEngine;

[System.Serializable]
public class SeedData
{
    [Header("Thông tin cơ bản")]
    public string seedName;
    public string description;
    public Sprite seedIcon;
    
    [Header("Giá cả")]
    public int price;
    public CurrencyType currencyType = CurrencyType.Gold;
    
    [Header("Thông tin trồng trọt")]
    public int growthTime; // Thời gian phát triển (giây)
    public int harvestAmount; // Số lượng thu hoạch
    public Sprite[] growthStages; // Các giai đoạn phát triển
    
    [Header("Thông tin bán")]
    public int sellPrice; // Giá bán khi thu hoạch
    public bool isUnlocked = true; // Có mở khóa chưa
    
    public enum CurrencyType
    {
        Gold,
        Gems,
        Coins
    }
}
