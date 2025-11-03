using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script test real-time sync - có thể xóa sau khi test xong
/// </summary>
public class RealTimeSyncTester : MonoBehaviour
{
    [Header("Test UI")]
    public Button testGoldButton;
    public Button testDiamondButton;
    public TextMeshProUGUI statusText;
    
    [Header("Test Values")]
    public int testGoldAmount = 9999;
    public int testDiamondAmount = 8888;
    
    void Start()
    {
        // Setup test buttons
        if (testGoldButton != null)
        {
            testGoldButton.onClick.AddListener(TestGoldSync);
        }
        
        if (testDiamondButton != null)
        {
            testDiamondButton.onClick.AddListener(TestDiamondSync);
        }
        
        UpdateStatusText("Real-time sync tester sẵn sàng!");
    }
    
    /// <summary>
    /// Test sync Gold từ game lên Firebase
    /// </summary>
    public void TestGoldSync()
    {
        if (PlayerGoldManager.Instance != null)
        {
            PlayerGoldManager.Instance.SetGold(testGoldAmount);
            UpdateStatusText($"Đã gửi Gold {testGoldAmount} lên Firebase!");
        }
        else
        {
            UpdateStatusText("PlayerGoldManager.Instance is null!");
        }
    }
    
    /// <summary>
    /// Test sync Diamond từ game lên Firebase
    /// </summary>
    public void TestDiamondSync()
    {
        if (PlayerGoldManager.Instance != null)
        {
            PlayerGoldManager.Instance.SetDiamond(testDiamondAmount);
            UpdateStatusText($"Đã gửi Diamond {testDiamondAmount} lên Firebase!");
        }
        else
        {
            UpdateStatusText("PlayerGoldManager.Instance is null!");
        }
    }
    
    /// <summary>
    /// Cập nhật status text
    /// </summary>
    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        }
        
        Debug.Log($"[RealTimeSyncTester] {message}");
    }
    
    /// <summary>
    /// Test từ code (có thể gọi từ console)
    /// </summary>
    [ContextMenu("Test Gold Sync")]
    public void TestGoldFromCode()
    {
        TestGoldSync();
    }
    
    /// <summary>
    /// Test từ code (có thể gọi từ console)
    /// </summary>
    [ContextMenu("Test Diamond Sync")]
    public void TestDiamondFromCode()
    {
        TestDiamondSync();
    }
}
