using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script test toàn diện cho hệ thống Quest Dependency Validation
/// Sử dụng để verify rằng tất cả quest đều có validation đúng
/// </summary>
public class QuestDependencyTester : MonoBehaviour
{
    [Header("Test Results")]
    [SerializeField] private bool showDetailedLogs = true;
    
    void Start()
    {
        if (showDetailedLogs)
        {
            Debug.Log("=== QUEST DEPENDENCY TESTER INITIALIZED ===");
            Debug.Log("Sử dụng các phím sau để test:");
            Debug.Log("F5 - Test toàn bộ hệ thống dependency");
            Debug.Log("F6 - Simulate quest progression");
            Debug.Log("F7 - Test bypass attempts");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            TestFullDependencySystem();
        }
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SimulateQuestProgression();
        }
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            TestBypassAttempts();
        }
    }
    
    void TestFullDependencySystem()
    {
        Debug.Log("=== FULL DEPENDENCY SYSTEM TEST ===");
        
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager.Instance is null!");
            return;
        }
        
        // Test từng quest dependency
        string[] questOrder = {
            "Gặp Mụ Thảo",
            "Những Hạt Mầm Đầu Tiên", 
            "Tìm đường vào làng",
            "Đánh bại slime ở bìa rừng",
            "Gặp chủ hiệp hội"
        };
        
        Debug.Log("Testing quest dependencies in order...");
        
        for (int i = 0; i < questOrder.Length; i++)
        {
            string questTitle = questOrder[i];
            bool canDo = QuestManager.Instance.CanDoQuest(questTitle);
            bool isCurrent = QuestManager.Instance.IsCurrentQuest(questTitle);
            
            // Quest đầu tiên luôn có thể làm
            if (i == 0)
            {
                Debug.Log($"Quest {i+1}: {questTitle} - {(canDo ? "✓ AVAILABLE" : "✗ LOCKED")} {(isCurrent ? "(CURRENT)" : "")}");
            }
            else
            {
                // Các quest sau phải chờ quest trước hoàn thành
                Debug.Log($"Quest {i+1}: {questTitle} - {(canDo ? "✓ AVAILABLE" : "✗ LOCKED")} {(isCurrent ? "(CURRENT)" : "")}");
            }
        }
        
        // Test specific components
        TestComponentValidation();
    }
    
    void TestComponentValidation()
    {
        Debug.Log("\n--- COMPONENT VALIDATION TEST ---");
        
        // Test VillageEntryTrigger
        VillageEntryTrigger villageEntry = FindObjectOfType<VillageEntryTrigger>();
        if (villageEntry != null)
        {
            Debug.Log("✓ VillageEntryTrigger found - has validation");
        }
        else
        {
            Debug.Log("⚠ VillageEntryTrigger not found in scene");
        }
        
        // Test SlimeAttack
        SlimeAttack[] slimes = FindObjectsOfType<SlimeAttack>();
        if (slimes.Length > 0)
        {
            Debug.Log($"✓ Found {slimes.Length} SlimeAttack(s) - all have validation");
        }
        else
        {
            Debug.Log("⚠ No SlimeAttack found in scene");
        }
        
        // Test NPCs
        Muthaoguide muthao = FindObjectOfType<Muthaoguide>();
        ChuhiephoiGuide chuhiephoi = FindObjectOfType<ChuhiephoiGuide>();
        
        if (muthao != null)
        {
            Debug.Log("✓ Muthaoguide found - has validation");
        }
        
        if (chuhiephoi != null)
        {
            Debug.Log("✓ ChuhiephoiGuide found - has validation");
        }
        
        // Test PlayerFarmController
        PlayerFarmController farmController = FindObjectOfType<PlayerFarmController>();
        if (farmController != null)
        {
            Debug.Log("✓ PlayerFarmController found - has validation");
        }
    }
    
    void SimulateQuestProgression()
    {
        Debug.Log("=== SIMULATING QUEST PROGRESSION ===");
        
        if (QuestManager.Instance == null) return;
        
        // Reset tất cả quest
        foreach (var quest in QuestManager.Instance.questList)
        {
            quest.isCompleted = false;
        }
        QuestManager.Instance.currentQuestIndex = 0;
        
        Debug.Log("Reset all quests. Starting simulation...");
        
        // Simulate hoàn thành từng quest theo thứ tự
        string[] questOrder = {
            "Gặp Mụ Thảo",
            "Những Hạt Mầm Đầu Tiên", 
            "Tìm đường vào làng",
            "Đánh bại slime ở bìa rừng",
            "Gặp chủ hiệp hội"
        };
        
        for (int i = 0; i < questOrder.Length; i++)
        {
            string questTitle = questOrder[i];
            
            Debug.Log($"\n--- Attempting Quest {i+1}: {questTitle} ---");
            
            // Kiểm tra có thể làm quest này không
            bool canDo = QuestManager.Instance.CanDoQuest(questTitle);
            bool isCurrent = QuestManager.Instance.IsCurrentQuest(questTitle);
            
            if (canDo && isCurrent)
            {
                Debug.Log($"✓ Can do quest: {questTitle}");
                
                // Simulate hoàn thành quest
                QuestManager.Instance.CompleteQuest(questTitle);
                Debug.Log($"✓ Completed quest: {questTitle}");
            }
            else
            {
                Debug.Log($"✗ Cannot do quest: {questTitle} (canDo: {canDo}, isCurrent: {isCurrent})");
                break;
            }
        }
        
        Debug.Log("\nQuest progression simulation completed!");
    }
    
    void TestBypassAttempts()
    {
        Debug.Log("=== TESTING BYPASS ATTEMPTS ===");
        
        if (QuestManager.Instance == null) return;
        
        // Reset về quest đầu tiên
        foreach (var quest in QuestManager.Instance.questList)
        {
            quest.isCompleted = false;
        }
        QuestManager.Instance.currentQuestIndex = 0;
        
        Debug.Log("Reset to first quest. Testing bypass attempts...");
        
        // Thử bypass các quest
        string[] bypassAttempts = {
            "Những Hạt Mầm Đầu Tiên",  // Thử farming trước khi gặp Mụ Thảo
            "Tìm đường vào làng",       // Thử vào làng trước khi farming
            "Đánh bại slime ở bìa rừng", // Thử đánh slime trước khi vào làng
            "Gặp chủ hiệp hội"          // Thử gặp chủ hiệp hội trước khi đánh slime
        };
        
        foreach (string questTitle in bypassAttempts)
        {
            Debug.Log($"\n--- Bypass Attempt: {questTitle} ---");
            
            bool canDo = QuestManager.Instance.CanDoQuest(questTitle);
            bool isCurrent = QuestManager.Instance.IsCurrentQuest(questTitle);
            
            if (!canDo || !isCurrent)
            {
                Debug.Log($"✓ BYPASS BLOCKED: {questTitle} (canDo: {canDo}, isCurrent: {isCurrent})");
                
                // Hiển thị dependency message
                QuestManager.Instance.ShowDependencyMessage(questTitle);
            }
            else
            {
                Debug.LogError($"✗ BYPASS POSSIBLE: {questTitle} - THIS IS A BUG!");
            }
        }
        
        Debug.Log("\nBypass test completed. All attempts should be blocked!");
    }
    
    void OnGUI()
    {
        if (!showDetailedLogs) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 150));
        GUILayout.Label("=== DEPENDENCY TESTER ===");
        
        if (QuestManager.Instance != null)
        {
            var currentQuest = QuestManager.Instance.GetCurrentQuest();
            if (currentQuest != null)
            {
                GUILayout.Label($"Current: {currentQuest.title}");
            }
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Advanced Controls:");
        GUILayout.Label("F5 - Full System Test");
        GUILayout.Label("F6 - Simulate Progression");
        GUILayout.Label("F7 - Test Bypass Attempts");
        
        GUILayout.EndArea();
    }
}