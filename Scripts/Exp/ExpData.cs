using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Cấu trúc dữ liệu EXP và Level
/// </summary>
[System.Serializable]
public class ExpData
{
    [Header("Level Info")]
    public int currentLevel = 1;           // Cấp độ hiện tại
    public int currentExp = 0;             // EXP hiện tại
    public int expToNextLevel = 100;       // EXP cần để lên cấp tiếp theo
    public int totalExpEarned = 0;         // Tổng EXP đã kiếm được
    
    [Header("Skill System")]
    public int statPoints = 0;           // Điểm chỉ số chưa sử dụng
    
    [Header("Level Progression")]
    public float expMultiplier = 1.2f;     // Hệ số tăng EXP cần thiết mỗi cấp
    public int baseExpRequired = 100;      // EXP cơ bản để lên cấp 2
    
    /// <summary>
    /// Constructor mặc định
    /// </summary>
    public ExpData()
    {
        currentLevel = 1;
        currentExp = 0;
        expToNextLevel = baseExpRequired;
        totalExpEarned = 0;
        statPoints = 0;
    }
    
    /// <summary>
    /// Constructor với tham số
    /// </summary>
    public ExpData(int level, int exp, int statPoints = 0)
    {
        this.currentLevel = level;
        this.currentExp = exp;
        this.statPoints = statPoints;
        this.totalExpEarned = CalculateTotalExpEarned();
        this.expToNextLevel = CalculateExpToNextLevel();
    }
    
    /// <summary>
    /// Thêm EXP và trả về số cấp đã lên
    /// </summary>
    public int AddExp(int expAmount)
    {
        if (expAmount <= 0) return 0;
        
        int levelsGained = 0;
        int remainingExp = expAmount;
        
        // Thêm vào tổng EXP
        totalExpEarned += expAmount;
        
        // Xử lý lên cấp
        while (remainingExp > 0)
        {
            int expNeeded = expToNextLevel - currentExp;
            
            if (remainingExp >= expNeeded)
            {
                // Đủ EXP để lên cấp
                remainingExp -= expNeeded;
                currentExp = 0;
                currentLevel++;
                levelsGained++;
                
                // Tính EXP cần cho cấp tiếp theo
                expToNextLevel = CalculateExpToNextLevel();
            }
            else
            {
                // Chưa đủ EXP để lên cấp
                currentExp += remainingExp;
                remainingExp = 0;
            }
        }
        
        return levelsGained;
    }
    
    /// <summary>
    /// Tính EXP cần để lên cấp tiếp theo
    /// </summary>
    int CalculateExpToNextLevel()
    {
        // Công thức: baseExpRequired * (expMultiplier ^ (currentLevel - 1))
        return Mathf.RoundToInt(baseExpRequired * Mathf.Pow(expMultiplier, currentLevel - 1));
    }
    
    /// <summary>
    /// Tính tổng EXP đã kiếm được dựa trên level hiện tại
    /// </summary>
    int CalculateTotalExpEarned()
    {
        int total = 0;
        
        // Tính tổng EXP của các cấp trước đó
        for (int level = 1; level < currentLevel; level++)
        {
            total += Mathf.RoundToInt(baseExpRequired * Mathf.Pow(expMultiplier, level - 1));
        }
        
        // Cộng thêm EXP hiện tại
        total += currentExp;
        
        return total;
    }
    
    /// <summary>
    /// Lấy phần trăm EXP hiện tại
    /// </summary>
    public float GetExpPercentage()
    {
        if (expToNextLevel <= 0) return 1f;
        return (float)currentExp / expToNextLevel;
    }
    
    /// <summary>
    /// Lấy EXP cần để lên cấp tiếp theo
    /// </summary>
    public int GetExpToNextLevel()
    {
        return expToNextLevel - currentExp;
    }
    
    /// <summary>
    /// Kiểm tra có thể lên cấp không
    /// </summary>
    public bool CanLevelUp()
    {
        return currentExp >= expToNextLevel;
    }
    
    /// <summary>
    /// Lấy thông tin cấp độ dưới dạng string
    /// </summary>
    public string GetLevelString()
    {
        return currentLevel.ToString();
    }
    
    /// <summary>
    /// Lấy thông tin EXP dưới dạng string
    /// </summary>
    public string GetExpString()
    {
        return $"{currentExp}/{expToNextLevel}";
    }
    
    /// <summary>
    /// Lấy thông tin phần trăm EXP dưới dạng string
    /// </summary>
    public string GetExpPercentageString()
    {
        return $"{GetExpPercentage():P1}";
    }
    
    /// <summary>
    /// Lấy thông tin điểm chỉ số dưới dạng string
    /// </summary>
    public string GetStatPointsString()
    {
        return statPoints > 0 ? $"{statPoints} điểm" : "0 điểm";
    }
    
    /// <summary>
    /// Serialize thành JSON
    /// </summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
    
    /// <summary>
    /// Deserialize từ JSON
    /// </summary>
    public static ExpData FromJson(string json)
    {
        try
        {
            ExpData data = JsonUtility.FromJson<ExpData>(json);
            // Đảm bảo tính toán lại các giá trị phụ thuộc
            data.expToNextLevel = data.CalculateExpToNextLevel();
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[ExpData] Lỗi parse JSON: {e.Message}");
            return new ExpData();
        }
    }
    
    /// <summary>
    /// Clone ExpData
    /// </summary>
    public ExpData Clone()
    {
        ExpData clone = new ExpData();
        clone.currentLevel = this.currentLevel;
        clone.currentExp = this.currentExp;
        clone.expToNextLevel = this.expToNextLevel;
        clone.totalExpEarned = this.totalExpEarned;
        clone.statPoints = this.statPoints;
        clone.expMultiplier = this.expMultiplier;
        clone.baseExpRequired = this.baseExpRequired;
        return clone;
    }
    
    /// <summary>
    /// Debug: In thông tin ExpData
    /// </summary>
    public void DebugInfo()
    {
        Debug.Log("=== EXP DATA INFO ===");
        Debug.Log($"Level: {currentLevel}");
        Debug.Log($"Current EXP: {currentExp}");
        Debug.Log($"EXP to Next Level: {expToNextLevel}");
        Debug.Log($"Total EXP Earned: {totalExpEarned}");
        Debug.Log($"Stat Points: {statPoints}");
        Debug.Log($"EXP Percentage: {GetExpPercentage():P1}");
        Debug.Log($"Can Level Up: {CanLevelUp()}");
    }
}