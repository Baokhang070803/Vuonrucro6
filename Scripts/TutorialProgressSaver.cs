using UnityEngine;

/// <summary>
/// Script lưu trạng thái tutorial vào Firebase
/// </summary>
public class TutorialProgressSaver : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public string tutorialKey = "TutorialCompleted";
    
    void Start()
    {
        // Load tutorial state từ Firebase khi khởi động
        LoadTutorialState();
    }
    
    /// <summary>
    /// Load trạng thái tutorial từ PlayerPrefs (đã được sync từ Firebase)
    /// </summary>
    void LoadTutorialState()
    {
        string tutorialState = PlayerPrefs.GetString(tutorialKey, "false");
        bool isCompleted = tutorialState == "true";
        
        Debug.Log($"[TutorialProgressSaver] Tutorial state: {isCompleted}");
        
        // Có thể thêm logic để skip tutorial nếu đã hoàn thành
        if (isCompleted)
        {
            SkipTutorialIfNeeded();
        }
    }
    
    /// <summary>
    /// Đánh dấu tutorial đã hoàn thành
    /// </summary>
    public void MarkTutorialCompleted()
    {
        // Lưu vào PlayerPrefs
        PlayerPrefs.SetString(tutorialKey, "true");
        PlayerPrefs.Save();
        
        // Lưu vào Firebase
        if (PlayerDataSyncManager.Instance != null)
        {
            PlayerDataSyncManager.Instance.UpdateTutorialProgress(true);
            Debug.Log("[TutorialProgressSaver] Đã đánh dấu tutorial hoàn thành!");
        }
    }
    
    /// <summary>
    /// Reset tutorial progress
    /// </summary>
    public void ResetTutorialProgress()
    {
        // Reset PlayerPrefs
        PlayerPrefs.SetString(tutorialKey, "false");
        PlayerPrefs.Save();
        
        // Reset Firebase
        if (PlayerDataSyncManager.Instance != null)
        {
            PlayerDataSyncManager.Instance.UpdateTutorialProgress(false);
            Debug.Log("[TutorialProgressSaver] Đã reset tutorial progress!");
        }
    }
    
    /// <summary>
    /// Kiểm tra xem tutorial đã hoàn thành chưa
    /// </summary>
    public bool IsTutorialCompleted()
    {
        string tutorialState = PlayerPrefs.GetString(tutorialKey, "false");
        return tutorialState == "true";
    }
    
    /// <summary>
    /// Skip tutorial nếu đã hoàn thành
    /// </summary>
    public void SkipTutorialIfNeeded()
    {
        // Tìm và tắt tutorial objects nếu có
        GameObject[] tutorialObjects = GameObject.FindGameObjectsWithTag("Tutorial");
        foreach (GameObject obj in tutorialObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        
        // Tìm và tắt GameTutorialManager
        var tutorialManager = FindObjectOfType<GameTutorialManager>();
        if (tutorialManager != null)
        {
            tutorialManager.gameObject.SetActive(false);
        }
        
        Debug.Log("[TutorialProgressSaver] Đã skip tutorial vì đã hoàn thành trước đó");
    }
    
    /// <summary>
    /// Test method - có thể gọi từ console
    /// </summary>
    [ContextMenu("Mark Tutorial Completed")]
    public void TestMarkTutorialCompleted()
    {
        MarkTutorialCompleted();
    }
    
    /// <summary>
    /// Test method - có thể gọi từ console
    /// </summary>
    [ContextMenu("Reset Tutorial Progress")]
    public void TestResetTutorialProgress()
    {
        ResetTutorialProgress();
    }
}
