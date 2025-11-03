using UnityEngine;
using System;

namespace PVP
{
    /// <summary>
    /// Enum cho team
    /// </summary>
    public enum Team
    {
        TeamA,  // Team bên trái
        TeamB   // Team bên phải
    }

    /// <summary>
    /// Enum cho vị trí trong team
    /// </summary>
    public enum Position
    {
        Top = 0,      // Vị trí trên
        Middle = 1,   // Vị trí giữa
        Bottom = 2    // Vị trí dưới
    }

    /// <summary>
    /// Data cho mỗi nhân vật trong 3v3
    /// </summary>
    [System.Serializable]
    public class CharacterData
    {
        [Header("Character Info")]
        public string characterName = "Character";
        public int characterIndex; // 0, 1, 2 trong team
        public Team team;
        public Position position;
        
        [Header("Components")]
        public HealthSystem healthSystem;
        public HealthBarUI healthBarUI;
        public CharacterSkills characterSkills;
        public DamageEffectController damageEffectController; // Hiệu ứng bị đánh
        
        [Header("Visual")]
        public GameObject characterObject; // GameObject của nhân vật
        public Transform targetPoint; // Điểm để đặt mũi tên target
        
        [Header("Stats")]
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        public float speed = 50f; // Dùng cho turn order (nếu cần)
        
        // Status
        public bool isAlive => currentHealth > 0;
        public bool isDead => currentHealth <= 0;
        
        // Events
        public event Action<CharacterData> OnCharacterDeath;

        /// <summary>
        /// Khởi tạo character
        /// </summary>
        public void Initialize()
        {
            currentHealth = maxHealth;
            
            // Setup HealthSystem
            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(maxHealth);
                healthSystem.SetCurrentHealth(currentHealth);
                healthSystem.OnDeath += OnDeath;
            }
            
            // Setup HealthBarUI
            if (healthBarUI != null && healthSystem != null)
            {
                healthBarUI.ConnectToHealthSystem(healthSystem);
            }
            
            Debug.Log($"[CharacterData] Đã khởi tạo {characterName} - Team {team} - Position {position}");
        }

        /// <summary>
        /// Nhận damage
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!isAlive) return;
            
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(damage);
            }
            
            // ✨ Trigger hiệu ứng bị đánh (flash đỏ + rung)
            if (damageEffectController != null)
            {
                damageEffectController.TriggerDamageEffect();
            }
            
            Debug.Log($"{characterName} nhận {damage} damage. HP: {currentHealth}/{maxHealth}");
            
            if (isDead)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Nhận damage từ một attacker (có hiệu ứng theo hướng)
        /// </summary>
        public void TakeDamageFrom(float damage, CharacterData attacker)
        {
            if (!isAlive) return;
            
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(damage);
            }
            
            // ✨ Trigger hiệu ứng bị đánh theo hướng
            if (damageEffectController != null && attacker?.characterObject != null)
            {
                damageEffectController.TriggerDamageEffectFromDirection(attacker.characterObject.transform.position);
            }
            else if (damageEffectController != null)
            {
                damageEffectController.TriggerDamageEffect();
            }
            
            Debug.Log($"{characterName} nhận {damage} damage từ {attacker?.characterName}. HP: {currentHealth}/{maxHealth}");
            
            if (isDead)
            {
                Die();
            }
        }

        /// <summary>
        /// Hồi máu
        /// </summary>
        public void Heal(float amount)
        {
            if (!isAlive) return;
            
            currentHealth += amount;
            currentHealth = Mathf.Min(maxHealth, currentHealth);
            
            if (healthSystem != null)
            {
                healthSystem.Heal(amount);
            }
            
            Debug.Log($"{characterName} hồi {amount} HP. HP: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Xử lý khi character chết
        /// </summary>
        private void Die()
        {
            Debug.Log($"💀 {characterName} đã chết!");
            
            // Visual effect - grey out character
            if (characterObject != null)
            {
                var spriteRenderer = characterObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Grey
                }
                
                // Hoặc disable character
                // characterObject.SetActive(false);
            }
            
            // Ẩn skills
            if (characterSkills != null)
            {
                characterSkills.HideSkills();
            }
            
            // Trigger event
            OnCharacterDeath?.Invoke(this);
        }

        /// <summary>
        /// Callback khi HealthSystem báo death
        /// </summary>
        private void OnDeath()
        {
            if (isAlive) // Chưa xử lý death
            {
                currentHealth = 0;
                Die();
            }
        }

        /// <summary>
        /// Reset character về trạng thái ban đầu
        /// </summary>
        public void Reset()
        {
            currentHealth = maxHealth;
            
            if (healthSystem != null)
            {
                healthSystem.SetCurrentHealth(maxHealth);
            }
            
            // Reset visual
            if (characterObject != null)
            {
                var spriteRenderer = characterObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                }
                characterObject.SetActive(true);
            }
            
            // Reset skills
            if (characterSkills != null)
            {
                characterSkills.ResetAllCooldowns();
            }
            
            Debug.Log($"{characterName} đã được reset!");
        }

        /// <summary>
        /// Get unique ID (dùng cho debug)
        /// </summary>
        public string GetID()
        {
            return $"{team}_{position}_{characterName}";
        }
    }
}
