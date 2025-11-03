using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PVP
{
    /// <summary>
    /// Target selector cho 3v3 - hiện/ẩn 3 buttons mũi tên cho 3 targets
    /// </summary>
    public class TargetSelector3v3 : MonoBehaviour
    {
        [Header("Target Buttons - Team A (3 characters)")]
        [Tooltip("Button mũi tên cho Team A - Position Top")]
        public Button targetTeamA_Top;
        
        [Tooltip("Button mũi tên cho Team A - Position Middle")]
        public Button targetTeamA_Middle;
        
        [Tooltip("Button mũi tên cho Team A - Position Bottom")]
        public Button targetTeamA_Bottom;

        [Header("Target Buttons - Team B (3 characters)")]
        [Tooltip("Button mũi tên cho Team B - Position Top")]
        public Button targetTeamB_Top;
        
        [Tooltip("Button mũi tên cho Team B - Position Middle")]
        public Button targetTeamB_Middle;
        
        [Tooltip("Button mũi tên cho Team B - Position Bottom")]
        public Button targetTeamB_Bottom;

        [Header("References")]
        public Turn3v3Manager turnManager;

        // Dictionary để map button với character
        private Dictionary<Button, CharacterData> buttonToCharacterMap = new Dictionary<Button, CharacterData>();

        private void Start()
        {
            // Ẩn tất cả buttons ban đầu
            HideAllTargets();
            
            // Setup click listeners
            SetupButtonListeners();
            
            Debug.Log("[TargetSelector3v3] Đã khởi tạo. Tất cả target buttons ẩn.");
        }

        /// <summary>
        /// Setup click listeners cho tất cả buttons
        /// </summary>
        private void SetupButtonListeners()
        {
            if (targetTeamA_Top != null)
                targetTeamA_Top.onClick.AddListener(() => OnTargetButtonClicked(targetTeamA_Top));
            
            if (targetTeamA_Middle != null)
                targetTeamA_Middle.onClick.AddListener(() => OnTargetButtonClicked(targetTeamA_Middle));
            
            if (targetTeamA_Bottom != null)
                targetTeamA_Bottom.onClick.AddListener(() => OnTargetButtonClicked(targetTeamA_Bottom));
            
            if (targetTeamB_Top != null)
                targetTeamB_Top.onClick.AddListener(() => OnTargetButtonClicked(targetTeamB_Top));
            
            if (targetTeamB_Middle != null)
                targetTeamB_Middle.onClick.AddListener(() => OnTargetButtonClicked(targetTeamB_Middle));
            
            if (targetTeamB_Bottom != null)
                targetTeamB_Bottom.onClick.AddListener(() => OnTargetButtonClicked(targetTeamB_Bottom));
        }

        /// <summary>
        /// Hiện target buttons cho địch (enemies)
        /// </summary>
        public void ShowEnemyTargets(List<CharacterData> enemies)
        {
            HideAllTargets();
            buttonToCharacterMap.Clear();
            
            Debug.Log($"[TargetSelector3v3] Hiện {enemies.Count} địch để chọn...");
            
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.isDead) continue;
                
                Button targetButton = GetButtonForCharacter(enemy);
                
                if (targetButton != null)
                {
                    targetButton.gameObject.SetActive(true);
                    buttonToCharacterMap[targetButton] = enemy;
                    Debug.Log($"  → {enemy.characterName} (Team {enemy.team}, Pos {enemy.position})");
                }
                else
                {
                    Debug.LogWarning($"Không tìm thấy button cho {enemy.characterName}!");
                }
            }
        }

        /// <summary>
        /// Hiện target buttons cho đồng đội (allies) - dùng cho skill buff
        /// </summary>
        public void ShowAllyTargets(List<CharacterData> allies)
        {
            HideAllTargets();
            buttonToCharacterMap.Clear();
            
            Debug.Log($"[TargetSelector3v3] Hiện {allies.Count} đồng đội để chọn...");
            
            foreach (var ally in allies)
            {
                if (ally == null || ally.isDead) continue;
                
                Button targetButton = GetButtonForCharacter(ally);
                
                if (targetButton != null)
                {
                    targetButton.gameObject.SetActive(true);
                    buttonToCharacterMap[targetButton] = ally;
                    Debug.Log($"  → {ally.characterName} (Team {ally.team}, Pos {ally.position})");
                }
            }
        }

        /// <summary>
        /// Ẩn tất cả target buttons
        /// </summary>
        public void HideAllTargets()
        {
            if (targetTeamA_Top != null) targetTeamA_Top.gameObject.SetActive(false);
            if (targetTeamA_Middle != null) targetTeamA_Middle.gameObject.SetActive(false);
            if (targetTeamA_Bottom != null) targetTeamA_Bottom.gameObject.SetActive(false);
            
            if (targetTeamB_Top != null) targetTeamB_Top.gameObject.SetActive(false);
            if (targetTeamB_Middle != null) targetTeamB_Middle.gameObject.SetActive(false);
            if (targetTeamB_Bottom != null) targetTeamB_Bottom.gameObject.SetActive(false);
            
            buttonToCharacterMap.Clear();
        }

        /// <summary>
        /// Get button tương ứng với character
        /// </summary>
        private Button GetButtonForCharacter(CharacterData character)
        {
            if (character.team == Team.TeamA)
            {
                switch (character.position)
                {
                    case Position.Top: return targetTeamA_Top;
                    case Position.Middle: return targetTeamA_Middle;
                    case Position.Bottom: return targetTeamA_Bottom;
                }
            }
            else if (character.team == Team.TeamB)
            {
                switch (character.position)
                {
                    case Position.Top: return targetTeamB_Top;
                    case Position.Middle: return targetTeamB_Middle;
                    case Position.Bottom: return targetTeamB_Bottom;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Xử lý khi click vào target button
        /// </summary>
        private void OnTargetButtonClicked(Button button)
        {
            if (!buttonToCharacterMap.ContainsKey(button))
            {
                Debug.LogWarning("Button không được map với character nào!");
                return;
            }
            
            CharacterData selectedTarget = buttonToCharacterMap[button];
            Debug.Log($"[TargetSelector3v3] Đã chọn: {selectedTarget.characterName}");
            
            // Ẩn tất cả buttons
            HideAllTargets();
            
            // Thông báo cho TurnManager
            if (turnManager != null)
            {
                turnManager.OnTargetSelected(selectedTarget);
            }
            else
            {
                Debug.LogWarning("turnManager chưa được gán!");
            }
        }

        /// <summary>
        /// Disable button cho character đã chết
        /// </summary>
        public void DisableDeadCharacterButton(CharacterData character)
        {
            Button button = GetButtonForCharacter(character);
            
            if (button != null)
            {
                button.interactable = false;
                
                // Visual effect - grey out
                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
            }
        }

        /// <summary>
        /// Re-enable button khi reset
        /// </summary>
        public void EnableAllButtons()
        {
            Button[] allButtons = new Button[] 
            {
                targetTeamA_Top, targetTeamA_Middle, targetTeamA_Bottom,
                targetTeamB_Top, targetTeamB_Middle, targetTeamB_Bottom
            };
            
            foreach (var button in allButtons)
            {
                if (button != null)
                {
                    button.interactable = true;
                    
                    var image = button.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = Color.white;
                    }
                }
            }
        }
    }
}
