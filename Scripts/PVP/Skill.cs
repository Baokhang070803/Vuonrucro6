using UnityEngine;
using UnityEngine.Video;

namespace PVP
{
    /// <summary>
    /// Data class cho một skill
    /// </summary>
    [System.Serializable]
    public class Skill
    {
        [Header("Skill Info")]
        public string skillName = "Skill";
        public string description = "Mô tả skill";
        public Sprite skillIcon;
        
        [Header("Skill Animation")]
        public VideoClip skillVideo; // Video animation của skill
        public bool hasVideo = false; // Có video animation không
        
        [Header("Skill Frame Animation")]
        public Sprite[] skillFrames; // Danh sách frame ảnh cho animation
        public bool hasFrameAnimation = false; // Có animation bằng frame không
        public float frameDuration = 0.1f; // Thời gian mỗi frame
        
        [Header("Skill Stats")]
        public float damage = 20f;
        public float healAmount = 0f;
        public float cooldown = 0f; // Số lượt chờ
        public int manaCost = 0; // Có thể thêm mana sau
        
        [Header("Skill Type")]
        public SkillType skillType = SkillType.Damage;
        public SkillTarget targetType = SkillTarget.Enemy;
        
        // Runtime data
        [System.NonSerialized]
        public float currentCooldown = 0f;
        
        public bool IsReady => currentCooldown <= 0;
        
        public void Use()
        {
            currentCooldown = cooldown;
        }
        
        public void ReduceCooldown(float amount = 1f)
        {
            currentCooldown = Mathf.Max(0, currentCooldown - amount);
        }
    }
    
    public enum SkillType
    {
        Damage,    // Gây sát thương
        Heal,      // Hồi máu
        Buff,      // Tăng buff
        Debuff     // Giảm debuff
    }
    
    public enum SkillTarget
    {
        Enemy,     // Mục tiêu là địch
        Self,      // Tự bản thân
        Ally       // Đồng minh
    }
}