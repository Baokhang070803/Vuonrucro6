using UnityEngine;

/// <summary>
/// Hướng dẫn setup Shop System từng bước chi tiết
/// </summary>
public class ShopSetupGuide : MonoBehaviour
{
    [Header("=== HƯỚNG DẪN SETUP SHOP ===")]
    [TextArea(10, 20)]
    public string setupInstructions = @"
BƯỚC 1: TẠO SHOP MANAGER
1. Tạo Empty GameObject → đặt tên 'ShopManager'
2. Add Component → ShopManager script
3. Gán PlayerGoldManager vào field 'playerGoldManager'

BƯỚC 2: TẠO UI PANEL
1. Tạo Canvas mới (UI → Canvas)
2. Tạo Panel con (UI → Panel) → đặt tên 'ShopPanel'
3. Tạo ScrollView (UI → Scroll View) → đặt tên 'ShopScrollView'
4. Trong ScrollView → Content → đặt tên 'ShopItemContainer'

BƯỚC 3: TẠO SHOP ITEM PREFAB
1. Tạo Empty GameObject → đặt tên 'ShopItem'
2. Add Component → ShopItem script
3. Tạo UI elements:
   - Image (SeedIcon)
   - Text (SeedName)
   - Text (Description) 
   - Text (PriceText)
   - Button (PurchaseButton)
   - Text (ButtonText) con của Button
4. Gán các component vào ShopItem script
5. Tạo Prefab từ ShopItem

BƯỚC 4: SETUP SHOP UI
1. Tạo GameObject → đặt tên 'ShopUI'
2. Add Component → ShopUI script
3. Gán các reference:
   - ShopPanel → shopPanel
   - ShopItemContainer → shopItemContainer
   - ShopItemPrefab → shopItemPrefab
   - CloseButton → closeButton
   - PlayerGoldText → playerGoldText

BƯỚC 5: TẠO SHOP TRIGGER
1. Tạo Empty GameObject → đặt tên 'ShopTrigger'
2. Add Component → BoxCollider2D (IsTrigger = true)
3. Add Component → ShopTrigger script
4. Gán ShopUI reference
5. Tạo Text hiển thị 'Nhấn E để mở shop'

BƯỚC 6: TẠO DỮ LIỆU MẪU
1. Right-click Project → Create → Shop → Shop Data
2. Đặt tên 'DefaultShopData'
3. Click 'Tạo dữ liệu mẫu' trong Inspector
4. Gán DefaultShopData vào ShopManager

BƯỚC 7: KẾT NỐI CUỐI CÙNG
1. ShopManager → gán ShopUI reference
2. ShopUI → gán PlayerGoldManager reference
3. Test hệ thống!

LƯU Ý:
- Đảm bảo Player có tag 'Player'
- PlayerGoldManager phải có trong scene
- UI Canvas phải có EventSystem
- ShopTrigger phải có Collider2D
";

    [Header("=== KIỂM TRA SETUP ===")]
    public bool hasShopManager = false;
    public bool hasShopUI = false;
    public bool hasShopTrigger = false;
    public bool hasPlayerGoldManager = false;
    public bool hasDefaultData = false;

    [ContextMenu("Kiểm tra Setup")]
    public void CheckSetup()
    {
        // Kiểm tra ShopManager
        hasShopManager = FindObjectOfType<ShopManager>() != null;
        
        // Kiểm tra ShopUI
        hasShopUI = FindObjectOfType<ShopUI>() != null;
        
        // Kiểm tra ShopTrigger
        hasShopTrigger = FindObjectOfType<ShopTrigger>() != null;
        
        // Kiểm tra PlayerGoldManager
        hasPlayerGoldManager = FindObjectOfType<PlayerGoldManager>() != null;
        
        // Kiểm tra dữ liệu mẫu
        hasDefaultData = Resources.FindObjectsOfTypeAll<ShopDataCreator>().Length > 0;
        
        Debug.Log($"=== KẾT QUẢ KIỂM TRA ===");
        Debug.Log($"ShopManager: {(hasShopManager ? "✅" : "❌")}");
        Debug.Log($"ShopUI: {(hasShopUI ? "✅" : "❌")}");
        Debug.Log($"ShopTrigger: {(hasShopTrigger ? "✅" : "❌")}");
        Debug.Log($"PlayerGoldManager: {(hasPlayerGoldManager ? "✅" : "❌")}");
        Debug.Log($"DefaultData: {(hasDefaultData ? "✅" : "❌")}");
        
        if (hasShopManager && hasShopUI && hasShopTrigger && hasPlayerGoldManager)
        {
            Debug.Log("🎉 SETUP HOÀN THÀNH! Shop system đã sẵn sàng!");
        }
        else
        {
            Debug.LogWarning("⚠️ Còn thiếu một số component. Vui lòng kiểm tra lại!");
        }
    }
}
