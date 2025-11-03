using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script lưu vị trí player trước khi vào PvP
/// Đặt script này vào scene map1, nó sẽ tự động lưu vị trí khi chuyển sang PvP
/// </summary>
public class PVPPositionSaver : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Tự động lưu vị trí khi chuyển scene")]
    public bool autoSaveOnSceneChange = true;
    
    [Tooltip("Khoảng cách tối thiểu để lưu vị trí (tránh lưu liên tục)")]
    public float minDistanceToSave = 1f;
    
    private Vector3 lastSavedPosition;
    private float lastSaveTime;
    private float saveInterval = 5f; // Lưu mỗi 5 giây
    
    private void Start()
    {
        // Khởi tạo vị trí ban đầu
        lastSavedPosition = transform.position;
        lastSaveTime = Time.time;
        
        Debug.Log("[PVPPositionSaver] Đã khởi tạo position saver");
    }
    
    private void Update()
    {
        // Auto save theo thời gian
        if (Time.time - lastSaveTime >= saveInterval)
        {
            SavePositionIfChanged();
        }
    }
    
    /// <summary>
    /// Lưu vị trí nếu đã thay đổi đáng kể
    /// </summary>
    void SavePositionIfChanged()
    {
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(currentPosition, lastSavedPosition);
        
        if (distance >= minDistanceToSave)
        {
            SavePositionToPlayerPrefs(currentPosition);
            lastSavedPosition = currentPosition;
            lastSaveTime = Time.time;
        }
    }
    
    /// <summary>
    /// Lưu vị trí vào PlayerPrefs
    /// </summary>
    void SavePositionToPlayerPrefs(Vector3 position)
    {
        PlayerPrefs.SetFloat("SavedPlayerX", position.x);
        PlayerPrefs.SetFloat("SavedPlayerY", position.y);
        PlayerPrefs.SetFloat("SavedPlayerZ", position.z);
        PlayerPrefs.Save();
        
        Debug.Log($"[PVPPositionSaver] Đã lưu vị trí: {position}");
    }
    
    /// <summary>
    /// Lưu vị trí ngay lập tức (có thể gọi từ script khác)
    /// </summary>
    public void SavePositionNow()
    {
        SavePositionToPlayerPrefs(transform.position);
        lastSavedPosition = transform.position;
        lastSaveTime = Time.time;
    }
    
    /// <summary>
    /// Lưu vị trí khi chuyển scene (nếu autoSaveOnSceneChange = true)
    /// </summary>
    void OnDisable()
    {
        if (autoSaveOnSceneChange)
        {
            SavePositionNow();
        }
    }
    
    /// <summary>
    /// Lưu vị trí khi destroy
    /// </summary>
    void OnDestroy()
    {
        if (autoSaveOnSceneChange)
        {
            SavePositionNow();
        }
    }
    
    /// <summary>
    /// Test method - có thể gọi từ console
    /// </summary>
    [ContextMenu("Save Position Now")]
    public void TestSavePosition()
    {
        SavePositionNow();
    }
}
