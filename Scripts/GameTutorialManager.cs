using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Quản lý bảng hướng dẫn game hiển thị khi người chơi vào map lần đầu
/// </summary>
public class GameTutorialManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject tutorialPanel;           // Panel chứa toàn bộ hướng dẫn
    public TextMeshProUGUI tutorialTitle;      // Tiêu đề "Hướng Dẫn Chơi"
    public TextMeshProUGUI tutorialContent;    // Nội dung hướng dẫn
    public Button closeButton;                 // Nút đóng
    public Button nextButton;                  // Nút tiếp theo (nếu có nhiều trang)
    public Button prevButton;                  // Nút quay lại
    public Button tutorialBookButton;          // Nút cuốn sách để mở lại tutorial
    
    [Header("Settings")]
    public bool showOnFirstTime = true;        // Chỉ hiện lần đầu
    public string tutorialKey = "TutorialShown"; // Key để lưu PlayerPrefs
    
    [Header("Tutorial Content")]
    [TextArea(5, 10)]
    public List<string> tutorialPages = new List<string>(); // Danh sách các trang hướng dẫn
    
    private int currentPageIndex = 0;
    private Canvas tutorialCanvas; // LƯU REFERENCE Canvas để tái sử dụng
    
    void Start()
    {
        // LƯU REFERENCE Canvas ngay từ đầu
        if (tutorialPanel != null)
        {
            tutorialCanvas = tutorialPanel.GetComponentInParent<Canvas>();
            if (tutorialCanvas == null)
            {
                Debug.LogError("❌ KHÔNG TÌM THẤY Canvas parent của tutorialPanel trong Start()!");
            }
            else
            {
                Debug.Log($"✅ Đã lưu reference TutorialCanvas: {tutorialCanvas.name}");
            }
        }
        
        // Thiết lập nội dung mặc định nếu chưa có
        if (tutorialPages.Count == 0)
        {
            SetupDefaultTutorialContent();
        }
        
        // Thiết lập nút
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseTutorial);
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);
        if (prevButton != null)
            prevButton.onClick.AddListener(PreviousPage);
        
        // KHÔNG ẨN panel ban đầu nữa - để nó hiện luôn
        // (Panel sẽ được ẩn trong Inspector nếu không muốn hiện ngay)
        
        // ĐẢM BẢO TUTORIAL LUÔN Ở TRÊN CÙNG
        EnsureTutorialOnTop();
        
        // Kiểm tra xem có cần hiện tutorial không - với delay nhỏ để đảm bảo nvnu1dituyen chạy trước
        Invoke(nameof(CheckAndShowTutorial), 0.1f);
    }
    
    void EnsureTutorialOnTop()
    {
        if (tutorialPanel != null)
        {
            Canvas tutorialCanvas = tutorialPanel.GetComponentInParent<Canvas>();
            if (tutorialCanvas != null)
            {
                // Đặt sorting order cực cao
                tutorialCanvas.sortingOrder = 10000;
                tutorialCanvas.overrideSorting = true;
                
                Debug.Log($"Tutorial Canvas sorting order set to: {tutorialCanvas.sortingOrder}");
            }
        }
    }
    
    void SetupDefaultTutorialContent()
{
    tutorialPages.Clear();

    // Trang 1: Giới thiệu
    tutorialPages.Add(
        "<b>CHÀO MỪNG ĐẾN VỚI VƯỜN RỰC RỠ!</b>\n\n" +
        "Chào bạn! Làng Hoa Rực từng rực rỡ nhờ Cây Pha Lê ở trung tâm.\n\n" +
        "Mười năm trước, Lời nguyền 'Ghen Sắc' làm cây vỡ thành nhiều mảnh, khiến sinh trưởng trở nên trì trệ.\n\n" +
        "Nhiệm vụ của bạn là giúp Mụ Thảo khôi phục lại Cây Pha Lê và đưa làng trở về thời kỳ hoàng kim!"
    );

    // Trang 2: Điều khiển cơ bản
    tutorialPages.Add(
        "<b>ĐIỀU KHIỂN CƠ BẢN</b>\n\n" +
        "<b>Di chuyển:</b>\n" +
        "• Phím mũi tên ↑↓←→ hoặc WASD\n" +
        "• Click chuột để di chuyển đến vị trí\n\n" +
        "<b>Tương tác:</b>\n" +
        "• Click vào NPC để nói chuyện\n" +
        "• Đứng gần và nhấn Space để tương tác\n\n" +
        "<b>Nhiệm vụ:</b>\n" +
        "• Nhấn Q để xem bảng nhiệm vụ hiện tại\n" +
        "• Theo dõi mục tiêu trong góc màn hình"
    );

    // Trang 3: Hệ thống trồng trọt
    tutorialPages.Add(
        "<b>HỆ THỐNG TRỒNG TRỌT</b>\n\n" +
        "<b>Canh tác:</b>\n" +
        "• Phím C: Dọn cỏ, làm đất\n" +
        "• Phím V: Gieo hạt giống\n" +
        "• Phím M: Thu hoạch cây trưởng thành\n\n" +
        "<b>Chu kỳ sinh trưởng:</b>\n" +
        "• Hạt giống → Cây con → Cây lớn → Thu hoạch\n" +
        "• Mỗi giai đoạn mất vài giây\n\n" +
        "<b>Lưu ý:</b> Trạng thái vườn sẽ được lưu tự động."
    );

    // Trang 4: Nhiệm vụ chính
    tutorialPages.Add(
        "<b>NHIỆM VỤ CHÍNH</b>\n\n" +
        "1. Gặp Mụ Thảo:\n   Tìm và nói chuyện với Mụ Thảo để hiểu tình hình.\n\n" +
        "2. Những Hạt Mầm Đầu Tiên:\n   Thu hoạch 10 cây hoa hướng dương đầu tiên.\n\n" +
        "3. Tìm Đường Vào Làng:\n   Khám phá và tìm lối vào làng bí mật.\n\n" +
        "4. Trận Chiến Cuối Cùng:\n   Đối đầu với Mụ Thảo và khôi phục Cây Pha Lê."
    );

    // Trang 5: Mẹo chơi game
    tutorialPages.Add(
        "<b>MẸO CHƠI GAME</b>\n\n" +
        "<b>Khám phá:</b>\n" +
        "• Di chuyển khắp bản đồ để tìm NPC và vật phẩm.\n" +
        "• Chú ý các dấu hiệu và manh mối.\n\n" +
        "<b>Hoàn thành nhiệm vụ:</b>\n" +
        "• Làm theo thứ tự nhiệm vụ được giao.\n" +
        "• Đọc kỹ hướng dẫn trong hội thoại.\n\n" +
        "<b>Lưu game:</b>\n" +
        "• Dữ liệu tự động lưu vào Firebase.\n" +
        "• Đăng nhập cùng tài khoản để tiếp tục.\n\n" +
        "Chúc bạn chơi game vui vẻ!"
    );
}

    
    void CheckAndShowTutorial()
    {
        // Kiểm tra xem có quay về từ PvP không
        string combatFlag = PlayerPrefs.GetString("JustFinishedCombat", "false");
        if (combatFlag == "true")
        {
            Debug.Log("[GameTutorialManager] Quay về từ PvP, không hiện tutorial!");
            return;
        }
        
        // Kiểm tra xem đã xem tutorial chưa
        string tutorialWatched = PlayerPrefs.GetString("TutorialCompleted", "false");
        if (tutorialWatched == "true")
        {
            Debug.Log("[GameTutorialManager] Đã xem tutorial, không hiện lại!");
            return;
        }
        
        // Hiện tutorial nếu chưa xem
        Invoke(nameof(ShowTutorial), 0.5f);
    }
    
    public void ShowTutorial()
    {
        Debug.Log("🎯 ShowTutorial() được gọi!");
        
        if (tutorialPanel == null)
        {
            Debug.LogError("❌ tutorialPanel là NULL!");
            return;
        }
        
        currentPageIndex = 0;
        
        // ĐẢM BẢO CANVAS Ở TRÊN CÙNG TRƯỚC KHI HIỆN
        EnsureTutorialOnTop();
        
        // SỬ DỤNG REFERENCE Canvas đã lưu
        if (tutorialCanvas != null)
        {
            Debug.Log($"🔍 Sử dụng Canvas đã lưu: {tutorialCanvas.name}, Active: {tutorialCanvas.gameObject.activeSelf}");
            
            // FORCE enable Canvas - ĐẢM BẢO HIỆN
            tutorialCanvas.gameObject.SetActive(true);  // BẬT Canvas
            Debug.Log("✅ TutorialCanvas đã BẬT!");
        }
        else
        {
            Debug.LogError("❌ tutorialCanvas reference là NULL! Đang thử tìm lại...");
            
            // Thử tìm lại Canvas
            tutorialCanvas = tutorialPanel.GetComponentInParent<Canvas>();
            if (tutorialCanvas != null)
            {
                tutorialCanvas.gameObject.SetActive(true);
                Debug.Log("✅ Đã tìm lại và BẬT Canvas!");
            }
            else
            {
                Debug.LogError("❌ VẪN KHÔNG tìm thấy Canvas!");
                return;
            }
        }
        
        // FORCE enable Panel
        tutorialPanel.SetActive(true);
        Debug.Log("✅ TutorialPanel đã BẬT!");
        
        // CẬP NHẬT NỘI DUNG SAU KHI ENABLE
        UpdateTutorialDisplay();
        
        // Đánh dấu đã hiện tutorial (chỉ lần đầu)
        if (PlayerPrefs.GetInt(tutorialKey, 0) == 0)
        {
            PlayerPrefs.SetInt(tutorialKey, 1);
            PlayerPrefs.Save();
        }
        
        // ẨN TẤT CẢ UI KHÁC (QUEST PANEL, BUTTONS, ETC)
        HideOtherUI(true);
        
        // TẮT NÚT CUỐN SÁCH ĐỂ TRÁNH CLICK NHIỀU LẦN
        if (tutorialBookButton != null)
        {
            tutorialBookButton.interactable = false;
            Debug.Log("🔒 Đã TẮT nút cuốn sách");
        }
        
        // KHÔNG PAUSE GAME - Người chơi vẫn di chuyển được!
        // Time.timeScale = 0f; // ĐÃ XÓA - Game vẫn chạy bình thường
        
        Debug.Log("✅ Tutorial hiển thị - Game vẫn CHẠY!");
    }
    
    /// <summary>
    /// Mở tutorial thủ công (không cần check đã xem hay chưa)
    /// </summary>
    public void ShowTutorialManual()
    {
        ShowTutorial();
    }
    
    // Lưu trữ trạng thái UI ban đầu
    private Dictionary<Canvas, int> originalCanvasSortOrders = new Dictionary<Canvas, int>();
    private bool questPanelWasActive = false;
    
    /// <summary>
    /// Ẩn/hiện các UI khác khi tutorial mở/đóng
    /// </summary>
    private void HideOtherUI(bool hide)
    {
        try
        {
            if (hide)
            {
                // ĐANG MỞ TUTORIAL → ẨN UI KHÁC
                
                // Lưu và ẩn Quest Panel
                if (QuestManager.Instance != null && QuestManager.Instance.questPanel != null)
                {
                    questPanelWasActive = QuestManager.Instance.questPanel.activeSelf;
                    QuestManager.Instance.questPanel.SetActive(false);
                    Debug.Log($"Quest Panel đã ẨN (trước đó: {(questPanelWasActive ? "hiện" : "ẩn")})");
                }
                
                // Lưu và giảm sorting order của các Canvas khác
                originalCanvasSortOrders.Clear();
                Canvas[] allCanvas = FindObjectsOfType<Canvas>(true);
                foreach (Canvas canvas in allCanvas)
                {
                    // BỎ QUA tutorial canvas
                    if (canvas.gameObject == tutorialPanel || 
                        tutorialPanel.transform.IsChildOf(canvas.transform) ||
                        canvas.transform.IsChildOf(tutorialPanel.transform))
                    {
                        continue;
                    }
                    
                    // Lưu sorting order ban đầu
                    originalCanvasSortOrders[canvas] = canvas.sortingOrder;
                    canvas.sortingOrder = -100; // Đưa xuống dưới cùng
                }
                
                Debug.Log($"Đã ẨN {originalCanvasSortOrders.Count} Canvas khác");
            }
            else
            {
                // ĐANG ĐÓNG TUTORIAL → HIỆN LẠI UI
                
                Debug.Log("🔄 Bắt đầu khôi phục UI...");
                
                // Khôi phục Quest Panel
                if (QuestManager.Instance != null && QuestManager.Instance.questPanel != null)
                {
                    // LUÔN HIỆN quest panel khi đóng tutorial
                    QuestManager.Instance.questPanel.SetActive(true);
                    Debug.Log("✅ Quest Panel đã HIỆN lại");
                }
                else
                {
                    Debug.LogWarning("⚠️ QuestManager.Instance hoặc questPanel là NULL!");
                }
                
                // Khôi phục sorting order của các Canvas
                int restoredCount = 0;
                foreach (var kvp in originalCanvasSortOrders)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.sortingOrder = kvp.Value;
                        restoredCount++;
                        Debug.Log($"  ↩️ Khôi phục Canvas '{kvp.Key.name}': SortOrder {kvp.Value}");
                    }
                }
                
                Debug.Log($"✅ Đã HIỆN lại {restoredCount}/{originalCanvasSortOrders.Count} Canvas");
                originalCanvasSortOrders.Clear();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Lỗi khi ẩn/hiện UI: {e.Message}\n{e.StackTrace}");
        }
    }
    
    void UpdateTutorialDisplay()
    {
        if (tutorialPages.Count == 0) return;
        
        // Cập nhật nội dung
        if (tutorialContent != null)
        {
            tutorialContent.text = tutorialPages[currentPageIndex];
        }
        
        // Cập nhật tiêu đề
        if (tutorialTitle != null)
        {
            tutorialTitle.text = $"📖 HƯỚNG DẪN CHƠI ({currentPageIndex + 1}/{tutorialPages.Count})";
        }
        
        // Cập nhật trạng thái nút
        if (prevButton != null)
        {
            prevButton.interactable = currentPageIndex > 0;
        }
        
        if (nextButton != null)
        {
            if (currentPageIndex < tutorialPages.Count - 1)
            {
                nextButton.gameObject.SetActive(true);
                // Đổi text thành "Tiếp theo"
                var btnText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = "Tiếp theo ►";
            }
            else
            {
                nextButton.gameObject.SetActive(true);
                // Đổi text thành "Bắt đầu chơi"
                var btnText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = "🎮 Bắt đầu chơi!";
            }
        }
    }
    
    public void NextPage()
    {
        if (currentPageIndex < tutorialPages.Count - 1)
        {
            currentPageIndex++;
            UpdateTutorialDisplay();
        }
        else
        {
            // Trang cuối cùng, đóng tutorial
            CloseTutorial();
        }
    }
    
    public void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdateTutorialDisplay();
        }
    }
    
    public void CloseTutorial()
    {
        // KHÔNG CẦN UNPAUSE NỮA vì game không bao giờ pause
        // Time.timeScale = 1f; // ĐÃ XÓA
        
        // BẬT LẠI NÚT CUỐN SÁCH
        if (tutorialBookButton != null)
        {
            tutorialBookButton.interactable = true;
            Debug.Log("🔓 Đã BẬT lại nút cuốn sách");
        }
        
        // ẨN TOÀN BỘ TUTORIAL CANVAS (không chỉ panel)
        if (tutorialPanel != null && tutorialCanvas != null)
        {
            // Sử dụng instance variable tutorialCanvas đã lưu từ Start()
            tutorialCanvas.gameObject.SetActive(false);
            Debug.Log("✅ TutorialCanvas đã bị ẨN hoàn toàn");
        }
        else if (tutorialPanel != null)
        {
            // Fallback: Nếu reference bị mất, ẩn panel
            tutorialPanel.SetActive(false);
            Debug.LogWarning("⚠️ tutorialCanvas reference NULL! Đã ẩn panel thay thế.");
        }
        
        // HIỆN LẠI CÁC UI KHÁC - LUÔN GỌI
        HideOtherUI(false);
        
        // ĐẢM BẢO QUEST PANEL LUÔN HIỆN (BACKUP)
        TryShowQuestPanel();
        
        Debug.Log("✅ Tutorial đã đóng, game tiếp tục!");
    }
    
    /// <summary>
    /// Đảm bảo Quest Panel luôn hiện khi đóng tutorial
    /// </summary>
    private void TryShowQuestPanel()
    {
        try
        {
            // Cách 1: Qua QuestManager
            if (QuestManager.Instance != null && QuestManager.Instance.questPanel != null)
            {
                QuestManager.Instance.questPanel.SetActive(true);
                Debug.Log("✅ Quest Panel hiện qua QuestManager");
                return;
            }
            
            // Cách 2: Tìm trực tiếp trong scene
            GameObject questPanel = GameObject.Find("Quest Panel");
            if (questPanel == null)
                questPanel = GameObject.Find("QuestPanel");
            if (questPanel == null)
                questPanel = GameObject.Find("questPanel");
                
            if (questPanel != null)
            {
                questPanel.SetActive(true);
                Debug.Log("✅ Quest Panel hiện qua GameObject.Find");
                return;
            }
            
            Debug.LogWarning("⚠️ Không tìm thấy Quest Panel để hiện");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Lỗi khi hiện Quest Panel: {e.Message}");
        }
    }
    
    // Phương thức công khai để reset tutorial (cho test hoặc settings)
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(tutorialKey);
        PlayerPrefs.Save();
        Debug.Log("Tutorial đã được reset!");
    }
    
    void OnDestroy()
    {
        // Đảm bảo game không bị pause khi destroy
        Time.timeScale = 1f;
    }
    
    // Cho phép gọi tutorial từ code khác
    public static void ShowTutorialManually()
    {
        var manager = FindObjectOfType<GameTutorialManager>();
        if (manager != null)
        {
            manager.ShowTutorial();
        }
    }
}
