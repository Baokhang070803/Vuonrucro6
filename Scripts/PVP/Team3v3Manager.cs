using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PVP
{
    /// <summary>
    /// Quản lý trận đấu 3v3
    /// </summary>
    public class Team3v3Manager : MonoBehaviour
    {
        [Header("Teams Setup")]
        [Tooltip("3 nhân vật của Team A (bên trái)")]
        public CharacterData[] teamA = new CharacterData[3];
        
        [Tooltip("3 nhân vật của Team B (bên phải)")]
        public CharacterData[] teamB = new CharacterData[3];

        [Header("Big Health Bars - Thanh máu lớn")]
        [Tooltip("Thanh máu lớn của nhân vật (VD: Tí Nị, Mụ Xám)")]
        public HealthBarUI bigHealthBarCharacter1; // Thanh máu lớn thứ 1 (VD: Tí Nị)
        
        [Tooltip("Thanh máu lớn của nhân vật thứ 2")]
        public HealthBarUI bigHealthBarCharacter2; // Thanh máu lớn thứ 2 (VD: Mụ Xám)

        [Header("Auto Setup")]
        [Tooltip("Tự động setup characters từ scene")]
        public bool autoSetupCharacters = true;

        // Lists để dễ quản lý
        private List<CharacterData> allCharacters = new List<CharacterData>();
        private List<CharacterData> aliveTeamA = new List<CharacterData>();
        private List<CharacterData> aliveTeamB = new List<CharacterData>();

        private void Start()
        {
            if (autoSetupCharacters)
            {
                AutoSetupCharacters();
            }
            
            InitializeAllCharacters();
        }

        /// <summary>
        /// Tự động tìm và setup characters từ scene
        /// </summary>
        private void AutoSetupCharacters()
        {
            Debug.Log("[Team3v3Manager] Đang tự động setup characters...");
            
            // Tìm tất cả components trong scene
            CharacterSkills[] allSkills = FindObjectsOfType<CharacterSkills>();
            HealthSystem[] allHealthSystems = FindObjectsOfType<HealthSystem>();
            HealthBarUI[] allHealthBars = FindObjectsOfType<HealthBarUI>();
            
            Debug.Log($"Tìm thấy: {allSkills.Length} CharacterSkills, {allHealthSystems.Length} HealthSystems");
            
            // ⚠️ QUAN TRỌNG: Setup CharacterData từ các components
            // Kiểm tra xem có GameObject nào chứa CharacterSkills
            foreach (var skill in allSkills)
            {
                Debug.Log($"  - GameObject: {skill.gameObject.name} có CharacterSkills");
            }
            
            // Tự động tạo CharacterData nếu teamA/teamB chưa setup
            // HOẶC nếu đã có nhưng thiếu characterSkills reference
            bool needsSetup = false;
            
            // Check nếu null hoặc thiếu characterSkills
            for (int i = 0; i < 3; i++)
            {
                if (teamA[i] == null || teamA[i].characterSkills == null)
                {
                    needsSetup = true;
                    break;
                }
                if (teamB[i] == null || teamB[i].characterSkills == null)
                {
                    needsSetup = true;
                    break;
                }
            }
            
            if (needsSetup)
            {
                Debug.LogWarning("⚠️ teamA[] hoặc teamB[] cần setup lại! Đang tự động tạo CharacterData...");
                AutoCreateCharacterData(allSkills, allHealthSystems, allHealthBars);
            }
            else
            {
                Debug.Log("✅ teamA[] và teamB[] đã có đầy đủ CharacterData với characterSkills!");
            }
        }
        
        /// <summary>
        /// Tự động tạo CharacterData từ các components trong scene
        /// </summary>
        private void AutoCreateCharacterData(CharacterSkills[] allSkills, HealthSystem[] allHealthSystems, HealthBarUI[] allHealthBars)
        {
            // Mapping tên GameObject -> Team và Position
            Dictionary<string, (Team team, Position pos)> nameMapping = new Dictionary<string, (Team, Position)>
            {
                // Team A
                { "tini", (Team.TeamA, Position.Top) },
                { "muxam", (Team.TeamA, Position.Middle) },
                { "hoitruong", (Team.TeamA, Position.Bottom) },
                
                // Team B
                { "muthao", (Team.TeamB, Position.Top) },
                { "moctinh", (Team.TeamB, Position.Middle) },
                { "huyetthu", (Team.TeamB, Position.Bottom) }
            };
            
            foreach (var skill in allSkills)
            {
                string objName = skill.gameObject.name.ToLower();
                
                // Tìm mapping
                foreach (var kvp in nameMapping)
                {
                    if (objName.Contains(kvp.Key))
                    {
                        Team team = kvp.Value.team;
                        Position pos = kvp.Value.pos;
                        int index = (int)pos;
                        
                        // Tạo CharacterData mới (không phải MonoBehaviour, nên dùng new)
                        CharacterData charData = new CharacterData();
                        
                        // Setup CharacterData
                        charData.characterName = skill.gameObject.name;
                        charData.team = team;
                        charData.position = pos;
                        charData.characterIndex = index;
                        charData.characterSkills = skill;
                        charData.characterObject = skill.gameObject;
                        
                        // Tìm HealthSystem và HealthBarUI trên cùng GameObject
                        charData.healthSystem = skill.gameObject.GetComponent<HealthSystem>();
                        charData.healthBarUI = skill.gameObject.GetComponent<HealthBarUI>();
                        
                        // Tìm hoặc thêm DamageEffectController
                        charData.damageEffectController = skill.gameObject.GetComponent<DamageEffectController>();
                        if (charData.damageEffectController == null)
                        {
                            charData.damageEffectController = skill.gameObject.AddComponent<DamageEffectController>();
                            Debug.Log($"  ➕ Đã thêm DamageEffectController cho {charData.characterName}");
                        }
                        
                        // Gán vào array
                        if (team == Team.TeamA)
                        {
                            teamA[index] = charData;
                            Debug.Log($"✅ Auto-setup Team A[{index}]: {charData.characterName}");
                        }
                        else
                        {
                            teamB[index] = charData;
                            Debug.Log($"✅ Auto-setup Team B[{index}]: {charData.characterName}");
                        }
                        
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Khởi tạo tất cả characters
        /// </summary>
        private void InitializeAllCharacters()
        {
            allCharacters.Clear();
            
            // Initialize Team A
            for (int i = 0; i < teamA.Length; i++)
            {
                if (teamA[i] != null)
                {
                    teamA[i].team = Team.TeamA;
                    teamA[i].characterIndex = i;
                    teamA[i].position = (Position)i;
                    teamA[i].Initialize();
                    teamA[i].OnCharacterDeath += OnCharacterDied;
                    allCharacters.Add(teamA[i]);
                }
            }
            
            // Initialize Team B
            for (int i = 0; i < teamB.Length; i++)
            {
                if (teamB[i] != null)
                {
                    teamB[i].team = Team.TeamB;
                    teamB[i].characterIndex = i;
                    teamB[i].position = (Position)i;
                    teamB[i].Initialize();
                    teamB[i].OnCharacterDeath += OnCharacterDied;
                    allCharacters.Add(teamB[i]);
                }
            }
            
            UpdateAliveLists();
            
            Debug.Log($"[Team3v3Manager] Đã khởi tạo {allCharacters.Count} characters");
            Debug.Log($"Team A: {aliveTeamA.Count} alive | Team B: {aliveTeamB.Count} alive");
        }

        /// <summary>
        /// Cập nhật danh sách characters còn sống
        /// </summary>
        private void UpdateAliveLists()
        {
            aliveTeamA = teamA.Where(c => c != null && c.isAlive).ToList();
            aliveTeamB = teamB.Where(c => c != null && c.isAlive).ToList();
        }

        /// <summary>
        /// Xử lý khi character chết
        /// </summary>
        private void OnCharacterDied(CharacterData character)
        {
            Debug.Log($"[Team3v3Manager] {character.characterName} (Team {character.team}) đã chết!");
            
            UpdateAliveLists();
            
            Debug.Log($"Team A còn: {aliveTeamA.Count} | Team B còn: {aliveTeamB.Count}");
            
            // Check win condition
            if (CheckWinCondition())
            {
                Team winner = GetWinningTeam();
                OnMatchEnd(winner);
            }
        }

        /// <summary>
        /// Kiểm tra điều kiện thắng
        /// </summary>
        public bool CheckWinCondition()
        {
            return aliveTeamA.Count == 0 || aliveTeamB.Count == 0;
        }

        /// <summary>
        /// Xác định team thắng
        /// </summary>
        public Team GetWinningTeam()
        {
            if (aliveTeamB.Count == 0)
                return Team.TeamA;
            else
                return Team.TeamB;
        }

        /// <summary>
        /// Kết thúc trận đấu
        /// </summary>
        private void OnMatchEnd(Team winner)
        {
            Debug.Log($"🏆 ===== TEAM {winner} THẮNG! =====");
            
            // TODO: Show victory screen
            // TODO: Play victory animation
            // TODO: Award rewards
        }

        /// <summary>
        /// Get enemy team của một character
        /// </summary>
        public List<CharacterData> GetEnemyTeam(CharacterData character)
        {
            if (character.team == Team.TeamA)
                return aliveTeamB;
            else
                return aliveTeamA;
        }

        /// <summary>
        /// Get ally team của một character
        /// </summary>
        public List<CharacterData> GetAllyTeam(CharacterData character)
        {
            if (character.team == Team.TeamA)
                return aliveTeamA;
            else
                return aliveTeamB;
        }

        /// <summary>
        /// Get character theo team và position
        /// </summary>
        public CharacterData GetCharacter(Team team, Position position)
        {
            CharacterData[] targetTeam = (team == Team.TeamA) ? teamA : teamB;
            int index = (int)position;
            
            if (index >= 0 && index < targetTeam.Length)
                return targetTeam[index];
            
            return null;
        }

        /// <summary>
        /// Apply damage từ attacker đến target
        /// </summary>
        public void ApplyDamage(CharacterData attacker, CharacterData target, float damage)
        {
            if (target == null || target.isDead)
            {
                Debug.LogWarning($"Target không hợp lệ hoặc đã chết!");
                return;
            }
            
            Debug.Log($"⚔️ {attacker.characterName} tấn công {target.characterName} gây {damage} damage!");
            
            // Dùng TakeDamageFrom để có hiệu ứng theo hướng
            target.TakeDamageFrom(damage, attacker);
        }

        /// <summary>
        /// Apply heal cho target
        /// </summary>
        public void ApplyHeal(CharacterData healer, CharacterData target, float healAmount)
        {
            if (target == null || target.isDead)
            {
                Debug.LogWarning($"Target không hợp lệ hoặc đã chết!");
                return;
            }
            
            Debug.Log($"💚 {healer.characterName} hồi {healAmount} HP cho {target.characterName}");
            target.Heal(healAmount);
        }

        /// <summary>
        /// Reset toàn bộ trận đấu
        /// </summary>
        public void ResetMatch()
        {
            Debug.Log("[Team3v3Manager] Reset trận đấu...");
            
            foreach (var character in allCharacters)
            {
                if (character != null)
                {
                    character.Reset();
                }
            }
            
            UpdateAliveLists();
            
            Debug.Log($"✅ Đã reset! Team A: {aliveTeamA.Count} | Team B: {aliveTeamB.Count}");
        }

        /// <summary>
        /// Get tất cả characters (debug)
        /// </summary>
        public List<CharacterData> GetAllCharacters()
        {
            return allCharacters;
        }

        /// <summary>
        /// Get số lượng characters còn sống của team
        /// </summary>
        public int GetAliveCount(Team team)
        {
            return (team == Team.TeamA) ? aliveTeamA.Count : aliveTeamB.Count;
        }

        /// <summary>
        /// Kiểm tra có phải trận đấu đã kết thúc
        /// </summary>
        public bool IsMatchOver()
        {
            return CheckWinCondition();
        }
    }
}
