using UnityEngine;
using TMPro;
using System.Collections;

public class NPCAutoTalk : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textUI;

    [Header("Timing")]
    public float interval = 6f;        // lặp lại mỗi 6s
    public float showDuration = 3f;  // thời gian hiện 3s balloon

    [Header("Content")]
    [TextArea] public string message = "Xin chào ! Anh hùng, xin hãy cứu lấy ngôi làng hoa rực";

    void OnEnable()
    {
        StartCoroutine(TalkLoop());
    }

    IEnumerator TalkLoop()
    {
        while (true)
        {
            if (textUI != null) textUI.text = message;     // hiện lời
            yield return new WaitForSeconds(showDuration);   // chờ X giây

            if (textUI != null) textUI.text = "";           // ẩn lời
            float wait = Mathf.Max(0.01f, interval - showDuration);
            yield return new WaitForSeconds(wait);           // chờ đến đủ 3s
        }
    }
}
