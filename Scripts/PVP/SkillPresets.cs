using UnityEngine;
using System.Collections.Generic;

namespace PVP
{
    /// <summary>
    /// Preset skills cho các nhân vật trong 3v3
    /// Team A: Tí Nị, Mụ Xám, Hội Trưởng
    /// Team B: Mụ Thảo, Mộc Tinh, Huyết Thú
    /// </summary>
    public static class SkillPresets
    {
        // ============================================
        // TÍ NỊ - TEAM A
        // ============================================
        
        /// <summary>
        /// Chiêu 1: Linh Thố - Tung chú thỏ bông tấn công
        /// </summary>
        public static Skill TiniSkill1_LinhTho()
        {
            return new Skill
            {
                skillName = "🐇 Linh Thố",
                description = "Tung chú thỏ bông tấn công địch",
                damage = 25f,
                healAmount = 0f,
                cooldown = 0f,
                hasVideo = false,
                hasFrameAnimation = true, // ✅ Có animation frames
                frameDuration = 0.15f, // Thời gian mỗi frame
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        /// <summary>
        /// Chiêu 2: Thánh Lực - Năng Lượng Sự Sống
        /// </summary>
        public static Skill TiniSkill2_ThanhLuc()
        {
            return new Skill
            {
                skillName = "💫 Thánh Lực",
                description = "Năng lượng Cây Pha Lê hồi phục HP",
                damage = 0f,
                healAmount = 35f,
                cooldown = 3f,
                hasVideo = false,
                skillType = SkillType.Heal,
                targetType = SkillTarget.Self
            };
        }
        
        /// <summary>
        /// Chiêu 3: Khúc Ca Sắc Màu - Ultimate skill
        /// </summary>
        public static Skill TiniSkill3_KhucCaSacMau()
        {
            return new Skill
            {
                skillName = "🎶 Khúc Ca Sắc Màu",
                description = "Ultimate! Âm vang từ Cây Pha Lê",
                damage = 50f,
                healAmount = 0f,
                cooldown = 4f,
                hasVideo = true, // ✅ Có video
                hasFrameAnimation = true, // ✅ Có animation frames sau video
                frameDuration = 0.1f, // Frame nhanh hơn cho ultimate
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static List<Skill> GetTiniSkills()
        {
            return new List<Skill>
            {
                TiniSkill1_LinhTho(),
                TiniSkill2_ThanhLuc(),
                TiniSkill3_KhucCaSacMau()
            };
        }
        
        // ============================================
        // MỤ XÁM - TEAM A
        // ============================================
        
        public static Skill MuxamSkill1_AnTamCoNgu()
        {
            return new Skill
            {
                skillName = "📿 Ấn Tâm Cổ Ngữ",
                description = "Tấn công đơn với ma pháp cổ xưa",
                damage = 35f,
                healAmount = 0f,
                cooldown = 0f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static Skill MuxamSkill2_GioiTuHoLinh()
        {
            return new Skill
            {
                skillName = "🛡️ Giới Tụ Hộ Linh",
                description = "Phòng thủ/Thanh tẩy buff bản thân",
                damage = 0f,
                healAmount = 0f,
                cooldown = 3f,
                hasVideo = false,
                skillType = SkillType.Buff,
                targetType = SkillTarget.Self
            };
        }
        
        public static Skill MuxamSkill3_TamGioiPhanHon()
        {
            return new Skill
            {
                skillName = "👻 Tâm Giới Phản Hồn",
                description = "ULTI - Phản sát thương",
                damage = 50f,
                healAmount = 0f,
                cooldown = 5f,
                hasVideo = false,
                skillType = SkillType.Buff,
                targetType = SkillTarget.Self
            };
        }
        
        public static List<Skill> GetMuxamSkills()
        {
            return new List<Skill>
            {
                MuxamSkill1_AnTamCoNgu(),
                MuxamSkill2_GioiTuHoLinh(),
                MuxamSkill3_TamGioiPhanHon()
            };
        }
        
        // ============================================
        // HỘI TRƯỞNG - TEAM A
        // ============================================
        
        public static Skill HoitruongSkill1_BangVanKich()
        {
            return new Skill
            {
                skillName = "❄️ Băng Vân Kích",
                description = "Tấn công thương đơn băng giá",
                damage = 32f,
                healAmount = 0f,
                cooldown = 0f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static Skill HoitruongSkill2_MuaGiaoBang()
        {
            return new Skill
            {
                skillName = "🌨️ Mưa Giáo Băng",
                description = "Tấn công diện rộng mưa băng",
                damage = 45f,
                healAmount = 0f,
                cooldown = 2f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static Skill HoitruongSkill3_ThuongQuyet()
        {
            return new Skill
            {
                skillName = "🔱 Thương Quyết",
                description = "ULTI - Chuyển dạng thần thương",
                damage = 0f,
                healAmount = 0f,
                cooldown = 5f,
                hasVideo = false,
                skillType = SkillType.Buff,
                targetType = SkillTarget.Self
            };
        }
        
        public static List<Skill> GetHoitruongSkills()
        {
            return new List<Skill>
            {
                HoitruongSkill1_BangVanKich(),
                HoitruongSkill2_MuaGiaoBang(),
                HoitruongSkill3_ThuongQuyet()
            };
        }
        
        // ============================================
        // MỤ THẢO - TEAM B
        // ============================================
        
        public static Skill MuthaoSkill1_LocTuVong()
        {
            return new Skill
            {
                skillName = "🪄 Lốc Tử Vong",
                description = "Lốc xoáy ma pháp tím",
                damage = 30f,
                healAmount = 0f,
                cooldown = 0f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static Skill MuthaoSkill2_HoaLienTranPhap()
        {
            return new Skill
            {
                skillName = "🔥 Hỏa Liên Trận Pháp",
                description = "Vòng hoa sen tím lửa phát nổ",
                damage = 45f,
                healAmount = 0f,
                cooldown = 2f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static Skill MuthaoSkill3_VuonHoaDietThe()
        {
            return new Skill
            {
                skillName = "🌑 Vườn Hoa Diệt Thế",
                description = "Tuyệt kỹ! Biển lửa ma quái",
                damage = 60f,
                healAmount = 0f,
                cooldown = 5f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static List<Skill> GetMuthaoSkills()
        {
            return new List<Skill>
            {
                MuthaoSkill1_LocTuVong(),
                MuthaoSkill2_HoaLienTranPhap(),
                MuthaoSkill3_VuonHoaDietThe()
            };
        }
        
        // ============================================
        // MỘC TINH - TEAM B
        // ============================================
        
        public static Skill MoctinhSkill1_ThuTamKichPhat()
        {
            return new Skill
            {
                skillName = "🌳 Thụ Tâm Kích Phạt",
                description = "Tấn công đơn mạnh từ linh hồn cây",
                damage = 48f,
                healAmount = 0f,
                cooldown = 0f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static Skill MoctinhSkill2_MucGioiHapLinh()
        {
            return new Skill
            {
                skillName = "🍃 Mục Giới Hấp Linh",
                description = "Đánh địch và hút máu hồi phục",
                damage = 35f,
                healAmount = 15f,
                cooldown = 2f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static Skill MoctinhSkill3_LinhTamHopThe()
        {
            return new Skill
            {
                skillName = "🌲 Linh Tâm Hợp Thể",
                description = "ULTI - Hòa nhập với thiên nhiên",
                damage = 80f,
                healAmount = 0f,
                cooldown = 5f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static List<Skill> GetMoctinhSkills()
        {
            return new List<Skill>
            {
                MoctinhSkill1_ThuTamKichPhat(),
                MoctinhSkill2_MucGioiHapLinh(),
                MoctinhSkill3_LinhTamHopThe()
            };
        }
        
        // ============================================
        // HUYẾT THÚ - TEAM B
        // ============================================
        
        public static Skill HuyetthuSkill1_TraoLietNhiet()
        {
            return new Skill
            {
                skillName = "🔴 Trảo Liệt Nhiệt",
                description = "Tấn công đơn cao với vuốt nóng",
                damage = 50f,
                healAmount = 0f,
                cooldown = 0f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static Skill HuyetthuSkill2_HoaHongLietPhun()
        {
            return new Skill
            {
                skillName = "🌋 Hỏa Hống Liệt Phun",
                description = "Phun lửa diện rộng từ miệng",
                damage = 60f,
                healAmount = 0f,
                cooldown = 2f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static Skill HuyetthuSkill3_GamRenHuyetDiet()
        {
            return new Skill
            {
                skillName = "💀 Gầm Rền Huyết Diệt",
                description = "ULTI - Tiếng gầm hủy diệt",
                damage = 85f,
                healAmount = 0f,
                cooldown = 5f,
                hasVideo = false,
                skillType = SkillType.Damage,
                targetType = SkillTarget.Enemy
            };
        }
        
        public static List<Skill> GetHuyetthuSkills()
        {
            return new List<Skill>
            {
                HuyetthuSkill1_TraoLietNhiet(),
                HuyetthuSkill2_HoaHongLietPhun(),
                HuyetthuSkill3_GamRenHuyetDiet()
            };
        }
        
        // ============================================
        // HELPER METHODS
        // ============================================
        
        /// <summary>
        /// Get skills theo tên nhân vật (auto-detect)
        /// </summary>
        public static List<Skill> GetSkillsByCharacterName(string characterName)
        {
            string lowerName = characterName.ToLower();
            
            // Team A
            if (lowerName.Contains("tini") || lowerName.Contains("tí nị"))
                return GetTiniSkills();
            
            if (lowerName.Contains("muxam") || lowerName.Contains("mụ xám"))
                return GetMuxamSkills();
            
            if (lowerName.Contains("hoitruong") || lowerName.Contains("hội trưởng") || lowerName.Contains("hồi trương"))
                return GetHoitruongSkills();
            
            // Team B
            if (lowerName.Contains("muthao") || lowerName.Contains("mụ thảo"))
                return GetMuthaoSkills();
            
            if (lowerName.Contains("moctinh") || lowerName.Contains("mộc tinh") || lowerName.Contains("mộc trìu"))
                return GetMoctinhSkills();
            
            if (lowerName.Contains("huyetthu") || lowerName.Contains("huyết thú"))
                return GetHuyetthuSkills();
            
            // Default
            Debug.LogWarning($"[SkillPresets] Không tìm thấy skills cho '{characterName}'. Dùng default skills.");
            return GetDefaultSkills();
        }
        
        /// <summary>
        /// Default skills khi không tìm thấy character
        /// </summary>
        private static List<Skill> GetDefaultSkills()
        {
            return new List<Skill>
            {
                new Skill 
                { 
                    skillName = "⚔️ Tấn công", 
                    description = "Tấn công cơ bản",
                    damage = 25f, 
                    cooldown = 0f, 
                    skillType = SkillType.Damage, 
                    targetType = SkillTarget.Enemy 
                },
                new Skill 
                { 
                    skillName = "💥 Skill mạnh", 
                    description = "Tấn công mạnh hơn",
                    damage = 40f, 
                    cooldown = 2f, 
                    skillType = SkillType.Damage, 
                    targetType = SkillTarget.Enemy 
                },
                new Skill 
                { 
                    skillName = "🌟 Ultimate", 
                    description = "Chiêu thức tối thượng",
                    damage = 60f, 
                    cooldown = 4f, 
                    skillType = SkillType.Damage, 
                    targetType = SkillTarget.Enemy 
                }
            };
        }
    }
}
