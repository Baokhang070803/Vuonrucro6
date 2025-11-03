using UnityEngine;
using TMPro;

public class ShowFirstHint : MonoBehaviour
{
    public TextMeshProUGUI hintText; // Kéo Text (TMP) vào đây
    public Transform player;        // Kéo nhân vật vào đây (hoặc tự tìm theo tag)
    public string message = "Hình như phía trước có ai đó hãy đến xem thử ..."; // Tin nhắn hiển thị
    public GameObject hintBackground; // Kéo Image nền vào đây
    
    [Header("Audio")]
    public AudioClip hintSound; // Âm thanh phát khi hiện hint
    [Range(0f, 1f)] public float hintVolume = 1f; // Âm lượng
    private AudioSource audioSource; // Nguồn phát âm thanh

    private Vector3 lastPos;
    private bool shown = false;
    private float showTimer = 0f;
    private bool isShowing = false;

    void Start()
    {
        Debug.Log("[ShowFirstHint] Script đã khởi tạo!");
        
        if (hintText != null)
        {
            hintText.gameObject.SetActive(false);
            Debug.Log("[ShowFirstHint] hintText đã được gán và ẩn");
        }
        else
        {
            Debug.LogError("[ShowFirstHint] hintText là NULL! Vui lòng gán TextMeshProUGUI trong Inspector!");
        }
            
        if (hintBackground != null)
        {
            hintBackground.SetActive(false);
            Debug.Log("[ShowFirstHint] hintBackground đã được gán và ẩn");
        }
        else
        {
            Debug.LogError("[ShowFirstHint] hintBackground là NULL! Vui lòng gán GameObject trong Inspector!");
        }

        // Đảm bảo Canvas luôn hiển thị trên cùng
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvas.overrideSorting = true;
        }

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                player = go.transform;
                Debug.Log("[ShowFirstHint] Đã tìm thấy player qua tag: " + go.name);
            }
            else
            {
                Debug.LogError("[ShowFirstHint] Không tìm thấy GameObject với tag 'Player'!");
            }
        }
        else
        {
            Debug.Log("[ShowFirstHint] Player đã được gán: " + player.name);
        }
        
        if (player != null)
        {
            lastPos = player.position;
            Debug.Log($"[ShowFirstHint] Player position: {lastPos}");
        }

        // Chuẩn bị AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (shown || player == null) return;

        float moved = (player.position - lastPos).magnitude;
        lastPos = player.position;

        if (!isShowing && moved > 0.01f)
        {
            // Nhân vật vừa nhúc nhích lần đầu
            Debug.Log($"[ShowFirstHint] Player đã di chuyển! Distance moved: {moved}");
            Debug.Log($"[ShowFirstHint] Hiển thị thông báo: {message}");
            
            if (hintText != null)
            {
                hintText.text = message;
                hintText.gameObject.SetActive(true);
                Debug.Log("[ShowFirstHint] hintText đã được hiển thị!");
            }
            else
            {
                Debug.LogError("[ShowFirstHint] hintText là NULL! Không thể hiển thị thông báo!");
            }
            
            if (hintBackground != null)
            {
                hintBackground.SetActive(true);
                Debug.Log("[ShowFirstHint] hintBackground đã được hiển thị!");
            }
            else
            {
                Debug.LogError("[ShowFirstHint] hintBackground là NULL! Không có nền cho thông báo!");
            }

            // Phát âm thanh nếu có cấu hình
            if (hintSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hintSound, hintVolume);
                Debug.Log("[ShowFirstHint] Đã phát âm thanh hint!");
            }
            else
            {
                Debug.LogWarning("[ShowFirstHint] Không có âm thanh hint hoặc AudioSource!");
            }

            isShowing = true;
            showTimer = 4f; // 4 giây
            Debug.Log("[ShowFirstHint] Bắt đầu đếm thời gian hiển thị: 4 giây");
        }

        if (isShowing)
        {
            showTimer -= Time.deltaTime;
            if (showTimer <= 0f)
            {
                if (hintText != null)
                    hintText.gameObject.SetActive(false);
                if (hintBackground != null)
                    hintBackground.SetActive(false);
                shown = true;
            }
        }
    }
}
