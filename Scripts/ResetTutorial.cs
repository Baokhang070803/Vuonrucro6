using UnityEngine;

/// <summary>
/// Script đơn giản để reset tutorial - Gắn vào 1 GameObject bất kỳ
/// Nhấn phím R trong game để reset
/// </summary>
public class ResetTutorial : MonoBehaviour
{
    void Update()
    {
        // Nhấn phím R để reset tutorial
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteKey("TutorialShown");
            PlayerPrefs.Save();
            Debug.Log("✅ ĐÃ RESET TUTORIAL! Play lại để xem tutorial từ đầu.");
        }
        
        // Nhấn phím T để xem trạng thái
        if (Input.GetKeyDown(KeyCode.T))
        {
            bool hasShown = PlayerPrefs.GetInt("TutorialShown", 0) == 1;
            Debug.Log($"Tutorial status: {(hasShown ? "ĐÃ XEM" : "CHƯA XEM")}");
        }
    }
    
    // Hoặc dùng trong Editor
    [ContextMenu("Reset Tutorial")]
    void ResetTutorialNow()
    {
        PlayerPrefs.DeleteKey("TutorialShown");
        PlayerPrefs.Save();
        Debug.Log("✅ ĐÃ RESET TUTORIAL!");
    }
}
