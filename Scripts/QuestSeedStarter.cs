using UnityEngine;

/// <summary>
/// Script để thêm hạt giống vào túi khi bắt đầu quest
/// </summary>
public class QuestSeedStarter : MonoBehaviour
{
    [Header("Quest Settings")]
    public string questName = "Những Hạt Mầm Đầu Tiên";
    public int seedQuantity = 10;
    public string seedName = "Hạt Giống Cơ Bản";
    
    [Header("Seed Icon")]
    public Sprite seedIcon;
    
    private bool hasGivenSeeds = false;
    
    void Start()
    {
        // Kiểm tra quest và thêm hạt giống
        CheckAndGiveSeeds();
    }
    
    void Update()
    {
        // Kiểm tra lại mỗi frame để đảm bảo quest được kích hoạt
        if (!hasGivenSeeds)
        {
            CheckAndGiveSeeds();
        }
    }
    
    void CheckAndGiveSeeds()
    {
        // Kiểm tra quest có được kích hoạt không
        if (QuestManager.Instance != null && 
            QuestManager.Instance.CanDoQuest(questName) && 
            !hasGivenSeeds)
        {
            // Thêm hạt giống vào túi
            if (BagManager.Instance != null)
            {
                bool success = BagManager.Instance.AddItem(seedName, seedIcon, seedQuantity, 5); // Giá 5 vàng mỗi hạt
                
                if (success)
                {
                    hasGivenSeeds = true;
                    Debug.Log($"✓ Đã thêm {seedQuantity} {seedName} vào túi cho quest {questName}!");
                }
                else
                {
                    Debug.LogWarning($"⚠ Không thể thêm {seedName} vào túi (túi đầy?)");
                }
            }
            else
            {
                Debug.LogWarning("⚠ BagManager.Instance is null! Không thể thêm hạt giống.");
            }
        }
    }
    
    // Method để reset (dùng cho testing)
    [ContextMenu("Reset Seed Given")]
    public void ResetSeedGiven()
    {
        hasGivenSeeds = false;
        Debug.Log("✓ Đã reset trạng thái thêm hạt giống!");
    }
    
    // Method để thêm hạt giống thủ công
    [ContextMenu("Give Seeds Manually")]
    public void GiveSeedsManually()
    {
        if (BagManager.Instance != null)
        {
            bool success = BagManager.Instance.AddItem(seedName, seedIcon, seedQuantity, 5);
            
            if (success)
            {
                hasGivenSeeds = true;
                Debug.Log($"✓ Đã thêm {seedQuantity} {seedName} vào túi thủ công!");
            }
            else
            {
                Debug.LogWarning($"⚠ Không thể thêm {seedName} vào túi!");
            }
        }
        else
        {
            Debug.LogWarning("⚠ BagManager.Instance is null!");
        }
    }
}
