using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Manager quản lý EXP và Level của người chơi
/// </summary>
public class PlayerExpManager : MonoBehaviour
{
    public static PlayerExpManager Instance;
    
    [Header("EXP Settings")]
    public ExpData expData = new ExpData();
    
    [Header("Level Rewards")]
    public int statPointsPerLevel = 5;        // Điểm chỉ số mỗi cấp
    public int goldRewardPerLevel = 50;        // Vàng thưởng mỗi cấp
    public int diamondRewardPerLevel = 5;      // Kim cương thưởng mỗi cấp
    
    [Header("EXP Sources")]
    public int farmingExp = 10;               // EXP khi trồng cây
    public int harvestingExp = 15;             // EXP khi thu hoạch
    public int questExp = 100;                // EXP khi hoàn thành quest
    public int combatExp = 50;                 // EXP khi chiến đấu
    
    // Events
    public System.Action<int> OnLevelUp;           // Khi lên cấp (tham số: số cấp đã lên)
    public System.Action<int> OnExpGained;         // Khi nhận EXP (tham số: số EXP)
    public System.Action<ExpData> OnExpChanged;    // Khi EXP thay đổi
    
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
        // Load dữ liệu từ Firebase nếu có
        LoadExpFromFirebase();
        
        Debug.Log("[PlayerExpManager] Đã khởi tạo!");
    }
    
    /// <summary>
    /// Thêm EXP từ farming
    /// </summary>
    public void AddFarmingExp()
    {
        AddExp(farmingExp, "Farming");
    }
    
    /// <summary>
    /// Thêm EXP từ harvesting
    /// </summary>
    public void AddHarvestingExp()
    {
        AddExp(harvestingExp, "Harvesting");
    }
    
    /// <summary>
    /// Thêm EXP từ quest
    /// </summary>
    public void AddQuestExp()
    {
        AddExp(questExp, "Quest Completion");
    }
    
    /// <summary>
    /// Thêm EXP từ combat
    /// </summary>
    public void AddCombatExp()
    {
        AddExp(combatExp, "Combat Victory");
    }
    
    /// <summary>
    /// Thêm EXP với lý do cụ thể
    /// </summary>
    public void AddExp(int expAmount, string reason = "")
    {
        if (expAmount <= 0) return;
        
        int levelsGained = expData.AddExp(expAmount);
        
        Debug.Log($"[PlayerExpManager] Nhận {expAmount} EXP từ {reason}! Lên {levelsGained} cấp!");
        
        // Trigger events
        OnExpGained?.Invoke(expAmount);
        OnExpChanged?.Invoke(expData);
        
        if (levelsGained > 0)
        {
            OnLevelUp?.Invoke(levelsGained);
            
            // Thưởng khi lên cấp
            GiveLevelUpRewards(levelsGained);
            
            // Hiển thị thông báo lên cấp
            ShowLevelUpNotification(levelsGained);
        }
        
        // Lưu vào Firebase
        SaveExpToFirebase();
    }
    
    /// <summary>
    /// Thưởng khi lên cấp
    /// </summary>
    void GiveLevelUpRewards(int levelsGained)
    {
        int totalGoldReward = goldRewardPerLevel * levelsGained;
        int totalDiamondReward = diamondRewardPerLevel * levelsGained;
        int totalStatPoints = statPointsPerLevel * levelsGained;
        
        // Thêm vàng và kim cương
        if (PlayerGoldManager.Instance != null)
        {
            PlayerGoldManager.Instance.AddGold(totalGoldReward);
            PlayerGoldManager.Instance.AddDiamond(totalDiamondReward);
        }
        
        // Thêm điểm chỉ số
        expData.statPoints += totalStatPoints;
        
        // Trigger event để UI cập nhật skill points
        OnExpChanged?.Invoke(expData);
        
        Debug.Log($"[PlayerExpManager] Thưởng lên cấp: {totalGoldReward} vàng, {totalDiamondReward} kim cương, {totalStatPoints} điểm chỉ số!");
    }
    
    /// <summary>
    /// Hiển thị thông báo lên cấp
    /// </summary>
    void ShowLevelUpNotification(int levelsGained)
    {
        string message = levelsGained == 1 ? 
            $"LÊN CẤP {expData.currentLevel}!" : 
            $"LÊN {levelsGained} CẤP! Hiện tại cấp {expData.currentLevel}!";
        
        // Hiển thị qua DialogueManager nếu có
        if (DialogueManager.I != null)
        {
            DialogueManager.I.Show(new System.Collections.Generic.List<string> 
            { 
                message,
                $"Nhận được {statPointsPerLevel * levelsGained} điểm chỉ số!",
                $"Thưởng: {goldRewardPerLevel * levelsGained} vàng, {diamondRewardPerLevel * levelsGained} kim cương!",
                "Chúc mừng bạn đã tiến bộ!"
            });
        }
        
        Debug.Log($"[PlayerExpManager] {message}");
    }
    
    /// <summary>
    /// Lấy thông tin EXP hiện tại
    /// </summary>
    public ExpData GetExpData()
    {
        return expData;
    }
    
    /// <summary>
    /// Lấy cấp độ hiện tại
    /// </summary>
    public int GetCurrentLevel()
    {
        return expData.currentLevel;
    }
    
    /// <summary>
    /// Lấy EXP hiện tại
    /// </summary>
    public int GetCurrentExp()
    {
        return expData.currentExp;
    }
    
    /// <summary>
    /// Lấy EXP cần để lên cấp
    /// </summary>
    public int GetExpToNextLevel()
    {
        return expData.expToNextLevel;
    }
    
    /// <summary>
    /// Lấy phần trăm EXP
    /// </summary>
    public float GetExpPercentage()
    {
        return expData.GetExpPercentage();
    }
    
    /// <summary>
    /// Lấy số điểm chỉ số
    /// </summary>
    public int GetStatPoints()
    {
        return expData.statPoints;
    }
    
    /// <summary>
    /// Sử dụng điểm chỉ số
    /// </summary>
    public bool SpendStatPoint()
    {
        if (expData.statPoints > 0)
        {
            expData.statPoints--;
            OnExpChanged?.Invoke(expData);
            SaveExpToFirebase();
            Debug.Log($"[PlayerExpManager] Đã sử dụng 1 điểm chỉ số! Còn lại: {expData.statPoints}");
            return true;
        }
        
        Debug.LogWarning("[PlayerExpManager] Không có điểm chỉ số để sử dụng!");
        return false;
    }
    
    /// <summary>
    /// Load EXP từ Firebase
    /// </summary>
    void LoadExpFromFirebase()
    {
        if (LoadDataManager.firebaseUser == null)
        {
            Debug.LogWarning("[PlayerExpManager] FirebaseUser is null! Sử dụng dữ liệu mặc định.");
            return;
        }
        
        // Sử dụng PlayerDataSyncManager để load
        if (PlayerDataSyncManager.Instance != null)
        {
            PlayerDataSyncManager.Instance.LoadAllPlayerData();
            Debug.Log("[PlayerExpManager] Đã yêu cầu load dữ liệu từ PlayerDataSyncManager!");
        }
        else
        {
            Debug.LogWarning("[PlayerExpManager] PlayerDataSyncManager.Instance is null!");
        }
    }
    
    /// <summary>
    /// Lưu EXP vào Firebase
    /// </summary>
    void SaveExpToFirebase()
    {
        if (LoadDataManager.firebaseUser == null)
        {
            Debug.LogWarning("[PlayerExpManager] FirebaseUser is null! Không thể lưu EXP.");
            return;
        }
        
        // Sử dụng PlayerDataSyncManager để lưu
        if (PlayerDataSyncManager.Instance != null)
        {
            PlayerDataSyncManager.Instance.UpdateExpData(expData);
            Debug.Log("[PlayerExpManager] Đã gửi ExpData để lưu vào Firebase!");
        }
        else
        {
            Debug.LogWarning("[PlayerExpManager] PlayerDataSyncManager.Instance is null!");
        }
    }
    
    /// <summary>
    /// Debug: In thông tin EXP
    /// </summary>
    [ContextMenu("Debug EXP Info")]
    public void DebugExpInfo()
    {
        Debug.Log("=== EXP INFO ===");
        Debug.Log($"Level: {expData.currentLevel}");
        Debug.Log($"EXP: {expData.currentExp}/{expData.expToNextLevel}");
        Debug.Log($"Stat Points: {expData.statPoints}");
        Debug.Log($"Total EXP Earned: {expData.totalExpEarned}");
        Debug.Log($"EXP Percentage: {expData.GetExpPercentage():P1}");
    }
}
