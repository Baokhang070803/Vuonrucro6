using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace PVP
{
    public class HealthBarUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image healthBarFill; // Thanh máu (mautini/maumuthao)
        [SerializeField] private Image healthBarBackground; // Viền trang trí (vienmautini/vienmaumuthao)
        [SerializeField] private TextMeshProUGUI nameLabel; // Label tên (tentini/tenmuthao)
        [SerializeField] private Image characterSprite; // Sprite nhân vật (nhanvattini/nhanvatmuthao)
        
        [Header("Big Health Bar - Thanh máu lớn đồng bộ")]
        [Tooltip("Thanh máu lớn (ảnh ở trên đầu màn hình) sẽ đồng bộ với thanh máu này")]
        [SerializeField] private Image bigHealthBarFill; // Thanh máu lớn đồng bộ (VD: máu Tí Nị/Mụ Thảo ở trên)
        
        [Header("Animation Settings")]
        [SerializeField] private bool enableSmoothTransition = true;
        [SerializeField] private float transitionSpeed = 2f;
        [SerializeField] private bool enableDamageFlash = true;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.2f;
        
        [Header("Text Settings")]
        [SerializeField] private bool showHealthText = true;
        [SerializeField] private bool showPercentage = false;
        
        private HealthSystem healthSystem;
        private float targetFillAmount;
        private Color originalBarColor;
        private Coroutine flashCoroutine;
        
        private void Awake()
        {
            // Tìm HealthSystem trên cùng GameObject hoặc parent
            healthSystem = GetComponentInParent<HealthSystem>();
            
            if (healthSystem == null)
            {
                Debug.LogError($"Không tìm thấy HealthSystem cho {gameObject.name}!");
                return;
            }
            
            // Khởi tạo màu ban đầu cho thanh máu
            if (healthBarFill != null)
            {
                // Nếu thanh máu đang có màu, lưu lại
                if (healthBarFill.color != Color.black && healthBarFill.color.a > 0)
                {
                    originalBarColor = healthBarFill.color;
                }
                else
                {
                    // Nếu không có màu, set màu mặc định là đỏ
                    healthBarFill.color = Color.red;
                    originalBarColor = Color.red;
                }
            }
        }
        
        private void Start()
        {
            if (healthSystem != null)
            {
                // Đăng ký sự kiện
                healthSystem.OnHealthChanged += UpdateHealthBar;
                healthSystem.OnDeath += OnCharacterDeath;
                
                // Cập nhật UI ban đầu
                UpdateHealthBar(healthSystem.CurrentHealth, healthSystem.MaxHealth);
            }
        }
        
        private void OnDestroy()
        {
            if (healthSystem != null)
            {
                // Hủy đăng ký sự kiện
                healthSystem.OnHealthChanged -= UpdateHealthBar;
                healthSystem.OnDeath -= OnCharacterDeath;
            }
        }
        
        private void Update()
        {
            // Smooth transition cho thanh máu nhỏ
            if (enableSmoothTransition && healthBarFill != null)
            {
                float currentFill = healthBarFill.fillAmount;
                if (Mathf.Abs(currentFill - targetFillAmount) > 0.01f)
                {
                    healthBarFill.fillAmount = Mathf.Lerp(currentFill, targetFillAmount, transitionSpeed * Time.deltaTime);
                    
                    // ✅ ĐỒNG BỘ: Cập nhật thanh máu LỚN theo
                    if (bigHealthBarFill != null)
                    {
                        bigHealthBarFill.fillAmount = healthBarFill.fillAmount;
                    }
                }
            }
        }
        
        /// <summary>
        /// Cập nhật thanh máu khi máu thay đổi
        /// </summary>
        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            float healthPercentage = currentHealth / maxHealth;
            
            // Cập nhật fill amount cho thanh máu nhỏ
            if (enableSmoothTransition)
            {
                targetFillAmount = healthPercentage;
            }
            else
            {
                if (healthBarFill != null)
                {
                    healthBarFill.fillAmount = healthPercentage;
                }
            }
            
            // ✅ ĐỒNG BỘ: Cập nhật thanh máu LỚN (nếu có)
            if (bigHealthBarFill != null)
            {
                bigHealthBarFill.fillAmount = healthPercentage;
            }
            
            // Cập nhật text
            UpdateHealthText(currentHealth, maxHealth, healthPercentage);
            
            // Hiệu ứng flash khi nhận sát thương
            if (enableDamageFlash && healthPercentage < 1f)
            {
                TriggerDamageFlash();
            }
        }
        
        /// <summary>
        /// Cập nhật text hiển thị máu
        /// </summary>
        private void UpdateHealthText(float currentHealth, float maxHealth, float percentage)
        {
            if (nameLabel == null || !showHealthText) return;
            
            string healthText;
            
            if (showPercentage)
            {
                healthText = $"{Mathf.RoundToInt(percentage * 100)}%";
            }
            else
            {
                healthText = $"{Mathf.Ceil(currentHealth)}/{Mathf.Ceil(maxHealth)}";
            }
            
            // Có thể hiển thị tên + máu hoặc chỉ máu
            // nameLabel.text = $"{characterName}\n{healthText}";
            nameLabel.text = healthText;
        }
        
        /// <summary>
        /// Hiệu ứng flash khi nhận sát thương
        /// </summary>
        private void TriggerDamageFlash()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(DamageFlashCoroutine());
        }
        
        private IEnumerator DamageFlashCoroutine()
        {
            if (healthBarFill == null) yield break;
            
            Color originalColor = healthBarFill.color;
            
            // Flash to damage color
            healthBarFill.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);
            
            // Flash back to original
            healthBarFill.color = originalColor;
        }
        
        /// <summary>
        /// Xử lý khi nhân vật chết
        /// </summary>
        private void OnCharacterDeath()
        {
            Debug.Log($"{gameObject.name} UI: Nhân vật đã chết!");
            
            // Có thể thêm hiệu ứng UI khi chết:
            // - Làm mờ UI
            // - Hiển thị "DEFEATED"
            // - Disable interaction
            
            StartCoroutine(DeathAnimation());
        }
        
        private IEnumerator DeathAnimation()
        {
            // Hiệu ứng UI khi chết
            float fadeTime = 1f;
            float elapsedTime = 0f;
            
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0.3f, elapsedTime / fadeTime);
                canvasGroup.alpha = alpha;
                yield return null;
            }
        }
        
        /// <summary>
        /// Đặt tên nhân vật
        /// </summary>
        public void SetCharacterName(string characterName)
        {
            if (nameLabel != null)
            {
                nameLabel.text = characterName;
            }
        }
        
        /// <summary>
        /// Đặt sprite nhân vật
        /// </summary>
        public void SetCharacterSprite(Sprite sprite)
        {
            if (characterSprite != null)
            {
                characterSprite.sprite = sprite;
            }
        }
        
        /// <summary>
        /// Hiển thị/ẩn UI
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
        
        /// <summary>
        /// Kết nối với HealthSystem mới
        /// </summary>
        public void ConnectToHealthSystem(HealthSystem newHealthSystem)
        {
            // Ngắt kết nối cũ
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged -= UpdateHealthBar;
                healthSystem.OnDeath -= OnCharacterDeath;
            }
            
            // Kết nối mới
            healthSystem = newHealthSystem;
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += UpdateHealthBar;
                healthSystem.OnDeath += OnCharacterDeath;
                
                // Cập nhật UI ngay lập tức
                UpdateHealthBar(healthSystem.CurrentHealth, healthSystem.MaxHealth);
            }
        }
        
        // Methods để setup UI elements từ code
        public void SetupUIReferences(Image healthFill, Image healthBg, TextMeshProUGUI nameText, Image charSprite)
        {
            healthBarFill = healthFill;
            healthBarBackground = healthBg;
            nameLabel = nameText;
            characterSprite = charSprite;
            
            // Khởi tạo màu ban đầu cho thanh máu
            if (healthBarFill != null)
            {
                // Nếu thanh máu đang có màu, lưu lại
                if (healthBarFill.color != Color.black && healthBarFill.color.a > 0)
                {
                    originalBarColor = healthBarFill.color;
                }
                else
                {
                    // Nếu không có màu, set màu mặc định là đỏ
                    healthBarFill.color = Color.red;
                    originalBarColor = Color.red;
                }
            }
        }
    }
}