using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PVP
{
    public class CharacterSkills : MonoBehaviour
    {
        [Header("Character Info")]
        [SerializeField] private string characterName = "Character";
        
        [Header("Skills")]
        [SerializeField] private List<Skill> skills = new List<Skill>();
        
        [Header("UI References")]
        [SerializeField] private GameObject skillPanel;
        [SerializeField] private Button[] skillButtons = new Button[3];
        [SerializeField] private Image[] skillIcons = new Image[3];
        [SerializeField] private TextMeshProUGUI[] skillNames = new TextMeshProUGUI[3];
        [SerializeField] private TextMeshProUGUI[] cooldownTexts = new TextMeshProUGUI[3];
        
        public System.Action<Skill> OnSkillUsed;
        
        private Skill pendingSkill;
        private int pendingSkillIndex;
        public bool IsWaitingForTarget { get; private set; }
        
        private void Start()
        {
            SetupSkillButtons();
            HideSkills();
            InitializeSkills();
        }
        
        private void InitializeSkills()
        {
            if (skills.Count == 0)
            {
                LoadSkillsByCharacterName();
            }
        }
        
        private void LoadSkillsByCharacterName()
        {
            var loadedSkills = SkillPresets.GetSkillsByCharacterName(characterName);
            skills.AddRange(loadedSkills);
            Debug.Log($"Đã load {skills.Count} skills cho {characterName}!");
        }
        
        private void SetupSkillButtons()
        {
            for (int i = 0; i < skillButtons.Length && i < 3; i++)
            {
                if (skillButtons[i] != null)
                {
                    int index = i;
                    skillButtons[i].onClick.AddListener(() => UseSkill(index));
                }
            }
        }
        
        public void ShowSkills()
        {
            if (skillPanel != null)
            {
                skillPanel.SetActive(true);
                UpdateSkillUI();
            }
        }
        
        public void HideSkills()
        {
            if (skillPanel != null)
            {
                skillPanel.SetActive(false);
            }
        }
        
        private void UpdateSkillUI()
        {
            for (int i = 0; i < skills.Count && i < 3; i++)
            {
                Skill skill = skills[i];
                
                if (skillIcons[i] != null && skill.skillIcon != null)
                    skillIcons[i].sprite = skill.skillIcon;
                
                if (skillNames[i] != null)
                    skillNames[i].text = skill.skillName;
                
                if (cooldownTexts[i] != null)
                {
                    if (skill.IsReady)
                    {
                        cooldownTexts[i].text = "SẴN SÀNG";
                        cooldownTexts[i].color = Color.green;
                    }
                    else
                    {
                        cooldownTexts[i].text = $"CD: {skill.currentCooldown}";
                        cooldownTexts[i].color = Color.red;
                    }
                }
                
                if (skillButtons[i] != null)
                {
                    skillButtons[i].interactable = skill.IsReady;
                    var buttonImage = skillButtons[i].GetComponent<Image>();
                    if (buttonImage != null)
                        buttonImage.color = skill.IsReady ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
            }
        }
        
        public void UseSkill(int skillIndex)
        {
            if (skillIndex < 0 || skillIndex >= skills.Count)
                return;
            
            Skill skill = skills[skillIndex];
            
            if (!skill.IsReady)
                return;
            
            pendingSkill = skill;
            pendingSkillIndex = skillIndex;
            IsWaitingForTarget = true;
            
            OnSkillUsed?.Invoke(skill);
        }
        
        public void ConfirmSkillUsage()
        {
            if (!IsWaitingForTarget || pendingSkill == null)
                return;
            
            pendingSkill.Use();
            pendingSkill = null;
            IsWaitingForTarget = false;
            
            UpdateSkillUI();
            HideSkills();
        }
        
        public void CancelPendingSkill()
        {
            if (IsWaitingForTarget)
            {
                pendingSkill = null;
                IsWaitingForTarget = false;
            }
        }
        
        public Skill GetPendingSkill()
        {
            return pendingSkill;
        }
        
        public void ReduceAllCooldowns()
        {
            foreach (var skill in skills)
                skill.ReduceCooldown(1f);
            
            UpdateSkillUI();
        }
        
        public Skill GetSkill(int index)
        {
            if (index >= 0 && index < skills.Count)
                return skills[index];
            return null;
        }
        
        public void ResetAllCooldowns()
        {
            foreach (var skill in skills)
                skill.currentCooldown = 0;
            UpdateSkillUI();
        }
        
        [ContextMenu("Reset và Reload Skills")]
        public void ReloadSkillsFromPresets()
        {
            skills.Clear();
            LoadSkillsByCharacterName();
        }
        
        public void SetupSkills(Skill skill1, Skill skill2, Skill skill3)
        {
            skills.Clear();
            skills.Add(skill1);
            skills.Add(skill2);
            skills.Add(skill3);
        }
        
        public void SetupUI(GameObject panel, Button[] buttons, Image[] icons, TextMeshProUGUI[] names, TextMeshProUGUI[] cooldowns)
        {
            skillPanel = panel;
            skillButtons = buttons;
            skillIcons = icons;
            skillNames = names;
            cooldownTexts = cooldowns;
            
            SetupSkillButtons();
        }
    }
}
