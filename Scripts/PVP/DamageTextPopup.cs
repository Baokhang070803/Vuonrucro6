using UnityEngine;
using TMPro;
using System.Collections;

namespace PVP
{
    /// <summary>
    /// Hiển thị số damage bay lên khi nhân vật nhận sát thương
    /// </summary>
    public class DamageTextPopup : MonoBehaviour
    {
        [Header("Text Reference - Kéo text trên đầu nhân vật vào đây")]
        [Tooltip("Text damage đã có sẵn trên đầu nhân vật (phải có TextMeshPro)")]
        [SerializeField] private TextMeshProUGUI damageText;
        
        [Header("Spawn Settings")]
        [Tooltip("Vị trí offset so với vị trí gốc của text")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0, 50f, 0);
        
        [Tooltip("Random offset để text không spawn cùng vị trí")]
        [SerializeField] private Vector2 randomOffset = new Vector2(30f, 20f);
        
        [Header("Animation Settings")]
        [Tooltip("Thời gian text tồn tại trước khi mất")]
        [SerializeField] private float lifetime = 1.5f;
        
        [Tooltip("Tốc độ bay lên")]
        [SerializeField] private float floatSpeed = 50f;
        
        [Tooltip("Fade out speed")]
        [SerializeField] private float fadeSpeed = 2f;
        
        [Header("Text Style Settings")]
        [Tooltip("Font size cho damage thường")]
        [SerializeField] private float normalFontSize = 48f;
        
        [Tooltip("Font size cho critical damage")]
        [SerializeField] private float criticalFontSize = 64f;
        
        [Tooltip("Font weight (bold)")]
        [SerializeField] private FontStyles fontStyle = FontStyles.Bold;
        
        [Tooltip("Viền text (outline)")]
        [SerializeField] private bool enableOutline = true;
        
        [Tooltip("Độ dày viền")]
        [SerializeField] private float outlineWidth = 0.3f;
        
        [Tooltip("Màu viền")]
        [SerializeField] private Color outlineColor = Color.white;
        
        [Header("Color Settings")]
        [SerializeField] private Color normalDamageColor = Color.red;
        [SerializeField] private Color criticalDamageColor = new Color(1f, 0.5f, 0f); // Cam
        [SerializeField] private Color healColor = Color.green;
        
        [Header("References")]
        private HealthSystem healthSystem;
        private RectTransform originalTextTransform;
        private Vector3 originalTextPosition;
        private Color originalTextColor;
        
        private void Awake()
        {
            // Tìm HealthSystem
            healthSystem = GetComponentInParent<HealthSystem>();
            if (healthSystem == null)
            {
                healthSystem = GetComponent<HealthSystem>();
            }
            
            // Lưu vị trí và màu gốc của text
            if (damageText != null)
            {
                originalTextTransform = damageText.GetComponent<RectTransform>();
                originalTextPosition = originalTextTransform.anchoredPosition;
                originalTextColor = damageText.color;
                
                // Ẩn text ban đầu
                damageText.gameObject.SetActive(false);
            }
        }
        
        private void Start()
        {
            if (healthSystem != null)
            {
                // Subscribe vào event damage
                // Note: Logic hiển thị damage được gọi trực tiếp từ HealthSystem.TakeDamage()
                // nên không cần subscribe OnHealthChanged ở đây nữa
            }
        }
        
        private void OnDestroy()
        {
            // Cleanup nếu cần
        }
        
        /// <summary>
        /// Hiển thị text damage
        /// </summary>
        public void ShowDamageText(float amount, bool isDamage, bool isCritical = false)
        {
            if (damageText == null) return;
            
            // Active text
            damageText.gameObject.SetActive(true);
            
            // Reset về vị trí gốc
            if (originalTextTransform != null)
            {
                originalTextTransform.anchoredPosition = originalTextPosition;
            }
            
            // Add random offset
            Vector3 randomPos = originalTextPosition;
            randomPos.x += Random.Range(-randomOffset.x, randomOffset.x);
            randomPos.y += Random.Range(-randomOffset.y, randomOffset.y);
            originalTextTransform.anchoredPosition = randomPos;
            
            // Set text và màu
            if (isDamage)
            {
                damageText.text = $"-{amount:F0}";
                damageText.color = isCritical ? criticalDamageColor : normalDamageColor;
                damageText.fontSize = isCritical ? 48 : 36;
            }
            else
            {
                damageText.text = $"+{amount:F0}";
                damageText.color = healColor;
                damageText.fontSize = 36;
            }
            
            // Start animation
            StartCoroutine(AnimateDamageText());
        }
        
        /// <summary>
        /// Animate text bay lên và fade out
        /// </summary>
        private IEnumerator AnimateDamageText()
        {
            if (damageText == null || originalTextTransform == null) yield break;
            
            float elapsed = 0f;
            Vector3 startPos = originalTextTransform.anchoredPosition;
            Color startColor = damageText.color;
            
            // ✅ Tăng font size để dễ nhìn
            float originalFontSize = damageText.fontSize;
            damageText.fontSize = originalFontSize * 1.5f; // Lớn hơn 50%
            
            // ✅ Bật Outline (viền trắng)
            damageText.fontStyle = FontStyles.Bold; // In đậm
            damageText.outlineWidth = 0.3f; // Độ dày viền
            damageText.outlineColor = Color.white; // Màu viền trắng
            
            while (elapsed < lifetime)
            {
                // ✅ Dùng unscaledDeltaTime để KHÔNG bị ảnh hưởng bởi game speed
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / lifetime;
                
                // Bay lên
                originalTextTransform.anchoredPosition = startPos + spawnOffset * elapsed;
                
                // Fade out
                Color color = startColor;
                color.a = Mathf.Lerp(1f, 0f, progress * fadeSpeed);
                damageText.color = color;
                
                yield return null;
            }
            
            // Ẩn text sau khi hết thời gian
            damageText.gameObject.SetActive(false);
            
            // Reset lại màu, vị trí và font size
            if (originalTextTransform != null)
            {
                originalTextTransform.anchoredPosition = originalTextPosition;
                damageText.color = originalTextColor;
                damageText.fontSize = originalFontSize;
            }
        }
        
        /// <summary>
        /// API để gọi từ bên ngoài
        /// </summary>
        public void ShowDamage(float damage, bool isCritical = false)
        {
            ShowDamageText(damage, true, isCritical);
        }
        
        public void ShowHeal(float healAmount)
        {
            ShowDamageText(healAmount, false);
        }
    }
}
