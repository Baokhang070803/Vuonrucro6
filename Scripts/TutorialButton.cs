using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script để gắn vào nút cuốn sách, cho phép mở lại tutorial bất cứ lúc nào
/// </summary>
public class TutorialButton : MonoBehaviour
{
    [Header("Tutorial Manager Reference")]
    public GameTutorialManager tutorialManager; // Kéo GameTutorialManager vào đây
    
    [Header("Button Reference")]
    public Button bookButton; // Nút cuốn sách (tự động tìm nếu không gán)
    
    [Header("Settings")]
    public bool showInGameUI = true; // Hiển thị nút trong game
    
    void Start()
    {
        // Tự động tìm button nếu chưa gán
        if (bookButton == null)
        {
            bookButton = GetComponent<Button>();
        }
        
        // Tự động tìm TutorialManager nếu chưa gán
        if (tutorialManager == null)
        {
            tutorialManager = FindObjectOfType<GameTutorialManager>();
            
            if (tutorialManager == null)
            {
                Debug.LogError("Không tìm thấy GameTutorialManager trong scene! Vui lòng kéo vào Inspector.");
            }
        }
        
        // Gắn sự kiện click cho nút
        if (bookButton != null)
        {
            bookButton.onClick.AddListener(OpenTutorial);
        }
        else
        {
            Debug.LogError("Không tìm thấy Button component! Vui lòng gắn script này vào GameObject có Button.");
        }
        
        // Hiển thị hoặc ẩn nút
        if (bookButton != null)
        {
            bookButton.gameObject.SetActive(showInGameUI);
        }
    }
    
    /// <summary>
    /// Mở tutorial khi nhấn nút cuốn sách
    /// </summary>
    public void OpenTutorial()
    {
        if (tutorialManager != null)
        {
            Debug.Log("Mở tutorial từ nút cuốn sách!");
            tutorialManager.ShowTutorial();
        }
        else
        {
            Debug.LogError("TutorialManager chưa được gán! Vui lòng kéo GameTutorialManager vào Inspector.");
        }
    }
    
    /// <summary>
    /// Ẩn/hiện nút cuốn sách
    /// </summary>
    public void SetButtonVisible(bool visible)
    {
        if (bookButton != null)
        {
            bookButton.gameObject.SetActive(visible);
        }
    }
}
