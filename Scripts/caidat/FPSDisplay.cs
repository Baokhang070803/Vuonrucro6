using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // ✅ Thêm Input System

/// <summary>
/// Script hiển thị FPS trong game
/// Hỗ trợ cả TextMeshPro và Legacy Text
/// </summary>
public class FPSDisplay : MonoBehaviour
{
    [Header("UI References (Chọn 1 trong 2)")]
    [Tooltip("TextMeshPro để hiển thị FPS (khuyến nghị)")]
    public TextMeshProUGUI fpsTextTMP;
    [Tooltip("Legacy Text để hiển thị FPS (fallback)")]
    public Text fpsText;
    
    [Header("Settings")]
    [Tooltip("Cập nhật FPS mỗi bao nhiêu giây")]
    [Range(0.1f, 1f)]
    public float updateInterval = 0.5f;
    
    [Tooltip("Hiển thị FPS trung bình (smooth)")]
    public bool showAverageFPS = true;
    
    [Tooltip("Hiển thị FPS ngay từ đầu")]
    public bool showOnStart = true;
    
    [Tooltip("Phím tắt để bật/tắt FPS display (nhấn 0 để tắt)")]
    public KeyCode toggleKey = KeyCode.F3;
    
    [Header("Color Settings")]
    [Tooltip("Tự động đổi màu theo FPS")]
    public bool useColorGradient = true;
    
    [Tooltip("Màu khi FPS cao (>=60)")]
    public Color colorHigh = new Color(0f, 1f, 0f, 1f); // Xanh lá
    
    [Tooltip("Màu khi FPS trung bình (30-59)")]
    public Color colorMedium = new Color(1f, 1f, 0f, 1f); // Vàng
    
    [Tooltip("Màu khi FPS thấp (<30)")]
    public Color colorLow = new Color(1f, 0f, 0f, 1f); // Đỏ
    
    [Header("Format Settings")]
    [Tooltip("Định dạng hiển thị (có thể dùng {0} cho FPS)")]
    public string displayFormat = "FPS: {0}";
    
    // Private variables
    private float deltaTime = 0.0f;
    private float fps = 0.0f;
    private float averageFPS = 0.0f;
    private float frameCount = 0f;
    private float totalFPS = 0f;
    private float lastUpdateTime = 0f;
    private bool isVisible = true;
    
    void Start()
    {
        // Kiểm tra có component text nào không
        if (fpsTextTMP == null && fpsText == null)
        {
            Debug.LogWarning("[FPSDisplay] Chưa gán Text hoặc TextMeshPro! Tự động tìm...");
            
            // Tự động tìm TextMeshPro hoặc Text
            fpsTextTMP = GetComponent<TextMeshProUGUI>();
            if (fpsTextTMP == null)
            {
                fpsTextTMP = GetComponentInChildren<TextMeshProUGUI>();
            }
            
            if (fpsTextTMP == null)
            {
                fpsText = GetComponent<Text>();
                if (fpsText == null)
                {
                    fpsText = GetComponentInChildren<Text>();
                }
            }
            
            if (fpsTextTMP == null && fpsText == null)
            {
                Debug.LogError("[FPSDisplay] Không tìm thấy Text hoặc TextMeshPro component! Vui lòng gán trong Inspector.");
                enabled = false;
                return;
            }
        }
        
        // Ẩn/hiện theo showOnStart
        isVisible = showOnStart;
        UpdateVisibility();
        
        // Khởi tạo
        lastUpdateTime = Time.time;
        Debug.Log("[FPSDisplay] FPS Display đã khởi tạo!");
    }
    
    void Update()
    {
        // Toggle với phím tắt (Input System)
        if (CheckKeyPressed(toggleKey))
        {
            isVisible = !isVisible;
            UpdateVisibility();
            Debug.Log($"[FPSDisplay] FPS Display: {(isVisible ? "BẬT" : "TẮT")}");
        }
        
        if (!isVisible) return;
        
        // Tính toán FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fps = 1.0f / deltaTime;
        
        // Tính FPS trung bình
        if (showAverageFPS)
        {
            frameCount++;
            totalFPS += fps;
            averageFPS = totalFPS / frameCount;
        }
        
        // Cập nhật hiển thị theo interval
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// Cập nhật hiển thị FPS
    /// </summary>
    void UpdateDisplay()
    {
        float displayFPS = showAverageFPS ? averageFPS : fps;
        int fpsInt = Mathf.RoundToInt(displayFPS);
        
        // Định dạng text
        string fpsString = string.Format(displayFormat, fpsInt);
        
        // Cập nhật TextMeshPro
        if (fpsTextTMP != null)
        {
            fpsTextTMP.text = fpsString;
            
            // Đổi màu theo FPS
            if (useColorGradient)
            {
                fpsTextTMP.color = GetFPSColor(fpsInt);
            }
        }
        // Cập nhật Legacy Text
        else if (fpsText != null)
        {
            fpsText.text = fpsString;
            
            // Đổi màu theo FPS
            if (useColorGradient)
            {
                fpsText.color = GetFPSColor(fpsInt);
            }
        }
    }
    
    /// <summary>
    /// Lấy màu dựa trên FPS
    /// </summary>
    Color GetFPSColor(int fps)
    {
        if (fps >= 60)
            return colorHigh;
        else if (fps >= 30)
            return colorMedium;
        else
            return colorLow;
    }
    
    /// <summary>
    /// Cập nhật visibility
    /// </summary>
    void UpdateVisibility()
    {
        if (fpsTextTMP != null)
        {
            fpsTextTMP.gameObject.SetActive(isVisible);
        }
        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(isVisible);
        }
    }
    
    /// <summary>
    /// Bật FPS display
    /// </summary>
    public void Show()
    {
        isVisible = true;
        UpdateVisibility();
    }
    
    /// <summary>
    /// Tắt FPS display
    /// </summary>
    public void Hide()
    {
        isVisible = false;
        UpdateVisibility();
    }
    
    /// <summary>
    /// Toggle FPS display
    /// </summary>
    public void Toggle()
    {
        isVisible = !isVisible;
        UpdateVisibility();
    }
    
    /// <summary>
    /// Reset FPS average
    /// </summary>
    public void ResetAverage()
    {
        frameCount = 0f;
        totalFPS = 0f;
        averageFPS = 0f;
    }
    
    /// <summary>
    /// Kiểm tra phím được nhấn (Input System compatible)
    /// </summary>
    private bool CheckKeyPressed(KeyCode keyCode)
    {
        // Kiểm tra Keyboard có sẵn không
        if (Keyboard.current == null)
            return false;
        
        // Map KeyCode sang Key từ Input System
        Key key = KeyCodeToKey(keyCode);
        
        // Kiểm tra key được nhấn
        return key != Key.None && Keyboard.current[key].wasPressedThisFrame;
    }
    
    /// <summary>
    /// Convert KeyCode sang Key (Input System)
    /// </summary>
    private Key KeyCodeToKey(KeyCode keyCode)
    {
        // Map các phím phổ biến
        switch (keyCode)
        {
            case KeyCode.F1: return Key.F1;
            case KeyCode.F2: return Key.F2;
            case KeyCode.F3: return Key.F3;
            case KeyCode.F4: return Key.F4;
            case KeyCode.F5: return Key.F5;
            case KeyCode.F6: return Key.F6;
            case KeyCode.F7: return Key.F7;
            case KeyCode.F8: return Key.F8;
            case KeyCode.F9: return Key.F9;
            case KeyCode.F10: return Key.F10;
            case KeyCode.F11: return Key.F11;
            case KeyCode.F12: return Key.F12;
            
            case KeyCode.Alpha0: return Key.Digit0;
            case KeyCode.Alpha1: return Key.Digit1;
            case KeyCode.Alpha2: return Key.Digit2;
            case KeyCode.Alpha3: return Key.Digit3;
            case KeyCode.Alpha4: return Key.Digit4;
            case KeyCode.Alpha5: return Key.Digit5;
            case KeyCode.Alpha6: return Key.Digit6;
            case KeyCode.Alpha7: return Key.Digit7;
            case KeyCode.Alpha8: return Key.Digit8;
            case KeyCode.Alpha9: return Key.Digit9;
            
            case KeyCode.A: return Key.A;
            case KeyCode.B: return Key.B;
            case KeyCode.C: return Key.C;
            case KeyCode.D: return Key.D;
            case KeyCode.E: return Key.E;
            case KeyCode.F: return Key.F;
            case KeyCode.G: return Key.G;
            case KeyCode.H: return Key.H;
            case KeyCode.I: return Key.I;
            case KeyCode.J: return Key.J;
            case KeyCode.K: return Key.K;
            case KeyCode.L: return Key.L;
            case KeyCode.M: return Key.M;
            case KeyCode.N: return Key.N;
            case KeyCode.O: return Key.O;
            case KeyCode.P: return Key.P;
            case KeyCode.Q: return Key.Q;
            case KeyCode.R: return Key.R;
            case KeyCode.S: return Key.S;
            case KeyCode.T: return Key.T;
            case KeyCode.U: return Key.U;
            case KeyCode.V: return Key.V;
            case KeyCode.W: return Key.W;
            case KeyCode.X: return Key.X;
            case KeyCode.Y: return Key.Y;
            case KeyCode.Z: return Key.Z;
            
            case KeyCode.Space: return Key.Space;
            case KeyCode.Return: return Key.Enter;
            case KeyCode.Escape: return Key.Escape;
            case KeyCode.Tab: return Key.Tab;
            case KeyCode.Backspace: return Key.Backspace;
            case KeyCode.Delete: return Key.Delete;
            
            case KeyCode.LeftShift: return Key.LeftShift;
            case KeyCode.RightShift: return Key.RightShift;
            case KeyCode.LeftControl: return Key.LeftCtrl;
            case KeyCode.RightControl: return Key.RightCtrl;
            case KeyCode.LeftAlt: return Key.LeftAlt;
            case KeyCode.RightAlt: return Key.RightAlt;
            
            case KeyCode.UpArrow: return Key.UpArrow;
            case KeyCode.DownArrow: return Key.DownArrow;
            case KeyCode.LeftArrow: return Key.LeftArrow;
            case KeyCode.RightArrow: return Key.RightArrow;
            
            default:
                Debug.LogWarning($"[FPSDisplay] KeyCode {keyCode} chưa được map, sử dụng F3 mặc định");
                return Key.F3; // Fallback
        }
    }
}

