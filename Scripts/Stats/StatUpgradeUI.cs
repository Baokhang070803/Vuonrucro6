using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// UI để sử dụng điểm chỉ số
/// </summary>
public class StatUpgradeUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject statPanel; // Panel chứa toàn bộ UI stats
    public Button openStatsButton; // Nút mở stats panel
    
    [Header("Stat Buttons")]
    public Button strengthButton;
    public Button agilityButton;
    public Button intelligenceButton;
    public Button vitalityButton;
    
    [Header("Stat Text")]
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI agilityText;
    public TextMeshProUGUI intelligenceText;
    public TextMeshProUGUI vitalityText;
    
    [Header("Info Text")]
    public TextMeshProUGUI statPointsText; // Hiển thị số điểm chỉ số còn lại
    public TextMeshProUGUI statInfoText; // Hiển thị thông tin chỉ số
    
    [Header("Close Button")]
    public Button closeButton;
    
    private bool isStatsOpen = false;
    
    void Start()
    {
        InitializeUI();
        SetupEvents();
        UpdateStatDisplay();
    }
    
    void Update()
    {
        // Đóng stats panel khi ấn Escape (sử dụng Input System)
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && isStatsOpen)
        {
            CloseStatsPanel();
        }
    }
    
    void InitializeUI()
    {
        // Ẩn panel ban đầu
        if (statPanel != null)
            statPanel.SetActive(false);
            
        Debug.Log("[StatUpgradeUI] Đã khởi tạo!");
    }
    
    void SetupEvents()
    {
        // Setup nút mở stats
        if (openStatsButton != null)
            openStatsButton.onClick.AddListener(ToggleStatsPanel);
            
        // Setup nút đóng stats
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseStatsPanel);
            
        // Setup nút nâng cấp chỉ số
        if (strengthButton != null)
            strengthButton.onClick.AddListener(UpgradeStrength);
            
        if (agilityButton != null)
            agilityButton.onClick.AddListener(UpgradeAgility);
            
        if (intelligenceButton != null)
            intelligenceButton.onClick.AddListener(UpgradeIntelligence);
            
        if (vitalityButton != null)
            vitalityButton.onClick.AddListener(UpgradeVitality);
    }
    
    /// <summary>
    /// Mở/đóng stats panel
    /// </summary>
    public void ToggleStatsPanel()
    {
        if (isStatsOpen)
            CloseStatsPanel();
        else
            OpenStatsPanel();
    }
    
    /// <summary>
    /// Mở stats panel
    /// </summary>
    public void OpenStatsPanel()
    {
        if (statPanel != null)
        {
            statPanel.SetActive(true);
            isStatsOpen = true;
            UpdateStatDisplay();
            Debug.Log("[StatUpgradeUI] Đã mở stats panel!");
        }
    }
    
    /// <summary>
    /// Đóng stats panel
    /// </summary>
    public void CloseStatsPanel()
    {
        if (statPanel != null)
        {
            statPanel.SetActive(false);
            isStatsOpen = false;
            Debug.Log("[StatUpgradeUI] Đã đóng stats panel!");
        }
    }
    
    /// <summary>
    /// Cập nhật hiển thị chỉ số
    /// </summary>
    void UpdateStatDisplay()
    {
        if (PlayerStatsManager.Instance == null || PlayerExpManager.Instance == null) return;
        
        var stats = PlayerStatsManager.Instance.GetPlayerStats();
        int availablePoints = PlayerExpManager.Instance.GetStatPoints();
        
        // Cập nhật text chỉ số
        if (strengthText != null)
            strengthText.text = $"Sức mạnh: {stats.strength}";
            
        if (agilityText != null)
            agilityText.text = $"Nhanh nhẹn: {stats.agility}";
            
        if (intelligenceText != null)
            intelligenceText.text = $"Trí tuệ: {stats.intelligence}";
            
        if (vitalityText != null)
            vitalityText.text = $"Thể lực: {stats.vitality}";
            
        // Cập nhật số điểm chỉ số còn lại
        if (statPointsText != null)
            statPointsText.text = $"Điểm chỉ số: {availablePoints}";
            
        // Cập nhật thông tin chỉ số
        if (statInfoText != null)
        {
            statInfoText.text = $"Sát thương: {stats.damageMultiplier:F2}x\n" +
                               $"Tốc độ: {stats.speedMultiplier:F2}x\n" +
                               $"EXP: {stats.expMultiplier:F2}x\n" +
                               $"Máu: {stats.healthMultiplier:F2}x";
        }
        
        // Enable/disable nút dựa trên số điểm còn lại
        bool canUpgrade = availablePoints > 0;
        
        if (strengthButton != null)
            strengthButton.interactable = canUpgrade;
            
        if (agilityButton != null)
            agilityButton.interactable = canUpgrade;
            
        if (intelligenceButton != null)
            intelligenceButton.interactable = canUpgrade;
            
        if (vitalityButton != null)
            vitalityButton.interactable = canUpgrade;
    }
    
    /// <summary>
    /// Nâng cấp Strength
    /// </summary>
    void UpgradeStrength()
    {
        if (PlayerStatsManager.Instance.UpgradeStrength())
        {
            UpdateStatDisplay();
            ShowUpgradeMessage("Sức mạnh", "Sát thương +5%");
        }
    }
    
    /// <summary>
    /// Nâng cấp Agility
    /// </summary>
    void UpgradeAgility()
    {
        if (PlayerStatsManager.Instance.UpgradeAgility())
        {
            UpdateStatDisplay();
            ShowUpgradeMessage("Nhanh nhẹn", "Tốc độ +3%");
        }
    }
    
    /// <summary>
    /// Nâng cấp Intelligence
    /// </summary>
    void UpgradeIntelligence()
    {
        if (PlayerStatsManager.Instance.UpgradeIntelligence())
        {
            UpdateStatDisplay();
            ShowUpgradeMessage("Trí tuệ", "EXP +10%");
        }
    }
    
    /// <summary>
    /// Nâng cấp Vitality
    /// </summary>
    void UpgradeVitality()
    {
        if (PlayerStatsManager.Instance.UpgradeVitality())
        {
            UpdateStatDisplay();
            ShowUpgradeMessage("Thể lực", "Máu +20%");
        }
    }
    
    /// <summary>
    /// Hiển thị thông báo nâng cấp
    /// </summary>
    void ShowUpgradeMessage(string statName, string effect)
    {
        Debug.Log($"🎉 Đã nâng cấp {statName}! {effect}");
        
        // Có thể thêm popup UI đẹp hơn ở đây
        if (DialogueManager.I != null)
        {
            DialogueManager.I.Show(new System.Collections.Generic.List<string>
            {
                $"🎉 Đã nâng cấp {statName}!",
                effect,
                $"Còn {PlayerExpManager.Instance.GetStatPoints()} điểm chỉ số"
            });
        }
    }
    
    /// <summary>
    /// Debug: Cập nhật UI thủ công
    /// </summary>
    [ContextMenu("Debug Update Display")]
    public void DebugUpdateDisplay()
    {
        UpdateStatDisplay();
        Debug.Log("[StatUpgradeUI] Đã cập nhật display thủ công!");
    }
}
