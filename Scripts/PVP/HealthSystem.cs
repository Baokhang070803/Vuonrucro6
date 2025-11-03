using UnityEngine;
using System;

namespace PVP
{
    [System.Serializable]
    public class HealthSystem : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        
        [Header("Regeneration")]
        [SerializeField] private bool enableHealthRegeneration = false;
        [SerializeField] private float regenerationRate = 1f; // Máu hồi phục mỗi giây
        [SerializeField] private float regenerationDelay = 3f; // Thời gian chờ sau khi bị sát thương
        
        private float lastDamageTime;
        
        // Events
        public event Action<float, float> OnHealthChanged; // current, max
        public event Action OnDeath;
        public event Action OnHealthFull;
        
        // Properties
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercentage => currentHealth / maxHealth;
        public bool IsDead => currentHealth <= 0;
        public bool IsFullHealth => currentHealth >= maxHealth;
        
        private void Awake()
        {
            // Khởi tạo máu ban đầu
            currentHealth = maxHealth;
        }
        
        private void Update()
        {
            // Tự động hồi máu nếu bật tính năng này
            if (enableHealthRegeneration && !IsDead && !IsFullHealth)
            {
                if (Time.time - lastDamageTime >= regenerationDelay)
                {
                    RegenerateHealth(regenerationRate * Time.deltaTime);
                }
            }
        }
        
        /// <summary>
        /// Gây sát thương cho nhân vật
        /// </summary>
        /// <param name="damage">Lượng sát thương</param>
        /// <param name="ignoreArmor">Có bỏ qua giáp không</param>
        public void TakeDamage(float damage, bool ignoreArmor = false)
        {
            if (IsDead) return;
            
            // Có thể thêm tính toán giáp ở đây
            float finalDamage = ignoreArmor ? damage : CalculateDamageAfterArmor(damage);
            
            currentHealth = Mathf.Max(0, currentHealth - finalDamage);
            lastDamageTime = Time.time;
            
            // ✅ Hiển thị số damage bay lên
            var damagePopup = GetComponent<DamageTextPopup>();
            if (damagePopup != null)
            {
                damagePopup.ShowDamage(finalDamage, false);
            }
            
            // Trigger events
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (IsDead)
            {
                OnDeath?.Invoke();
                HandleDeath();
            }
            
            Debug.Log($"{gameObject.name} nhận {finalDamage} sát thương. Máu còn: {currentHealth}/{maxHealth}");
        }
        
        /// <summary>
        /// Hồi phục máu
        /// </summary>
        /// <param name="healAmount">Lượng máu hồi phục</param>
        public void Heal(float healAmount)
        {
            if (IsDead) return;
            
            float oldHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
            float actualHeal = currentHealth - oldHealth;
            
            // ✅ Hiển thị số heal bay lên
            if (actualHeal > 0)
            {
                var damagePopup = GetComponent<DamageTextPopup>();
                if (damagePopup != null)
                {
                    damagePopup.ShowHeal(actualHeal);
                }
            }
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (IsFullHealth && oldHealth < maxHealth)
            {
                OnHealthFull?.Invoke();
            }
            
            Debug.Log($"{gameObject.name} hồi phục {healAmount} máu. Máu hiện tại: {currentHealth}/{maxHealth}");
        }
        
        /// <summary>
        /// Hồi phục máu tự động
        /// </summary>
        private void RegenerateHealth(float regenAmount)
        {
            Heal(regenAmount);
        }
        
        /// <summary>
        /// Đặt lại máu về mức tối đa
        /// </summary>
        public void ResetToFullHealth()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnHealthFull?.Invoke();
        }
        
        /// <summary>
        /// Đặt máu tối đa mới
        /// </summary>
        /// <param name="newMaxHealth">Máu tối đa mới</param>
        /// <param name="healToFull">Có hồi phục về đầy không</param>
        public void SetMaxHealth(float newMaxHealth, bool healToFull = false)
        {
            maxHealth = newMaxHealth;
            
            if (healToFull)
            {
                currentHealth = maxHealth;
            }
            else
            {
                // Giữ phần trăm máu hiện tại
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        /// <summary>
        /// Đặt máu hiện tại (dùng cho initialization hoặc reset)
        /// </summary>
        /// <param name="newCurrentHealth">Máu hiện tại mới</param>
        public void SetCurrentHealth(float newCurrentHealth)
        {
            currentHealth = Mathf.Clamp(newCurrentHealth, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (IsDead)
            {
                OnDeath?.Invoke();
            }
            else if (IsFullHealth)
            {
                OnHealthFull?.Invoke();
            }
        }
        
        /// <summary>
        /// Tính toán sát thương sau khi trừ giáp (có thể mở rộng)
        /// </summary>
        private float CalculateDamageAfterArmor(float damage)
        {
            // Có thể thêm logic tính toán giáp ở đây
            // Ví dụ: return damage * (1 - armorReduction);
            return damage;
        }
        
        /// <summary>
        /// Xử lý khi nhân vật chết
        /// </summary>
        private void HandleDeath()
        {
            Debug.Log($"{gameObject.name} đã chết!");
            
            // Có thể thêm logic xử lý chết ở đây:
            // - Disable character movement
            // - Play death animation
            // - Show death UI
            // - etc.
        }
        
        /// <summary>
        /// Kiểm tra có thể hồi phục máu không
        /// </summary>
        public bool CanHeal()
        {
            return !IsDead && !IsFullHealth;
        }
        
        /// <summary>
        /// Lấy thông tin máu dưới dạng string
        /// </summary>
        public string GetHealthString()
        {
            return $"{Mathf.Ceil(currentHealth)}/{Mathf.Ceil(maxHealth)}";
        }
        
        // Debug methods
        [ContextMenu("Test Take Damage")]
        private void TestTakeDamage()
        {
            TakeDamage(20f);
        }
        
        [ContextMenu("Test Heal")]
        private void TestHeal()
        {
            Heal(15f);
        }
        
        [ContextMenu("Test Reset Health")]
        private void TestResetHealth()
        {
            ResetToFullHealth();
        }
    }
}