using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace PVP
{
    /// <summary>
    /// Tự động chơi game - Auto Play
    /// Tự động chọn skill và target
    /// </summary>
    public class AutoPlayController : MonoBehaviour
    {
        [Header("Button Reference")]
        [SerializeField] private Button autoPlayButton;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Image buttonImage;
        
        [Header("Button Sprites (Optional)")]
        [SerializeField] private Sprite autoOnSprite;  // Ảnh khi bật Auto
        [SerializeField] private Sprite autoOffSprite; // Ảnh khi tắt Auto
        
        [Header("Button Colors")]
        [SerializeField] private Color autoOnColor = new Color(0f, 1f, 0f, 1f);  // Xanh lá
        [SerializeField] private Color autoOffColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Xám
        
        [Header("Auto Play Settings")]
        [SerializeField] private float autoPlayDelay = 0.5f; // Delay giữa các action
        [SerializeField] private bool randomizeSkill = true; // Random skill hay chọn skill đầu
        [SerializeField] private bool randomizeTarget = true; // Random target
        
        [Header("References")]
        [SerializeField] private Turn3v3Manager turnManager;
        private Team3v3Manager teamManager; // Cache team manager
        private VideoSkillPlayer videoPlayer; // Cache video player
        
        [Header("AI Settings")]
        [SerializeField] private bool usePythonAI = true; // Sử dụng Python AI
        [SerializeField] private string aiDifficulty = "medium"; // easy, medium, hard
        [SerializeField] private string aiStrategy = "balanced"; // aggressive, defensive, balanced
        
        // AI Components
        private PythonAutoPlayAI pythonAI;
        
        // State
        private bool isAutoPlayEnabled = false;
        private Coroutine autoPlayCoroutine;
        
        // Events
        public System.Action<bool> OnAutoPlayChanged;
        
        private void Start()
        {
            // Tìm turn manager nếu chưa gán
            if (turnManager == null)
            {
                turnManager = FindObjectOfType<Turn3v3Manager>();
            }
            
            // Tìm team manager
            if (teamManager == null)
            {
                teamManager = FindObjectOfType<Team3v3Manager>();
            }
            
            // Tìm video player
            if (videoPlayer == null)
            {
                videoPlayer = FindObjectOfType<VideoSkillPlayer>();
            }
            
            // Tìm Python AI
            if (usePythonAI)
            {
                pythonAI = GetComponent<PythonAutoPlayAI>();
                if (pythonAI == null)
                {
                    pythonAI = gameObject.AddComponent<PythonAutoPlayAI>();
                    Debug.Log("✅ Đã thêm PythonAutoPlayAI component");
                }
            }
            
            // Tìm button nếu chưa gán
            if (autoPlayButton == null)
            {
                autoPlayButton = GetComponent<Button>();
            }
            
            // Tìm image và text
            if (autoPlayButton != null)
            {
                if (buttonImage == null)
                    buttonImage = autoPlayButton.GetComponent<Image>();
                
                if (buttonText == null)
                    buttonText = autoPlayButton.GetComponentInChildren<TextMeshProUGUI>();
            }
            
            // Setup button click
            if (autoPlayButton != null)
            {
                autoPlayButton.onClick.AddListener(ToggleAutoPlay);
            }
            
            // Set trạng thái ban đầu
            UpdateButtonVisual();
        }
        
        /// <summary>
        /// Bật/tắt Auto Play
        /// </summary>
        public void ToggleAutoPlay()
        {
            SetAutoPlay(!isAutoPlayEnabled);
        }
        
        /// <summary>
        /// Set Auto Play on/off
        /// </summary>
        public void SetAutoPlay(bool enabled)
        {
            isAutoPlayEnabled = enabled;
            
            UpdateButtonVisual();
            OnAutoPlayChanged?.Invoke(isAutoPlayEnabled);
            
            if (isAutoPlayEnabled)
            {
                Debug.Log("🤖 Auto Play BẬT!");
                // Bắt đầu auto play nếu đang có turn
                if (autoPlayCoroutine == null)
                {
                    autoPlayCoroutine = StartCoroutine(AutoPlayLoop());
                }
            }
            else
            {
                Debug.Log("✋ Auto Play TẮT!");
                // Dừng auto play
                if (autoPlayCoroutine != null)
                {
                    StopCoroutine(autoPlayCoroutine);
                    autoPlayCoroutine = null;
                }
            }
        }
        
        /// <summary>
        /// Loop auto play
        /// </summary>
        private IEnumerator AutoPlayLoop()
        {
            while (isAutoPlayEnabled)
            {
                // Đợi lâu hơn để animation kết thúc
                yield return new WaitForSeconds(2f);
                
                // ✅ CHECK: Nếu trận đấu đã kết thúc → TẮT AUTO PLAY
                if (teamManager != null && teamManager.IsMatchOver())
                {
                    Debug.Log("🏁 Trận đấu kết thúc! Tự động TẮT Auto Play.");
                    SetAutoPlay(false); // Tắt auto play
                    yield break; // Thoát coroutine
                }
                
                // Check xem có đang trong turn không
                if (turnManager != null && !turnManager.IsProcessingTurn())
                {
                    // Check xem có đang phát video không
                    if (videoPlayer != null && videoPlayer.IsPlaying)
                    {
                        continue;
                    }
                    
                    // Thực hiện auto play
                    AutoPlayTurn();
                }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Tự động chơi 1 turn
        /// </summary>
        private void AutoPlayTurn()
        {
            if (turnManager == null) return;
            
            var currentCharacter = turnManager.GetCurrentCharacter();
            if (currentCharacter == null || currentCharacter.isDead) return;
            
            var characterSkills = currentCharacter.characterSkills;
            if (characterSkills == null) return;
            
            // Chọn skill
            int skillIndex = ChooseSkill(characterSkills);
            if (skillIndex < 0) return;
            
            var skill = characterSkills.GetSkill(skillIndex);
            if (skill == null || !skill.IsReady) return;
            
            // Dùng skill
            characterSkills.UseSkill(skillIndex);
            
            // Đợi một chút để UI hiện target selector
            StartCoroutine(WaitAndSelectTarget(skill));
        }
        
        /// <summary>
        /// Chọn skill tự động - Sử dụng Python AI hoặc C# fallback
        /// </summary>
        private int ChooseSkill(CharacterSkills characterSkills)
        {
            var currentCharacter = turnManager.GetCurrentCharacter();
            if (currentCharacter == null) return -1;
            
            // Sử dụng Python AI nếu có
            if (usePythonAI && pythonAI != null)
            {
                try
                {
                    var allCharacters = teamManager.GetAllCharacters();
                    var recommendation = pythonAI.GetAIRecommendation(currentCharacter, allCharacters);
                    
                    if (recommendation.skillIndex >= 0)
                    {
                        Debug.Log($"🤖 Python AI chọn skill: {recommendation.skillName} (Confidence: {recommendation.confidence:F2}) - {recommendation.reason}");
                        return recommendation.skillIndex;
                    }
                    else
                    {
                        Debug.LogWarning($"🤖 Python AI không tìm thấy skill phù hợp: {recommendation.reason}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"🤖 Python AI error: {e.Message} - Fallback to C# AI");
                }
            }
            
            // Fallback to C# AI
            return ChooseSkillFallback(characterSkills);
        }
        
        /// <summary>
        /// C# Fallback AI cho skill selection
        /// </summary>
        private int ChooseSkillFallback(CharacterSkills characterSkills)
        {
            if (randomizeSkill)
            {
                // Random skill có thể dùng
                var availableSkills = new System.Collections.Generic.List<int>();
                for (int i = 0; i < 3; i++)
                {
                    var skill = characterSkills.GetSkill(i);
                    if (skill != null && skill.IsReady)
                    {
                        availableSkills.Add(i);
                    }
                }
                
                if (availableSkills.Count > 0)
                {
                    return availableSkills[Random.Range(0, availableSkills.Count)];
                }
            }
            else
            {
                // Chọn skill đầu tiên có thể dùng
                for (int i = 0; i < 3; i++)
                {
                    var skill = characterSkills.GetSkill(i);
                    if (skill != null && skill.IsReady)
                    {
                        return i;
                    }
                }
            }
            
            return -1;
        }
        
        /// <summary>
        /// Chọn target - Sử dụng Python AI hoặc C# fallback
        /// </summary>
        private CharacterData ChooseTarget(CharacterData currentCharacter, Skill skill, System.Collections.Generic.List<CharacterData> targets)
        {
            // Sử dụng Python AI nếu có
            if (usePythonAI && pythonAI != null)
            {
                try
                {
                    var allCharacters = teamManager.GetAllCharacters();
                    var recommendation = pythonAI.GetAIRecommendation(currentCharacter, allCharacters);
                    
                    if (recommendation.targetIndex >= 0 && recommendation.targetIndex < allCharacters.Count)
                    {
                        var target = allCharacters[recommendation.targetIndex];
                        if (target.isAlive)
                        {
                            Debug.Log($"🤖 Python AI chọn target: {recommendation.targetName} - {recommendation.reason}");
                            return target;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"🤖 Python AI target selection error: {e.Message} - Fallback to C# AI");
                }
            }
            
            // Fallback to C# AI
            return ChooseTargetFallback(targets);
        }
        
        /// <summary>
        /// C# Fallback AI cho target selection
        /// </summary>
        private CharacterData ChooseTargetFallback(System.Collections.Generic.List<CharacterData> targets)
        {
            if (randomizeTarget)
            {
                // Random target còn sống
                var aliveTargets = targets.FindAll(t => t.isAlive);
                if (aliveTargets.Count > 0)
                {
                    return aliveTargets[Random.Range(0, aliveTargets.Count)];
                }
            }
            else
            {
                // Chọn target đầu tiên còn sống
                return targets.Find(t => t.isAlive);
            }
            
            return null;
        }
        
        /// <summary>
        /// Đợi và chọn target
        /// </summary>
        private IEnumerator WaitAndSelectTarget(Skill skill)
        {
            // Đợi UI hiện
            yield return new WaitForSeconds(0.2f);
            
            if (turnManager == null) yield break;
            
            // Lấy list targets
            var currentCharacter = turnManager.GetCurrentCharacter();
            if (currentCharacter == null) yield break;
            
            var teamManager = FindObjectOfType<Team3v3Manager>();
            if (teamManager == null) yield break;
            
            System.Collections.Generic.List<CharacterData> targets = null;
            
            // Chọn target dựa vào skill type
            if (skill.targetType == SkillTarget.Enemy)
            {
                targets = teamManager.GetEnemyTeam(currentCharacter);
            }
            else if (skill.targetType == SkillTarget.Ally)
            {
                targets = teamManager.GetAllyTeam(currentCharacter);
            }
            else if (skill.targetType == SkillTarget.Self)
            {
                // Target là bản thân
                turnManager.SelectTarget(currentCharacter);
                yield break;
            }
            
            if (targets == null || targets.Count == 0) yield break;
            
            // Chọn target - Sử dụng Python AI hoặc C# fallback
            CharacterData selectedTarget = ChooseTarget(currentCharacter, skill, targets);
            
            if (selectedTarget != null)
            {
                turnManager.SelectTarget(selectedTarget);
            }
        }
        
        /// <summary>
        /// Update button visual
        /// </summary>
        private void UpdateButtonVisual()
        {
            if (autoPlayButton == null) return;
            
            // Update sprite
            if (buttonImage != null)
            {
                if (isAutoPlayEnabled && autoOnSprite != null)
                {
                    buttonImage.sprite = autoOnSprite;
                }
                else if (!isAutoPlayEnabled && autoOffSprite != null)
                {
                    buttonImage.sprite = autoOffSprite;
                }
                
                // Update color
                buttonImage.color = isAutoPlayEnabled ? autoOnColor : autoOffColor;
            }
            
            // Update text
            if (buttonText != null)
            {
                buttonText.text = isAutoPlayEnabled ? "AUTO ON" : "AUTO OFF";
            }
        }
        
        /// <summary>
        /// Get trạng thái auto play
        /// </summary>
        public bool IsAutoPlayEnabled()
        {
            return isAutoPlayEnabled;
        }
        
        /// <summary>
        /// Get AI analysis của trận đấu
        /// </summary>
        public void ShowAIAnalysis()
        {
            if (usePythonAI && pythonAI != null && teamManager != null)
            {
                try
                {
                    var allCharacters = teamManager.GetAllCharacters();
                    var analysis = pythonAI.AnalyzeMatchState(allCharacters);
                    
                    Debug.Log($"🤖 AI Analysis:");
                    Debug.Log($"  Team A: {analysis.teamACount} characters, Avg HP: {analysis.teamAAvgHP:F1%}");
                    Debug.Log($"  Team B: {analysis.teamBCount} characters, Avg HP: {analysis.teamBAvgHP:F1%}");
                    Debug.Log($"  Advantage: {analysis.advantage}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"🤖 AI Analysis error: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Setup button từ code
        /// </summary>
        public void SetupButton(Button button)
        {
            autoPlayButton = button;
            
            if (autoPlayButton != null)
            {
                autoPlayButton.onClick.RemoveAllListeners();
                autoPlayButton.onClick.AddListener(ToggleAutoPlay);
                
                buttonImage = autoPlayButton.GetComponent<Image>();
                buttonText = autoPlayButton.GetComponentInChildren<TextMeshProUGUI>();
            }
            
            UpdateButtonVisual();
        }
        
        private void OnDestroy()
        {
            if (autoPlayCoroutine != null)
            {
                StopCoroutine(autoPlayCoroutine);
            }
        }
    }
}
