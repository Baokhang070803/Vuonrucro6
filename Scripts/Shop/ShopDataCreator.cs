using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "Shop/Shop Data")]
public class ShopDataCreator : ScriptableObject
{
    [Header("Dữ liệu shop")]
    public SeedData[] defaultSeeds;
    
    [Header("Cài đặt shop")]
    public int maxShopItems = 10;
    public bool autoRefreshShop = true;
    public float refreshInterval = 300f; // 5 phút
    
    [ContextMenu("Tạo dữ liệu mẫu")]
    public void CreateSampleData()
    {
        // Tạo dữ liệu mẫu cho các loại hạt giống
        defaultSeeds = new SeedData[]
        {
            CreateSeedData("Cà chua", "Cà chua ngon ngọt, thời gian thu hoạch nhanh", 50, 30, 3, 25),
            CreateSeedData("Cà rốt", "Cà rốt giòn, giàu vitamin", 75, 45, 4, 35),
            CreateSeedData("Khoai tây", "Khoai tây bổ dưỡng, năng suất cao", 100, 60, 5, 50),
            CreateSeedData("Bắp cải", "Bắp cải tươi, chống lạnh tốt", 80, 40, 4, 40),
            CreateSeedData("Ớt", "Ớt cay, gia vị không thể thiếu", 60, 35, 3, 30),
            CreateSeedData("Dưa chuột", "Dưa chuột mát, giải nhiệt", 40, 25, 2, 20),
            CreateSeedData("Hành tây", "Hành tây thơm, gia vị chính", 55, 30, 3, 25),
            CreateSeedData("Tỏi", "Tỏi thơm, tăng cường sức khỏe", 45, 25, 2, 22),
            CreateSeedData("Rau diếp", "Rau diếp tươi, salad ngon", 35, 20, 2, 18),
            CreateSeedData("Cà tím", "Cà tím tím, món ăn ngon", 70, 40, 4, 35)
        };
        
        Debug.Log($"Đã tạo {defaultSeeds.Length} loại hạt giống mẫu");
    }
    
    private SeedData CreateSeedData(string name, string description, int price, int growthTime, int harvestAmount, int sellPrice)
    {
        SeedData seed = new SeedData();
        seed.seedName = name;
        seed.description = description;
        seed.price = price;
        seed.growthTime = growthTime;
        seed.harvestAmount = harvestAmount;
        seed.sellPrice = sellPrice;
        seed.currencyType = SeedData.CurrencyType.Gold;
        seed.isUnlocked = true;
        
        return seed;
    }
}
