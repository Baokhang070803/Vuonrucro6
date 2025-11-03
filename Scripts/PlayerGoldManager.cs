using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manager quản lý vàng và kim cương của người chơi
/// </summary>
public class PlayerGoldManager : MonoBehaviour
{
    public static PlayerGoldManager Instance;
    
    [Header("UI References")]
    public Text goldText;           // Text hiển thị vàng
    public TextMeshProUGUI goldTextTMP; // Text TMP hiển thị vàng
    public Text diamondText;        // Text hiển thị kim cương
    public TextMeshProUGUI diamondTextTMP; // Text TMP hiển thị kim cương
    
    [Header("Gold Settings")]
    public int startingGold = 0;    // Vàng ban đầu
    public int startingDiamond = 0; // Kim cương ban đầu
    
    // Events
    public System.Action<int> OnGoldChanged;
    public System.Action<int> OnDiamondChanged;
    
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
        // Khởi tạo từ LoadDataManager nếu có
        if (LoadDataManager.userInGame != null)
        {
            SetGold(LoadDataManager.userInGame.Gold);
            SetDiamond(LoadDataManager.userInGame.Diamond);
        }
        else
        {
            SetGold(startingGold);
            SetDiamond(startingDiamond);
        }
        
        Debug.Log("[PlayerGoldManager] Đã khởi tạo!");
    }
    
    /// <summary>
    /// Thêm vàng
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        
        int newGold = GetGold() + amount;
        SetGold(newGold);
        
        Debug.Log($"[PlayerGoldManager] Đã thêm {amount} vàng! Tổng: {newGold}");
        
        // Hiển thị thông báo nhận vàng
        ShowGoldNotification(amount);
    }
    
    /// <summary>
    /// Trừ vàng
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;
        
        int currentGold = GetGold();
        if (currentGold < amount)
        {
            Debug.LogWarning($"[PlayerGoldManager] Không đủ vàng! Cần: {amount}, Có: {currentGold}");
            return false;
        }
        
        int newGold = currentGold - amount;
        SetGold(newGold);
        
        Debug.Log($"[PlayerGoldManager] Đã tiêu {amount} vàng! Còn lại: {newGold}");
        return true;
    }
    
    /// <summary>
    /// Thêm kim cương
    /// </summary>
    public void AddDiamond(int amount)
    {
        if (amount <= 0) return;
        
        int newDiamond = GetDiamond() + amount;
        SetDiamond(newDiamond);
        
        Debug.Log($"[PlayerGoldManager] Đã thêm {amount} kim cương! Tổng: {newDiamond}");
    }
    
    /// <summary>
    /// Trừ kim cương
    /// </summary>
    public bool SpendDiamond(int amount)
    {
        if (amount <= 0) return false;
        
        int currentDiamond = GetDiamond();
        if (currentDiamond < amount)
        {
            Debug.LogWarning($"[PlayerGoldManager] Không đủ kim cương! Cần: {amount}, Có: {currentDiamond}");
            return false;
        }
        
        int newDiamond = currentDiamond - amount;
        SetDiamond(newDiamond);
        
        Debug.Log($"[PlayerGoldManager] Đã tiêu {amount} kim cương! Còn lại: {newDiamond}");
        return true;
    }
    
    /// <summary>
    /// Set vàng
    /// </summary>
    public void SetGold(int amount)
    {
        amount = Mathf.Max(0, amount);
        
        // Cập nhật LoadDataManager
        if (LoadDataManager.userInGame != null)
        {
            LoadDataManager.userInGame.Gold = amount;
        }
        
        // Cập nhật UI
        UpdateGoldUI(amount);
        
        // Trigger event
        OnGoldChanged?.Invoke(amount);
        
        // Lưu vào Firebase
        SaveToFirebase();
    }
    
    /// <summary>
    /// Set vàng từ Firebase (không trigger save lại)
    /// </summary>
    public void SetGoldFromFirebase(int amount)
    {
        amount = Mathf.Max(0, amount);
        
        // Cập nhật LoadDataManager
        if (LoadDataManager.userInGame != null)
        {
            LoadDataManager.userInGame.Gold = amount;
        }
        
        // Cập nhật UI
        UpdateGoldUI(amount);
        
        // Trigger event
        OnGoldChanged?.Invoke(amount);
        
        // KHÔNG lưu vào Firebase (tránh loop)
        Debug.Log($"[PlayerGoldManager] Đã cập nhật Gold từ Firebase: {amount}");
    }
    
    /// <summary>
    /// Set kim cương
    /// </summary>
    public void SetDiamond(int amount)
    {
        amount = Mathf.Max(0, amount);
        
        // Cập nhật LoadDataManager
        if (LoadDataManager.userInGame != null)
        {
            LoadDataManager.userInGame.Diamond = amount;
        }
        
        // Cập nhật UI
        UpdateDiamondUI(amount);
        
        // Trigger event
        OnDiamondChanged?.Invoke(amount);
        
        // Lưu vào Firebase
        SaveToFirebase();
    }
    
    /// <summary>
    /// Set kim cương từ Firebase (không trigger save lại)
    /// </summary>
    public void SetDiamondFromFirebase(int amount)
    {
        amount = Mathf.Max(0, amount);
        
        // Cập nhật LoadDataManager
        if (LoadDataManager.userInGame != null)
        {
            LoadDataManager.userInGame.Diamond = amount;
        }
        
        // Cập nhật UI
        UpdateDiamondUI(amount);
        
        // Trigger event
        OnDiamondChanged?.Invoke(amount);
        
        // KHÔNG lưu vào Firebase (tránh loop)
        Debug.Log($"[PlayerGoldManager] Đã cập nhật Diamond từ Firebase: {amount}");
    }
    
    /// <summary>
    /// Lấy số vàng hiện tại
    /// </summary>
    public int GetGold()
    {
        if (LoadDataManager.userInGame != null)
            return LoadDataManager.userInGame.Gold;
        return startingGold;
    }
    
    /// <summary>
    /// Lấy số kim cương hiện tại
    /// </summary>
    public int GetDiamond()
    {
        if (LoadDataManager.userInGame != null)
            return LoadDataManager.userInGame.Diamond;
        return startingDiamond;
    }
    
    /// <summary>
    /// Cập nhật UI vàng
    /// </summary>
    void UpdateGoldUI(int amount)
    {
        string goldString = amount.ToString();
        
        if (goldText != null)
            goldText.text = " " + goldString;
            
        if (goldTextTMP != null)
            goldTextTMP.text = " " + goldString;
    }
    
    /// <summary>
    /// Cập nhật UI kim cương
    /// </summary>
    void UpdateDiamondUI(int amount)
    {
        string diamondString = amount.ToString();
        
        if (diamondText != null)
            diamondText.text = " " + diamondString;
            
        if (diamondTextTMP != null)
            diamondTextTMP.text = " " + diamondString;
    }
    
    /// <summary>
    /// Lưu vàng/kim cương vào Firebase (sử dụng PlayerDataSyncManager)
    /// </summary>
    void SaveToFirebase()
    {
        if (LoadDataManager.userInGame != null && LoadDataManager.firebaseUser != null)
        {
            // Sử dụng PlayerDataSyncManager để tránh xung đột dữ liệu
            if (PlayerDataSyncManager.Instance != null)
            {
                PlayerDataSyncManager.Instance.UpdateGold(LoadDataManager.userInGame.Gold);
                PlayerDataSyncManager.Instance.UpdateDiamond(LoadDataManager.userInGame.Diamond);
                Debug.Log("[PlayerGoldManager] Đã gửi vàng/kim cương để lưu vào Firebase!");
            }
            else
            {
                Debug.LogWarning("[PlayerGoldManager] PlayerDataSyncManager.Instance is null!");
            }
        }
    }
    
    /// <summary>
    /// Hiển thị thông báo nhận vàng
    /// </summary>
    void ShowGoldNotification(int amount)
    {
        // TODO: Hiển thị popup thông báo nhận vàng
        Debug.Log($"💰 Nhận được {amount} vàng!");
        
        // Có thể thêm hiệu ứng UI ở đây
        // VD: Popup text, animation, sound effect
    }
}
