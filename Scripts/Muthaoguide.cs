using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Video; // Thêm dòng này

[RequireComponent(typeof(Collider2D))]
public class Muthaoguide : MonoBehaviour
{
    [Header("Player")]
    public Transform player;                 // kéo Asset 34@2x_0 vào đây
    public float interactDistance = 1.5f;    // khoảng cách cần đứng gần

    [Header("Nội dung hướng dẫn")]
    [TextArea(2,5)] public string line1 = "Chào con, ta là Mụ Thảo. Làng Hoa Rực từng rực rỡ nhờ Cây Pha Lê ở trung tâm.";
    [TextArea(2,5)] public string line2 = "Mười năm trước, Lời nguyền 'Ghen Sắc' làm cây vỡ thành nhiều mảnh, sinh trưởng trở nên trì trệ.";
    [TextArea(2,5)] public string line3 = "Giờ đây, ta là người duy nhất biết cách khôi phục Cây Pha Lê.";
    [TextArea(2,5)] public string line4 = "Đây là ký ức của ta về sự kiện đó, sau khi coi xong con hãy dọn cỏ bên kia và gieo dùm ta 10 hạt giống.";

    [Header("Video")]
    public GameObject videoPanel; // Kéo panel chứa VideoPlayer vào đây
    public VideoPlayer videoPlayer; // Kéo VideoPlayer vào đây (có thể nằm trong videoPanel)

    Camera _cam;
    Collider2D _colliderOnSelf;
    bool _hasShown;
    private bool _videoPlayed = false;
    private bool hasInteracted = false; // chỉ cho phép 1 lần
    private float lastValidationMessageTime = 0f;
    private float validationMessageCooldown = 3f; // 3 giây cooldown

    void Start()
    {
        _cam = Camera.main;
        _colliderOnSelf = GetComponent<Collider2D>();
        if (_colliderOnSelf == null)
        {
            // Đảm bảo có collider để nhận click
            _colliderOnSelf = gameObject.AddComponent<BoxCollider2D>();
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Fit collider theo sprite bounds
                var box = _colliderOnSelf as BoxCollider2D;
                if (box != null)
                {
                    box.size = sr.bounds.size;
                }
            }
        }
        if (player == null)
        {
            var p = GameObject.Find("Asset 34@2x_0");
            if (p) player = p.transform;
        }
        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }

        // Đăng ký sự kiện để ẩn panel khi video kết thúc
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void OnMouseDown()
    {
        if (hasInteracted) return; // Đã tương tác rồi thì bỏ qua
        if (!player) return;

        float d = Vector2.Distance(player.position, transform.position);
        if (d > interactDistance)
        {
            // Có thể hiện 1 balloon nhỏ: "Lại gần hơn nhé!"
            return;
        }

        TryShowDialogue();
    }

    void Update()
    {
        // Khai báo keyboard 1 lần đầu hàm
        var keyboard = Keyboard.current;
        // Xử lý ESC để tắt videoPanel luôn hoạt động, không phụ thuộc hasInteracted
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (videoPanel != null && videoPanel.activeSelf)
            {
                if (videoPlayer != null)
                {
                    videoPlayer.Stop();
                    videoPlayer.frame = 0;
                }
                videoPanel.SetActive(false);
                _videoPlayed = true; // mark as played/closed
                
                // Hoàn thành nhiệm vụ khi người chơi tắt video (coi như đã xem)
                QuestManager.CompleteCurrentQuest("Gặp Mụ Thảo");
                hasInteracted = true;
                
                Debug.Log("Đã tắt video và hoàn thành nhiệm vụ gặp Mụ Thảo!");
            }
        }

        if (hasInteracted) return; // Đã tương tác rồi thì bỏ qua các thao tác khác

        // Tự hiện khi lại gần
        if (!_hasShown && player != null)
        {
            float d = Vector2.Distance(player.position, transform.position);
            if (d <= interactDistance)
            {
                TryShowDialogue();
                _hasShown = true;
            }
        }

        // Bắt click thủ công để vẫn hoạt động nếu collider nằm ở child
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mouseWorld = _cam != null ? _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()) : Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 p2 = new Vector2(mouseWorld.x, mouseWorld.y);
            var hits = Physics2D.OverlapPointAll(p2);
            foreach (var h in hits)
            {
                if (h == null) continue;
                if (h.transform == transform || h.transform.IsChildOf(transform))
                {
                    if (hasInteracted) return; // Đã tương tác rồi thì bỏ qua
                    if (!player) return;
                    float d = Vector2.Distance(player.position, transform.position);
                    if (d > interactDistance) return;
                    TryShowDialogue();
                    _hasShown = true;
                    break;
                }
            }
        }
    }

    void TryShowDialogue()
    {
        if (hasInteracted) return; // Đã tương tác rồi thì bỏ qua
        if (DialogueManager.I == null) return;
        
        // Kiểm tra xem có thể làm nhiệm vụ này không
        if (QuestManager.Instance != null && QuestManager.Instance.CanDoQuest("Gặp Mụ Thảo"))
        {
            // Kiểm tra xem đây có phải quest hiện tại không
            if (QuestManager.Instance.IsCurrentQuest("Gặp Mụ Thảo"))
            {
                var lines = new List<string> { line1, line2, line3, line4 };
                DialogueManager.I.Show(lines);

                // KHÔNG hoàn thành nhiệm vụ ngay, chờ xem hết video
                // QuestManager.CompleteCurrentQuest("Gặp Mụ Thảo");

                // Đăng ký callback khi hội thoại đóng
                DialogueManagerWithCallback.RegisterOnClose(OnDialogueClosed);
            }
            else
            {
                DialogueManager.I.Show(new List<string> { "Hãy hoàn thành nhiệm vụ hiện tại trước khi nói chuyện với ta." });
            }
        }
        else
        {
            // CHỈ hiển thị thông báo nếu đã đủ thời gian cooldown
            if (Time.time - lastValidationMessageTime >= validationMessageCooldown)
            {
                QuestManager.Instance.ShowDependencyMessage("Gặp Mụ Thảo");
                lastValidationMessageTime = Time.time;
            }
        }
    }

    void OnDialogueClosed()
    {
        // Phát video nếu chưa phát lần nào
        if (!_videoPlayed && videoPanel != null && videoPlayer != null)
        {
            videoPanel.SetActive(true);
            videoPlayer.Play();
            
            // Đăng ký sự kiện khi video kết thúc
            videoPlayer.loopPointReached += OnVideoFinished;
            _videoPlayed = true;
        }
        else
        {
            // Nếu không có video hoặc đã xem rồi, đánh dấu đã tương tác
            hasInteracted = true;
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    void OnVideoFinished(UnityEngine.Video.VideoPlayer vp)
    {
        if (videoPanel != null)
            videoPanel.SetActive(false);

        // Dừng và reset video nếu cần
        if (vp != null)
        {
            vp.Stop();
            vp.frame = 0;
        }

        // Hoàn thành nhiệm vụ "Gặp Mụ Thảo" sau khi xem hết video
        QuestManager.CompleteCurrentQuest("Gặp Mụ Thảo");
        
        // Đánh dấu đã tương tác xong
        hasInteracted = true;
        
        Debug.Log("Đã xem hết video và hoàn thành nhiệm vụ gặp Mụ Thảo!");
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
        


