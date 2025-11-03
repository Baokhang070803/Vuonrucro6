using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PVP
{
    /// <summary>
    /// Helper script để tự động setup UI cho CharacterSkills
    /// Attach vào character GameObject, click "Auto Setup UI" trong Inspector
    /// </summary>
    public class CharacterSkillsAutoSetup : MonoBehaviour
    {
        [Header("References")]
        public CharacterSkills characterSkills;
        
        [Header("UI Paths (relative to this GameObject)")]
        public string skillPanelPath = "Canvas/SkillPanel";
        public string[] skillButtonPaths = new string[3]
        {
            "Canvas/SkillPanel/Skill1Button",
            "Canvas/SkillPanel/Skill2Button",
            "Canvas/SkillPanel/Skill3Button"
        };
        
        [ContextMenu("Auto Setup UI")]
        public void AutoSetupUI()
        {
            if (characterSkills == null)
            {
                characterSkills = GetComponent<CharacterSkills>();
            }
            
            if (characterSkills == null)
            {
                Debug.LogError("❌ Không tìm thấy CharacterSkills component!");
                return;
            }
            
            // Find Skill Panel
            GameObject skillPanel = GameObject.Find(skillPanelPath);
            if (skillPanel == null)
            {
                Debug.LogWarning($"⚠️ Không tìm thấy Skill Panel tại: {skillPanelPath}");
                // Thử tìm bằng tên
                skillPanel = GameObject.Find("SkillPanel");
            }
            
            // Find Skill Buttons
            Button[] buttons = new Button[3];
            Image[] icons = new Image[3];
            TextMeshProUGUI[] names = new TextMeshProUGUI[3];
            TextMeshProUGUI[] cooldowns = new TextMeshProUGUI[3];
            
            for (int i = 0; i < 3; i++)
            {
                GameObject buttonObj = GameObject.Find(skillButtonPaths[i]);
                if (buttonObj == null)
                {
                    // Thử tìm bằng tên
                    buttonObj = GameObject.Find($"Skill{i + 1}Button");
                }
                
                if (buttonObj != null)
                {
                    buttons[i] = buttonObj.GetComponent<Button>();
                    
                    // Tìm Icon (Image con)
                    Transform iconTransform = buttonObj.transform.Find("Icon");
                    if (iconTransform != null)
                    {
                        icons[i] = iconTransform.GetComponent<Image>();
                    }
                    
                    // Tìm Name (TextMeshProUGUI)
                    Transform nameTransform = buttonObj.transform.Find("Name");
                    if (nameTransform != null)
                    {
                        names[i] = nameTransform.GetComponent<TextMeshProUGUI>();
                    }
                    
                    // Tìm Cooldown (TextMeshProUGUI)
                    Transform cooldownTransform = buttonObj.transform.Find("Cooldown");
                    if (cooldownTransform != null)
                    {
                        cooldowns[i] = cooldownTransform.GetComponent<TextMeshProUGUI>();
                    }
                    
                    Debug.Log($"✅ Found Skill Button {i + 1}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Không tìm thấy Skill Button {i + 1}");
                }
            }
            
            // Setup UI
            characterSkills.SetupUI(skillPanel, buttons, icons, names, cooldowns);
            
            Debug.Log($"✅ Auto Setup UI hoàn tất cho {gameObject.name}!");
        }
        
        [ContextMenu("Print Hierarchy")]
        public void PrintHierarchy()
        {
            Debug.Log("=== HIERARCHY ===");
            PrintChildren(transform, 0);
        }
        
        private void PrintChildren(Transform parent, int level)
        {
            string indent = new string(' ', level * 2);
            Debug.Log($"{indent}└─ {parent.name} [{parent.gameObject.GetComponents<Component>().Length - 1} components]");
            
            foreach (Transform child in parent)
            {
                PrintChildren(child, level + 1);
            }
        }
    }
}
