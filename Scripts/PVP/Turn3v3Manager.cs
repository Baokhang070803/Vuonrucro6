using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace PVP
{
    /// <summary>
    /// Quản lý lượt chơi trong chế độ 3v3
    /// </summary>
    public class Turn3v3Manager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Team3v3Manager teamManager;
        [SerializeField] private TargetSelector3v3 targetSelector;
        [SerializeField] private VideoSkillPlayer videoSkillPlayer;
        [SerializeField] private GameSpeedToggle gameSpeedToggle; // Đổi từ GameSpeedController
        [SerializeField] private BackgroundVideoController backgroundVideoController; // Background video
        
        [Header("Turn Display")]
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private GameObject turnIndicator;
        [SerializeField] private Image turnAvatar; // Avatar của nhân vật đang đến lượt
        
        [Header("Next Turn Display - Thanh hiển thị lượt")]
        [SerializeField] private Image nextTurnAvatar1; // Avatar lượt tiếp theo thứ 1
        [SerializeField] private Image nextTurnAvatar2; // Avatar lượt tiếp theo thứ 2
        
        [Header("Character Avatars - Kéo 6 avatar vào đây")]
        [SerializeField] private Sprite avatarTini;     // Avatar Tí Nị
        [SerializeField] private Sprite avatarMuxam;   // Avatar Mụ Xám  
        [SerializeField] private Sprite avatarHoitruong; // Avatar Hội Trưởng
        [SerializeField] private Sprite avatarMuthao;  // Avatar Mụ Thảo
        [SerializeField] private Sprite avatarMoctinh; // Avatar Mộc Tinh
        [SerializeField] private Sprite avatarHuyetthu; // Avatar Huyết Thú
        
        [Header("Settings")]
        [SerializeField] private float turnDelay = 1f;
        [Tooltip("Chế độ turn order: RoundRobin hoặc SpeedBased")]
        public TurnOrderMode turnOrderMode = TurnOrderMode.RoundRobin;

        // Turn state
        private int turnCount = 1;
        private bool isProcessingTurn = false;
        private Queue<CharacterData> turnQueue = new Queue<CharacterData>();
        private CharacterData currentCharacter;
        
        // Pending skill
        private Skill pendingSkill;
        private CharacterData pendingTarget;

        public enum TurnOrderMode
        {
            RoundRobin,  // Luân phiên: A1 → B1 → A2 → B2 → A3 → B3 → A1...
            SpeedBased   // Dựa trên speed stat
        }

        private void Start()
        {
            if (teamManager == null)
                teamManager = GetComponent<Team3v3Manager>();
            
            if (targetSelector == null)
                targetSelector = GetComponent<TargetSelector3v3>();
            
            if (videoSkillPlayer == null)
                videoSkillPlayer = GetComponent<VideoSkillPlayer>();
            
            if (gameSpeedToggle == null)
                gameSpeedToggle = GetComponent<GameSpeedToggle>();
            
            if (backgroundVideoController == null)
                backgroundVideoController = GetComponent<BackgroundVideoController>();
            
            // Kết nối GameSpeedToggle với VideoSkillPlayer
            if (gameSpeedToggle != null && videoSkillPlayer != null)
            {
                // VideoSkillPlayer sẽ tự động được tìm trong GameSpeedToggle.Start()
                Debug.Log("✅ GameSpeedToggle connected!");
            }
            
            // Start background video
            if (backgroundVideoController != null)
            {
                Debug.Log("🎬 Background video controller found!");
            }
            
            // Đợi 1 frame để Team3v3Manager khởi tạo xong
            StartCoroutine(InitializeWithDelay());
        }
        
        /// <summary>
        /// Initialize sau khi Team3v3Manager setup xong
        /// </summary>
        private IEnumerator InitializeWithDelay()
        {
            // Đợi 1 frame
            yield return null;
            
            // Subscribe to all character skills
            SubscribeToAllCharacters();
            
            // Bắt đầu trận đấu
            StartCoroutine(StartBattle());
        }

        /// <summary>
        /// Subscribe to skill events của tất cả characters
        /// </summary>
        private void SubscribeToAllCharacters()
        {
            var allCharacters = teamManager.GetAllCharacters();
            foreach (var character in allCharacters)
            {
                if (character.characterSkills != null)
                {
                    character.characterSkills.OnSkillUsed += (skill) => OnCharacterSkillUsed(character, skill);
                }
                
                // Subscribe to death event để rebuild turn queue
                character.OnCharacterDeath += OnCharacterDied;
            }
            
            Debug.Log($"[Turn3v3Manager] Đã subscribe to {allCharacters.Count} characters");
        }

        /// <summary>
        /// Bắt đầu trận đấu
        /// </summary>
        private IEnumerator StartBattle()
        {
            yield return new WaitForSeconds(1f);
            
            Debug.Log("🎮 ===== BẮT ĐẦU TRẬN ĐẤU 3v3! =====");
            
            BuildTurnQueue();
            StartNextTurn();
        }

        /// <summary>
        /// Xây dựng turn queue
        /// </summary>
        private void BuildTurnQueue()
        {
            turnQueue.Clear();
            
            var allCharacters = teamManager.GetAllCharacters()
                .Where(c => c.isAlive)
                .ToList();
            
            if (turnOrderMode == TurnOrderMode.RoundRobin)
            {
                // Luân phiên: A0, B0, A1, B1, A2, B2
                for (int i = 0; i < 3; i++)
                {
                    var teamAChar = teamManager.GetCharacter(Team.TeamA, (Position)i);
                    var teamBChar = teamManager.GetCharacter(Team.TeamB, (Position)i);
                    
                    if (teamAChar != null && teamAChar.isAlive)
                        turnQueue.Enqueue(teamAChar);
                    
                    if (teamBChar != null && teamBChar.isAlive)
                        turnQueue.Enqueue(teamBChar);
                }
            }
            else if (turnOrderMode == TurnOrderMode.SpeedBased)
            {
                // Sort theo speed - CHỈ THÊM CHARACTER CÒN SỐNG
                var sortedChars = allCharacters
                    .Where(c => c.isAlive)
                    .OrderByDescending(c => c.speed)
                    .ToList();
                foreach (var character in sortedChars)
                {
                    turnQueue.Enqueue(character);
                }
            }
            
            Debug.Log($"[Turn3v3Manager] Turn queue đã sẵn sàng với {turnQueue.Count} lượt");
        }

        /// <summary>
        /// Bắt đầu lượt tiếp theo
        /// </summary>
        private void StartNextTurn()
        {
            if (isProcessingTurn) return;
            
            // Check win condition
            if (teamManager.IsMatchOver())
            {
                EndMatch();
                return;
            }
            
            // Get character tiếp theo (GetNextAliveCharacter sẽ tự rebuild queue nếu cần)
            currentCharacter = GetNextAliveCharacter();
            
            if (currentCharacter == null)
            {
                Debug.LogError("Không tìm thấy character nào còn sống!");
                return;
            }
            
            // Giảm cooldown
            if (currentCharacter.characterSkills != null)
            {
                currentCharacter.characterSkills.ReduceAllCooldowns();
            }
            
            // Update UI
            UpdateTurnDisplay();
            
            // Hiển thị skills
            if (currentCharacter.characterSkills != null)
            {
                Debug.Log($"[Turn3v3Manager] Đang gọi ShowSkills() cho {currentCharacter.characterName}...");
                currentCharacter.characterSkills.ShowSkills();
            }
            else
            {
                Debug.LogError($"❌ [{currentCharacter.characterName}] CharacterSkills = NULL! Component chưa gán!");
            }
            
            Debug.Log($"🎯 LƯỢT {turnCount}: {currentCharacter.characterName} (Team {currentCharacter.team})");
        }

        /// <summary>
        /// Get character tiếp theo còn sống (skip character đã chết)
        /// </summary>
        private CharacterData GetNextAliveCharacter()
        {
            while (turnQueue.Count > 0)
            {
                var character = turnQueue.Dequeue();
                if (character.isAlive)
                    return character;
                else
                    Debug.Log($"⏭️ Skip {character.characterName} (đã chết)");
            }
            
            // Nếu queue rỗng nhưng trận đấu chưa kết thúc, rebuild queue
            if (!teamManager.IsMatchOver())
            {
                Debug.Log("🔄 Queue rỗng! Rebuild turn queue...");
                turnCount++;
                BuildTurnQueue();
                
                // Try lại sau khi rebuild
                if (turnQueue.Count > 0)
                {
                    var character = turnQueue.Dequeue();
                    if (character.isAlive)
                        return character;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Kết thúc lượt hiện tại
        /// </summary>
        public void EndTurn()
        {
            if (isProcessingTurn) return;
            
            // Ẩn skills
            if (currentCharacter?.characterSkills != null)
            {
                currentCharacter.characterSkills.HideSkills();
            }
            
            // Ẩn target selector
            if (targetSelector != null)
            {
                targetSelector.HideAllTargets();
            }
            
            StartCoroutine(NextTurnDelay());
        }

        private IEnumerator NextTurnDelay()
        {
            isProcessingTurn = true;
            yield return new WaitForSeconds(turnDelay);
            isProcessingTurn = false;
            StartNextTurn();
        }

        /// <summary>
        /// Xử lý khi character sử dụng skill
        /// </summary>
        private void OnCharacterSkillUsed(CharacterData character, Skill skill)
        {
            Debug.Log($"[Turn3v3Manager] {character.characterName} chọn skill: {skill.skillName}");
            
            // Lưu pending skill
            pendingSkill = skill;
            
            // Hiện target selector
            if (targetSelector != null)
            {
                if (skill.targetType == SkillTarget.Enemy)
                {
                    // Hiện 3 địch còn sống
                    var enemies = teamManager.GetEnemyTeam(character);
                    targetSelector.ShowEnemyTargets(enemies);
                    Debug.Log($"Chờ chọn 1 trong {enemies.Count} địch...");
                }
                else if (skill.targetType == SkillTarget.Self)
                {
                    // Tự động chọn bản thân
                    OnTargetSelected(character);
                }
                else if (skill.targetType == SkillTarget.Ally)
                {
                    // Hiện đồng đội
                    var allies = teamManager.GetAllyTeam(character);
                    targetSelector.ShowAllyTargets(allies);
                    Debug.Log($"Chờ chọn 1 trong {allies.Count} đồng đội...");
                }
            }
        }

        /// <summary>
        /// Được gọi khi người chơi chọn target
        /// </summary>
        public void OnTargetSelected(CharacterData target)
        {
            if (pendingSkill == null || currentCharacter == null)
            {
                Debug.LogWarning("Không có skill đang chờ xác nhận!");
                return;
            }
            
            Debug.Log($"✅ Đã chọn target: {target.characterName}");
            
            pendingTarget = target;
            
            // Xác nhận sử dụng skill
            if (currentCharacter.characterSkills != null)
            {
                currentCharacter.characterSkills.ConfirmSkillUsage();
            }
            
            // Ẩn target selector
            if (targetSelector != null)
            {
                targetSelector.HideAllTargets();
            }
            
            // Execute skill
            ExecuteSkill(currentCharacter, pendingTarget, pendingSkill);
        }

        /// <summary>
        /// Thực thi skill
        /// </summary>
        private void ExecuteSkill(CharacterData user, CharacterData target, Skill skill)
        {
            Debug.Log($"⚡ {user.characterName} thực thi {skill.skillName} lên {target.characterName}");
            
            // Kiểm tra có video và animation frames không
            bool hasVideo = skill.hasVideo && skill.skillVideo != null;
            bool hasFrameAnimation = skill.hasFrameAnimation && skill.skillFrames != null && skill.skillFrames.Length > 0;
            
            if (hasVideo && hasFrameAnimation && videoSkillPlayer != null)
            {
                // Phát video rồi animation frames
                StartCoroutine(PlaySkillWithVideoAndAnimation(user, target, skill));
            }
            else if (hasVideo && videoSkillPlayer != null)
            {
                // Chỉ phát video
                StartCoroutine(PlaySkillWithVideo(user, target, skill));
            }
            else if (hasFrameAnimation && videoSkillPlayer != null)
            {
                // Chỉ phát animation frames
                StartCoroutine(PlaySkillWithAnimation(user, target, skill));
            }
            else
            {
                // Apply effect ngay
                ApplySkillEffect(user, target, skill);
                EndTurn();
            }
        }
        
        /// <summary>
        /// Phát video rồi animation frames
        /// </summary>
        private IEnumerator PlaySkillWithVideoAndAnimation(CharacterData user, CharacterData target, Skill skill)
        {
            bool finished = false;
            
            videoSkillPlayer.PlaySkillVideoWithAnimation(skill.skillVideo, skill.skillFrames, user, target, () => 
            {
                finished = true;
            });
            
            // Chờ hoàn thành
            yield return new WaitUntil(() => finished);
            
            // Apply effect
            ApplySkillEffect(user, target, skill);
            
            // End turn
            EndTurn();
        }
        
        /// <summary>
        /// Phát chỉ animation frames - LUÔN ở vị trí target
        /// </summary>
        private IEnumerator PlaySkillWithAnimation(CharacterData user, CharacterData target, Skill skill)
        {
            bool finished = false;
            
            // Tìm SkillAnimationController
            var animationController = FindObjectOfType<SkillAnimationController>();
            if (animationController != null)
            {
                Debug.Log($"🎬 Phát animation frames tại vị trí target: {target.characterName}");
                
                animationController.OnAnimationFinished += () => finished = true;
                animationController.PlaySkillAnimation(skill.skillFrames, user, target);
                
                // Chờ animation kết thúc
                yield return new WaitUntil(() => finished);
            }
            
            // Apply effect
            ApplySkillEffect(user, target, skill);
            
            // End turn
            EndTurn();
        }
        
        /// <summary>
        /// Phát video rồi apply effect
        /// </summary>
        private IEnumerator PlaySkillWithVideo(CharacterData user, CharacterData target, Skill skill)
        {
            bool videoFinished = false;
            
            videoSkillPlayer.PlaySkillVideo(skill.skillVideo, () => 
            {
                videoFinished = true;
            });
            
            // Chờ video phát xong
            yield return new WaitUntil(() => videoFinished);
            
            // Apply effect
            ApplySkillEffect(user, target, skill);
            
            // End turn
            EndTurn();
        }

        /// <summary>
        /// Apply skill effect
        /// </summary>
        private void ApplySkillEffect(CharacterData user, CharacterData target, Skill skill)
        {
            Debug.Log($"🎯 ApplySkillEffect: {user.characterName} -> {target.characterName} | Skill: {skill.skillName}");
            
            // Apply logic effect
            switch (skill.skillType)
            {
                case SkillType.Damage:
                    teamManager.ApplyDamage(user, target, skill.damage);
                    break;
                    
                case SkillType.Heal:
                    teamManager.ApplyHeal(user, target, skill.healAmount);
                    break;
                    
                case SkillType.Buff:
                    Debug.Log($"🔼 {user.characterName} buff {target.characterName}");
                    // TODO: Implement buff system
                    break;
                    
                case SkillType.Debuff:
                    Debug.Log($"🔽 {user.characterName} debuff {target.characterName}");
                    // TODO: Implement debuff system
                    break;
            }
        }

        /// <summary>
        /// Update turn display UI
        /// </summary>
        private void UpdateTurnDisplay()
        {
            if (turnText != null)
            {
                turnText.text = $"Turn {turnCount}";
            }
            
            // Cập nhật avatar của nhân vật đang đến lượt
            if (turnAvatar != null && currentCharacter != null)
            {
                // Tìm sprite avatar từ CharacterData hoặc CharacterSkills
                Sprite avatarSprite = GetCharacterAvatar(currentCharacter);
                if (avatarSprite != null)
                {
                    turnAvatar.sprite = avatarSprite;
                    turnAvatar.gameObject.SetActive(true);
                }
                else
                {
                    turnAvatar.gameObject.SetActive(false);
                }
            }
            
            // Cập nhật 2 avatar lượt tiếp theo
            UpdateNextTurnAvatars();
        }

        /// <summary>
        /// Kết thúc trận đấu
        /// </summary>
        private void EndMatch()
        {
            Team winner = teamManager.GetWinningTeam();
            Debug.Log($"🏆 ===== TEAM {winner} THẮNG CUộC! =====");
            
            // Tắt processing turn để Auto Play dừng lại
            isProcessingTurn = false;
            
            // Chuyển về map1 sau khi kết thúc trận đấu
            StartCoroutine(ReturnToMap1());
        }
        
        /// <summary>
        /// Chuyển về map1 sau khi kết thúc trận đấu
        /// </summary>
        private System.Collections.IEnumerator ReturnToMap1()
        {
            // Hiển thị thông báo kết thúc
            if (DialogueManager.I != null)
            {
                Team winner = teamManager.GetWinningTeam();
                string winnerName = winner == Team.TeamA ? "Team A" : "Team B";
                
                DialogueManager.I.Show(new System.Collections.Generic.List<string>
                {
                    $"🏆 {winnerName} đã thắng cuộc!",
                    "Chuyển về map chính...",
                    "Nhấn Space để tiếp tục"
                });
            }
            
            // Đợi 3 giây trước khi chuyển scene
            yield return new WaitForSeconds(3f);
            
            // LƯU VỊ TRÍ PLAYER TRƯỚC KHI CHUYỂN SCENE
            SavePlayerPositionBeforeReturn();
            
            // Đặt flag để khôi phục vị trí khi quay về map1 (đồng bộ với SlimeAttack)
            PlayerPrefs.SetString("JustFinishedCombat", "true");
            PlayerPrefs.Save();
            
            // Chuyển qua Loading scene trước khi vào map1
            LoadingManager.NEXT_SCENE = "map1";
            SceneManager.LoadScene("Loading");
        }

        /// <summary>
        /// Reset trận đấu
        /// </summary>
        public void ResetMatch()
        {
            turnCount = 1;
            turnQueue.Clear();
            currentCharacter = null;
            pendingSkill = null;
            pendingTarget = null;
            
            teamManager.ResetMatch();
            
            StartCoroutine(StartBattle());
        }
        
        // ============================================
        // PUBLIC METHODS FOR AUTO PLAY
        // ============================================
        
        /// <summary>
        /// Kiểm tra có đang xử lý turn không (cho Auto Play)
        /// </summary>
        public bool IsProcessingTurn()
        {
            return isProcessingTurn;
        }
        
        /// <summary>
        /// Get character hiện tại (cho Auto Play)
        /// </summary>
        public CharacterData GetCurrentCharacter()
        {
            return currentCharacter;
        }
        
        /// <summary>
        /// Chọn target (được gọi từ Auto Play)
        /// </summary>
        public void SelectTarget(CharacterData target)
        {
            if (target != null)
            {
                OnTargetSelected(target);
            }
        }
        
        /// <summary>
        /// Xử lý khi có nhân vật chết - rebuild turn queue
        /// </summary>
        private void OnCharacterDied(CharacterData deadCharacter)
        {
            Debug.Log($"[Turn3v3Manager] {deadCharacter.characterName} đã chết! Rebuild turn queue...");
            
            // Rebuild turn queue để loại bỏ nhân vật đã chết
            BuildTurnQueue();
            
            // Nếu nhân vật chết là nhân vật hiện tại, chuyển sang lượt tiếp theo
            if (deadCharacter == currentCharacter)
            {
                Debug.Log($"[Turn3v3Manager] Nhân vật hiện tại đã chết! Chuyển sang lượt tiếp theo...");
                
                // Chuyển sang lượt tiếp theo
                StartNextTurn();
            }
            else
            {
                Debug.Log($"[Turn3v3Manager] Nhân vật khác đã chết, chỉ cập nhật thanh hiển thị");
                
                // Cập nhật lại thanh hiển thị lượt với delay nhỏ để đảm bảo turn queue đã được rebuild
                StartCoroutine(UpdateTurnDisplayWithDelay());
            }
            
            Debug.Log($"[Turn3v3Manager] Turn queue đã được rebuild. Còn {turnQueue.Count} nhân vật sống.");
        }
        
        /// <summary>
        /// Cập nhật thanh hiển thị lượt với delay nhỏ
        /// </summary>
        private IEnumerator UpdateTurnDisplayWithDelay()
        {
            yield return new WaitForEndOfFrame(); // Đợi 1 frame để đảm bảo mọi thứ đã được cập nhật
            UpdateTurnDisplay();
        }
        
        /// <summary>
        /// Cập nhật thanh hiển thị lượt tiếp theo
        /// </summary>
        private void UpdateNextTurnAvatars()
        {
            // Lấy danh sách nhân vật từ turn queue thực tế
            var turnQueueArray = turnQueue.ToArray();
            
            Debug.Log($"[Turn3v3Manager] UpdateNextTurnAvatars - Turn queue có {turnQueueArray.Length} nhân vật:");
            for (int i = 0; i < turnQueueArray.Length; i++)
            {
                Debug.Log($"  [{i}] {turnQueueArray[i].characterName} (Alive: {turnQueueArray[i].isAlive})");
            }
            
            if (turnQueueArray.Length <= 1)
            {
                // Chỉ còn 1 nhân vật hoặc ít hơn, ẩn next turn avatars
                Debug.Log($"[Turn3v3Manager] Chỉ còn {turnQueueArray.Length} nhân vật, ẩn next turn avatars");
                if (nextTurnAvatar1 != null) nextTurnAvatar1.gameObject.SetActive(false);
                if (nextTurnAvatar2 != null) nextTurnAvatar2.gameObject.SetActive(false);
                return;
            }
            
            // Tìm vị trí của nhân vật hiện tại trong turn queue
            int currentIndex = -1;
            for (int i = 0; i < turnQueueArray.Length; i++)
            {
                if (turnQueueArray[i] == currentCharacter)
                {
                    currentIndex = i;
                    break;
                }
            }
            
            Debug.Log($"[Turn3v3Manager] Current character: {currentCharacter?.characterName}, Index: {currentIndex}");
            
            // Cập nhật lượt tiếp theo thứ 1
            if (nextTurnAvatar1 != null)
            {
                int nextIndex1 = (currentIndex + 1) % turnQueueArray.Length;
                CharacterData nextCharacter1 = turnQueueArray[nextIndex1];
                Sprite nextAvatar1 = GetCharacterAvatar(nextCharacter1);
                
                Debug.Log($"[Turn3v3Manager] Next turn 1: {nextCharacter1?.characterName} (Index: {nextIndex1})");
                
                if (nextAvatar1 != null)
                {
                    nextTurnAvatar1.sprite = nextAvatar1;
                    nextTurnAvatar1.gameObject.SetActive(true);
                }
                else
                {
                    nextTurnAvatar1.gameObject.SetActive(false);
                }
            }
            
            // Cập nhật lượt tiếp theo thứ 2
            if (nextTurnAvatar2 != null)
            {
                int nextIndex2 = (currentIndex + 2) % turnQueueArray.Length;
                CharacterData nextCharacter2 = turnQueueArray[nextIndex2];
                Sprite nextAvatar2 = GetCharacterAvatar(nextCharacter2);
                
                Debug.Log($"[Turn3v3Manager] Next turn 2: {nextCharacter2?.characterName} (Index: {nextIndex2})");
                
                if (nextAvatar2 != null)
                {
                    nextTurnAvatar2.sprite = nextAvatar2;
                    nextTurnAvatar2.gameObject.SetActive(true);
                }
                else
                {
                    nextTurnAvatar2.gameObject.SetActive(false);
                }
            }
        }
        
        
        /// <summary>
        /// Lấy avatar sprite của nhân vật từ preset avatars
        /// </summary>
        private Sprite GetCharacterAvatar(CharacterData character)
        {
            if (character == null) return null;
            
            string characterName = character.characterName.ToLower();
            
            // Team A
            if (characterName.Contains("tini") || characterName.Contains("tí nị"))
                return avatarTini;
            
            if (characterName.Contains("muxam") || characterName.Contains("mụ xám"))
                return avatarMuxam;
            
            if (characterName.Contains("hoitruong") || characterName.Contains("hội trưởng") || characterName.Contains("hồi trương"))
                return avatarHoitruong;
            
            // Team B
            if (characterName.Contains("muthao") || characterName.Contains("mụ thảo"))
                return avatarMuthao;
            
            if (characterName.Contains("moctinh") || characterName.Contains("mộc tinh") || characterName.Contains("mộc trìu"))
                return avatarMoctinh;
            
            if (characterName.Contains("huyetthu") || characterName.Contains("huyết thú"))
                return avatarHuyetthu;
            
            return null;
        }
        
        /// <summary>
        /// Lưu vị trí player trước khi chuyển về map1
        /// </summary>
        private void SavePlayerPositionBeforeReturn()
        {
            // Tìm player trong scene pkchuong6
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player != null)
            {
                Vector3 playerPos = player.transform.position;
                PlayerPrefs.SetFloat("SavedPlayerX", playerPos.x);
                PlayerPrefs.SetFloat("SavedPlayerY", playerPos.y);
                PlayerPrefs.SetFloat("SavedPlayerZ", playerPos.z);
                PlayerPrefs.Save();
                
                Debug.Log($"[Turn3v3Manager] Đã lưu vị trí player từ pkchuong6: {playerPos}");
            }
            else
            {
                // Fallback: Lưu vị trí mặc định nếu không tìm thấy player
                Vector3 defaultPos = new Vector3(0, 0, 0);
                PlayerPrefs.SetFloat("SavedPlayerX", defaultPos.x);
                PlayerPrefs.SetFloat("SavedPlayerY", defaultPos.y);
                PlayerPrefs.SetFloat("SavedPlayerZ", defaultPos.z);
                PlayerPrefs.Save();
                
                Debug.LogWarning("[Turn3v3Manager] Không tìm thấy Player, đã lưu vị trí mặc định!");
            }
        }
    }
}
