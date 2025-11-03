using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class ChuhiephoiGuide : MonoBehaviour
{
    [Header("Player")]
    public Transform player;                 // kéo nhân vật chính vào đây
    public float interactDistance = 1.5f;    // khoảng cách cần đứng gần

    [Header("Nội dung hội thoại")]
    [TextArea(2,5)] public string line1 = "Bé cưng, có bị thương không? Đừng lo, ta đã xử lý con slime đó rồi.";
    [TextArea(2,5)] public string line2 = "Ta là Scylla Campbell, chủ hiệp hội mạo hiểm của làng Hoa Rực.";
    [TextArea(2,5)] public string line3 = "Slime hiếm khi dám tiến gần làng… Có điều gì đó bất thường đang diễn ra.";
    [TextArea(2,5)] public string line4 = "Nguy hiểm vừa rồi thật sự khiến ta lo lắng cho bé.";
    [TextArea(2,5)] public string line5 = "Hãy theo ta về hiệp hội, ở đó an toàn hơn nhiều.";
    [TextArea(2,5)] public string line6 = "Trong hiệp hội, em sẽ học cách chiến đấu, nhận nhiệm vụ và gặp gỡ nhiều mạo hiểm giả khác.";
    [TextArea(2,5)] public string line7 = "Ta sẽ hướng dẫn để em trở thành một mạo hiểm giả thật sự, vững vàng hơn.";
    [TextArea(2,5)] public string line8 = "Hãy coi hiệp hội như mái nhà mới, nơi em có thể bắt đầu hành trình của mình.";
    [TextArea(2,5)] public string line9 = "Đi thôi nào, bé cưng. Ta sẽ đưa em về hiệp hội ngay bây giờ.";

    [Header("Video (Tùy chọn)")]
    public GameObject videoPanel; // Kéo panel chứa VideoPlayer vào đây (nếu có)
    public VideoPlayer videoPlayer; // Kéo VideoPlayer vào đây (nếu có)

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
            // Tự động tìm nhân vật chính
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
            return;
        }

        TryShowDialogue();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        // Xử lý ESC để tắt videoPanel
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
                _videoPlayed = true;
                
                // Hoàn thành nhiệm vụ khi người chơi tắt video (nếu có)
                QuestManager.CompleteCurrentQuest("Gặp chủ hiệp hội");
                hasInteracted = true;
                
                Debug.Log("Đã tắt video chủ hiệp hội!");
            }
        }

        if (hasInteracted) return;

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

        // Bắt click thủ công
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
                    if (hasInteracted) return;
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
        if (hasInteracted) return;
        if (DialogueManager.I == null) return;
        
        // Kiểm tra xem có thể làm nhiệm vụ này không
        if (QuestManager.Instance != null && QuestManager.Instance.CanDoQuest("Gặp chủ hiệp hội"))
        {
            // Kiểm tra xem đây có phải quest hiện tại không
            if (QuestManager.Instance.IsCurrentQuest("Gặp chủ hiệp hội"))
            {
                var lines = new List<string> { line1, line2, line3, line4, line5, line6, line7, line8, line9 };
                DialogueManager.I.Show(lines);

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
                QuestManager.Instance.ShowDependencyMessage("Gặp chủ hiệp hội");
                lastValidationMessageTime = Time.time;
            }
        }
    }

    void OnDialogueClosed()
    {
        // Luôn phát video sau khi hết hội thoại (nếu có video)
        if (!_videoPlayed && videoPanel != null && videoPlayer != null)
        {
            videoPanel.SetActive(true);
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;
            _videoPlayed = true;
            
            Debug.Log("Đang phát video của Scylla Campbell...");
        }
        else
        {
            // Không có video hoặc đã phát rồi, đánh dấu đã tương tác
            QuestManager.CompleteCurrentQuest("Gặp chủ hiệp hội");
            hasInteracted = true;
            Debug.Log("Đã hoàn thành hội thoại với Scylla Campbell!");
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

        if (vp != null)
        {
            vp.Stop();
            vp.frame = 0;
        }

        // Hoàn thành nhiệm vụ nếu cần
        QuestManager.CompleteCurrentQuest("Gặp chủ hiệp hội");
        
        hasInteracted = true;
        
        Debug.Log("Đã xem hết video chủ hiệp hội!");
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}