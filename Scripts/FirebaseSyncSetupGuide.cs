using UnityEngine;

/// <summary>
/// Hướng dẫn setup PlayerDataSyncManager để tránh xung đột dữ liệu Firebase
/// </summary>
public class FirebaseSyncSetupGuide : MonoBehaviour
{
    [Header("Hướng dẫn Setup Firebase Sync")]
    [TextArea(20, 30)]
    public string setupGuide = @"
=== HƯỚNG DẪN SETUP FIREBASE SYNC ===

🚨 VẤN ĐỀ ĐÃ SỬA:
- TileMapManager không còn ghi đè dữ liệu balo
- BagManager không còn ghi đè dữ liệu tilemap
- Tất cả dữ liệu được đồng bộ qua PlayerDataSyncManager

🎯 CÁC BƯỚC THỰC HIỆN:

1️⃣ TẠO PLAYER DATA SYNC MANAGER:
   - Tạo Empty GameObject tên 'PlayerDataSyncManager'
   - Add component 'PlayerDataSyncManager'
   - Đảm bảo nó tồn tại trong scene trước khi các script khác chạy

2️⃣ KIỂM TRA TÍCH HỢP:
   - TileMapManager đã sử dụng PlayerDataSyncManager.UpdateMapInGame()
   - BagManager đã sử dụng PlayerDataSyncManager.UpdateBagData()
   - PlayerGoldManager đã sử dụng PlayerDataSyncManager.UpdateGold()

3️⃣ CẤU TRÚC FIREBASE MỚI:
   Users/{userId}/
     ├── Name: 'Player Name'
     ├── Gold: 100
     ├── Diamond: 50
     ├── MapInGame: {...}
     └── BagData: {...}

4️⃣ TEST HỆ THỐNG:
   - Thu hoạch cây → Chỉ cập nhật BagData
   - Đào đất → Chỉ cập nhật MapInGame
   - Bán hàng → Cập nhật cả BagData và Gold
   - Không còn xung đột dữ liệu!

🎮 CÁCH HOẠT ĐỘNG:

💰 THU HOẠCH VÀ BÁN:
1. Thu hoạch cây → BagManager.AddSunflower()
2. BagManager → PlayerDataSyncManager.UpdateBagData()
3. Chỉ cập nhật BagData → Không động đến MapInGame

🌱 ĐÀO ĐẤT VÀ TRỒNG CÂY:
1. Đào đất → TileMapManager.SetStateForTilemapDetail()
2. TileMapManager → PlayerDataSyncManager.UpdateMapInGame()
3. Chỉ cập nhật MapInGame → Không động đến BagData

🛒 BÁN HÀNG:
1. Bán hàng → BagManager.SellItem()
2. BagManager → PlayerDataSyncManager.UpdateBagData()
3. PlayerGoldManager → PlayerDataSyncManager.UpdateGold()
4. Cập nhật cả BagData và Gold → Không động đến MapInGame

🔧 DEBUG TOOLS:
- PlayerDataSyncManager có Context Menu 'Debug Firebase Structure'
- Console sẽ hiển thị log khi đồng bộ
- Mỗi hệ thống log riêng biệt

⚠️ LƯU Ý:
- Đảm bảo PlayerDataSyncManager.Instance không null
- Kiểm tra tất cả script đã sử dụng PlayerDataSyncManager
- Test trên các scene khác nhau
- Kiểm tra Firebase có cấu trúc đúng không

🎯 KẾT QUẢ MONG MUỐN:
- Thu hoạch cây → BagData được lưu, MapInGame không bị xóa
- Đào đất → MapInGame được lưu, BagData không bị xóa
- Bán hàng → Cả BagData và Gold được lưu
- Không còn xung đột dữ liệu Firebase!
";

    void Start()
    {
        Debug.Log("=== FIREBASE SYNC SETUP GUIDE LOADED ===");
        Debug.Log("Xem Inspector của FirebaseSyncSetupGuide để đọc hướng dẫn đầy đủ!");
    }
    
    void Update()
    {
        // Hiển thị hướng dẫn khi nhấn F14
        if (Input.GetKeyDown(KeyCode.F14))
        {
            Debug.Log(setupGuide);
        }
    }
    
    void OnGUI()
    {
        // Hiển thị hướng dẫn ngắn gọn
        GUILayout.BeginArea(new Rect(10, Screen.height - 160, 500, 150));
        GUILayout.Label("=== FIREBASE SYNC SETUP GUIDE ===");
        GUILayout.Label("F14 - Show Full Setup Guide");
        GUILayout.Label("Xem Inspector để đọc hướng dẫn chi tiết!");
        
        if (PlayerDataSyncManager.Instance != null)
        {
            GUILayout.Label("✓ PlayerDataSyncManager đã được setup");
            GUILayout.Label("✓ Không còn xung đột dữ liệu Firebase");
        }
        else
        {
            GUILayout.Label("⚠ PlayerDataSyncManager chưa được setup");
            GUILayout.Label("⚠ Có thể còn xung đột dữ liệu Firebase");
        }
        
        GUILayout.EndArea();
    }
}
