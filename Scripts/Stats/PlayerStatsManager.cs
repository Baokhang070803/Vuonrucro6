using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;

/// <summary>
/// Manager quản lý chỉ số của người chơi
/// </summary>
public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance;
    
    [Header("Player Stats")]
    public PlayerStats playerStats = new PlayerStats();
    
    [Header("Audio")]
    public AudioClip statUpgradeSound; // Âm thanh khi nâng cấp chỉ số
    [Range(0f, 1f)] public float statUpgradeVolume = 1f;
    private AudioSource audioSource;
    
    [Header("Firebase Settings")]
    public bool enableFirebaseSync = true; // Bật/tắt đồng bộ Firebase
    private DatabaseReference statsDatabaseRef;
    
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
        // Chuẩn bị AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Khởi tạo Firebase reference
        if (enableFirebaseSync)
        {
            InitializeFirebase();
        }
        
        Debug.Log("[PlayerStatsManager] Đã khởi tạo!");
    }
    
    /// <summary>
    /// Sử dụng điểm chỉ số để tăng Strength
    /// </summary>
    public bool UpgradeStrength()
    {
        if (CanUpgradeStat())
        {
            if (PlayerExpManager.Instance.SpendStatPoint())
            {
                playerStats.IncreaseStrength();
                PlayUpgradeSound();
                SaveStatsToFirebase(); // Lưu vào Firebase
                Debug.Log($"[PlayerStatsManager] Đã nâng cấp Strength! Còn {PlayerExpManager.Instance.GetStatPoints()} điểm chỉ số");
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Sử dụng điểm chỉ số để tăng Agility
    /// </summary>
    public bool UpgradeAgility()
    {
        if (CanUpgradeStat())
        {
            if (PlayerExpManager.Instance.SpendStatPoint())
            {
                playerStats.IncreaseAgility();
                PlayUpgradeSound();
                SaveStatsToFirebase(); // Lưu vào Firebase
                Debug.Log($"[PlayerStatsManager] Đã nâng cấp Agility! Còn {PlayerExpManager.Instance.GetStatPoints()} điểm chỉ số");
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Sử dụng điểm chỉ số để tăng Intelligence
    /// </summary>
    public bool UpgradeIntelligence()
    {
        if (CanUpgradeStat())
        {
            if (PlayerExpManager.Instance.SpendStatPoint())
            {
                playerStats.IncreaseIntelligence();
                PlayUpgradeSound();
                SaveStatsToFirebase(); // Lưu vào Firebase
                Debug.Log($"[PlayerStatsManager] Đã nâng cấp Intelligence! Còn {PlayerExpManager.Instance.GetStatPoints()} điểm chỉ số");
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Sử dụng điểm chỉ số để tăng Vitality
    /// </summary>
    public bool UpgradeVitality()
    {
        if (CanUpgradeStat())
        {
            if (PlayerExpManager.Instance.SpendStatPoint())
            {
                playerStats.IncreaseVitality();
                PlayUpgradeSound();
                SaveStatsToFirebase(); // Lưu vào Firebase
                Debug.Log($"[PlayerStatsManager] Đã nâng cấp Vitality! Còn {PlayerExpManager.Instance.GetStatPoints()} điểm chỉ số");
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Kiểm tra có thể nâng cấp chỉ số không
    /// </summary>
    bool CanUpgradeStat()
    {
        if (PlayerExpManager.Instance == null)
        {
            Debug.LogWarning("[PlayerStatsManager] PlayerExpManager.Instance is null!");
            return false;
        }
        
        int availablePoints = PlayerExpManager.Instance.GetStatPoints();
        if (availablePoints <= 0)
        {
            Debug.LogWarning("[PlayerStatsManager] Không có điểm chỉ số để nâng cấp!");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Phát âm thanh khi nâng cấp
    /// </summary>
    void PlayUpgradeSound()
    {
        if (statUpgradeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(statUpgradeSound, statUpgradeVolume);
        }
    }
    
    /// <summary>
    /// Lấy chỉ số hiện tại
    /// </summary>
    public PlayerStats GetPlayerStats()
    {
        return playerStats;
    }
    
    /// <summary>
    /// Debug: Hiển thị thông tin chỉ số
    /// </summary>
    [ContextMenu("Show Stats Info")]
    public void ShowStatsInfo()
    {
        Debug.Log("=== PLAYER STATS INFO ===");
        Debug.Log(playerStats.GetStatsString());
        Debug.Log($"Tổng điểm đã sử dụng: {playerStats.GetTotalSpentPoints()}");
        Debug.Log($"Điểm chỉ số còn lại: {PlayerExpManager.Instance?.GetStatPoints() ?? 0}");
        Debug.Log($"Firebase Sync: {(enableFirebaseSync ? "BẬT" : "TẮT")}");
    }
    
    /// <summary>
    /// Debug: Lưu stats thủ công
    /// </summary>
    [ContextMenu("Save Stats to Firebase")]
    public void DebugSaveStats()
    {
        SaveStatsToFirebase();
    }
    
    /// <summary>
    /// Debug: Load stats thủ công
    /// </summary>
    [ContextMenu("Load Stats from Firebase")]
    public void DebugLoadStats()
    {
        LoadStatsFromFirebase();
    }
    
    /// <summary>
    /// Reset tất cả chỉ số (dùng cho debug)
    /// </summary>
    [ContextMenu("Reset All Stats")]
    public void ResetAllStats()
    {
        playerStats.ResetStats();
        Debug.Log("[PlayerStatsManager] Đã reset tất cả chỉ số!");
    }
    
    /// <summary>
    /// Khởi tạo Firebase reference
    /// </summary>
    void InitializeFirebase()
    {
        try
        {
            if (FirebaseApp.DefaultInstance != null)
            {
                // Sử dụng LoadDataManager như các script khác
                string userId = LoadDataManager.firebaseUser?.UserId ?? "default";
                statsDatabaseRef = FirebaseDatabase.DefaultInstance.GetReference("Users").Child(userId).Child("stats");
                Debug.Log("[PlayerStatsManager] Đã khởi tạo Firebase reference!");
                
                // Load stats từ Firebase
                LoadStatsFromFirebase();
            }
            else
            {
                Debug.LogWarning("[PlayerStatsManager] Firebase chưa được khởi tạo!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerStatsManager] Lỗi khởi tạo Firebase: {e.Message}");
        }
    }
    
    /// <summary>
    /// Lưu stats vào Firebase
    /// </summary>
    public async void SaveStatsToFirebase()
    {
        if (!enableFirebaseSync || statsDatabaseRef == null) return;
        
        try
        {
            string json = JsonUtility.ToJson(playerStats);
            await statsDatabaseRef.SetRawJsonValueAsync(json);
            Debug.Log("[PlayerStatsManager] Đã lưu stats vào Firebase!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerStatsManager] Lỗi lưu stats: {e.Message}");
        }
    }
    
    /// <summary>
    /// Load stats từ Firebase
    /// </summary>
    public async void LoadStatsFromFirebase()
    {
        if (!enableFirebaseSync || statsDatabaseRef == null) return;
        
        try
        {
            var snapshot = await statsDatabaseRef.GetValueAsync();
            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                playerStats = JsonUtility.FromJson<PlayerStats>(json);
                Debug.Log("[PlayerStatsManager] Đã load stats từ Firebase!");
            }
            else
            {
                Debug.Log("[PlayerStatsManager] Chưa có stats trong Firebase, sử dụng stats mặc định");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerStatsManager] Lỗi load stats: {e.Message}");
        }
    }
}
