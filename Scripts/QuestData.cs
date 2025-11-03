using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data class để lưu trữ quest progress
/// </summary>
[System.Serializable]
public class QuestData
{
    public int currentQuestIndex;
    public List<Quest> questList;
    
    public QuestData()
    {
        currentQuestIndex = 0;
        questList = new List<Quest>();
    }
    
    public QuestData(int currentIndex, List<Quest> quests)
    {
        currentQuestIndex = currentIndex;
        questList = quests;
    }
}
