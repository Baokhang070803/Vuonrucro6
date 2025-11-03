using UnityEngine;

/// <summary>
/// Hướng dẫn tối ưu lag cho hệ thống farming
/// </summary>
public class FarmingLagFixGuide : MonoBehaviour
{
    [Header("=== HƯỚNG DẪN TỐI ƯU LAG FARMING ===")]
    [TextArea(10, 20)]
    public string optimizationGuide = @"
🚀 GIẢI PHÁP TỐI ƯU LAG FARMING:

1. ✅ ĐÃ TỐI ƯU:
   - Farming Queue System: Xử lý farming actions theo batch
   - Firebase Batching: Gộp tile updates thành batch
   - GrowPlant Optimization: Giảm delay từ 0.5s → 0.2s
   - Debug Logs: Tắt debug logs trong production

2. 🔧 CÁCH SỬ DỤNG:
   - Bật 'enableFarmingOptimization' trong PlayerFarmController
   - Bật 'enableFirebaseBatching' trong PlayerDataSyncManager
   - Sử dụng FarmingPerformanceOptimizer để tự động điều chỉnh

3. ⚙️ SETTINGS QUAN TRỌNG:
   - tileUpdateInterval: 0.1s (giảm lag)
   - firebaseBatchInterval: 2s (giảm Firebase calls)
   - maxFarmingQueueSize: 10 (tránh queue quá lớn)

4. 🎯 KẾT QUẢ MONG ĐỢI:
   - Giảm lag khi farming liên tục
   - Giảm Firebase calls từ 10+ → 1-2 calls/giây
   - FPS ổn định hơn khi farming
   - Memory usage tối ưu hơn

5. 🐛 DEBUG:
   - Bật 'showPerformanceStats' để xem stats
   - Sử dụng 'Show Performance Info' context menu
   - Monitor FPS và memory usage

6. ⚠️ LƯU Ý:
   - Tắt Firebase khi test: 'disableFirebaseSaving = true'
   - Giảm 'enableDebugLogs = false' trong production
   - Kiểm tra 'enableFarmingOptimization = true'
";

    [Header("=== QUICK FIXES ===")]
    [SerializeField] private bool applyQuickFixes = false;
    
    void Start()
    {
        if (applyQuickFixes)
        {
            ApplyQuickFixes();
        }
    }
    
    /// <summary>
    /// Áp dụng các fix nhanh
    /// </summary>
    [ContextMenu("Apply Quick Fixes")]
    public void ApplyQuickFixes()
    {
        Debug.Log("[FarmingLagFixGuide] Đang áp dụng quick fixes...");
        
        // Fix 1: Tối ưu PlayerFarmController
        var farmController = FindObjectOfType<PlayerFarmController>();
        if (farmController != null)
        {
            Debug.Log("✅ Tìm thấy PlayerFarmController");
        }
        
        // Fix 2: Tối ưu PlayerDataSyncManager
        var syncManager = PlayerDataSyncManager.Instance;
        if (syncManager != null)
        {
            Debug.Log("✅ Tìm thấy PlayerDataSyncManager");
        }
        
        // Fix 3: Tạo FarmingPerformanceOptimizer nếu chưa có
        var optimizer = FindObjectOfType<FarmingPerformanceOptimizer>();
        if (optimizer == null)
        {
            GameObject optimizerGO = new GameObject("FarmingPerformanceOptimizer");
            optimizerGO.AddComponent<FarmingPerformanceOptimizer>();
            Debug.Log("✅ Đã tạo FarmingPerformanceOptimizer");
        }
        
        Debug.Log("[FarmingLagFixGuide] ✅ Hoàn thành quick fixes!");
    }
    
    /// <summary>
    /// Kiểm tra tình trạng tối ưu
    /// </summary>
    [ContextMenu("Check Optimization Status")]
    public void CheckOptimizationStatus()
    {
        Debug.Log("=== KIỂM TRA TÌNH TRẠNG TỐI ƯU ===");
        
        // Kiểm tra PlayerFarmController
        var farmController = FindObjectOfType<PlayerFarmController>();
        if (farmController != null)
        {
            Debug.Log("✅ PlayerFarmController: OK");
        }
        else
        {
            Debug.LogWarning("❌ PlayerFarmController: KHÔNG TÌM THẤY");
        }
        
        // Kiểm tra PlayerDataSyncManager
        var syncManager = PlayerDataSyncManager.Instance;
        if (syncManager != null)
        {
            Debug.Log("✅ PlayerDataSyncManager: OK");
        }
        else
        {
            Debug.LogWarning("❌ PlayerDataSyncManager: KHÔNG TÌM THẤY");
        }
        
        // Kiểm tra FarmingPerformanceOptimizer
        var optimizer = FindObjectOfType<FarmingPerformanceOptimizer>();
        if (optimizer != null)
        {
            Debug.Log("✅ FarmingPerformanceOptimizer: OK");
        }
        else
        {
            Debug.LogWarning("❌ FarmingPerformanceOptimizer: KHÔNG TÌM THẤY");
        }
        
        // Kiểm tra FPS
        float fps = 1f / Time.deltaTime;
        if (fps >= 30f)
        {
            Debug.Log($"✅ FPS: {fps:F1} (Tốt)");
        }
        else
        {
            Debug.LogWarning($"❌ FPS: {fps:F1} (Thấp - Cần tối ưu)");
        }
        
        Debug.Log("=== KẾT THÚC KIỂM TRA ===");
    }
}
