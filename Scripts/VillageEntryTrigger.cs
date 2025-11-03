using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class VillageEntryTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string playerTag = "Player";
    
    private bool hasTriggered = false;
    private float lastValidationMessageTime = 0f;
    private float validationMessageCooldown = 3f; // 3 giây cooldown
    
    void Start()
    {
        // Đảm bảo collider là trigger
        GetComponent<Collider2D>().isTrigger = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        
        if (other.CompareTag(playerTag))
        {
            // Kiểm tra xem có thể làm nhiệm vụ này không
            if (QuestManager.Instance != null && QuestManager.Instance.CanDoQuest("Tìm đường vào làng"))
            {
                // Kiểm tra xem đây có phải quest hiện tại không
                if (QuestManager.Instance.IsCurrentQuest("Tìm đường vào làng"))
                {
                    hasTriggered = true;
                    
                    // Hoàn thành nhiệm vụ "Tìm đường vào làng"
                    QuestManager.CompleteCurrentQuest("Tìm đường vào làng");
                    
                    Debug.Log("Người chơi đã tìm thấy lối vào làng!");
                }
                else
                {
                    Debug.Log("Chưa đến lúc tìm đường vào làng. Hãy hoàn thành nhiệm vụ hiện tại trước.");
                }
            }
            else
            {
                // CHỈ hiển thị thông báo nếu đã đủ thời gian cooldown
                if (Time.time - lastValidationMessageTime >= validationMessageCooldown)
                {
                    QuestManager.Instance.ShowDependencyMessage("Tìm đường vào làng");
                    lastValidationMessageTime = Time.time;
                }
            }
        }
    }
    
    // Vẽ gizmo để dễ thấy trigger area trong Scene view
    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}