using UnityEngine;

/// <summary>
/// Script đơn giản để đảm bảo Managers được khởi tạo trong map1
/// Đặt script này vào map1 scene
/// </summary>
public class Map1ManagerEnsurer : MonoBehaviour
{
    void Start()
    {
        Debug.Log("[Map1ManagerEnsurer] Kiểm tra và đảm bảo Managers tồn tại...");
        
        // Đảm bảo PlayerDataSyncManager tồn tại
        EnsureManager<PlayerDataSyncManager>("PlayerDataSyncManager");
        
        // Đảm bảo QuestManager tồn tại
        EnsureManager<QuestManager>("QuestManager");
        
        // Đảm bảo BagManager tồn tại
        EnsureManager<BagManager>("BagManager");
        
        // Đảm bảo PlayerGoldManager tồn tại
        EnsureManager<PlayerGoldManager>("PlayerGoldManager");
        
        // Đảm bảo PlayerExpManager tồn tại
        EnsureManager<PlayerExpManager>("PlayerExpManager");
        
        // Đảm bảo DialogueManager tồn tại
        EnsureManager<DialogueManager>("DialogueManager");
        
        Debug.Log("[Map1ManagerEnsurer] Hoàn thành kiểm tra Managers!");
    }
    
    /// <summary>
    /// Đảm bảo một Manager tồn tại
    /// </summary>
    private void EnsureManager<T>(string managerName) where T : MonoBehaviour
    {
        T manager = FindObjectOfType<T>();
        
        if (manager == null)
        {
            Debug.LogWarning($"[Map1ManagerEnsurer] {managerName} không tồn tại! Tạo mới...");
            
            // Tạo GameObject mới và thêm component
            GameObject managerObj = new GameObject(managerName);
            managerObj.AddComponent<T>();
            
            Debug.Log($"[Map1ManagerEnsurer] Đã tạo {managerName} mới.");
        }
        else
        {
            Debug.Log($"[Map1ManagerEnsurer] ✅ {managerName} đã tồn tại.");
        }
    }
}
