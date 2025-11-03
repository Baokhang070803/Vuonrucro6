using UnityEngine;

/// <summary>
/// Helper script để kiểm tra và debug player movement state
/// Đặc biệt hữu ích để verify rằng nhân vật dừng khi dialogue mở
/// </summary>
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Debug")]
    [SerializeField] private bool showMovementDebug = true;
    
    private nvnu1dituyen playerController;
    private Rigidbody2D playerRb;
    private Animator playerAnimator;
    
    void Start()
    {
        // Tìm player controller
        playerController = FindObjectOfType<nvnu1dituyen>();
        
        if (playerController != null)
        {
            playerRb = playerController.GetComponent<Rigidbody2D>();
            playerAnimator = playerController.GetComponent<Animator>();
        }
        
        if (showMovementDebug)
        {
            Debug.Log("=== PLAYER MOVEMENT CONTROLLER INITIALIZED ===");
            Debug.Log("Player sẽ tự động dừng khi dialogue/validation message hiển thị");
        }
    }
    
    void Update()
    {
        if (showMovementDebug && Input.GetKeyDown(KeyCode.F12))
        {
            ShowMovementStatus();
        }
    }
    
    void ShowMovementStatus()
    {
        Debug.Log("=== PLAYER MOVEMENT STATUS ===");
        
        // Kiểm tra dialogue state
        bool dialogueOpen = DialogueManager.IsDialogueOpen;
        Debug.Log($"Dialogue Open: {dialogueOpen}");
        
        // Kiểm tra player velocity
        if (playerRb != null)
        {
            Debug.Log($"Player Velocity: {playerRb.linearVelocity}");
            Debug.Log($"Player Position: {playerController.transform.position}");
        }
        
        // Kiểm tra animator parameters
        if (playerAnimator != null)
        {
            float horizontal = playerAnimator.GetFloat("Horizontal");
            float vertical = playerAnimator.GetFloat("Vertical");
            float speed = playerAnimator.GetFloat("Speed");
            
            Debug.Log($"Animator - Horizontal: {horizontal}, Vertical: {vertical}, Speed: {speed}");
        }
        
        // Kiểm tra quest state
        if (QuestManager.Instance != null)
        {
            var currentQuest = QuestManager.Instance.GetCurrentQuest();
            if (currentQuest != null)
            {
                Debug.Log($"Current Quest: {currentQuest.title}");
            }
        }
        
        // Kết luận
        if (dialogueOpen)
        {
            Debug.Log("✓ PLAYER SHOULD BE STOPPED (Dialogue is open)");
        }
        else
        {
            Debug.Log("✓ PLAYER CAN MOVE (No dialogue open)");
        }
    }
    
    void OnGUI()
    {
        if (!showMovementDebug) return;
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 200, 400, 180));
        GUILayout.Label("=== MOVEMENT DEBUG ===");
        
        // Hiển thị trạng thái dialogue
        bool dialogueOpen = DialogueManager.IsDialogueOpen;
        string dialogueStatus = dialogueOpen ? "OPEN (Player Stopped)" : "CLOSED (Player Can Move)";
        GUILayout.Label($"Dialogue: {dialogueStatus}");
        
        // Hiển thị velocity
        if (playerRb != null)
        {
            GUILayout.Label($"Velocity: {playerRb.linearVelocity.magnitude:F2}");
        }
        
        // Hiển thị quest hiện tại
        if (QuestManager.Instance != null)
        {
            var currentQuest = QuestManager.Instance.GetCurrentQuest();
            if (currentQuest != null)
            {
                GUILayout.Label($"Quest: {currentQuest.title}");
            }
        }
        
        GUILayout.Space(10);
        GUILayout.Label("F12 - Show Detailed Status");
        GUILayout.Label("F8 - Start First Quest");
        
        // Hiển thị trạng thái movement
        if (dialogueOpen)
        {
            GUI.color = Color.red;
            GUILayout.Label("🛑 PLAYER MOVEMENT BLOCKED");
        }
        else
        {
            GUI.color = Color.green;
            GUILayout.Label("✅ PLAYER CAN MOVE");
        }
        GUI.color = Color.white;
        
        GUILayout.EndArea();
    }
}