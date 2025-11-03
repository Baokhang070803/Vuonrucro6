using UnityEngine;

/// <summary>
/// Script áp dụng chỉ số vào player movement
/// </summary>
public class PlayerStatApplier : MonoBehaviour
{
    [Header("Player Components")]
    public nvnu1dituyen playerMovement; // Script di chuyển của player
    
    [Header("Base Values")]
    public float baseMoveSpeed = 5f; // Tốc độ di chuyển cơ bản
    
    private PlayerStatsManager statsManager;
    
    void Start()
    {
        // Tìm player movement script
        if (playerMovement == null)
            playerMovement = GetComponent<nvnu1dituyen>();
            
        // Tìm stats manager
        statsManager = PlayerStatsManager.Instance;
        
        if (statsManager == null)
        {
            Debug.LogWarning("[PlayerStatApplier] PlayerStatsManager.Instance is null!");
        }
        
        Debug.Log("[PlayerStatApplier] Đã khởi tạo!");
    }
    
    void Update()
    {
        ApplyStatsToPlayer();
    }
    
    /// <summary>
    /// Áp dụng chỉ số vào player
    /// </summary>
    void ApplyStatsToPlayer()
    {
        if (statsManager == null || playerMovement == null) return;
        
        var stats = statsManager.GetPlayerStats();
        
        // Áp dụng Agility vào tốc độ di chuyển
        float newSpeed = baseMoveSpeed * stats.speedMultiplier;
        playerMovement.moveSpeed = newSpeed;
        
        // Có thể thêm các hiệu ứng khác ở đây:
        // - Strength: ảnh hưởng đến farming speed
        // - Intelligence: ảnh hưởng đến EXP gain
        // - Vitality: ảnh hưởng đến health (nếu có)
    }
    
    /// <summary>
    /// Debug: Hiển thị thông tin áp dụng stats
    /// </summary>
    [ContextMenu("Debug Show Applied Stats")]
    public void DebugShowAppliedStats()
    {
        if (statsManager == null || playerMovement == null) return;
        
        var stats = statsManager.GetPlayerStats();
        Debug.Log($"=== APPLIED STATS ===");
        Debug.Log($"Base Speed: {baseMoveSpeed}");
        Debug.Log($"Speed Multiplier: {stats.speedMultiplier:F2}x");
        Debug.Log($"Current Speed: {playerMovement.moveSpeed:F2}");
        Debug.Log($"Damage Multiplier: {stats.damageMultiplier:F2}x");
        Debug.Log($"EXP Multiplier: {stats.expMultiplier:F2}x");
        Debug.Log($"Health Multiplier: {stats.healthMultiplier:F2}x");
    }
}
