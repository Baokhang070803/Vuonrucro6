using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI hiển thị thanh EXP và text cấp độ
/// </summary>
public class ExpUI : MonoBehaviour
{
    [Header("UI Components")]
    public Slider expSlider;                    // Thanh EXP
    public TextMeshProUGUI levelText;           // Text hiển thị cấp độ
    public TextMeshProUGUI expText;             // Text hiển thị EXP (VD: 150/200)
    public TextMeshProUGUI statPointsText;     // Text hiển thị điểm chỉ số
    
    [Header("Animation Settings")]
    public float expBarAnimationSpeed = 2f;     // Tốc độ animation thanh EXP
    public float textAnimationDuration = 0.5f; // Thời gian animation text
    public Color levelUpColor = Color.yellow;   // Màu khi lên cấp
    public Color normalColor = Color.white;     // Màu bình thường
    
    [Header("Level Up Effects")]
    public GameObject levelUpEffect;            // Effect khi lên cấp
    public AudioClip levelUpSound;              // Âm thanh lên cấp
    public float effectDuration = 2f;           // Thời gian hiển thị effect
    
    private PlayerExpManager expManager;
    private AudioSource audioSource;
    private Coroutine expBarAnimation;
    private Coroutine textAnimation;
    
    void Start()
    {
        // Tìm PlayerExpManager
        expManager = PlayerExpManager.Instance;
        if (expManager == null)
        {
            Debug.LogError("[ExpUI] Không tìm thấy PlayerExpManager!");
            return;
        }
        
        // Tìm AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Đăng ký events
        expManager.OnExpChanged += UpdateExpUI;
        expManager.OnLevelUp += OnLevelUp;
        expManager.OnExpGained += OnExpGained;
        
        // Cập nhật UI ban đầu
        UpdateExpUI(expManager.GetExpData());
        
        Debug.Log("[ExpUI] Đã khởi tạo!");
    }
    
    void OnDestroy()
    {
        // Hủy đăng ký events
        if (expManager != null)
        {
            expManager.OnExpChanged -= UpdateExpUI;
            expManager.OnLevelUp -= OnLevelUp;
            expManager.OnExpGained -= OnExpGained;
        }
    }
    
    /// <summary>
    /// Cập nhật UI EXP
    /// </summary>
    void UpdateExpUI(ExpData expData)
    {
        // Cập nhật thanh EXP
        UpdateExpBar(expData);
        
        // Cập nhật text
        UpdateTexts(expData);
    }
    
    /// <summary>
    /// Cập nhật thanh EXP với animation
    /// </summary>
    void UpdateExpBar(ExpData expData)
    {
        float targetValue = expData.GetExpPercentage();
        
        if (expBarAnimation != null)
        {
            StopCoroutine(expBarAnimation);
        }
        
        expBarAnimation = StartCoroutine(AnimateExpBar(targetValue));
    }
    
    /// <summary>
    /// Animation thanh EXP
    /// </summary>
    IEnumerator AnimateExpBar(float targetValue)
    {
        if (expSlider == null) yield break;
        
        float startValue = expSlider.value;
        float elapsedTime = 0f;
        
        while (elapsedTime < 1f / expBarAnimationSpeed)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime * expBarAnimationSpeed;
            
            expSlider.value = Mathf.Lerp(startValue, targetValue, progress);
            
            yield return null;
        }
        
        expSlider.value = targetValue;
        expBarAnimation = null;
    }
    
    /// <summary>
    /// Cập nhật các text với animation
    /// </summary>
    void UpdateTexts(ExpData expData)
    {
        // Cập nhật level text
        if (levelText != null)
        {
            levelText.text = expData.GetLevelString();
            Debug.Log($"[ExpUI] Cập nhật LevelText: {levelText.text}");
        }
        
        // Cập nhật EXP text
        if (expText != null)
        {
            expText.text = expData.GetExpString();
            Debug.Log($"[ExpUI] Cập nhật ExpText: {expText.text}");
        }
        
        // Cập nhật điểm chỉ số text
        if (statPointsText != null)
        {
            statPointsText.text = $"Chỉ số: {expData.GetStatPointsString()}";
            Debug.Log($"[ExpUI] Cập nhật StatPointsText: {statPointsText.text} (Stat Points: {expData.statPoints})");
        }
    }
    
    /// <summary>
    /// Khi lên cấp
    /// </summary>
    void OnLevelUp(int levelsGained)
    {
        Debug.Log($"[ExpUI] Lên {levelsGained} cấp!");
        
        // Hiệu ứng lên cấp
        StartCoroutine(LevelUpEffect(levelsGained));
        
        // Âm thanh lên cấp
        PlayLevelUpSound();
        
        // Animation text
        if (textAnimation != null)
        {
            StopCoroutine(textAnimation);
        }
        textAnimation = StartCoroutine(LevelUpTextAnimation());
    }
    
    /// <summary>
    /// Khi nhận EXP
    /// </summary>
    void OnExpGained(int expAmount)
    {
        Debug.Log($"[ExpUI] Nhận {expAmount} EXP!");
        
        // Có thể thêm hiệu ứng nhận EXP ở đây
        // VD: Popup text, particle effect, etc.
    }
    
    /// <summary>
    /// Hiệu ứng lên cấp
    /// </summary>
    IEnumerator LevelUpEffect(int levelsGained)
    {
        // Hiển thị effect
        if (levelUpEffect != null)
        {
            levelUpEffect.SetActive(true);
        }
        
        // Đợi một chút
        yield return new WaitForSeconds(effectDuration);
        
        // Ẩn effect
        if (levelUpEffect != null)
        {
            levelUpEffect.SetActive(false);
        }
    }
    
    /// <summary>
    /// Animation text khi lên cấp
    /// </summary>
    IEnumerator LevelUpTextAnimation()
    {
        if (levelText == null) yield break;
        
        Color originalColor = levelText.color;
        
        // Đổi màu vàng
        levelText.color = levelUpColor;
        
        // Scale up
        Vector3 originalScale = levelText.transform.localScale;
        levelText.transform.localScale = originalScale * 1.2f;
        
        // Đợi
        yield return new WaitForSeconds(textAnimationDuration);
        
        // Scale down và đổi màu về bình thường
        float elapsedTime = 0f;
        while (elapsedTime < textAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / textAnimationDuration;
            
            levelText.transform.localScale = Vector3.Lerp(originalScale * 1.2f, originalScale, progress);
            levelText.color = Color.Lerp(levelUpColor, normalColor, progress);
            
            yield return null;
        }
        
        // Đảm bảo về trạng thái ban đầu
        levelText.transform.localScale = originalScale;
        levelText.color = originalColor;
        
        textAnimation = null;
    }
    
    /// <summary>
    /// Phát âm thanh lên cấp
    /// </summary>
    void PlayLevelUpSound()
    {
        if (audioSource != null && levelUpSound != null)
        {
            audioSource.PlayOneShot(levelUpSound, 0.3f);
        }
    }
    
    /// <summary>
    /// Thiết lập màu sắc cho UI
    /// </summary>
    public void SetUIColor(Color levelColor, Color expColor, Color statColor)
    {
        if (levelText != null)
            levelText.color = levelColor;
        
        if (expText != null)
            expText.color = expColor;
        
        if (statPointsText != null)
            statPointsText.color = statColor;
    }
    
    /// <summary>
    /// Thiết lập màu thanh EXP
    /// </summary>
    public void SetExpBarColor(Color fillColor, Color backgroundColor)
    {
        if (expSlider == null) return;
        
        // Fill color
        if (expSlider.fillRect != null)
        {
            Image fillImage = expSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
                fillImage.color = fillColor;
        }
        
        // Background color
        Image backgroundImage = expSlider.GetComponent<Image>();
        if (backgroundImage != null)
            backgroundImage.color = backgroundColor;
    }
    
    /// <summary>
    /// Hiển thị/ẩn UI
    /// </summary>
    public void SetUIVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    
    /// <summary>
    /// Debug: Cập nhật UI thủ công
    /// </summary>
    [ContextMenu("Debug Update UI")]
    public void DebugUpdateUI()
    {
        if (expManager != null)
        {
            UpdateExpUI(expManager.GetExpData());
            Debug.Log("[ExpUI] Đã cập nhật UI thủ công!");
        }
        else
        {
            Debug.LogError("[ExpUI] PlayerExpManager is null!");
        }
    }
}
