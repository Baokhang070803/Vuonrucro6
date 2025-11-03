using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

/// <summary>
/// Controller cho nút nạp tiền - có thể kéo và mở web nạp tiền
/// </summary>
public class RechargeButtonController : MonoBehaviour
{
    [Header("UI References")]
    public Button rechargeButton;
    public GameObject rechargePanel;
    public Text rechargeButtonText;
    
    [Header("Drag Settings")]
    public bool enableDrag = true;
    public float dragSensitivity = 1f;
    public RectTransform dragArea; // Vùng có thể kéo (mặc định là toàn màn hình)
    
    [Header("Web Settings")]
    public string rechargeUrl = "https://vuonrucro.netlify.app/recharge.html";
    public bool openInBrowser = true;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    // Private variables
    private bool isDragging = false;
    private Vector2 dragOffset;
    private RectTransform buttonRect;
    private Canvas parentCanvas;
    
    // Input System
    private Mouse mouse;
    private Vector2 originalPosition;
    private bool isPanelVisible = false;
    
    void Start()
    {
        InitializeComponents();
        SetupButtonEvents();
        StoreOriginalPosition();
        
        // ✅ KHỞI TẠO INPUT SYSTEM
        mouse = Mouse.current;
    }
    
    void Update()
    {
        HandleDrag();
    }
    
    /// <summary>
    /// Khởi tạo các component cần thiết
    /// </summary>
    void InitializeComponents()
    {
        // Tìm button nếu chưa được gán
        if (rechargeButton == null)
        {
            rechargeButton = GetComponent<Button>();
        }
        
        // Tìm text nếu chưa được gán
        if (rechargeButtonText == null && rechargeButton != null)
        {
            rechargeButtonText = rechargeButton.GetComponentInChildren<Text>();
        }
        
        // Lấy RectTransform
        buttonRect = GetComponent<RectTransform>();
        
        // Lấy Canvas parent
        parentCanvas = GetComponentInParent<Canvas>();
        
        // Tìm drag area nếu chưa được gán
        if (dragArea == null && parentCanvas != null)
        {
            dragArea = parentCanvas.GetComponent<RectTransform>();
        }
        
        DebugLog("✅ Đã khởi tạo RechargeButtonController");
    }
    
    /// <summary>
    /// Thiết lập events cho button
    /// </summary>
    void SetupButtonEvents()
    {
        if (rechargeButton != null)
        {
            rechargeButton.onClick.AddListener(OnRechargeButtonClick);
            DebugLog("✅ Đã thiết lập click event cho nút nạp tiền");
        }
        else
        {
            DebugLogWarning("❌ Không tìm thấy Button component!");
        }
    }
    
    /// <summary>
    /// Lưu vị trí ban đầu
    /// </summary>
    void StoreOriginalPosition()
    {
        if (buttonRect != null)
        {
            originalPosition = buttonRect.anchoredPosition;
            DebugLog($"📍 Vị trí ban đầu: {originalPosition}");
        }
    }
    
    /// <summary>
    /// Xử lý kéo thả
    /// </summary>
    void HandleDrag()
    {
        if (!enableDrag || buttonRect == null) return;
        
        // Bắt đầu kéo
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && IsMouseOverButton())
        {
            StartDrag();
        }
        
        // Đang kéo
        if (isDragging && mouse != null && mouse.leftButton.isPressed)
        {
            UpdateDragPosition();
        }
        
        // Kết thúc kéo
        if (isDragging && mouse != null && mouse.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }
    
    /// <summary>
    /// Kiểm tra chuột có đang ở trên button không
    /// </summary>
    bool IsMouseOverButton()
    {
        if (buttonRect == null) return false;
        
        Vector2 mousePosition = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            buttonRect, mousePosition, parentCanvas.worldCamera, out Vector2 localPoint);
        
        return buttonRect.rect.Contains(localPoint);
    }
    
    /// <summary>
    /// Bắt đầu kéo
    /// </summary>
    void StartDrag()
    {
        isDragging = true;
        
        // Tính offset từ vị trí chuột đến tâm button
        Vector2 mousePosition = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            buttonRect, mousePosition, parentCanvas.worldCamera, out Vector2 localPoint);
        
        dragOffset = localPoint;
        
        DebugLog("🖱️ Bắt đầu kéo nút nạp tiền");
    }
    
    /// <summary>
    /// Cập nhật vị trí khi kéo
    /// </summary>
    void UpdateDragPosition()
    {
        if (buttonRect == null || dragArea == null) return;
        
        Vector2 mousePosition = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragArea, mousePosition, parentCanvas.worldCamera, out Vector2 localPoint);
        
        // Giới hạn trong vùng drag area
        Vector2 clampedPosition = new Vector2(
            Mathf.Clamp(localPoint.x - dragOffset.x, dragArea.rect.xMin, dragArea.rect.xMax),
            Mathf.Clamp(localPoint.y - dragOffset.y, dragArea.rect.yMin, dragArea.rect.yMax)
        );
        
        buttonRect.anchoredPosition = clampedPosition;
    }
    
    /// <summary>
    /// Kết thúc kéo
    /// </summary>
    void EndDrag()
    {
        isDragging = false;
        DebugLog($"📍 Kết thúc kéo tại vị trí: {buttonRect.anchoredPosition}");
    }
    
    /// <summary>
    /// Xử lý khi click nút nạp tiền
    /// </summary>
    public void OnRechargeButtonClick()
    {
        DebugLog("💰 Nút nạp tiền được click!");
        
        // Hiển thị panel nạp tiền
        ShowRechargePanel();
        
        // Mở web nạp tiền
        OpenRechargeWebsite();
    }
    
    /// <summary>
    /// Hiển thị panel nạp tiền
    /// </summary>
    void ShowRechargePanel()
    {
        if (rechargePanel != null)
        {
            if (!isPanelVisible)
            {
                StartCoroutine(FadeInPanel());
            }
            else
            {
                StartCoroutine(FadeOutPanel());
            }
        }
    }
    
    /// <summary>
    /// Mở website nạp tiền
    /// </summary>
    void OpenRechargeWebsite()
    {
        DebugLog($"🌐 Đang mở website nạp tiền: {rechargeUrl}");
        
        if (openInBrowser)
        {
            // Mở trong browser mặc định
            Application.OpenURL(rechargeUrl);
        }
        else
        {
            // Có thể implement WebView nếu cần
            DebugLog("📱 Mở trong WebView (chưa implement)");
        }
    }
    
    /// <summary>
    /// Fade in panel
    /// </summary>
    IEnumerator FadeInPanel()
    {
        if (rechargePanel == null) yield break;
        
        rechargePanel.SetActive(true);
        isPanelVisible = true;
        
        CanvasGroup canvasGroup = rechargePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = rechargePanel.AddComponent<CanvasGroup>();
        }
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            canvasGroup.alpha = fadeCurve.Evaluate(progress);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        DebugLog("✅ Panel nạp tiền đã hiển thị");
    }
    
    /// <summary>
    /// Fade out panel
    /// </summary>
    IEnumerator FadeOutPanel()
    {
        if (rechargePanel == null) yield break;
        
        CanvasGroup canvasGroup = rechargePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = rechargePanel.AddComponent<CanvasGroup>();
        }
        
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutDuration;
            canvasGroup.alpha = fadeCurve.Evaluate(1f - progress);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        rechargePanel.SetActive(false);
        isPanelVisible = false;
        DebugLog("✅ Panel nạp tiền đã ẩn");
    }
    
    /// <summary>
    /// Reset về vị trí ban đầu
    /// </summary>
    [ContextMenu("Reset Position")]
    public void ResetToOriginalPosition()
    {
        if (buttonRect != null)
        {
            buttonRect.anchoredPosition = originalPosition;
            DebugLog("🔄 Đã reset về vị trí ban đầu");
        }
    }
    
    /// <summary>
    /// Ẩn/hiện nút nạp tiền
    /// </summary>
    public void ToggleRechargeButton()
    {
        if (rechargeButton != null)
        {
            rechargeButton.gameObject.SetActive(!rechargeButton.gameObject.activeSelf);
            DebugLog($"👁️ Nút nạp tiền: {(rechargeButton.gameObject.activeSelf ? "Hiện" : "Ẩn")}");
        }
    }
    
    /// <summary>
    /// Cập nhật text nút
    /// </summary>
    public void UpdateButtonText(string newText)
    {
        if (rechargeButtonText != null)
        {
            rechargeButtonText.text = newText;
            DebugLog($"📝 Đã cập nhật text nút: {newText}");
        }
    }
    
    /// <summary>
    /// Debug log
    /// </summary>
    void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[RechargeButtonController] {message}");
        }
    }
    
    /// <summary>
    /// Debug log warning
    /// </summary>
    void DebugLogWarning(string message)
    {
        if (showDebugLogs)
        {
            Debug.LogWarning($"[RechargeButtonController] {message}");
        }
    }
    
    /// <summary>
    /// Debug: Hiển thị thông tin button
    /// </summary>
    [ContextMenu("Debug Button Info")]
    public void DebugButtonInfo()
    {
        Debug.Log("=== RECHARGE BUTTON INFO ===");
        Debug.Log($"Button: {(rechargeButton != null ? "OK" : "NULL")}");
        Debug.Log($"Panel: {(rechargePanel != null ? "OK" : "NULL")}");
        Debug.Log($"Text: {(rechargeButtonText != null ? "OK" : "NULL")}");
        Debug.Log($"Drag Enabled: {enableDrag}");
        Debug.Log($"Position: {buttonRect?.anchoredPosition}");
        Debug.Log($"Original Position: {originalPosition}");
        Debug.Log($"Is Dragging: {isDragging}");
        Debug.Log($"Panel Visible: {isPanelVisible}");
        Debug.Log($"Recharge URL: {rechargeUrl}");
        Debug.Log("==========================");
    }
}
