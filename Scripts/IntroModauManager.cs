using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class IntroModauManager : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    
    [Header("Scene Settings")]
    public string nextSceneName = "map1";
    public bool allowSkip = false;
    public float skipDelayTime = 2f; // Chờ 2 giây trước khi cho phép skip
    
    [Header("Skip Intro Settings")]
    public bool checkIfWatchedBefore = false; // Luôn phát intro, không kiểm tra đã xem
    public string introWatchedKey = "IntroVideoWatched"; // Key để lưu trạng thái đã xem
    
    private bool hasStarted = false;
    private float startTime;

    void Start()
    {
        // Luôn phát intro, không auto-skip dù đã xem trước đó

        // Tìm VideoPlayer nếu chưa được gán
        if (videoPlayer == null)
        {
            videoPlayer = FindObjectOfType<VideoPlayer>();
            if (videoPlayer == null)
            {
                Debug.LogError("Không tìm thấy VideoPlayer trong scene!");
                LoadNextScene(); // Nếu không có video thì chuyển scene luôn
                return;
            }
        }

        // Đăng ký sự kiện khi video kết thúc
        videoPlayer.loopPointReached += OnVideoEnd;
        
        // Bắt đầu phát video
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
        
        startTime = Time.time;
        hasStarted = true;
        
        Debug.Log("IntroModau đã bắt đầu phát video");
    }

    void Update()
    {
        if (!hasStarted) return;
        
        // Cho phép skip video bằng phím
        if (allowSkip && Time.time - startTime >= skipDelayTime)
        {
            CheckSkipInput();
        }
    }

    void CheckSkipInput()
    {
        // Skip bằng Space, Enter, hoặc Escape
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SkipVideo();
            }
        }
    }

    void SkipVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            Debug.Log("Video đã được skip");
        }
        
        // Đánh dấu đã xem intro khi skip
        MarkIntroAsWatched();
        
        LoadNextScene();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video intro đã kết thúc, chuyển sang scene: " + nextSceneName);
        
        // Đánh dấu đã xem intro
        MarkIntroAsWatched();
        
        LoadNextScene();
    }

    void LoadNextScene()
    {
        // Chuyển trực tiếp sang map1 thay vì qua Loading lần nữa
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Kiểm tra xem đã xem intro chưa
    /// </summary>
    bool HasWatchedIntro()
    {
        // Kiểm tra từ PlayerPrefs (local)
        bool localWatched = PlayerPrefs.GetInt(introWatchedKey, 0) == 1;
        
        // Kiểm tra từ Firebase nếu có
        bool firebaseWatched = false;
        if (LoadDataManager.userInGame != null)
        {
            // Có thể thêm logic kiểm tra từ Firebase data
            // Hiện tại chỉ dùng PlayerPrefs
        }
        
        bool hasWatched = localWatched || firebaseWatched;
        Debug.Log($"[IntroModauManager] Đã xem intro: Local={localWatched}, Firebase={firebaseWatched}, Kết quả={hasWatched}");
        
        return hasWatched;
    }
    
    /// <summary>
    /// Đánh dấu đã xem intro
    /// </summary>
    void MarkIntroAsWatched()
    {
        // Lưu vào PlayerPrefs
        PlayerPrefs.SetInt(introWatchedKey, 1);
        PlayerPrefs.Save();
        
        // Lưu vào Firebase nếu có
        if (PlayerDataSyncManager.Instance != null)
        {
            PlayerDataSyncManager.Instance.UpdateTutorialProgress(true);
            Debug.Log("[IntroModauManager] Đã lưu trạng thái xem intro vào Firebase!");
        }
        
        Debug.Log("[IntroModauManager] Đã đánh dấu đã xem intro!");
    }
    
    /// <summary>
    /// Reset trạng thái xem intro (để test)
    /// </summary>
    [ContextMenu("Reset Intro Watched Status")]
    public void ResetIntroWatchedStatus()
    {
        // Reset PlayerPrefs
        PlayerPrefs.DeleteKey(introWatchedKey);
        PlayerPrefs.DeleteKey("TutorialShown"); // Reset cả tutorial key
        PlayerPrefs.DeleteKey("SimpleTutorialShown"); // Reset cả simple tutorial
        PlayerPrefs.Save();
        
        // Reset Firebase
        if (PlayerDataSyncManager.Instance != null)
        {
            PlayerDataSyncManager.Instance.UpdateTutorialProgress(false);
        }
        
        Debug.Log("[IntroModauManager] Đã reset HOÀN TOÀN trạng thái xem intro và tutorial!");
        Debug.Log("[IntroModauManager] Tài khoản mới sẽ xem intro từ đầu!");
    }
    
    /// <summary>
    /// Test method - đánh dấu đã xem intro
    /// </summary>
    [ContextMenu("Mark Intro As Watched")]
    public void TestMarkIntroAsWatched()
    {
        MarkIntroAsWatched();
    }
    
    /// <summary>
    /// Debug: Kiểm tra trạng thái intro
    /// </summary>
    [ContextMenu("Debug Intro Status")]
    public void DebugIntroStatus()
    {
        bool localWatched = PlayerPrefs.GetInt(introWatchedKey, 0) == 1;
        bool tutorialShown = PlayerPrefs.GetInt("TutorialShown", 0) == 1;
        bool simpleTutorialShown = PlayerPrefs.GetInt("SimpleTutorialShown", 0) == 1;
        
        Debug.Log("=== DEBUG INTRO STATUS ===");
        Debug.Log($"IntroVideoWatched: {localWatched}");
        Debug.Log($"TutorialShown: {tutorialShown}");
        Debug.Log($"SimpleTutorialShown: {simpleTutorialShown}");
        Debug.Log($"HasWatchedIntro(): {HasWatchedIntro()}");
        Debug.Log($"checkIfWatchedBefore: {checkIfWatchedBefore}");
        
        if (HasWatchedIntro())
        {
            Debug.LogWarning("⚠️ INTRO SẼ BỊ SKIP! Sử dụng 'Reset Intro Watched Status' để reset!");
        }
        else
        {
            Debug.Log("✅ INTRO SẼ PHÁT BÌNH THƯỜNG!");
        }
    }

    void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh memory leak
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}