using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script demo để test hệ thống validation quest dependency
/// Attach vào một GameObject để test trong game
/// </summary>
public class QuestValidationDemo : MonoBehaviour
{
    [Header("Demo Controls")]
    [SerializeField] private bool showDebugInfo = true;
    
    void Update()
    {
        if (showDebugInfo && QuestManager.Instance != null)
        {
            // Hiển thị thông tin quest hiện tại
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ShowCurrentQuestInfo();
            }
            
            // Test validation cho từng quest
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TestQuestValidation();
            }
            
            // Reset tất cả quest (chỉ để test)
            if (Input.GetKeyDown(KeyCode.F3))
            {
                ResetAllQuests();
            }
            
            // Test slime combat validation
            if (Input.GetKeyDown(KeyCode.F4))
            {
                TestSlimeCombatValidation();
            }
        }
    }
    
    void ShowCurrentQuestInfo()
    {
        var currentQuest = QuestManager.Instance.GetCurrentQuest();
        if (currentQuest != null)
        {
            Debug.Log($"=== QUEST INFO ===");
            Debug.Log($"Current Quest: {currentQuest.title}");
            Debug.Log($"Description: {currentQuest.description}");
            Debug.Log($"Quest Index: {QuestManager.Instance.currentQuestIndex}");
            
            // Hiển thị trạng thái tất cả quest
            for (int i = 0; i < QuestManager.Instance.questList.Count; i++)
            {
                var quest = QuestManager.Instance.questList[i];
                string status = quest.isCompleted ? "✓ COMPLETED" : "○ PENDING";
                Debug.Log($"Quest {i + 1}: {quest.title} - {status}");
            }
        }
    }
    
    void TestQuestValidation()
    {
        string[] questTitles = {
            "Gặp Mụ Thảo",
            "Những Hạt Mầm Đầu Tiên", 
            "Tìm đường vào làng",
            "Đánh bại slime ở bìa rừng",
            "Gặp chủ hiệp hội"
        };
        
        Debug.Log("=== QUEST VALIDATION TEST ===");
        
        foreach (string questTitle in questTitles)
        {
            bool canDo = QuestManager.Instance.CanDoQuest(questTitle);
            bool isCurrent = QuestManager.Instance.IsCurrentQuest(questTitle);
            
            string status = "";
            if (isCurrent) status = "CURRENT";
            else if (canDo) status = "AVAILABLE";
            else status = "LOCKED";
            
            Debug.Log($"{questTitle}: {status}");
        }
    }
    
    void ResetAllQuests()
    {
        Debug.Log("=== RESETTING ALL QUESTS ===");
        
        // Reset tất cả quest về trạng thái chưa hoàn thành
        foreach (var quest in QuestManager.Instance.questList)
        {
            quest.isCompleted = false;
        }
        
        // Reset về quest đầu tiên
        QuestManager.Instance.currentQuestIndex = 0;
        
        // Cập nhật UI
        var updateMethod = typeof(QuestManager).GetMethod("UpdateQuestUI", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updateMethod?.Invoke(QuestManager.Instance, null);
        
        Debug.Log("All quests have been reset!");
    }
    
    void TestSlimeCombatValidation()
    {
        Debug.Log("=== SLIME COMBAT VALIDATION TEST ===");
        
        // Tìm tất cả SlimeAttack trong scene
        SlimeAttack[] slimes = FindObjectsOfType<SlimeAttack>();
        
        if (slimes.Length == 0)
        {
            Debug.Log("Không tìm thấy slime nào trong scene hiện tại.");
            return;
        }
        
        Debug.Log($"Tìm thấy {slimes.Length} slime(s) trong scene.");
        
        // Kiểm tra validation cho quest slime
        bool canFightSlime = QuestManager.Instance != null && 
                            QuestManager.Instance.CanDoQuest("Đánh bại slime ở bìa rừng");
        bool isSlimeQuestCurrent = QuestManager.Instance != null && 
                                  QuestManager.Instance.IsCurrentQuest("Đánh bại slime ở bìa rừng");
        
        Debug.Log($"Can fight slime: {canFightSlime}");
        Debug.Log($"Is slime quest current: {isSlimeQuestCurrent}");
        
        if (canFightSlime && isSlimeQuestCurrent)
        {
            Debug.Log("✓ Player có thể combat với slime!");
        }
        else if (canFightSlime && !isSlimeQuestCurrent)
        {
            Debug.Log("⚠ Player có thể combat với slime nhưng chưa phải quest hiện tại.");
        }
        else
        {
            Debug.Log("✗ Player KHÔNG thể combat với slime - cần hoàn thành quest trước đó.");
        }
        
        // Reset tất cả slime attack state để test lại
        foreach (var slime in slimes)
        {
            slime.ResetAttack();
        }
        Debug.Log("Đã reset tất cả slime attack state.");
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== QUEST DEBUG PANEL ===");
        
        if (QuestManager.Instance != null)
        {
            var currentQuest = QuestManager.Instance.GetCurrentQuest();
            if (currentQuest != null)
            {
                GUILayout.Label($"Current: {currentQuest.title}");
                GUILayout.Label($"Index: {QuestManager.Instance.currentQuestIndex}");
            }
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label("F1 - Show Quest Info");
        GUILayout.Label("F2 - Test Validation");
        GUILayout.Label("F3 - Reset All Quests");
        GUILayout.Label("F4 - Test Slime Combat");
        
        GUILayout.EndArea();
    }
}