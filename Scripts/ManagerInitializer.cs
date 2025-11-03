using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script đảm bảo các Managers được khởi tạo đúng cách khi vào map1
/// Đặt script này vào map1 scene để đảm bảo Managers không bị mất
/// </summary>
public class ManagerInitializer : MonoBehaviour
{
    [Header("Manager Prefabs")]
    public GameObject playerDataSyncManagerPrefab;
    public GameObject questManagerPrefab;
    public GameObject bagManagerPrefab;
    public GameObject playerGoldManagerPrefab;
    public GameObject playerExpManagerPrefab;
    public GameObject dialogueManagerPrefab;
    public GameObject musicManagerPrefab;
    
    [Header("Settings")]
    [Tooltip("Tự động khởi tạo Managers khi Start")]
    public bool autoInitializeOnStart = true;
    
    [Tooltip("Debug logs")]
    public bool enableDebugLogs = true;
    
    void Start()
    {
        if (autoInitializeOnStart)
        {
            InitializeAllManagers();
        }
    }
    
    /// <summary>
    /// Khởi tạo tất cả Managers cần thiết
    /// </summary>
    public void InitializeAllManagers()
    {
        if (enableDebugLogs)
            Debug.Log("[ManagerInitializer] Bắt đầu khởi tạo Managers...");
        
        // Khởi tạo PlayerDataSyncManager
        InitializeManager<PlayerDataSyncManager>("PlayerDataSyncManager", playerDataSyncManagerPrefab);
        
        // Khởi tạo QuestManager
        InitializeManager<QuestManager>("QuestManager", questManagerPrefab);
        
        // Khởi tạo BagManager
        InitializeManager<BagManager>("BagManager", bagManagerPrefab);
        
        // Khởi tạo PlayerGoldManager
        InitializeManager<PlayerGoldManager>("PlayerGoldManager", playerGoldManagerPrefab);
        
        // Khởi tạo PlayerExpManager
        InitializeManager<PlayerExpManager>("PlayerExpManager", playerExpManagerPrefab);
        
        // Khởi tạo DialogueManager
        InitializeManager<DialogueManager>("DialogueManager", dialogueManagerPrefab);
        
        // Khởi tạo MusicManager
        InitializeManager<MusicManager>("MusicManager", musicManagerPrefab);
        
        if (enableDebugLogs)
            Debug.Log("[ManagerInitializer] Hoàn thành khởi tạo Managers!");
    }
    
    /// <summary>
    /// Khởi tạo một Manager cụ thể
    /// </summary>
    private void InitializeManager<T>(string managerName, GameObject prefab) where T : MonoBehaviour
    {
        // Kiểm tra xem Manager đã tồn tại chưa
        T existingManager = FindObjectOfType<T>();
        
        if (existingManager != null)
        {
            if (enableDebugLogs)
                Debug.Log($"[ManagerInitializer] {managerName} đã tồn tại, bỏ qua khởi tạo.");
            return;
        }
        
        // Tạo Manager mới từ prefab
        if (prefab != null)
        {
            GameObject managerObj = Instantiate(prefab);
            managerObj.name = managerName;
            
            if (enableDebugLogs)
                Debug.Log($"[ManagerInitializer] Đã tạo {managerName} từ prefab.");
        }
        else
        {
            // Tạo GameObject mới và thêm component
            GameObject managerObj = new GameObject(managerName);
            managerObj.AddComponent<T>();
            
            if (enableDebugLogs)
                Debug.Log($"[ManagerInitializer] Đã tạo {managerName} mới.");
        }
    }
    
    /// <summary>
    /// Kiểm tra trạng thái tất cả Managers
    /// </summary>
    [ContextMenu("Check All Managers Status")]
    public void CheckAllManagersStatus()
    {
        Debug.Log("=== MANAGER STATUS CHECK ===");
        
        CheckManagerStatus<PlayerDataSyncManager>("PlayerDataSyncManager");
        CheckManagerStatus<QuestManager>("QuestManager");
        CheckManagerStatus<BagManager>("BagManager");
        CheckManagerStatus<PlayerGoldManager>("PlayerGoldManager");
        CheckManagerStatus<PlayerExpManager>("PlayerExpManager");
        CheckManagerStatus<DialogueManager>("DialogueManager");
        CheckManagerStatus<MusicManager>("MusicManager");
        
        Debug.Log("=== END MANAGER STATUS CHECK ===");
    }
    
    /// <summary>
    /// Kiểm tra trạng thái một Manager
    /// </summary>
    private void CheckManagerStatus<T>(string managerName) where T : MonoBehaviour
    {
        T manager = FindObjectOfType<T>();
        
        if (manager != null)
        {
            Debug.Log($"✅ {managerName}: OK (Instance = {manager})");
        }
        else
        {
            Debug.LogError($"❌ {managerName}: MISSING!");
        }
    }
    
    /// <summary>
    /// Force khởi tạo lại tất cả Managers
    /// </summary>
    [ContextMenu("Force Reinitialize All Managers")]
    public void ForceReinitializeAllManagers()
    {
        Debug.Log("[ManagerInitializer] Force khởi tạo lại tất cả Managers...");
        
        // Destroy tất cả Managers hiện tại
        DestroyManager<PlayerDataSyncManager>();
        DestroyManager<QuestManager>();
        DestroyManager<BagManager>();
        DestroyManager<PlayerGoldManager>();
        DestroyManager<PlayerExpManager>();
        DestroyManager<DialogueManager>();
        DestroyManager<MusicManager>();
        
        // Khởi tạo lại
        InitializeAllManagers();
        
        Debug.Log("[ManagerInitializer] Hoàn thành force reinitialize!");
    }
    
    /// <summary>
    /// Destroy một Manager
    /// </summary>
    private void DestroyManager<T>() where T : MonoBehaviour
    {
        T manager = FindObjectOfType<T>();
        if (manager != null)
        {
            DestroyImmediate(manager.gameObject);
        }
    }
}
