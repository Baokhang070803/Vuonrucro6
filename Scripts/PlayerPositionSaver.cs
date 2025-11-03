using UnityEngine;

/// <summary>
/// Script tự động lưu vị trí player vào Firebase
/// </summary>
public class PlayerPositionSaver : MonoBehaviour
{
    [Header("Auto Save Settings")]
    public float saveInterval = 10f; // Lưu mỗi 10 giây
    private float lastSaveTime;
    private Vector3 lastSavedPosition;
    
    [Header("Distance Threshold")]
    public float minDistanceToSave = 1f; // Chỉ lưu khi di chuyển ít nhất 1 unit
    
    void Start()
    {
        lastSavedPosition = transform.position;
    }
    
    void Update()
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
            SavePositionToFirebase(currentPosition);
            lastSavedPosition = currentPosition;
        }
        
        lastSaveTime = Time.time;
    }
    
    /// <summary>
    /// Lưu vị trí vào Firebase
    /// </summary>
    void SavePositionToFirebase(Vector3 position)
    {
        if (PlayerDataSyncManager.Instance != null)
        {
            PlayerDataSyncManager.Instance.UpdatePlayerPosition(position);
            Debug.Log($"[PlayerPositionSaver] Đã lưu vị trí: {position}");
        }
    }
    
    /// <summary>
    /// Lưu vị trí ngay lập tức (có thể gọi từ script khác)
    /// </summary>
    public void SavePositionNow()
    {
        SavePositionToFirebase(transform.position);
        lastSavedPosition = transform.position;
        lastSaveTime = Time.time;
    }
    
    /// <summary>
    /// Lưu vị trí khi chuyển scene
    /// </summary>
    void OnDisable()
    {
        SavePositionNow();
    }
    
    /// <summary>
    /// Lưu vị trí khi destroy
    /// </summary>
    void OnDestroy()
    {
        SavePositionNow();
    }
}
