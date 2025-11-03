using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Helper script để dễ dàng bắt đầu quest đầu tiên và test hệ thống
/// Attach vào một GameObject để sử dụng
/// </summary>
public class QuestStarter : MonoBehaviour
{
    [Header("Quick Start Options")]
    [SerializeField] private bool showQuickStartUI = true;
    
    void Update()
    {
        if (showQuickStartUI)
        {
            // F8 - Bắt đầu quest đầu tiên (simulate gặp Mụ Thảo)
            if (Input.GetKeyDown(KeyCode.F8))
            {
                StartFirstQuest();
            }
            
            // F9 - Complete quest hiện tại (để test progression)
            if (Input.GetKeyDown(KeyCode.F9))
            {
                CompleteCurrentQuest();
            }
            
            // F11 - Toggle debug logs
            if (Input.GetKeyDown(KeyCode.F11))
            {
                ToggleDebugLogs();
            }
        }
    }
    
    void StartFirstQuest()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager.Instance is null!");
            return;
        }
        
        var currentQuest = QuestManager.Instance.GetCurrentQuest();
        if (currentQuest != null && currentQuest.title == "Gặp Mụ Thảo")
        {
            Debug.Log("=== STARTING FIRST QUEST ===");
            Debug.Log("Simulating meeting Mụ Thảo...");
            
            // Simulate hoàn thành quest đầu tiên
            QuestManager.Instance.CompleteQuest("Gặp Mụ Thảo");
            
            Debug.Log("✓ Quest 'Gặp Mụ Thảo' completed!");
            Debug.Log("Now you can start farming (Quest 2)!");
            
            // Hiển thị hướng dẫn
            if (DialogueManager.I != null)
            {
                var instructions = new List<string> 
                {
                    "✓ Đã hoàn thành: Gặp Mụ Thảo",
                    "Bây giờ bạn có thể bắt đầu farming!",
                    "Sử dụng: C (dọn cỏ), V (gieo hạt), M (thu hoạch)"
                };
                DialogueManager.I.Show(instructions);
            }
        }
        else
        {
            Debug.Log("Quest đầu tiên đã hoàn thành hoặc không phải quest hiện tại.");
        }
    }
    
    void CompleteCurrentQuest()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager.Instance is null!");
            return;
        }
        
        var currentQuest = QuestManager.Instance.GetCurrentQuest();
        if (currentQuest != null)
        {
            Debug.Log($"=== COMPLETING CURRENT QUEST ===");
            Debug.Log($"Completing: {currentQuest.title}");
            
            // Special handling cho từng quest
            switch (currentQuest.title)
            {
                case "Gặp Mụ Thảo":
                    QuestManager.Instance.CompleteQuest("Gặp Mụ Thảo");
                    ShowInstructions("Bây giờ bạn có thể farming! Sử dụng C, V, M");
                    break;
                    
                case "Những Hạt Mầm Đầu Tiên":
                    // Simulate hoàn thành farming quest
                    var farmController = FindObjectOfType<PlayerFarmController>();
                    if (farmController != null)
                    {
                        // Set harvestedCount to 10 using reflection
                        var harvestedCountField = typeof(PlayerFarmController).GetField("harvestedCount", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        harvestedCountField?.SetValue(farmController, 10);
                        
                        QuestManager.Instance.CompleteQuest("Những Hạt Mầm Đầu Tiên");
                        ShowInstructions("Bây giờ bạn có thể tìm đường vào làng!");
                    }
                    break;
                    
                case "Tìm đường vào làng":
                    QuestManager.Instance.CompleteQuest("Tìm đường vào làng");
                    ShowInstructions("Bây giờ bạn có thể đánh slime!");
                    break;
                    
                case "Đánh bại slime ở bìa rừng":
                    QuestManager.Instance.CompleteQuest("Đánh bại slime ở bìa rừng");
                    ShowInstructions("Bây giờ bạn có thể gặp chủ hiệp hội!");
                    break;
                    
                case "Gặp chủ hiệp hội":
                    QuestManager.Instance.CompleteQuest("Gặp chủ hiệp hội");
                    ShowInstructions("Tất cả quest đã hoàn thành!");
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown quest: {currentQuest.title}");
                    break;
            }
        }
        else
        {
            Debug.Log("Không có quest hiện tại để hoàn thành.");
        }
    }
    
    void ShowInstructions(string message)
    {
        if (DialogueManager.I != null)
        {
            DialogueManager.I.Show(new List<string> { message });
        }
    }
    
    void ToggleDebugLogs()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.enableDebugLogs = !QuestManager.Instance.enableDebugLogs;
            string status = QuestManager.Instance.enableDebugLogs ? "ENABLED" : "DISABLED";
            Debug.Log($"=== DEBUG LOGS {status} ===");
            
            if (DialogueManager.I != null)
            {
                DialogueManager.I.Show(new List<string> { $"Debug Logs: {status}" });
            }
        }
    }
    
    void OnGUI()
    {
        if (!showQuickStartUI) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 320, Screen.height - 120, 300, 100));
        GUILayout.Label("=== QUEST STARTER ===");
        
        if (QuestManager.Instance != null)
        {
            var currentQuest = QuestManager.Instance.GetCurrentQuest();
            if (currentQuest != null)
            {
                GUILayout.Label($"Current: {currentQuest.title}");
            }
        }
        
        GUILayout.Space(5);
        GUILayout.Label("Quick Controls:");
        GUILayout.Label("F8 - Start First Quest");
        GUILayout.Label("F9 - Complete Current Quest");
        
        if (QuestManager.Instance != null)
        {
            string debugStatus = QuestManager.Instance.enableDebugLogs ? "ON" : "OFF";
            GUILayout.Label($"F11 - Debug Logs ({debugStatus})");
        }
        
        GUILayout.EndArea();
    }
}