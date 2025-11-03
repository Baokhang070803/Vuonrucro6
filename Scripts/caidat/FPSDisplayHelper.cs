using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper script để tự động tạo FPS Display trong scene
/// Đặt script này vào một GameObject bất kỳ trong scene
/// </summary>
public class FPSDisplayHelper : MonoBehaviour
{
    [Header("Auto Setup Settings")]
    [Tooltip("Tự động tạo FPS Display khi Start")]
    public bool autoCreateOnStart = false;
    
    [Tooltip("Vị trí FPS Display (góc màn hình)")]
    public TextAnchor anchorPosition = TextAnchor.UpperLeft;
    
    [Tooltip("Offset từ góc màn hình (pixel)")]
    public Vector2 offset = new Vector2(10, 10);
    
    [Header("FPS Display Settings")]
    [Tooltip("Font size")]
    public int fontSize = 24;
    
    [Tooltip("Màu chữ")]
    public Color textColor = Color.white;
    
    [Tooltip("Hiển thị ngay từ đầu")]
    public bool showOnStart = true;
    
    void Start()
    {
        if (autoCreateOnStart)
        {
            CreateFPSDisplay();
        }
    }
    
    /// <summary>
    /// Tạo FPS Display tự động
    /// </summary>
    [ContextMenu("Create FPS Display")]
    public void CreateFPSDisplay()
    {
        // Kiểm tra xem đã có FPS Display chưa
        if (FindObjectOfType<FPSDisplay>() != null)
        {
            Debug.LogWarning("[FPSDisplayHelper] Đã có FPS Display trong scene! Bỏ qua tạo mới.");
            return;
        }
        
        // Tạo Canvas mới cho FPS Display
        GameObject canvasObj = new GameObject("FPS Display Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Ở trên cùng
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Tạo Text GameObject
        GameObject textObj = new GameObject("FPS Text");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        // Thêm RectTransform và đặt vị trí
        RectTransform rectTransform = textObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = GetAnchorMin(anchorPosition);
        rectTransform.anchorMax = GetAnchorMax(anchorPosition);
        rectTransform.pivot = GetPivot(anchorPosition);
        rectTransform.anchoredPosition = offset;
        rectTransform.sizeDelta = new Vector2(200, 50);
        
        // Ưu tiên TextMeshPro
        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = "FPS: 60";
        tmpText.fontSize = fontSize;
        tmpText.color = textColor;
        tmpText.alignment = TextAlignmentOptions.Left;
        
        // Thêm FPSDisplay component
        FPSDisplay fpsDisplay = textObj.AddComponent<FPSDisplay>();
        fpsDisplay.fpsTextTMP = tmpText;
        fpsDisplay.showOnStart = showOnStart;
        fpsDisplay.useColorGradient = true;
        
        Debug.Log("[FPSDisplayHelper] ✅ Đã tạo FPS Display tự động!");
    }
    
    /// <summary>
    /// Lấy anchor min dựa trên anchor position
    /// </summary>
    Vector2 GetAnchorMin(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.LowerLeft:
                return new Vector2(0, 0);
            case TextAnchor.UpperCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.LowerCenter:
                return new Vector2(0.5f, 0);
            case TextAnchor.UpperRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.LowerRight:
                return new Vector2(1, 0);
            default:
                return new Vector2(0, 1);
        }
    }
    
    /// <summary>
    /// Lấy anchor max dựa trên anchor position
    /// </summary>
    Vector2 GetAnchorMax(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.LowerLeft:
                return new Vector2(0, 1);
            case TextAnchor.UpperCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.LowerCenter:
                return new Vector2(0.5f, 1);
            case TextAnchor.UpperRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.LowerRight:
                return new Vector2(1, 1);
            default:
                return new Vector2(1, 1);
        }
    }
    
    /// <summary>
    /// Lấy pivot dựa trên anchor position
    /// </summary>
    Vector2 GetPivot(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                return new Vector2(0, 1);
            case TextAnchor.UpperCenter:
                return new Vector2(0.5f, 1);
            case TextAnchor.UpperRight:
                return new Vector2(1, 1);
            case TextAnchor.MiddleLeft:
                return new Vector2(0, 0.5f);
            case TextAnchor.MiddleCenter:
                return new Vector2(0.5f, 0.5f);
            case TextAnchor.MiddleRight:
                return new Vector2(1, 0.5f);
            case TextAnchor.LowerLeft:
                return new Vector2(0, 0);
            case TextAnchor.LowerCenter:
                return new Vector2(0.5f, 0);
            case TextAnchor.LowerRight:
                return new Vector2(1, 0);
            default:
                return new Vector2(0, 1);
        }
    }
}

