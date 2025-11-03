using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SlimeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 2f;          // Phạm vi tấn công
    public Transform playerTarget;          // Kéo nhân vật nữ vào đây
    public string playerTag = "Player";     // Tag của người chơi (backup nếu không kéo playerTarget)
    
    [Tooltip("Scene sẽ load khi Slime tấn công. MẶC ĐỊNH: intro7 (video intro) → Loading → pkchuong6 (combat)")]
    public string battleSceneName = "intro7"; // Scene intro video trước combat
    
    [Header("⚠️ LƯU Ý: Nếu muốn skip video intro, đổi thành 'pkchuong6' hoặc 'giaodienpk'")]
    
    [Header("Attack Behavior")]
    public float attackCooldown = 1f;       // Thời gian hồi chiêu tấn công
    public bool canAttack = true;
    
    [Header("Detection Method")]
    [Tooltip("Chọn cách phát hiện player: true = dùng Distance, false = dùng Trigger")]
    public bool useDistanceCheck = true;    // MẶC ĐỊNH: dùng distance check
    
    private Transform player;
    private bool hasAttacked = false;
    private SlimeRandomJump jumpScript;
    private float lastValidationMessageTime = 0f;
    private float validationMessageCooldown = 3f; // 3 giây cooldown giữa các thông báo
    
    void Start()
    {
        // Reset trạng thái khi load scene
        hasAttacked = false;
        canAttack = true;
        
        // Kiểm tra QuestManager
        if (QuestManager.Instance == null)
        {
            Debug.LogError("❌❌❌ CẢNH BÁO: QuestManager.Instance là NULL! Slime sẽ KHÔNG CHO PHÉP combat!");
        }
        else
        {
            Debug.Log("✅ QuestManager tồn tại. Slime sẽ kiểm tra quest trước khi combat.");
            var currentQuest = QuestManager.Instance.GetCurrentQuest();
            if (currentQuest != null)
            {
                Debug.Log($"📋 Quest hiện tại: {currentQuest.title}");
            }
        }
        
        // Ưu tiên sử dụng playerTarget nếu đã kéo vào
        if (playerTarget != null)
        {
            player = playerTarget;
            Debug.Log($"✅ Player target đã được gán: {playerTarget.name}");
        }
        else
        {
            // Tìm người chơi theo tag nếu chưa kéa vào
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"✅ Tìm thấy player theo tag: {playerObj.name}");
            }
            else
            {
                Debug.LogError($"❌ Không tìm thấy player với tag '{playerTag}'!");
            }
        }
        
        // Lấy script nhảy
        jumpScript = GetComponent<SlimeRandomJump>();
        
        Debug.Log($"🎮 SlimeAttack Start - hasAttacked: {hasAttacked}, canAttack: {canAttack}, useDistanceCheck: {useDistanceCheck}, attackRange: {attackRange}m");
    }
    
    System.Collections.IEnumerator EnableAttackAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        canAttack = true;
        Debug.Log("Slime attack đã được bật lại");
    }
    
    void Update()
    {
        // CHỈ check khi bật distance check
        if (!useDistanceCheck) return;
        
        if (player == null || hasAttacked || !canAttack) return;
        
        CheckPlayerInRange();
    }
    
    void CheckPlayerInRange()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        
        // Debug log để theo dõi
        if (distance <= attackRange * 1.5f) // Log khi player gần
        {
            Debug.Log($"🔍 Player cách Slime: {distance:F2}m (Attack range: {attackRange}m)");
        }
        
        if (distance <= attackRange)
        {
            Debug.Log("⚠️ Player trong tầm tấn công! Bắt đầu kiểm tra điều kiện...");
            StartAttack();
        }
    }
    
    void StartAttack()
    {
        // Kiểm tra quest dependency trước khi cho phép combat
        if (!CanStartCombat())
        {
            return; // Không cho phép combat
        }
        
        hasAttacked = true;
        
        Debug.Log("Slime tấn công người chơi! Chuyển sang scene chiến đấu...");
        
        // Dừng slime nhảy
        if (jumpScript != null)
        {
            jumpScript.enabled = false;
        }
        
        // Có thể thêm hiệu ứng tấn công ở đây
        StartCoroutine(AttackSequence());
    }
    
    bool CanStartCombat()
    {
        // ⚠️ QUAN TRỌNG: Kiểm tra QuestManager tồn tại
        if (QuestManager.Instance == null)
        {
            Debug.LogError("❌ QuestManager không tồn tại! KHÔNG CHO PHÉP COMBAT!");
            return false;
        }
        
        // Kiểm tra xem có thể làm quest đánh slime không
        if (QuestManager.Instance.CanDoQuest("Trận chiến cuối cùng"))
        {
            // Kiểm tra xem đây có phải quest hiện tại không
            if (QuestManager.Instance.IsCurrentQuest("Trận chiến cuối cùng"))
            {
                Debug.Log("✅ Đủ điều kiện bắt đầu combat!");
                return true;
            }
            else
            {
                // CHỈ hiển thị thông báo nếu đã đủ thời gian cooldown
                if (Time.time - lastValidationMessageTime >= validationMessageCooldown)
                {
                    Debug.Log("⚠️ Chưa đến lúc bắt đầu trận chiến cuối. Hãy hoàn thành nhiệm vụ hiện tại trước.");
                    if (DialogueManager.I != null)
                    {
                        DialogueManager.I.Show(new List<string> { "Hãy hoàn thành nhiệm vụ hiện tại trước khi bắt đầu trận chiến cuối cùng!" });
                    }
                    lastValidationMessageTime = Time.time;
                }
                
                // Reset hasAttacked để có thể thử lại sau
                StartCoroutine(ResetAttackAfterDelay());
                return false;
            }
        }
        else
        {
            // CHỈ hiển thị thông báo nếu đã đủ thời gian cooldown
            if (Time.time - lastValidationMessageTime >= validationMessageCooldown)
            {
                Debug.Log("⚠️ Bạn cần hoàn thành các nhiệm vụ trước đó mới có thể bắt đầu trận chiến cuối cùng!");
                
                // Hiển thị thông báo dependency cụ thể
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.ShowDependencyMessage("Trận chiến cuối cùng");
                }
                
                lastValidationMessageTime = Time.time;
            }
            
            // Reset hasAttacked để có thể thử lại sau
            StartCoroutine(ResetAttackAfterDelay());
            return false;
        }
    }
    
    IEnumerator ResetAttackAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Chờ 2 giây trước khi reset
        hasAttacked = false; // Cho phép thử lại
        Debug.Log("Slime attack đã được reset, có thể thử lại khi đủ điều kiện quest.");
    }
    
    IEnumerator AttackSequence()
    {
        // Hiệu ứng tấn công (có thể thêm animation, sound, v.v.)
        Debug.Log("Slime chuẩn bị tấn công...");
        
        // Có thể thêm hiệu ứng flash hoặc animation tấn công
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Flash đỏ để báo hiệu tấn công
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.3f);
            sr.color = originalColor;
        }
        
        // Chuyển scene
        LoadBattleScene();
    }
    
    void LoadBattleScene()
    {
        // Log để debug
        Debug.Log($"[SlimeAttack] Đang chuẩn bị load scene: '{battleSceneName}'");
        
        // Lưu vị trí player trước khi chuyển scene - PHIÊN BẢN ĐƠN GIẢN
        if (player != null)
        {
            PlayerPrefs.SetFloat("SavedPlayerX", player.position.x);
            PlayerPrefs.SetFloat("SavedPlayerY", player.position.y);
            PlayerPrefs.SetFloat("SavedPlayerZ", player.position.z);
            Debug.Log($"Đã lưu vị trí player trước combat: {player.position}");
        }
        
        // Kiểm tra scene có tồn tại trong Build Settings không
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == battleSceneName)
            {
                sceneExists = true;
                break;
            }
        }
        
        if (!sceneExists)
        {
            Debug.LogError($"❌ Scene '{battleSceneName}' KHÔNG TỒN TẠI trong Build Settings!");
            Debug.LogError($"Vui lòng thêm scene '{battleSceneName}' vào File → Build Profiles!");
            
            if (DialogueManager.I != null)
            {
                DialogueManager.I.Show(new List<string> 
                { 
                    $"LỖI: Scene '{battleSceneName}' chưa được thêm vào Build Settings!",
                    "Vui lòng kiểm tra Unity Editor."
                });
            }
            return;
        }
        
        try
        {
            Debug.Log($"✅ Đang load scene '{battleSceneName}'...");
            SceneManager.LoadScene(battleSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Không thể load scene '{battleSceneName}': {e.Message}");
            Debug.LogError("Đảm bảo scene đã được thêm vào Build Settings!");
        }
    }
    
    // Phương thức để reset trạng thái tấn công (nếu cần)
    public void ResetAttack()
    {
        hasAttacked = false;
        canAttack = true;
        
        if (jumpScript != null)
        {
            jumpScript.enabled = true;
        }
    }
    
    // Vẽ gizmo để hiển thị phạm vi tấn công
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    // Trigger alternative - nếu muốn dùng Collider trigger thay vì distance check
    void OnTriggerEnter2D(Collider2D other)
    {
        // CHỈ hoạt động khi TẮT distance check
        if (useDistanceCheck) return;
        
        // Kiểm tra xem có phải player target hoặc có đúng tag không
        bool isPlayer = (playerTarget != null && other.transform == playerTarget) || 
                       other.CompareTag(playerTag);
                       
        if (isPlayer && !hasAttacked && canAttack)
        {
            Debug.Log("🎯 OnTriggerEnter2D - Player vào vùng trigger của Slime");
            StartAttack(); // StartAttack() đã có validation bên trong rồi
        }
    }
}