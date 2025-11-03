using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PVP
{
    /// <summary>
    /// Controller cho Scene Intro - Phát video rồi chuyển sang Scene PVP
    /// Đặt script này vào Scene "intro7"
    /// </summary>
    public class IntroSceneController : MonoBehaviour
    {
        [Header("Video Settings")]
        [Tooltip("VideoPlayer component để phát video")]
        public VideoPlayer videoPlayer;
        
        [Tooltip("Video clip sẽ phát")]
        public VideoClip introVideoClip;
        
        [Tooltip("Canvas chứa video (sẽ ẩn sau khi video xong)")]
        public GameObject videoCanvas;
        
        [Header("Scene Settings")]
        [Tooltip("Tên scene PVP sẽ load sau khi video xong")]
        public string pvpSceneName = "pkchuong6";
        
        [Tooltip("Dùng Loading scene (true) hay load thẳng (false)")]
        public bool useLoadingScene = true;
        
        [Tooltip("Tự động load scene PVP sau khi video xong")]
        public bool autoLoadPVPScene = true;
        
        [Tooltip("Delay sau video trước khi load scene (giây)")]
        public float delayAfterVideo = 1f;
        
        [Header("Skip Settings")]
        [Tooltip("Cho phép skip video")]
        public bool allowSkip = true;
        
        [Tooltip("Sau bao lâu mới được skip (giây)")]
        public float skipableAfter = 2f;
        
        [Header("UI (Optional)")]
        [Tooltip("UI hiện hint skip (optional)")]
        public GameObject skipHintUI;
        
        [Tooltip("Text hiện loading")]
        public TMPro.TextMeshProUGUI loadingText;
        
        [Header("Debug")]
        [Tooltip("Hiện debug logs")]
        public bool showDebugLogs = true;
        
        // State
        private bool isPlaying = false;
        private bool canSkip = false;
        private bool hasSkipped = false;
        private bool isLoading = false;
        private float playTime = 0f;
        
        private void Awake()
        {
            // Auto tìm VideoPlayer nếu chưa assign
            if (videoPlayer == null)
            {
                videoPlayer = FindObjectOfType<VideoPlayer>();
            }
            
            // Setup VideoPlayer
            if (videoPlayer != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = false;
                videoPlayer.skipOnDrop = true;
                videoPlayer.waitForFirstFrame = true;
                
                // Subscribe events
                videoPlayer.loopPointReached += OnVideoEnd;
                videoPlayer.errorReceived += OnVideoError;
            }
            else
            {
                Debug.LogError("[IntroScene] Không tìm thấy VideoPlayer!");
            }
            
            // Ẩn skip hint
            if (skipHintUI != null)
                skipHintUI.SetActive(false);
            
            // Ẩn loading text
            if (loadingText != null)
                loadingText.gameObject.SetActive(false);
        }
        
        private void Start()
        {
            // Tự động phát video
            PlayIntroVideo();
        }
        
        private void Update()
        {
            if (!isPlaying || isLoading) return;
            
            playTime += Time.deltaTime;
            
            // Check có thể skip chưa
            if (allowSkip && playTime >= skipableAfter && !canSkip)
            {
                canSkip = true;
                ShowSkipHint();
            }
            
            // Check input để skip
            if (canSkip && !hasSkipped && IsSkipKeyPressed())
            {
                SkipVideo();
            }
        }
        
        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoEnd;
                videoPlayer.errorReceived -= OnVideoError;
            }
        }
        
        /// <summary>
        /// Phát video intro
        /// </summary>
        private void PlayIntroVideo()
        {
            if (introVideoClip == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[IntroScene] Không có video clip! Chuyển thẳng sang PVP.");
                LoadPVPScene();
                return;
            }
            
            StartCoroutine(PlayVideoCoroutine());
        }
        
        /// <summary>
        /// Coroutine phát video
        /// </summary>
        private IEnumerator PlayVideoCoroutine()
        {
            isPlaying = true;
            playTime = 0f;
            canSkip = false;
            hasSkipped = false;
            
            if (showDebugLogs)
                Debug.Log("[IntroScene] 🎬 Bắt đầu phát video intro...");
            
            // Hiện video canvas
            if (videoCanvas != null)
                videoCanvas.SetActive(true);
            
            // Setup và phát video
            if (videoPlayer != null && introVideoClip != null)
            {
                videoPlayer.clip = introVideoClip;
                videoPlayer.Prepare();
                
                if (showDebugLogs)
                    Debug.Log("[IntroScene] Đang chuẩn bị video...");
                
                // Chờ video prepare
                while (!videoPlayer.isPrepared)
                {
                    yield return null;
                }
                
                // Phát video
                videoPlayer.Play();
                
                if (showDebugLogs)
                {
                    Debug.Log($"[IntroScene] ▶️ Đang phát: {introVideoClip.name}");
                    Debug.Log($"[IntroScene] 📊 Video: {videoPlayer.length:F1}s, {videoPlayer.frameCount} frames");
                }
                
                // Chờ video phát xong hoặc skip
                while (videoPlayer.isPlaying && !hasSkipped)
                {
                    yield return null;
                }
                
                if (showDebugLogs)
                {
                    if (hasSkipped)
                        Debug.Log("[IntroScene] ⏭ Video đã bị skip!");
                    else
                        Debug.Log("[IntroScene] ✅ Video phát xong!");
                }
            }
            
            // Kết thúc
            OnVideoComplete();
        }
        
        /// <summary>
        /// Callback khi video kết thúc
        /// </summary>
        private void OnVideoEnd(VideoPlayer vp)
        {
            if (showDebugLogs)
                Debug.Log("[IntroScene] Video reached end point");
        }
        
        /// <summary>
        /// Callback khi video lỗi
        /// </summary>
        private void OnVideoError(VideoPlayer source, string message)
        {
            Debug.LogError($"[IntroScene] ❌ Video Error: {message}");
            // Skip sang PVP nếu video lỗi
            OnVideoComplete();
        }
        
        /// <summary>
        /// Xử lý khi video hoàn thành
        /// </summary>
        private void OnVideoComplete()
        {
            isPlaying = false;
            
            // Ẩn video canvas
            if (videoCanvas != null)
                videoCanvas.SetActive(false);
            
            // Ẩn skip hint
            if (skipHintUI != null)
                skipHintUI.SetActive(false);
            
            if (showDebugLogs)
                Debug.Log("[IntroScene] 🎯 Intro hoàn thành! Đang chuyển sang PVP...");
            
            // Load scene PVP
            if (autoLoadPVPScene)
            {
                StartCoroutine(LoadPVPAfterDelay());
            }
        }
        
        /// <summary>
        /// Skip video
        /// </summary>
        private void SkipVideo()
        {
            if (hasSkipped) return;
            
            hasSkipped = true;
            
            if (showDebugLogs)
                Debug.Log("[IntroScene] ⏭ Đã skip video!");
            
            // Dừng video
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
        }
        
        /// <summary>
        /// Hiện skip hint
        /// </summary>
        private void ShowSkipHint()
        {
            if (skipHintUI != null)
            {
                skipHintUI.SetActive(true);
            }
            
            if (showDebugLogs)
            {
                Debug.Log("[IntroScene] ⏭ Có thể skip video! Nhấn Space/Enter/Esc");
            }
        }
        
        /// <summary>
        /// Load scene PVP sau delay
        /// </summary>
        private IEnumerator LoadPVPAfterDelay()
        {
            yield return new WaitForSeconds(delayAfterVideo);
            LoadPVPScene();
        }
        
        /// <summary>
        /// Load scene PVP
        /// </summary>
        private void LoadPVPScene()
        {
            if (isLoading) return;
            
            isLoading = true;
            
            if (showDebugLogs)
                Debug.Log($"[IntroScene] 🔄 Đang load scene: {pvpSceneName}");
            
            // Hiện loading text
            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(true);
                loadingText.text = "Loading...";
            }
            
            // Check dùng Loading scene hay load thẳng
            if (useLoadingScene)
            {
                // Dùng LoadingManager
                LoadingManager.NEXT_SCENE = pvpSceneName;
                SceneManager.LoadScene("Loading");
                
                if (showDebugLogs)
                    Debug.Log($"[IntroScene] 🔄 Load qua Loading scene → {pvpSceneName}");
            }
            else
            {
                // Load thẳng
                StartCoroutine(LoadSceneAsync());
            }
        }
        
        /// <summary>
        /// Load scene async
        /// </summary>
        private IEnumerator LoadSceneAsync()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(pvpSceneName);
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                
                if (loadingText != null)
                {
                    loadingText.text = $"Loading... {progress * 100:F0}%";
                }
                
                if (showDebugLogs && Mathf.FloorToInt(progress * 10) % 2 == 0)
                {
                    Debug.Log($"[IntroScene] Loading: {progress * 100:F0}%");
                }
                
                yield return null;
            }
            
            if (showDebugLogs)
                Debug.Log("[IntroScene] ✅ Đã load xong scene PVP!");
        }
        
        /// <summary>
        /// Check phím skip (support cả 2 Input Systems)
        /// </summary>
        private bool IsSkipKeyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            // New Input System
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            
            return keyboard.spaceKey.wasPressedThisFrame ||
                   keyboard.enterKey.wasPressedThisFrame ||
                   keyboard.escapeKey.wasPressedThisFrame;
#else
            // Old Input System
            return Input.GetKeyDown(KeyCode.Space) ||
                   Input.GetKeyDown(KeyCode.Return) ||
                   Input.GetKeyDown(KeyCode.Escape);
#endif
        }
        
        /// <summary>
        /// Force skip từ code (có thể gọi từ UI button)
        /// </summary>
        public void ForceSkip()
        {
            if (isPlaying && !hasSkipped)
            {
                SkipVideo();
            }
        }
    }
}
