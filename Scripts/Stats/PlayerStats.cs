using UnityEngine;

/// <summary>
/// Hệ thống chỉ số của người chơi
/// </summary>
[System.Serializable]
public class PlayerStats
{
    [Header("Chỉ số cơ bản")]
    public int strength = 0;        // Sức mạnh - ảnh hưởng đến sát thương
    public int agility = 0;         // Nhanh nhẹn - ảnh hưởng đến tốc độ di chuyển
    public int intelligence = 0;    // Trí tuệ - ảnh hưởng đến EXP bonus
    public int vitality = 0;        // Thể lực - ảnh hưởng đến máu
    
    [Header("Chỉ số phái sinh")]
    public float damageMultiplier = 1f;     // Hệ số sát thương
    public float speedMultiplier = 1f;      // Hệ số tốc độ
    public float expMultiplier = 1f;       // Hệ số EXP
    public float healthMultiplier = 1f;    // Hệ số máu
    
    /// <summary>
    /// Tăng chỉ số Strength
    /// </summary>
    public void IncreaseStrength()
    {
        strength++;
        UpdateDerivedStats();
        Debug.Log($"[PlayerStats] Tăng Strength lên {strength}");
    }
    
    /// <summary>
    /// Tăng chỉ số Agility
    /// </summary>
    public void IncreaseAgility()
    {
        agility++;
        UpdateDerivedStats();
        Debug.Log($"[PlayerStats] Tăng Agility lên {agility}");
    }
    
    /// <summary>
    /// Tăng chỉ số Intelligence
    /// </summary>
    public void IncreaseIntelligence()
    {
        intelligence++;
        UpdateDerivedStats();
        Debug.Log($"[PlayerStats] Tăng Intelligence lên {intelligence}");
    }
    
    /// <summary>
    /// Tăng chỉ số Vitality
    /// </summary>
    public void IncreaseVitality()
    {
        vitality++;
        UpdateDerivedStats();
        Debug.Log($"[PlayerStats] Tăng Vitality lên {vitality}");
    }
    
    /// <summary>
    /// Cập nhật các chỉ số phái sinh dựa trên chỉ số cơ bản
    /// </summary>
    void UpdateDerivedStats()
    {
        // Strength: +5% damage mỗi điểm
        damageMultiplier = 1f + (strength * 0.05f);
        
        // Agility: +3% speed mỗi điểm
        speedMultiplier = 1f + (agility * 0.03f);
        
        // Intelligence: +10% EXP mỗi điểm
        expMultiplier = 1f + (intelligence * 0.1f);
        
        // Vitality: +20% health mỗi điểm
        healthMultiplier = 1f + (vitality * 0.2f);
        
        Debug.Log($"[PlayerStats] Cập nhật chỉ số phái sinh - Damage: {damageMultiplier:F2}x, Speed: {speedMultiplier:F2}x, EXP: {expMultiplier:F2}x, Health: {healthMultiplier:F2}x");
    }
    
    /// <summary>
    /// Lấy tổng điểm đã sử dụng
    /// </summary>
    public int GetTotalSpentPoints()
    {
        return strength + agility + intelligence + vitality;
    }
    
    /// <summary>
    /// Reset tất cả chỉ số về 0
    /// </summary>
    public void ResetStats()
    {
        strength = 0;
        agility = 0;
        intelligence = 0;
        vitality = 0;
        UpdateDerivedStats();
        Debug.Log("[PlayerStats] Đã reset tất cả chỉ số về 0");
    }
    
    /// <summary>
    /// Lấy thông tin chỉ số dưới dạng string
    /// </summary>
    public string GetStatsString()
    {
        return $"Strength: {strength}\nAgility: {agility}\nIntelligence: {intelligence}\nVitality: {vitality}";
    }
}
