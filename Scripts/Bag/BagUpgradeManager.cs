using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manager quản lý hệ thống nâng cấp balo
/// </summary>
public class BagUpgradeManager : MonoBehaviour
{
    public static BagUpgradeManager Instance;
    
    [Header("Bag Upgrade Settings")]
    public int baseBagCapacity = 20;        // Số ô ban đầu
    public int maxBagCapacity = 100;        // Số ô tối đa
    public int currentBagLevel = 1;          // Cấp độ hiện tại
    public int maxBagLevel = 10;             // Cấp độ tối đa
    
    [Header("Upgrade Settings")]
    public float upgradeCooldown = 1.0f; // Cooldown giữa các lần nâng cấp
    private float lastUpgradeTime = 0f;  // Thời gian nâng cấp cuối cùng
    
    [Header("Upgrade Costs")]
    public List<int> upgradeCosts = new List<int> { 100, 200, 400, 800, 1600, 3200, 6400, 12800, 25600, 51200 }; // Chi phí nâng cấp từng cấp
    
    [Header("UI References")]
    public GameObject upgradePanel;           // Panel nâng cấp balo
    public Button upgradeButton;             // Nút nâng cấp
    public Button closeUpgradeButton;        // Nút đóng panel
    public TextMeshProUGUI currentCapacityText;  // Text hiển thị số ô hiện tại
    public TextMeshProUGUI nextCapacityText;     // Text hiển thị số ô sau nâng cấp
    public TextMeshProUGUI upgradeCostText;      // Text hiển thị chi phí nâng cấp
    public TextMeshProUGUI bagLevelText;         // Text hiển thị cấp độ balo
    public TextMeshProUGUI upgradeStatusText;    // Text hiển thị trạng thái nâng cấp
    
    [Header("Bag Integration")]
    public BagManager bagManager;            // Reference đến BagManager
    
    // Events
    public System.Action<int> OnBagUpgraded; // Event khi balo được nâng cấp
    
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
        InitializeUpgradeSystem();
        SetupUI();
        LoadBagLevelFromFirebase();
        
        // Kiểm tra nếu quay về từ PvP thì đóng panel
        string combatFlag = PlayerPrefs.GetString("JustFinishedCombat", "false");
        if (combatFlag == "true")
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
                Debug.Log("[BagUpgradeManager] Quay về từ PvP, đã đóng upgradePanel!");
            }
            return; // Không active Canvas
        }
        
        // Đảm bảo Canvas luôn Active (chỉ khi không quay về từ PvP)
        if (upgradePanel != null)
        {
            Canvas parentCanvas = upgradePanel.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                parentCanvas.gameObject.SetActive(true);
                Debug.Log($"[BagUpgradeManager] Đã Active parent Canvas: {parentCanvas.name}");
            }
        }
    }
    
    /// <summary>
    /// Khởi tạo hệ thống nâng cấp
    /// </summary>
    void InitializeUpgradeSystem()
    {
        // Khởi tạo chi phí nâng cấp nếu chưa có
        if (upgradeCosts.Count == 0)
        {
            upgradeCosts = new List<int>();
            for (int i = 0; i < maxBagLevel; i++)
            {
                upgradeCosts.Add(100 * (int)Mathf.Pow(2, i)); // Chi phí tăng gấp đôi mỗi cấp
            }
        }
        
        Debug.Log("[BagUpgradeManager] Đã khởi tạo hệ thống nâng cấp balo!");
    }
    
    /// <summary>
    /// Setup UI
    /// </summary>
    void SetupUI()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
            
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeBag);
            
        if (closeUpgradeButton != null)
            closeUpgradeButton.onClick.AddListener(CloseUpgradePanel);
            
        UpdateUpgradeUI();
    }
    
    /// <summary>
    /// Mở panel nâng cấp balo
    /// </summary>
    public void OpenUpgradePanel()
    {
        Debug.Log($"[BagUpgradeManager] upgradePanel = {(upgradePanel != null ? upgradePanel.name : "NULL")}");
        
        if (upgradePanel != null)
        {
            Debug.Log($"[BagUpgradeManager] upgradePanel.activeInHierarchy = {upgradePanel.activeInHierarchy}");
            
            // Active cả parent Canvas nếu cần
            Canvas parentCanvas = upgradePanel.GetComponentInParent<Canvas>();
            if (parentCanvas != null && !parentCanvas.gameObject.activeInHierarchy)
            {
                Debug.Log($"[BagUpgradeManager] Active parent Canvas: {parentCanvas.name}");
                parentCanvas.gameObject.SetActive(true);
            }
            
            upgradePanel.SetActive(true);
            Debug.Log($"[BagUpgradeManager] Sau SetActive(true), activeInHierarchy = {upgradePanel.activeInHierarchy}");
            UpdateUpgradeUI();
            Debug.Log("[BagUpgradeManager] Đã mở panel nâng cấp balo!");
        }
        else
        {
            Debug.LogError("[BagUpgradeManager] upgradePanel is NULL! Hãy gán BagUpgradePanel vào Inspector!");
        }
    }
    
    /// <summary>
    /// Đóng panel nâng cấp balo
    /// </summary>
    public void CloseUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            Debug.Log("[BagUpgradeManager] Đã đóng panel nâng cấp balo!");
        }
    }
    
    /// <summary>
    /// Nâng cấp balo
    /// </summary>
    public void UpgradeBag()
    {
        // Kiểm tra cooldown
        if (Time.time - lastUpgradeTime < upgradeCooldown)
        {
            Debug.LogWarning($"[BagUpgradeManager] Đang trong cooldown! Còn {(upgradeCooldown - (Time.time - lastUpgradeTime)):F1}s");
            UpdateUpgradeStatusText("Nâng cấp thành công!");
            return;
        }
        
        Debug.Log($"[BagUpgradeManager] UpgradeBag() được gọi! Cấp hiện tại: {currentBagLevel}");
        
        if (currentBagLevel >= maxBagLevel)
        {
            Debug.LogWarning("[BagUpgradeManager] Balo đã đạt cấp độ tối đa!");
            UpdateUpgradeStatusText("Balo đã đạt cấp độ tối đa!");
            return;
        }
        
        int upgradeCost = GetUpgradeCost();
        Debug.Log($"[BagUpgradeManager] Chi phí nâng cấp: {upgradeCost} vàng");
        
        // Kiểm tra đủ vàng không
        if (PlayerGoldManager.Instance != null)
        {
            int currentGold = PlayerGoldManager.Instance.GetGold();
            Debug.Log($"[BagUpgradeManager] Vàng hiện có: {currentGold}");
            
            if (currentGold >= upgradeCost)
            {
                // Trừ vàng
                if (PlayerGoldManager.Instance.SpendGold(upgradeCost))
                {
                    // Cập nhật thời gian nâng cấp
                    lastUpgradeTime = Time.time;
                    
                    // Nâng cấp balo
                    currentBagLevel++;
                    int newCapacity = GetBagCapacity();
                    Debug.Log($"[BagUpgradeManager] Nâng cấp thành công! Cấp mới: {currentBagLevel}, Số ô mới: {newCapacity}");
                    
                    // Cập nhật BagManager
                    if (bagManager != null)
                    {
                        bagManager.SetMaxBagSlots(newCapacity);
                        Debug.Log($"[BagUpgradeManager] Đã cập nhật BagManager với {newCapacity} slots");
                    }
                    
                    // Lưu vào Firebase
                    SaveBagLevelToFirebase();
                    
                    // Cập nhật UI
                    UpdateUpgradeUI();
                    
                    // Trigger event
                    OnBagUpgraded?.Invoke(newCapacity);
                    
                    Debug.Log($"[BagUpgradeManager] Đã nâng cấp balo lên cấp {currentBagLevel}! Số ô: {newCapacity}");
                    UpdateUpgradeStatusText($"Nâng cấp thành công! Balo cấp {currentBagLevel}");
                }
                else
                {
                    Debug.LogWarning("[BagUpgradeManager] Không thể trừ vàng!");
                    UpdateUpgradeStatusText("Không thể trừ vàng!");
                }
            }
            else
            {
                Debug.LogWarning("[BagUpgradeManager] Không đủ vàng để nâng cấp!");
                UpdateUpgradeStatusText("Không đủ vàng để nâng cấp!");
            }
        }
        else
        {
            Debug.LogError("[BagUpgradeManager] PlayerGoldManager.Instance is null!");
        }
    }
    
    /// <summary>
    /// Lấy số ô balo hiện tại
    /// </summary>
    public int GetBagCapacity()
    {
        return baseBagCapacity + (currentBagLevel - 1) * 2; // Mỗi cấp tăng 2 ô
    }
    
    /// <summary>
    /// Lấy số ô balo sau nâng cấp
    /// </summary>
    public int GetNextBagCapacity()
    {
        if (currentBagLevel >= maxBagLevel)
            return GetBagCapacity();
        return baseBagCapacity + currentBagLevel * 2; // Mỗi cấp tăng 2 ô
    }
    
    /// <summary>
    /// Lấy chi phí nâng cấp
    /// </summary>
    public int GetUpgradeCost()
    {
        if (currentBagLevel >= maxBagLevel)
            return 0;
        return upgradeCosts[currentBagLevel - 1];
    }
    
    /// <summary>
    /// Cập nhật UI nâng cấp
    /// </summary>
    void UpdateUpgradeUI()
    {
        int currentCapacity = GetBagCapacity();
        int nextCapacity = GetNextBagCapacity();
        int upgradeCost = GetUpgradeCost();
        
        if (currentCapacityText != null)
            currentCapacityText.text = $"Số ô hiện tại: {currentCapacity}";
            
        if (nextCapacityText != null)
            nextCapacityText.text = $"Số ô sau nâng cấp: {nextCapacity}";
            
        if (upgradeCostText != null)
            upgradeCostText.text = $"Chi phí: {upgradeCost} vàng";
            
        if (bagLevelText != null)
            bagLevelText.text = $"Cấp độ: {currentBagLevel}/{maxBagLevel}";
            
        // Cập nhật trạng thái nút nâng cấp
        if (upgradeButton != null)
        {
            bool canUpgrade = currentBagLevel < maxBagLevel && 
                            PlayerGoldManager.Instance != null && 
                            PlayerGoldManager.Instance.GetGold() >= upgradeCost;
            upgradeButton.interactable = canUpgrade;
        }
        
        UpdateUpgradeStatusText("");
    }
    
    /// <summary>
    /// Cập nhật text trạng thái nâng cấp
    /// </summary>
    void UpdateUpgradeStatusText(string message)
    {
        if (upgradeStatusText != null)
        {
            upgradeStatusText.text = message;
        }
    }
    
    /// <summary>
    /// Lưu cấp độ balo vào Firebase
    /// </summary>
    void SaveBagLevelToFirebase()
    {
        if (PlayerDataSyncManager.Instance != null)
        {
            // Tạo BagUpgradeData
            BagUpgradeData upgradeData = new BagUpgradeData(currentBagLevel, GetBagCapacity());
            PlayerDataSyncManager.Instance.UpdateBagUpgradeData(upgradeData);
            Debug.Log("[BagUpgradeManager] Đã lưu cấp độ balo vào Firebase!");
        }
        else
        {
            Debug.LogWarning("[BagUpgradeManager] PlayerDataSyncManager.Instance is null!");
        }
    }
    
    /// <summary>
    /// Load cấp độ balo từ Firebase
    /// </summary>
    public void LoadBagLevelFromFirebase()
    {
        // TODO: Load từ Firebase khi có PlayerDataSyncManager
        Debug.Log($"[BagUpgradeManager] Load cấp độ balo: {currentBagLevel}");
        
        // Cập nhật BagManager
        if (bagManager != null)
        {
            bagManager.SetMaxBagSlots(GetBagCapacity());
        }
        
        UpdateUpgradeUI();
    }
    
    /// <summary>
    /// Load dữ liệu nâng cấp từ BagUpgradeData (được gọi bởi PlayerDataSyncManager)
    /// </summary>
    public void LoadUpgradeData(BagUpgradeData upgradeData)
    {
        if (upgradeData == null)
        {
            Debug.LogWarning("[BagUpgradeManager] BagUpgradeData is null!");
            return;
        }
        
        // Cập nhật cấp độ balo
        currentBagLevel = upgradeData.bagLevel;
        
        Debug.Log($"[BagUpgradeManager] Đã load BagUpgradeData: Level {currentBagLevel}");
        
        // Cập nhật BagManager
        if (bagManager != null)
        {
            bagManager.SetMaxBagSlots(GetBagCapacity());
        }
        
        // Cập nhật UI
        UpdateUpgradeUI();
        
        // Trigger event
        OnBagUpgraded?.Invoke(currentBagLevel);
    }
    
    /// <summary>
    /// Debug: In thông tin nâng cấp balo
    /// </summary>
    [ContextMenu("Debug Bag Upgrade Info")]
    public void DebugBagUpgradeInfo()
    {
        Debug.Log("=== BAG UPGRADE INFO ===");
        Debug.Log($"Cấp độ hiện tại: {currentBagLevel}/{maxBagLevel}");
        Debug.Log($"Số ô hiện tại: {GetBagCapacity()}");
        Debug.Log($"Chi phí nâng cấp: {GetUpgradeCost()} vàng");
        Debug.Log($"Vàng hiện có: {(PlayerGoldManager.Instance != null ? PlayerGoldManager.Instance.GetGold() : 0)}");
    }
}

