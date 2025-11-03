using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Bảng hướng dẫn đơn giản hiển thị 1 trang duy nhất
/// </summary>
public class SimpleTutorialPanel : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public Button startButton;
    
    [Header("Tutorial Content")]
    [TextArea(10, 20)]
    public string tutorialContent = 
        "🌸 <size=36><b>CHÀO MỪNG ĐẾN VƯỜN RỰC RỠ!</b></size> 🌸\n\n" +
        
        "<color=#FFD700><b>📖 CÂU CHUYỆN</b></color>\n" +
        "Làng Hoa Rực từng rực rỡ nhờ Cây Pha Lê ở trung tâm.\n" +
        "Mười năm trước, Lời nguyền 'Ghen Sắc' làm cây vỡ thành nhiều mảnh.\n" +
        "Hãy giúp Mụ Thảo khôi phục Cây Pha Lê!\n\n" +
        
        "<color=#90EE90><b>🎮 ĐIỀU KHIỂN</b></color>\n" +
        "• Di chuyển: <b>Phím mũi tên</b> hoặc <b>WASD</b>\n" +
        "• Tương tác: <b>Click vào NPC</b> để nói chuyện\n" +
        "• Xem nhiệm vụ: <b>Phím Q</b>\n\n" +
        
        "<color=#87CEEB><b>🌻 CANH TÁC</b></color>\n" +
        "• <b>Phím C</b>: Dọn cỏ, làm đất\n" +
        "• <b>Phím V</b>: Gieo hạt giống\n" +
        "• <b>Phím M</b>: Thu hoạch cây\n\n" +
        
        "<color=#FFA500><b>🎯 NHIỆM VỤ</b></color>\n" +
        "1. Gặp Mụ Thảo\n" +
        "2. Thu hoạch 10 cây hoa hướng dương\n" +
        "3. Tìm đường vào làng\n" +
        "4. Trận chiến cuối cùng\n\n" +
        
        "<color=#FF69B4><b>💡 MẸO:</b></color> Di chuyển khắp bản đồ để khám phá!\n\n" +
        
        "<size=28><b>🎉 Chúc bạn chơi game vui vẻ! 🎉</b></size>";
    
    void Start()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        
        // Kiểm tra xem đã xem tutorial chưa
        CheckAndShowTutorial();
    }
    
    void CheckAndShowTutorial()
    {
        bool hasShown = PlayerPrefs.GetInt("SimpleTutorialShown", 0) == 1;
        
        if (!hasShown)
        {
            Invoke(nameof(ShowTutorial), 0.3f);
        }
    }
    
    public void ShowTutorial()
    {
        if (tutorialPanel == null) return;
        
        if (tutorialText != null)
            tutorialText.text = tutorialContent;
        
        tutorialPanel.SetActive(true);
        
        // Tạm dừng game
        Time.timeScale = 0f;
        
        // Đánh dấu đã xem
        PlayerPrefs.SetInt("SimpleTutorialShown", 1);
        PlayerPrefs.Save();
    }
    
    public void StartGame()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        
        // Tiếp tục game
        Time.timeScale = 1f;
        
        Debug.Log("Bắt đầu chơi game!");
    }
    
    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
    
    // Reset để xem lại tutorial
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey("SimpleTutorialShown");
        PlayerPrefs.Save();
    }
}
