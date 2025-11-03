using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PVP
{
    /// <summary>
    /// Toggle nút tăng tốc: X1 → X2 → X4 → X1 → ...
    /// Chỉ cần 1 nút duy nhất, thay đổi ảnh khi click!
    /// </summary>
    public class GameSpeedToggle : MonoBehaviour
    {
        [Header("Button Reference")]
        [SerializeField] private Button speedButton;
        [SerializeField] private Image buttonImage; // Ảnh của button
        
        [Header("Button Images")]
        [SerializeField] private Sprite imageX1; // Ảnh khi X1
        [SerializeField] private Sprite imageX2; // Ảnh khi X2
        [SerializeField] private Sprite imageX4; // Ảnh khi X4
        
        [Header("Optional Text")]
        [SerializeField] private TextMeshProUGUI buttonText; // Text hiển thị X1/X2/X4 (tùy chọn)
        
        [Header("Glow Effect")]
        [SerializeField] private bool enableGlowEffect = true;
        [SerializeField] private Image glowImage; // Image riêng cho glow (tùy chọn)
        [SerializeField] private Color glowColorX2 = new Color(1f, 1f, 0f, 0.5f); // Vàng cho X2
        [SerializeField] private Color glowColorX4 = new Color(1f, 0.3f, 0f, 0.7f); // Cam/đỏ cho X4
        [SerializeField] private float glowDuration = 0.3f; // Thời gian fade in
        [SerializeField] private bool pulseEffect = true; // Hiệu ứng nhấp nháy
        [SerializeField] private float pulseSpeed = 2f; // Tốc độ pulse
        
        [Header("Video Integration")]
        [SerializeField] private VideoSkillPlayer videoSkillPlayer;
        
        [Header("Settings")]
        [SerializeField] private float speed1x = 1f;
        [SerializeField] private float speed2x = 2f;
        [SerializeField] private float speed4x = 4f;
        
        // Current state
        private SpeedState currentState = SpeedState.X1;
        private UnityEngine.Coroutine glowCoroutine;
        
        public enum SpeedState
        {
            X1 = 1,
            X2 = 2,
            X4 = 4
        }
        
        // Event
        public System.Action<float, SpeedState> OnSpeedChanged;
        
        private void Start()
        {
            // Tìm VideoSkillPlayer nếu chưa gán
            if (videoSkillPlayer == null)
            {
                videoSkillPlayer = FindObjectOfType<VideoSkillPlayer>();
            }
            
            // Tìm button nếu chưa gán
            if (speedButton == null)
            {
                speedButton = GetComponent<Button>();
            }
            
            // Tìm image nếu chưa gán
            if (buttonImage == null && speedButton != null)
            {
                buttonImage = speedButton.GetComponent<Image>();
            }
            
            // Tìm hoặc tạo glow image
            if (enableGlowEffect && glowImage == null && speedButton != null)
            {
                // Tìm child có tên "Glow" hoặc tạo mới
                Transform glowTransform = speedButton.transform.Find("Glow");
                if (glowTransform != null)
                {
                    glowImage = glowTransform.GetComponent<Image>();
                }
                else
                {
                    // Tự động tạo Glow Image
                    CreateGlowImage();
                }
            }
            
            // Ẩn glow ban đầu
            if (glowImage != null)
            {
                glowImage.color = new Color(1, 1, 1, 0);
            }
            
            // Tìm text nếu chưa gán
            if (buttonText == null && speedButton != null)
            {
                buttonText = speedButton.GetComponentInChildren<TextMeshProUGUI>();
            }
            
            // Setup button click
            if (speedButton != null)
            {
                speedButton.onClick.AddListener(ToggleSpeed);
            }
            
            // Set trạng thái ban đầu X1
            SetSpeed(SpeedState.X1);
        }
        
        /// <summary>
        /// Toggle qua tốc độ tiếp theo: X1 → X2 → X4 → X1
        /// </summary>
        public void ToggleSpeed()
        {
            switch (currentState)
            {
                case SpeedState.X1:
                    SetSpeed(SpeedState.X2);
                    break;
                case SpeedState.X2:
                    SetSpeed(SpeedState.X4);
                    break;
                case SpeedState.X4:
                    SetSpeed(SpeedState.X1);
                    break;
            }
        }
        
        /// <summary>
        /// Set tốc độ cụ thể
        /// </summary>
        public void SetSpeed(SpeedState state)
        {
            currentState = state;
            
            float speed = 1f;
            string displayText = "X1";
            Sprite displayImage = imageX1;
            
            switch (state)
            {
                case SpeedState.X1:
                    speed = speed1x;
                    displayText = "X1";
                    displayImage = imageX1;
                    break;
                case SpeedState.X2:
                    speed = speed2x;
                    displayText = "X2";
                    displayImage = imageX2;
                    break;
                case SpeedState.X4:
                    speed = speed4x;
                    displayText = "X4";
                    displayImage = imageX4;
                    break;
            }
            
            // Áp dụng Time.timeScale
            Time.timeScale = speed;
            
            // Update video speed
            if (videoSkillPlayer != null)
            {
                videoSkillPlayer.UpdateVideoSpeed(speed);
            }
            
            // Update button image
            if (buttonImage != null && displayImage != null)
            {
                buttonImage.sprite = displayImage;
                Debug.Log($"🖼️ Button image changed to: {displayImage.name}");
            }
            
            // Update button text (nếu có)
            if (buttonText != null)
            {
                buttonText.text = displayText;
            }
            
            // Apply glow effect
            if (enableGlowEffect)
            {
                ApplyGlowEffect(state);
            }
            
            // Trigger event
            OnSpeedChanged?.Invoke(speed, state);
            
            Debug.Log($"⚡ Game speed: {displayText} (Time.timeScale = {speed})");
        }
        
        /// <summary>
        /// Áp dụng hiệu ứng phát sáng
        /// </summary>
        private void ApplyGlowEffect(SpeedState state)
        {
            if (glowImage == null) return;
            
            // Stop coroutine cũ nếu có
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
            }
            
            switch (state)
            {
                case SpeedState.X1:
                    // Tắt glow khi X1
                    glowCoroutine = StartCoroutine(FadeGlow(Color.clear, glowDuration));
                    break;
                    
                case SpeedState.X2:
                    // Glow vàng cho X2
                    glowCoroutine = StartCoroutine(GlowWithPulse(glowColorX2));
                    break;
                    
                case SpeedState.X4:
                    // Glow cam/đỏ cho X4
                    glowCoroutine = StartCoroutine(GlowWithPulse(glowColorX4));
                    break;
            }
        }
        
        /// <summary>
        /// Fade glow đến màu mục tiêu
        /// </summary>
        private System.Collections.IEnumerator FadeGlow(Color targetColor, float duration)
        {
            if (glowImage == null) yield break;
            
            Color startColor = glowImage.color;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                glowImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            
            glowImage.color = targetColor;
        }
        
        /// <summary>
        /// Glow với hiệu ứng pulse (nhấp nháy)
        /// </summary>
        private System.Collections.IEnumerator GlowWithPulse(Color glowColor)
        {
            if (glowImage == null) yield break;
            
            // Fade in nhanh
            yield return StartCoroutine(FadeGlow(glowColor, glowDuration));
            
            // Pulse effect nếu bật
            if (pulseEffect)
            {
                while (true)
                {
                    float alpha = glowColor.a;
                    float minAlpha = alpha * 0.5f;
                    float maxAlpha = alpha;
                    
                    // Fade out
                    float elapsed = 0f;
                    float pulseDuration = 1f / pulseSpeed;
                    
                    while (elapsed < pulseDuration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        float t = elapsed / pulseDuration;
                        float currentAlpha = Mathf.Lerp(maxAlpha, minAlpha, t);
                        
                        Color currentColor = glowColor;
                        currentColor.a = currentAlpha;
                        glowImage.color = currentColor;
                        
                        yield return null;
                    }
                    
                    // Fade in
                    elapsed = 0f;
                    while (elapsed < pulseDuration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        float t = elapsed / pulseDuration;
                        float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);
                        
                        Color currentColor = glowColor;
                        currentColor.a = currentAlpha;
                        glowImage.color = currentColor;
                        
                        yield return null;
                    }
                }
            }
        }
        
        /// <summary>
        /// Tự động tạo Glow Image
        /// </summary>
        private void CreateGlowImage()
        {
            if (speedButton == null) return;
            
            // Tạo GameObject mới cho glow
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(speedButton.transform, false);
            
            // Add Image component
            glowImage = glowObj.AddComponent<Image>();
            
            // Setup RectTransform (phủ toàn bộ button)
            RectTransform glowRect = glowImage.GetComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(20, 20); // Rộng hơn button một chút
            glowRect.anchoredPosition = Vector2.zero;
            
            // Đặt ở dưới các element khác
            glowObj.transform.SetAsFirstSibling();
            
            // Màu trắng, alpha = 0 ban đầu
            glowImage.color = new Color(1, 1, 1, 0);
            
            // Dùng sprite tròn nếu có
            // glowImage.sprite = Resources.Load<Sprite>("UI/Circle"); // Nếu có sprite
            
            Debug.Log("✨ Auto-created Glow Image!");
        }
        
        /// <summary>
        /// Get tốc độ hiện tại
        /// </summary>
        public float GetCurrentSpeed()
        {
            switch (currentState)
            {
                case SpeedState.X1: return speed1x;
                case SpeedState.X2: return speed2x;
                case SpeedState.X4: return speed4x;
                default: return 1f;
            }
        }
        
        /// <summary>
        /// Get state hiện tại
        /// </summary>
        public SpeedState GetCurrentState()
        {
            return currentState;
        }
        
        /// <summary>
        /// Setup button và images từ code
        /// </summary>
        public void SetupButton(Button button, Sprite imgX1, Sprite imgX2, Sprite imgX4)
        {
            speedButton = button;
            imageX1 = imgX1;
            imageX2 = imgX2;
            imageX4 = imgX4;
            
            if (speedButton != null)
            {
                speedButton.onClick.RemoveAllListeners();
                speedButton.onClick.AddListener(ToggleSpeed);
                
                buttonImage = speedButton.GetComponent<Image>();
                buttonText = speedButton.GetComponentInChildren<TextMeshProUGUI>();
            }
            
            SetSpeed(SpeedState.X1);
        }
        
        private void OnDestroy()
        {
            // Stop glow coroutine
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
            }
            
            // Reset Time.timeScale về 1 khi destroy
            Time.timeScale = 1f;
        }
        
        private void OnApplicationQuit()
        {
            // Reset Time.timeScale về 1 khi thoát game
            Time.timeScale = 1f;
        }
    }
}
