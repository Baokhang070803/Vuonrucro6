using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Quest
{
    public string title;
    public string description;
    public bool isCompleted;
    
    public Quest(string title, string description)
    {
        this.title = title;
        this.description = description;
        this.isCompleted = false;
    }
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    
    [Header("Quest Audio")]
    public AudioClip quest2StartVoice; // Giọng nói khi bắt đầu nhiệm vụ 2
    [Range(0f, 1f)] public float quest2StartVolume = 1f; // Âm lượng giọng nói
    private AudioSource questAudioSource; // Nguồn phát âm thanh cho quest
    
    [Header("UI Quest Panel")]
    public GameObject questPanel;
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;
    
    [Header("Quests")]
    public List<Quest> questList = new List<Quest>();
    public int currentQuestIndex = 0; // Đổi từ private thành public để GameStateManager có thể truy cập
    
    [Header("UI Settings")]
    public bool showQuestPanelOnStart = false; // Tắt hiển thị tự động khi chuyển scene
    
    [Header("Debug Settings")]
    public bool enableDebugLogs = false; // Bật/tắt debug logs
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeQuests();
        UpdateQuestUI();
        
        // Ẩn bảng nhiệm vụ khi bắt đầu nếu không muốn hiển thị
        if (!showQuestPanelOnStart && questPanel != null)
        {
            questPanel.SetActive(false);
        }
        
        // Chuẩn bị AudioSource cho quest audio
        questAudioSource = GetComponent<AudioSource>();
        if (questAudioSource == null)
        {
            questAudioSource = gameObject.AddComponent<AudioSource>();
            questAudioSource.playOnAwake = false;
        }
    }
    
    void InitializeQuests()
    {
        questList.Clear();
        questList.Add(new Quest("Gặp Mụ Thảo", "Nói chuyện với Mụ Thảo để tìm hiểu về tình hình làng"));
        questList.Add(new Quest("Những Hạt Mầm Đầu Tiên", "Thu hoạch 10 cây đầu tiên, ấn C để dọn cỏ, ấn V để gieo hạt và ấn M để thu hoạch"));
        questList.Add(new Quest("Tìm đường vào làng", "Khám phá và tìm lối vào làng, hình như kế Mụ Thảo có một con đường mòn"));
        questList.Add(new Quest("Trận chiến cuối cùng", "Đánh bại Mụ Thảo - trận chiến quyết định tại trung tâm"));
        
        // Phát âm thanh giọng nói khi bắt đầu nhiệm vụ 2 (nếu đã hoàn thành nhiệm vụ 1)
        if (currentQuestIndex == 1 && quest2StartVoice != null && questAudioSource != null)
        {
            questAudioSource.PlayOneShot(quest2StartVoice, quest2StartVolume);
            Debug.Log("[QuestManager] Đã phát âm thanh bắt đầu nhiệm vụ 2 khi khởi tạo!");
        }
    }
    
    public void CompleteQuest(string questTitle)
    {
        if (currentQuestIndex < questList.Count)
        {
            Quest currentQuest = questList[currentQuestIndex];
            if (currentQuest.title == questTitle && !currentQuest.isCompleted)
            {
                currentQuest.isCompleted = true;
                Debug.Log($"Hoàn thành nhiệm vụ: {questTitle}");
                
                // THÊM EXP TỪ QUEST
                if (PlayerExpManager.Instance != null)
                {
                    PlayerExpManager.Instance.AddQuestExp();
                }
                
                // Chuyển sang nhiệm vụ tiếp theo
                currentQuestIndex++;
                
                // Phát âm thanh giọng nói khi bắt đầu nhiệm vụ 2
                if (currentQuestIndex == 1 && quest2StartVoice != null && questAudioSource != null)
                {
                    questAudioSource.PlayOneShot(quest2StartVoice, quest2StartVolume);
                    Debug.Log("[QuestManager] Đã phát âm thanh bắt đầu nhiệm vụ 2!");
                }
                
                UpdateQuestUI();
                
                // Hiển thị thông báo hoàn thành
                if (DialogueManager.I != null)
                {
                    DialogueManager.I.Show(new List<string> { $"✓ Hoàn thành: {questTitle}" });
                }
                
                // Tự động lưu quest data vào Firebase
                SaveQuestDataToFirebase();
            }
        }
    }
    
    // Kiểm tra xem có thể làm nhiệm vụ này không (tất cả nhiệm vụ trước đã hoàn thành)
    public bool CanDoQuest(string questTitle)
    {
        // Tìm index của quest cần kiểm tra
        int questIndex = -1;
        for (int i = 0; i < questList.Count; i++)
        {
            if (questList[i].title == questTitle)
            {
                questIndex = i;
                break;
            }
        }
        
        if (questIndex == -1)
        {
            Debug.LogWarning($"Không tìm thấy quest: {questTitle}");
            return false;
        }
        
        // Kiểm tra tất cả quest trước đó đã hoàn thành chưa
        for (int i = 0; i < questIndex; i++)
        {
            if (!questList[i].isCompleted)
            {
                // Chỉ log khi enable debug
                if (enableDebugLogs)
                {
                    Debug.Log($"Không thể làm quest '{questTitle}' vì chưa hoàn thành quest '{questList[i].title}'");
                }
                return false;
            }
        }
        
        return true;
    }
    
    // Kiểm tra quest hiện tại có phải là quest được chỉ định không
    public bool IsCurrentQuest(string questTitle)
    {
        Quest currentQuest = GetCurrentQuest();
        return currentQuest != null && currentQuest.title == questTitle;
    }
    
    // Hiển thị thông báo dependency với tên quest cần hoàn thành trước
    public void ShowDependencyMessage(string questTitle)
    {
        // Tìm quest cần hoàn thành trước
        int questIndex = -1;
        for (int i = 0; i < questList.Count; i++)
        {
            if (questList[i].title == questTitle)
            {
                questIndex = i;
                break;
            }
        }
        
        if (questIndex > 0)
        {
            // Tìm quest chưa hoàn thành đầu tiên
            for (int i = 0; i < questIndex; i++)
            {
                if (!questList[i].isCompleted)
                {
                    string message = $"Bạn cần hoàn thành nhiệm vụ '{questList[i].title}' trước!";
                    Debug.Log(message);
                    
                    if (DialogueManager.I != null)
                    {
                        // Hiển thị thông báo và nhân vật sẽ tự động dừng di chuyển
                        var messages = new List<string> 
                        { 
                            message,
                            "Nhấn Space hoặc Enter để đóng thông báo."
                        };
                        DialogueManager.I.Show(messages);
                    }
                    return;
                }
            }
        }
        
        // Nếu không có quest dependency, hiển thị thông báo chung
        Quest currentQuest = GetCurrentQuest();
        if (currentQuest != null)
        {
            string message = $"Hãy hoàn thành nhiệm vụ '{currentQuest.title}' trước!";
            if (DialogueManager.I != null)
            {
                var messages = new List<string> 
                { 
                    message,
                    "Nhấn Space hoặc Enter để đóng thông báo."
                };
                DialogueManager.I.Show(messages);
            }
        }
    }
    
    public Quest GetCurrentQuest()
    {
        if (currentQuestIndex < questList.Count)
        {
            return questList[currentQuestIndex];
        }
        return null;
    }
    
    void UpdateQuestUI()
    {
        Quest currentQuest = GetCurrentQuest();
        
        if (currentQuest != null)
        {
            if (questTitleText != null)
                questTitleText.text = currentQuest.title;
            if (questDescriptionText != null)
                questDescriptionText.text = currentQuest.description;
            
            // Không tự động hiện bảng quest khi update
            // Người chơi sẽ tự bấm phím để hiện/ẩn
        }
        else
        {
            // Hết nhiệm vụ
            if (questTitleText != null)
                questTitleText.text = "Tất cả nhiệm vụ đã hoàn thành!";
            if (questDescriptionText != null)
                questDescriptionText.text = "Cảm ơn bạn đã cứu làng!";
        }
    }
    
    // Phương thức để hiện/ẩn bảng nhiệm vụ
    public void ToggleQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(!questPanel.activeSelf);
        }
    }
    
    public void ShowQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(true);
        }
    }
    
    public void HideQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
    }
    
    // Gọi từ script khác để hoàn thành nhiệm vụ
    public static void CompleteCurrentQuest(string questTitle = "")
    {
        if (Instance != null)
        {
            if (string.IsNullOrEmpty(questTitle))
            {
                Quest current = Instance.GetCurrentQuest();
                if (current != null)
                    questTitle = current.title;
            }
            Instance.CompleteQuest(questTitle);
        }
    }
    
    /// <summary>
    /// Lưu quest data vào Firebase
    /// </summary>
    public void SaveQuestDataToFirebase()
    {
        if (PlayerDataSyncManager.Instance != null)
        {
            QuestData questData = new QuestData();
            questData.currentQuestIndex = currentQuestIndex;
            questData.questList = questList;
            
            PlayerDataSyncManager.Instance.UpdateQuestData(questData);
            Debug.Log("[QuestManager] Đã lưu quest data vào Firebase!");
        }
    }
    
    /// <summary>
    /// Load quest data từ Firebase
    /// </summary>
    public void LoadQuestDataFromFirebase(QuestData questData)
    {
        if (questData != null)
        {
            currentQuestIndex = questData.currentQuestIndex;
            questList = questData.questList;
            UpdateQuestUI();
            Debug.Log($"[QuestManager] Đã load quest data từ Firebase! Current quest: {currentQuestIndex}");
        }
    }
    
}