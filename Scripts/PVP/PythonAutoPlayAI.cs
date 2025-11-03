using UnityEngine;
using Python.Runtime;
using System.Collections.Generic;
using System;

namespace PVP
{
    /// <summary>
    /// C# wrapper cho Python Auto Play AI
    /// Tích hợp Python AI vào Unity AutoPlayController
    /// </summary>
    public class PythonAutoPlayAI : MonoBehaviour
    {
        [Header("Python AI Settings")]
        [SerializeField] private bool usePythonAI = true;
        [SerializeField] private string aiDifficulty = "medium"; // easy, medium, hard
        [SerializeField] private string aiStrategy = "balanced"; // aggressive, defensive, balanced
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Python references
        private static PyObject autoPlayAIModule;
        private bool pythonInitialized = false;
        
        // Fallback C# AI
        private AutoPlayController fallbackController;
        
        private void Awake()
        {
            // Tìm fallback controller
            fallbackController = GetComponent<AutoPlayController>();
            if (fallbackController == null)
            {
                fallbackController = FindObjectOfType<AutoPlayController>();
            }
        }
        
        private void Start()
        {
            if (usePythonAI)
            {
                // Đợi 1 frame để đảm bảo Unity đã khởi tạo xong
                StartCoroutine(InitializePythonAIDelayed());
            }
        }
        
        private System.Collections.IEnumerator InitializePythonAIDelayed()
        {
            yield return new WaitForEndOfFrame();
            InitializePythonAI();
        }
        
        private void OnDestroy()
        {
            // Cleanup Python nếu cần
            if (pythonInitialized)
            {
                try
                {
                    using (Py.GIL())
                    {
                        autoPlayAIModule?.Dispose();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Python cleanup warning: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Initialize Python AI
        /// </summary>
        private void InitializePythonAI()
        {
            try
            {
                // Initialize Python engine nếu chưa có
                if (!PythonEngine.IsInitialized)
                {
                    PythonEngine.Initialize();
                    DebugLog("🐍 Python Engine initialized");
                }
                
                using (Py.GIL())
                {
                    // Add current directory to Python path
                    DebugLog("🔍 Adding PVP directory to Python path...");
                    var sys = Py.Import("sys");
                    var path = sys.GetAttr("path");
                    var pvpPath = System.IO.Path.Combine(Application.dataPath, "Scripts", "PVP");
                    path.InvokeMethod("append", new PyObject[] { new PyString(pvpPath) });
                    DebugLog($"🐍 Added to Python path: {pvpPath}");
                    
                    // Load Python module
                    DebugLog("📦 Loading auto_play_ai module...");
                    autoPlayAIModule = Py.Import("auto_play_ai");
                    
                    if (autoPlayAIModule == null)
                    {
                        DebugLogError("❌ Không thể load auto_play_ai module!");
                        DebugLogError("❌ Kiểm tra file auto_play_ai.py có trong thư mục PVP không");
                        return;
                    }
                    
                    DebugLog("✅ auto_play_ai module loaded successfully");
                    
                    // Initialize AI
                    var result = autoPlayAIModule.InvokeMethod("initialize_ai", 
                        new PyObject[] { 
                            new PyString(aiDifficulty), 
                            new PyString(aiStrategy) 
                        });
                    
                    if (result != null)
                    {
                        var success = result["success"].As<bool>();
                        var message = result["message"].As<string>();
                        
                        if (success)
                        {
                            pythonInitialized = true;
                            DebugLog($"✅ Python AI initialized: {message}");
                        }
                        else
                        {
                            DebugLogError($"❌ Python AI initialization failed: {message}");
                        }
                    }
                    else
                    {
                        DebugLogError("❌ Python AI returned null result");
                    }
                }
            }
            catch (Exception e)
            {
                DebugLogError($"❌ Python AI initialization error: {e.Message}");
                DebugLogError($"❌ Stack trace: {e.StackTrace}");
                pythonInitialized = false;
            }
        }
        
        /// <summary>
        /// Get AI recommendation cho character
        /// </summary>
        public AIRecommendation GetAIRecommendation(CharacterData currentCharacter, 
                                                  List<CharacterData> allCharacters)
        {
            if (!usePythonAI || !pythonInitialized)
            {
                return GetFallbackRecommendation(currentCharacter, allCharacters);
            }
            
            try
            {
                using (Py.GIL())
                {
                    // Prepare character data
                    var currentCharData = PrepareCharacterData(currentCharacter);
                    var allCharsData = PrepareAllCharactersData(allCharacters);
                    var skillsData = PrepareSkillsData(currentCharacter);
                    
                    // Call Python AI
                    var result = autoPlayAIModule.InvokeMethod("get_ai_recommendation",
                        new PyObject[] {
                            new PyString((string)currentCharData["name"]),
                            new PyString((string)currentCharData["team"]),
                            new PyString((string)currentCharData["position"]),
                            new PyFloat((float)currentCharData["current_health"]),
                            new PyFloat((float)currentCharData["max_health"]),
                            new PyInt((bool)currentCharData["is_alive"] ? 1 : 0),
                            new PyList(skillsData.ToArray()),
                            new PyList(allCharsData.ToArray())
                        });
                    
                    if (result != null)
                    {
                        var success = result["success"].As<bool>();
                        if (success)
                        {
                            var recommendation = result["recommendation"];
                            return ParseAIRecommendation(recommendation);
                        }
                        else
                        {
                            var message = result["message"].As<string>();
                            DebugLogError($"Python AI error: {message}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugLogError($"Python AI call error: {e.Message}");
            }
            
            // Fallback to C# AI
            return GetFallbackRecommendation(currentCharacter, allCharacters);
        }
        
        /// <summary>
        /// Analyze match state với Python AI
        /// </summary>
        public MatchAnalysis AnalyzeMatchState(List<CharacterData> allCharacters)
        {
            if (!usePythonAI || !pythonInitialized)
            {
                return GetFallbackMatchAnalysis(allCharacters);
            }
            
            try
            {
                using (Py.GIL())
                {
                    var allCharsData = PrepareAllCharactersData(allCharacters);
                    
                    var result = autoPlayAIModule.InvokeMethod("analyze_match_state",
                        new PyObject[] { new PyList(allCharsData.ToArray()) });
                    
                    if (result != null)
                    {
                        var success = result["success"].As<bool>();
                        if (success)
                        {
                            var analysis = result["analysis"];
                            return ParseMatchAnalysis(analysis);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugLogError($"Python match analysis error: {e.Message}");
            }
            
            return GetFallbackMatchAnalysis(allCharacters);
        }
        
        /// <summary>
        /// Prepare character data for Python
        /// </summary>
        private Dictionary<string, object> PrepareCharacterData(CharacterData character)
        {
            return new Dictionary<string, object>
            {
                ["name"] = character.characterName,
                ["team"] = character.team.ToString(),
                ["position"] = character.position.ToString(),
                ["current_health"] = character.currentHealth,
                ["max_health"] = character.maxHealth,
                ["is_alive"] = character.isAlive
            };
        }
        
        /// <summary>
        /// Prepare all characters data for Python
        /// </summary>
        private List<PyObject> PrepareAllCharactersData(List<CharacterData> characters)
        {
            var result = new List<PyObject>();
            
            foreach (var character in characters)
            {
                var charData = PrepareCharacterData(character);
                var pyDict = new PyDict();
                
                foreach (var kvp in charData)
                {
                    if (kvp.Value is string)
                        pyDict[kvp.Key] = new PyString((string)kvp.Value);
                    else if (kvp.Value is float)
                        pyDict[kvp.Key] = new PyFloat((float)kvp.Value);
                    else if (kvp.Value is bool)
                        pyDict[kvp.Key] = new PyInt((bool)kvp.Value ? 1 : 0);
                    else
                        pyDict[kvp.Key] = new PyString(kvp.Value.ToString());
                }
                
                result.Add(pyDict);
            }
            
            return result;
        }
        
        /// <summary>
        /// Prepare skills data for Python
        /// </summary>
        private List<PyObject> PrepareSkillsData(CharacterData character)
        {
            var skills = new List<PyObject>();
            
            if (character.characterSkills != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    var skill = character.characterSkills.GetSkill(i);
                    if (skill != null)
                    {
                        var skillDict = new PyDict();
                        skillDict["skill_name"] = new PyString(skill.skillName);
                        skillDict["skill_type"] = new PyString(skill.skillType.ToString().ToLower());
                        skillDict["target_type"] = new PyString(skill.targetType.ToString().ToLower());
                        skillDict["damage"] = new PyFloat(skill.damage);
                        skillDict["heal_amount"] = new PyFloat(skill.healAmount);
                        skillDict["cooldown"] = new PyFloat(skill.cooldown);
                        skillDict["is_ready"] = new PyInt(skill.IsReady ? 1 : 0);
                        
                        skills.Add(skillDict);
                    }
                }
            }
            
            return skills;
        }
        
        /// <summary>
        /// Parse Python AI recommendation
        /// </summary>
        private AIRecommendation ParseAIRecommendation(PyObject recommendation)
        {
            return new AIRecommendation
            {
                skillIndex = recommendation["skill_index"].As<int>(),
                targetIndex = recommendation["target_index"].As<int>(),
                skillName = recommendation["skill_name"].As<string>(),
                targetName = recommendation["target_name"].As<string>(),
                reason = recommendation["reason"].As<string>(),
                confidence = recommendation["confidence"].As<float>()
            };
        }
        
        /// <summary>
        /// Parse Python match analysis
        /// </summary>
        private MatchAnalysis ParseMatchAnalysis(PyObject analysis)
        {
            return new MatchAnalysis
            {
                teamACount = analysis["team_a_count"].As<int>(),
                teamBCount = analysis["team_b_count"].As<int>(),
                teamAAvgHP = analysis["team_a_avg_hp"].As<float>(),
                teamBAvgHP = analysis["team_b_avg_hp"].As<float>(),
                advantage = analysis["advantage"].As<string>()
            };
        }
        
        /// <summary>
        /// Fallback C# AI recommendation
        /// </summary>
        private AIRecommendation GetFallbackRecommendation(CharacterData currentCharacter, 
                                                          List<CharacterData> allCharacters)
        {
            // Simple C# AI logic
            var availableSkills = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                var skill = currentCharacter.characterSkills?.GetSkill(i);
                if (skill != null && skill.IsReady)
                {
                    availableSkills.Add(i);
                }
            }
            
            if (availableSkills.Count == 0)
            {
                return new AIRecommendation
                {
                    skillIndex = -1,
                    targetIndex = -1,
                    reason = "No skills available",
                    confidence = 0.0f
                };
            }
            
            // Random skill selection
            int skillIndex = availableSkills[UnityEngine.Random.Range(0, availableSkills.Count)];
            
            // Find enemies
            var enemies = new List<CharacterData>();
            foreach (var character in allCharacters)
            {
                if (character.team != currentCharacter.team && character.isAlive)
                {
                    enemies.Add(character);
                }
            }
            
            int targetIndex = -1;
            if (enemies.Count > 0)
            {
                var target = enemies[UnityEngine.Random.Range(0, enemies.Count)];
                targetIndex = allCharacters.IndexOf(target);
            }
            
            return new AIRecommendation
            {
                skillIndex = skillIndex,
                targetIndex = targetIndex,
                reason = "C# Fallback AI",
                confidence = 0.5f
            };
        }
        
        /// <summary>
        /// Fallback C# match analysis
        /// </summary>
        private MatchAnalysis GetFallbackMatchAnalysis(List<CharacterData> allCharacters)
        {
            int teamACount = 0, teamBCount = 0;
            float teamAHP = 0, teamBHP = 0;
            
            foreach (var character in allCharacters)
            {
                if (character.isAlive)
                {
                    if (character.team == Team.TeamA)
                    {
                        teamACount++;
                        teamAHP += character.currentHealth / character.maxHealth;
                    }
                    else
                    {
                        teamBCount++;
                        teamBHP += character.currentHealth / character.maxHealth;
                    }
                }
            }
            
            return new MatchAnalysis
            {
                teamACount = teamACount,
                teamBCount = teamBCount,
                teamAAvgHP = teamACount > 0 ? teamAHP / teamACount : 0,
                teamBAvgHP = teamBCount > 0 ? teamBHP / teamBCount : 0,
                advantage = teamACount > teamBCount ? "TeamA" : teamBCount > teamACount ? "TeamB" : "Even"
            };
        }
        
        private void DebugLog(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[PythonAutoPlayAI] {message}");
            }
        }
        
        private void DebugLogError(string message)
        {
            Debug.LogError($"[PythonAutoPlayAI] {message}");
        }
    }
    
    /// <summary>
    /// AI Recommendation data structure
    /// </summary>
    [System.Serializable]
    public class AIRecommendation
    {
        public int skillIndex;
        public int targetIndex;
        public string skillName;
        public string targetName;
        public string reason;
        public float confidence;
    }
    
    /// <summary>
    /// Match Analysis data structure
    /// </summary>
    [System.Serializable]
    public class MatchAnalysis
    {
        public int teamACount;
        public int teamBCount;
        public float teamAAvgHP;
        public float teamBAvgHP;
        public string advantage;
    }
}