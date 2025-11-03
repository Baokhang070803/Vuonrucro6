using UnityEngine;

/// <summary>
/// Hướng dẫn sử dụng hệ thống Quest Dependency Validation
/// Script này chỉ để hiển thị hướng dẫn, không có logic game
/// </summary>
public class QuestSystemGuide : MonoBehaviour
{
    [Header("System Information")]
    [TextArea(10, 20)]
    public string systemGuide = @"
=== HƯỚNG DẪN HỆ THỐNG QUEST DEPENDENCY ===

🎯 TỔNG QUAN:
Hệ thống đảm bảo player phải làm quest theo thứ tự:
1. Gặp Mụ Thảo
2. Những Hạt Mầm Đầu Tiên (Farming)
3. Tìm đường vào làng
4. Đánh bại slime ở bìa rừng
5. Gặp chủ hiệp hội

🔒 BẢO MẬT:
- KHÔNG thể bypass quest nào
- Tất cả trigger đều có validation
- Thông báo cooldown 3 giây (tránh spam)
- Nhân vật TỰ ĐỘNG DỪNG khi hiện thông báo validation

🎮 CÁCH SỬ DỤNG:

📱 QUICK START (QuestStarter.cs):
F8 - Bắt đầu quest đầu tiên (simulate gặp Mụ Thảo)
F9 - Hoàn thành quest hiện tại (để test)

🔍 DEBUG TOOLS (QuestValidationDemo.cs):
F1 - Hiển thị thông tin quest hiện tại
F2 - Test validation tất cả quest
F3 - Reset tất cả quest về đầu
F4 - Test slime combat validation

🧪 ADVANCED TESTING (QuestDependencyTester.cs):
F5 - Test toàn bộ hệ thống dependency
F6 - Simulate quest progression từ đầu đến cuối
F7 - Test tất cả bypass attempts (phải tất cả bị chặn)

⚠️ LƯU Ý:
- Nếu thấy thông báo spam, hệ thống đã có cooldown 3 giây
- Sử dụng F8 để bắt đầu quest đầu tiên nếu bị stuck
- Tất cả validation message sẽ tự động biến mất sau 3 giây
- Khi thông báo hiện, nhân vật sẽ DỪNG di chuyển tự động
- Nhấn Space/Enter để đóng thông báo và tiếp tục di chuyển

🔧 MOVEMENT DEBUG:
F12 - Hiển thị trạng thái movement chi tiết

🎯 QUEST PROGRESSION:
Quest 1 → Quest 2 → Quest 3 → Quest 4 → Quest 5
(Bắt buộc theo thứ tự, không thể skip)
";

    void Start()
    {
        Debug.Log("=== QUEST SYSTEM GUIDE LOADED ===");
        Debug.Log("Xem Inspector của QuestSystemGuide để đọc hướng dẫn đầy đủ!");
        Debug.Log("Hoặc nhấn F8 để bắt đầu quest đầu tiên ngay!");
    }
    
    void Update()
    {
        // Hiển thị hướng dẫn khi nhấn F10
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.Log(systemGuide);
        }
    }
    
    void OnGUI()
    {
        // Hiển thị hướng dẫn ngắn gọn
        GUILayout.BeginArea(new Rect(10, Screen.height - 100, 400, 90));
        GUILayout.Label("=== QUEST SYSTEM ACTIVE ===");
        GUILayout.Label("F8 - Start First Quest | F10 - Show Full Guide");
        GUILayout.Label("Xem Inspector của QuestSystemGuide để đọc hướng dẫn!");
        
        if (QuestManager.Instance != null)
        {
            var currentQuest = QuestManager.Instance.GetCurrentQuest();
            if (currentQuest != null)
            {
                GUILayout.Label($"Current Quest: {currentQuest.title}");
            }
        }
        
        GUILayout.EndArea();
    }
}