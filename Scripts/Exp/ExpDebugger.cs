using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script debug để kiểm tra hệ thống EXP
/// </summary>
public class ExpDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    public bool showDebugUI = true;
    
    [Header("Test Buttons")]
    public Button testAddExpButton;
    public Button testFarmingExpButton;
    public Button testHarvestingExpButton;
    public Button testQuestExpButton;
    
    [Header("Debug UI")]
    public TextMeshProUGUI debugText;
    
    private PlayerExpManager expManager;
    private ExpUI expUI;
    
    void Start()
    {
        // Tìm các components
        expManager = PlayerExpManager.Instance;
        expUI = FindObjectOfType<ExpUI>();
        
        // Setup test buttons
        SetupTestButtons();
        
        // Setup debug UI
        SetupDebugUI();
        
        Debug.Log("[ExpDebugger] Đã khởi tạo!");
    }
    
    void Update()
    {
        // Cập nhật debug UI mỗi frame
        if (showDebugUI && debugText != null)
        {
            UpdateDebugUI();
        }
        
        // Test với phím tắt
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TestAddExp(50);
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TestFarmingExp();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestHarvestingExp();
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            TestQuestExp();
        }
    }
    
    void SetupTestButtons()
    {
        if (testAddExpButton != null)
            testAddExpButton.onClick.AddListener(() => TestAddExp(50));
        
        if (testFarmingExpButton != null)
            testFarmingExpButton.onClick.AddListener(TestFarmingExp);
        
        if (testHarvestingExpButton != null)
            testHarvestingExpButton.onClick.AddListener(TestHarvestingExp);
        
        if (testQuestExpButton != null)
            testQuestExpButton.onClick.AddListener(TestQuestExp);
    }
    
    void SetupDebugUI()
    {
        if (debugText == null)
        {
            // Tạo debug text nếu chưa có
            GameObject debugGO = new GameObject("DebugText");
            debugGO.transform.SetParent(transform);
            debugText = debugGO.AddComponent<TextMeshProUGUI>();
            debugText.text = "Debug UI";
            debugText.fontSize = 14;
            debugText.color = Color.white;
        }
    }
    
    void UpdateDebugUI()
    {
        if (debugText == null) return;
        
        string debugInfo = "=== EXP DEBUG INFO ===\n";
        
        // Kiểm tra PlayerExpManager
        if (expManager != null)
        {
            var expData = expManager.GetExpData();
            debugInfo += $"✓ PlayerExpManager: OK\n";
            debugInfo += $"  Level: {expData.GetLevelString()}\n";
            debugInfo += $"  EXP: {expData.currentExp}/{expData.expToNextLevel}\n";
            debugInfo += $"  Stat Points: {expData.statPoints}\n";
            debugInfo += $"  EXP %: {expData.GetExpPercentage():P1}\n";
        }
        else
        {
            debugInfo += "❌ PlayerExpManager: NULL\n";
        }
        
        // Kiểm tra ExpUI
        if (expUI != null)
        {
            debugInfo += "✓ ExpUI: OK\n";
            
            // Kiểm tra UI components
            if (expUI.expSlider != null)
                debugInfo += $"  ✓ ExpSlider: {expUI.expSlider.value:P1}\n";
            else
                debugInfo += "  ❌ ExpSlider: NULL\n";
                
            if (expUI.levelText != null)
                debugInfo += $"  ✓ LevelText: {expUI.levelText.text}\n";
            else
                debugInfo += "  ❌ LevelText: NULL\n";
                
            if (expUI.expText != null)
                debugInfo += $"  ✓ ExpText: {expUI.expText.text}\n";
            else
                debugInfo += "  ❌ ExpText: NULL\n";
        }
        else
        {
            debugInfo += "❌ ExpUI: NULL\n";
        }
        
        debugInfo += "\n=== CONTROLS ===\n";
        debugInfo += "F1: Add 50 EXP\n";
        debugInfo += "F2: Add Farming EXP\n";
        debugInfo += "F3: Add Harvesting EXP\n";
        debugInfo += "F4: Add Quest EXP\n";
        
        debugText.text = debugInfo;
    }
    
    void TestAddExp(int amount)
    {
        if (expManager != null)
        {
            expManager.AddExp(amount, "Debug Test");
            LogDebug($"Đã thêm {amount} EXP!");
        }
        else
        {
            LogDebug("❌ PlayerExpManager is NULL!");
        }
    }
    
    void TestFarmingExp()
    {
        if (expManager != null)
        {
            expManager.AddFarmingExp();
            LogDebug("Đã thêm Farming EXP!");
        }
        else
        {
            LogDebug("❌ PlayerExpManager is NULL!");
        }
    }
    
    void TestHarvestingExp()
    {
        if (expManager != null)
        {
            expManager.AddHarvestingExp();
            LogDebug("Đã thêm Harvesting EXP!");
        }
        else
        {
            LogDebug("❌ PlayerExpManager is NULL!");
        }
    }
    
    void TestQuestExp()
    {
        if (expManager != null)
        {
            expManager.AddQuestExp();
            LogDebug("Đã thêm Quest EXP!");
        }
        else
        {
            LogDebug("❌ PlayerExpManager is NULL!");
        }
    }
    
    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ExpDebugger] {message}");
        }
    }
    
    /// <summary>
    /// Debug: Kiểm tra tất cả components
    /// </summary>
    [ContextMenu("Debug All Components")]
    public void DebugAllComponents()
    {
        Debug.Log("=== EXP SYSTEM DEBUG ===");
        
        // Kiểm tra PlayerExpManager
        if (expManager != null)
        {
            Debug.Log("✓ PlayerExpManager: Found");
            expManager.DebugExpInfo();
        }
        else
        {
            Debug.LogError("❌ PlayerExpManager: NOT FOUND!");
        }
        
        // Kiểm tra ExpUI
        if (expUI != null)
        {
            Debug.Log("✓ ExpUI: Found");
            
            if (expUI.expSlider != null)
                Debug.Log($"  ✓ ExpSlider: {expUI.expSlider.value}");
            else
                Debug.LogError("  ❌ ExpSlider: NULL");
                
            if (expUI.levelText != null)
                Debug.Log($"  ✓ LevelText: {expUI.levelText.text}");
            else
                Debug.LogError("  ❌ LevelText: NULL");
                
            if (expUI.expText != null)
                Debug.Log($"  ✓ ExpText: {expUI.expText.text}");
            else
                Debug.LogError("  ❌ ExpText: NULL");
        }
        else
        {
            Debug.LogError("❌ ExpUI: NOT FOUND!");
        }
        
        // Kiểm tra events
        if (expManager != null)
        {
            Debug.Log($"OnExpChanged subscribers: {expManager.OnExpChanged?.GetInvocationList()?.Length ?? 0}");
            Debug.Log($"OnLevelUp subscribers: {expManager.OnLevelUp?.GetInvocationList()?.Length ?? 0}");
        }
    }
}
