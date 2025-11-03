using UnityEngine;

/// <summary>
/// Hướng dẫn setup PlayerGoldManager
/// </summary>
public class GoldSystemSetupGuide : MonoBehaviour
{
    [Header("Hướng dẫn Setup Hệ thống Vàng")]
    [TextArea(20, 30)]
    public string setupGuide = @"
=== HƯỚNG DẪN SETUP HỆ THỐNG VÀNG ===

🎯 CÁC BƯỚC THỰC HIỆN:

1️⃣ TẠO PLAYER GOLD MANAGER:
   - Tạo Empty GameObject tên 'PlayerGoldManager'
   - Add component 'PlayerGoldManager'
   - Kéo các Text UI vào:
     * Gold Text (Text component hiển thị vàng)
     * Gold Text TMP (TextMeshPro hiển thị vàng)
     * Diamond Text (Text component hiển thị kim cương)
     * Diamond Text TMP (TextMeshPro hiển thị kim cương)

2️⃣ SETUP UI REFERENCES:
   - Tìm các Text hiển thị vàng/kim cương trong scene
   - Kéo vào PlayerGoldManager:
     * goldText → Text hiển thị vàng
     * goldTextTMP → TextMeshPro hiển thị vàng
     * diamondText → Text hiển thị kim cương
     * diamondTextTMP → TextMeshPro hiển thị kim cương

3️⃣ KIỂM TRA TÍCH HỢP:
   - BagManager đã được sửa để cộng vàng khi bán
   - BagUI đã được sửa để hiển thị thông báo
   - PlayerGoldManager tự động lưu vào Firebase

4️⃣ TEST HỆ THỐNG:
   - Thu hoạch cây (phím M)
   - Mở balo và bán hoa hướng dương
   - Kiểm tra vàng có tăng không
   - Kiểm tra UI có cập nhật không

🎮 CÁCH HOẠT ĐỘNG:

💰 THU HOẠCH VÀ BÁN:
1. Thu hoạch cây → Hoa hướng dương vào balo
2. Mở balo → Click vào hoa hướng dương
3. Nhấn 'Bán' → Nhận 15 vàng
4. Vàng tự động cộng vào tài khoản
5. UI vàng tự động cập nhật
6. Dữ liệu tự động lưu vào Firebase

🔧 DEBUG TOOLS:
- PlayerGoldManager có Context Menu 'Debug Gold Info'
- Console sẽ hiển thị log khi bán hàng
- BagUI hiển thị thông báo nhận vàng

⚠️ LƯU Ý:
- Đảm bảo PlayerGoldManager.Instance không null
- Kiểm tra tất cả UI references đã được gán
- Test trên các scene khác nhau
- Kiểm tra Firebase có lưu đúng không

🎯 KẾT QUẢ MONG MUỐN:
- Bán 1 hoa hướng dương → +15 vàng
- UI vàng cập nhật ngay lập tức
- Dữ liệu lưu vào Firebase
- Thông báo hiển thị khi nhận vàng
";

    void Start()
    {
        Debug.Log("=== GOLD SYSTEM SETUP GUIDE LOADED ===");
        Debug.Log("Xem Inspector của GoldSystemSetupGuide để đọc hướng dẫn đầy đủ!");
    }
    
    void Update()
    {
        // Hiển thị hướng dẫn khi nhấn F13
        if (Input.GetKeyDown(KeyCode.F13))
        {
            Debug.Log(setupGuide);
        }
    }
    
    void OnGUI()
    {
        // Hiển thị hướng dẫn ngắn gọn
        GUILayout.BeginArea(new Rect(10, Screen.height - 140, 400, 130));
        GUILayout.Label("=== GOLD SYSTEM SETUP GUIDE ===");
        GUILayout.Label("F13 - Show Full Setup Guide");
        GUILayout.Label("Xem Inspector để đọc hướng dẫn chi tiết!");
        
        if (PlayerGoldManager.Instance != null)
        {
            GUILayout.Label("✓ PlayerGoldManager đã được setup");
            GUILayout.Label($"Vàng hiện tại: {PlayerGoldManager.Instance.GetGold()}");
        }
        else
        {
            GUILayout.Label("⚠ PlayerGoldManager chưa được setup");
        }
        
        GUILayout.EndArea();
    }
}
