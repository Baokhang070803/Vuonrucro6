using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Script tối ưu performance cho hệ thống farming
/// Giảm lag bằng cách tối ưu các operations
/// </summary>
public class FarmingPerformanceOptimizer : MonoBehaviour
{
    [Header("Performance Settings")]
    [SerializeField] private bool enableOptimization = true;
    [SerializeField] private float tileUpdateInterval = 0.1f; // Interval giữa các tile updates
    [SerializeField] private int maxConcurrentOperations = 3; // Số operations đồng thời tối đa
    
    [Header("Firebase Optimization")]
    [SerializeField] private bool enableFirebaseBatching = true;
    [SerializeField] private float firebaseBatchInterval = 2f; // Batch Firebase calls mỗi 2 giây
    [SerializeField] private int maxFirebaseBatchSize = 20; // Số tiles tối đa trong 1 batch
    
    [Header("Debug")]
    [SerializeField] private bool showPerformanceStats = false;
    
    // Performance tracking
    private int totalFarmingOperations = 0;
    private int totalFirebaseCalls = 0;
    private float lastPerformanceLogTime = 0f;
    
    // Singleton
    public static FarmingPerformanceOptimizer Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (enableOptimization)
        {
            Debug.Log("[FarmingPerformanceOptimizer] Đã bật tối ưu performance!");
        }
    }
    
    void Update()
    {
        if (showPerformanceStats && Time.time - lastPerformanceLogTime >= 5f)
        {
            LogPerformanceStats();
            lastPerformanceLogTime = Time.time;
        }
    }
    
    /// <summary>
    /// Tối ưu tile update với delay
    /// </summary>
    public IEnumerator OptimizedTileUpdate(Vector3Int position, System.Action<Vector3Int> updateAction)
    {
        if (!enableOptimization)
        {
            updateAction(position);
            yield break;
        }
        
        yield return new WaitForSeconds(tileUpdateInterval);
        updateAction(position);
        totalFarmingOperations++;
    }
    
    /// <summary>
    /// Tối ưu batch operations
    /// </summary>
    public IEnumerator ProcessBatchOperations(List<System.Action> operations)
    {
        if (!enableOptimization || operations.Count == 0)
        {
            foreach (var operation in operations)
            {
                operation?.Invoke();
            }
            yield break;
        }
        
        int processed = 0;
        while (processed < operations.Count)
        {
            int batchSize = Mathf.Min(maxConcurrentOperations, operations.Count - processed);
            
            for (int i = 0; i < batchSize; i++)
            {
                operations[processed + i]?.Invoke();
            }
            
            processed += batchSize;
            
            if (processed < operations.Count)
            {
                yield return new WaitForSeconds(tileUpdateInterval);
            }
        }
    }
    
    /// <summary>
    /// Tối ưu Firebase batch calls
    /// </summary>
    public void OptimizeFirebaseCall(System.Action firebaseAction)
    {
        if (!enableFirebaseBatching)
        {
            firebaseAction?.Invoke();
            return;
        }
        
        // Delay Firebase call để batch
        StartCoroutine(DelayedFirebaseCall(firebaseAction));
    }
    
    private IEnumerator DelayedFirebaseCall(System.Action firebaseAction)
    {
        yield return new WaitForSeconds(firebaseBatchInterval);
        firebaseAction?.Invoke();
        totalFirebaseCalls++;
    }
    
    /// <summary>
    /// Kiểm tra performance và tự động điều chỉnh
    /// </summary>
    public void CheckAndAdjustPerformance()
    {
        if (!enableOptimization) return;
        
        // Tự động điều chỉnh dựa trên performance
        float currentFPS = 1f / Time.deltaTime;
        
        if (currentFPS < 30f) // Nếu FPS thấp
        {
            tileUpdateInterval = Mathf.Min(tileUpdateInterval + 0.05f, 0.5f);
            maxConcurrentOperations = Mathf.Max(maxConcurrentOperations - 1, 1);
            
            Debug.Log($"[FarmingPerformanceOptimizer] FPS thấp ({currentFPS:F1}), điều chỉnh: interval={tileUpdateInterval}, maxOps={maxConcurrentOperations}");
        }
        else if (currentFPS > 50f) // Nếu FPS cao
        {
            tileUpdateInterval = Mathf.Max(tileUpdateInterval - 0.02f, 0.05f);
            maxConcurrentOperations = Mathf.Min(maxConcurrentOperations + 1, 5);
        }
    }
    
    /// <summary>
    /// Log performance stats
    /// </summary>
    private void LogPerformanceStats()
    {
        Debug.Log($"[FarmingPerformanceOptimizer] Stats - Operations: {totalFarmingOperations}, Firebase: {totalFirebaseCalls}, FPS: {1f/Time.deltaTime:F1}");
    }
    
    /// <summary>
    /// Reset performance stats
    /// </summary>
    public void ResetStats()
    {
        totalFarmingOperations = 0;
        totalFirebaseCalls = 0;
        Debug.Log("[FarmingPerformanceOptimizer] Đã reset performance stats");
    }
    
    /// <summary>
    /// Tối ưu memory usage
    /// </summary>
    public void OptimizeMemoryUsage()
    {
        if (!enableOptimization) return;
        
        // Force garbage collection nếu cần
        if (System.GC.GetTotalMemory(false) > 100 * 1024 * 1024) // Nếu > 100MB
        {
            System.GC.Collect();
            Debug.Log("[FarmingPerformanceOptimizer] Đã force garbage collection");
        }
    }
    
    /// <summary>
    /// Debug: Hiển thị performance info
    /// </summary>
    [ContextMenu("Show Performance Info")]
    public void ShowPerformanceInfo()
    {
        Debug.Log("=== FARMING PERFORMANCE INFO ===");
        Debug.Log($"Optimization Enabled: {enableOptimization}");
        Debug.Log($"Tile Update Interval: {tileUpdateInterval}s");
        Debug.Log($"Max Concurrent Operations: {maxConcurrentOperations}");
        Debug.Log($"Firebase Batching: {enableFirebaseBatching}");
        Debug.Log($"Firebase Batch Interval: {firebaseBatchInterval}s");
        Debug.Log($"Current FPS: {1f/Time.deltaTime:F1}");
        Debug.Log($"Total Operations: {totalFarmingOperations}");
        Debug.Log($"Total Firebase Calls: {totalFirebaseCalls}");
        Debug.Log("================================");
    }
}
