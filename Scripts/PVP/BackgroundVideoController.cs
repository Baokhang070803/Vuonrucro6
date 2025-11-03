using UnityEngine;
using UnityEngine.Video;
using System.Collections;

namespace PVP
{
    /// <summary>
    /// Controller cho Background Video trong PVP Scene
    /// Phát video nền liên tục, có thể pause/resume theo game state
    /// </summary>
    public class BackgroundVideoController : MonoBehaviour
    {
        [Header("Background Video Settings")]
        [Tooltip("VideoPlayer component cho background")]
        [SerializeField] private VideoPlayer backgroundVideoPlayer;
        
        [Tooltip("Video clip nền")]
        [SerializeField] private VideoClip backgroundVideoClip;
        
        [Tooltip("Canvas chứa background video")]
        [SerializeField] private GameObject backgroundVideoCanvas;
        
        [Tooltip("RawImage hiển thị video (trên Canvas)")]
        [SerializeField] private UnityEngine.UI.RawImage backgroundVideoImage;
        
        [Header("Video Behavior")]
        [Tooltip("Tự động phát khi Start")]
        [SerializeField] private bool autoPlayOnStart = true;
        
        [Tooltip("Lặp lại video")]
        [SerializeField] private bool loopVideo = true;
        
        [Tooltip("Pause video khi có skill animation")]
        [SerializeField] private bool pauseOnSkillAnimation = true;
        
        [Tooltip("Resume video sau skill animation")]
        [SerializeField] private bool resumeAfterSkillAnimation = true;
        
        [Header("Video Quality")]
        [Tooltip("Render texture cho video (optional)")]
        [SerializeField] private RenderTexture renderTexture;
        
        [Tooltip("Video resolution")]
        [SerializeField] private Vector2Int videoResolution = new Vector2Int(1920, 1080);
        
        [Header("Integration")]
        [Tooltip("Tích hợp với VideoSkillPlayer")]
        [SerializeField] private VideoSkillPlayer skillVideoPlayer;
        
        [Tooltip("Tích hợp với SkillAnimationController")]
        [SerializeField] private SkillAnimationController skillAnimationController;
        
        [Tooltip("Tích hợp với GameSpeedToggle")]
        [SerializeField] private GameSpeedToggle gameSpeedToggle;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // State
        private bool isPlaying = false;
        private bool isPaused = false;
        private bool wasPlayingBeforePause = false;
        
        // Events
        public System.Action OnBackgroundVideoStarted;
        public System.Action OnBackgroundVideoStopped;
        public System.Action OnBackgroundVideoPaused;
        public System.Action OnBackgroundVideoResumed;
        
        private void Awake()
        {
            SetupBackgroundVideo();
        }
        
        private void Start()
        {
            // Subscribe to skill events để pause/resume
            SubscribeToSkillEvents();
            
            // Subscribe to game speed events
            SubscribeToGameSpeedEvents();
            
            // Auto play nếu được bật
            if (autoPlayOnStart)
            {
                StartCoroutine(PlayBackgroundVideoDelayed());
            }
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromSkillEvents();
            UnsubscribeFromGameSpeedEvents();
        }
        
        /// <summary>
        /// Setup background video player
        /// </summary>
        private void SetupBackgroundVideo()
        {
            // Tìm VideoPlayer nếu chưa assign
            if (backgroundVideoPlayer == null)
            {
                backgroundVideoPlayer = GetComponent<VideoPlayer>();
                if (backgroundVideoPlayer == null)
                {
                    backgroundVideoPlayer = gameObject.AddComponent<VideoPlayer>();
                }
            }
            
            // Setup VideoPlayer
            if (backgroundVideoPlayer != null)
            {
                backgroundVideoPlayer.playOnAwake = false;
                backgroundVideoPlayer.isLooping = loopVideo;
                backgroundVideoPlayer.skipOnDrop = true;
                backgroundVideoPlayer.waitForFirstFrame = true;
                
                // Subscribe events
                backgroundVideoPlayer.loopPointReached += OnBackgroundVideoLoop;
                backgroundVideoPlayer.errorReceived += OnBackgroundVideoError;
                
                DebugLog("✅ Background VideoPlayer setup complete");
            }
            
            // Setup RenderTexture nếu cần
            if (renderTexture == null && backgroundVideoImage != null)
            {
                CreateRenderTexture();
            }
            
            // Setup video target
            SetupVideoTarget();
        }
        
        /// <summary>
        /// Tạo RenderTexture cho video
        /// </summary>
        private void CreateRenderTexture()
        {
            renderTexture = new RenderTexture(videoResolution.x, videoResolution.y, 0);
            renderTexture.name = "BackgroundVideo_RenderTexture";
            
            if (backgroundVideoPlayer != null)
            {
                backgroundVideoPlayer.targetTexture = renderTexture;
            }
            
            if (backgroundVideoImage != null)
            {
                backgroundVideoImage.texture = renderTexture;
            }
            
            DebugLog($"✅ Created RenderTexture: {videoResolution.x}x{videoResolution.y}");
        }
        
        /// <summary>
        /// Setup video target (RenderTexture hoặc Camera)
        /// </summary>
        private void SetupVideoTarget()
        {
            if (backgroundVideoPlayer == null) return;
            
            if (renderTexture != null)
            {
                // Dùng RenderTexture
                backgroundVideoPlayer.targetTexture = renderTexture;
                DebugLog("📺 Video target: RenderTexture");
            }
            else
            {
                // Dùng Camera (nếu có)
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    backgroundVideoPlayer.targetCamera = mainCamera;
                    DebugLog("📺 Video target: Camera");
                }
                else
                {
                    DebugLogWarning("⚠️ No RenderTexture or Camera found for video target!");
                }
            }
        }
        
        /// <summary>
        /// Subscribe to skill events để pause/resume background video
        /// </summary>
        private void SubscribeToSkillEvents()
        {
            // Subscribe to VideoSkillPlayer events
            if (skillVideoPlayer != null)
            {
                skillVideoPlayer.OnVideoStarted += OnSkillVideoStarted;
                skillVideoPlayer.OnVideoFinished += OnSkillVideoFinished;
            }
            
            // Subscribe to SkillAnimationController events
            if (skillAnimationController != null)
            {
                skillAnimationController.OnAnimationStarted += OnSkillAnimationStarted;
                skillAnimationController.OnAnimationFinished += OnSkillAnimationFinished;
            }
            
            DebugLog("🔗 Subscribed to skill events");
        }
        
        /// <summary>
        /// Unsubscribe from skill events
        /// </summary>
        private void UnsubscribeFromSkillEvents()
        {
            if (skillVideoPlayer != null)
            {
                skillVideoPlayer.OnVideoStarted -= OnSkillVideoStarted;
                skillVideoPlayer.OnVideoFinished -= OnSkillVideoFinished;
            }
            
            if (skillAnimationController != null)
            {
                skillAnimationController.OnAnimationStarted -= OnSkillAnimationStarted;
                skillAnimationController.OnAnimationFinished -= OnSkillAnimationFinished;
            }
        }
        
        /// <summary>
        /// Subscribe to game speed events
        /// </summary>
        private void SubscribeToGameSpeedEvents()
        {
            // Tìm GameSpeedToggle nếu chưa assign
            if (gameSpeedToggle == null)
            {
                gameSpeedToggle = FindObjectOfType<GameSpeedToggle>();
            }
            
            if (gameSpeedToggle != null)
            {
                gameSpeedToggle.OnSpeedChanged += OnGameSpeedChanged;
                DebugLog("🔗 Subscribed to GameSpeedToggle events");
            }
            else
            {
                DebugLogWarning("⚠️ GameSpeedToggle not found! Background video won't sync with game speed");
            }
        }
        
        /// <summary>
        /// Unsubscribe from game speed events
        /// </summary>
        private void UnsubscribeFromGameSpeedEvents()
        {
            if (gameSpeedToggle != null)
            {
                gameSpeedToggle.OnSpeedChanged -= OnGameSpeedChanged;
            }
        }
        
        /// <summary>
        /// Callback khi game speed thay đổi
        /// </summary>
        private void OnGameSpeedChanged(float speed, GameSpeedToggle.SpeedState state)
        {
            UpdateVideoSpeed(speed);
            DebugLog($"⚡ Game speed changed to {state} (x{speed}) - Background video speed updated");
        }
        
        /// <summary>
        /// Update video speed theo game speed
        /// </summary>
        public void UpdateVideoSpeed(float speed)
        {
            if (backgroundVideoPlayer != null && backgroundVideoPlayer.isPlaying)
            {
                // Set playback speed của video
                backgroundVideoPlayer.playbackSpeed = speed;
                DebugLog($"🎬 Background video speed updated to: {speed}x");
            }
        }
        
        /// <summary>
        /// Play background video với delay
        /// </summary>
        private IEnumerator PlayBackgroundVideoDelayed()
        {
            // Đợi 1 frame để đảm bảo UI đã setup xong
            yield return new WaitForEndOfFrame();
            
            PlayBackgroundVideo();
        }
        
        /// <summary>
        /// Phát background video
        /// </summary>
        public void PlayBackgroundVideo()
        {
            if (backgroundVideoPlayer == null || backgroundVideoClip == null)
            {
                DebugLogWarning("❌ Background video player or clip is null!");
                return;
            }
            
            StartCoroutine(PlayBackgroundVideoCoroutine());
        }
        
        /// <summary>
        /// Coroutine phát background video
        /// </summary>
        private IEnumerator PlayBackgroundVideoCoroutine()
        {
            DebugLog("🎬 Starting background video...");
            
            // Setup video
            backgroundVideoPlayer.clip = backgroundVideoClip;
            backgroundVideoPlayer.isLooping = loopVideo;
            
            // Set initial playback speed theo game speed hiện tại
            if (gameSpeedToggle != null)
            {
                float currentSpeed = gameSpeedToggle.GetCurrentSpeed();
                backgroundVideoPlayer.playbackSpeed = currentSpeed;
                DebugLog($"🎬 Initial video speed set to: {currentSpeed}x");
            }
            
            // Prepare video
            backgroundVideoPlayer.Prepare();
            
            // Chờ prepare xong
            while (!backgroundVideoPlayer.isPrepared)
            {
                yield return null;
            }
            
            // Show video canvas
            if (backgroundVideoCanvas != null)
            {
                backgroundVideoCanvas.SetActive(true);
            }
            
            // Phát video
            backgroundVideoPlayer.Play();
            isPlaying = true;
            isPaused = false;
            
            DebugLog($"▶️ Background video playing: {backgroundVideoClip.name}");
            DebugLog($"📊 Video: {backgroundVideoPlayer.length:F1}s, {backgroundVideoPlayer.frameCount} frames");
            
            // Trigger event
            OnBackgroundVideoStarted?.Invoke();
        }
        
        /// <summary>
        /// Dừng background video
        /// </summary>
        public void StopBackgroundVideo()
        {
            if (backgroundVideoPlayer != null && backgroundVideoPlayer.isPlaying)
            {
                backgroundVideoPlayer.Stop();
                isPlaying = false;
                isPaused = false;
                
                DebugLog("⏹ Background video stopped");
                OnBackgroundVideoStopped?.Invoke();
            }
        }
        
        /// <summary>
        /// Pause background video
        /// </summary>
        public void PauseBackgroundVideo()
        {
            if (backgroundVideoPlayer != null && backgroundVideoPlayer.isPlaying)
            {
                backgroundVideoPlayer.Pause();
                isPaused = true;
                wasPlayingBeforePause = true;
                
                DebugLog("⏸ Background video paused");
                OnBackgroundVideoPaused?.Invoke();
            }
        }
        
        /// <summary>
        /// Resume background video
        /// </summary>
        public void ResumeBackgroundVideo()
        {
            if (backgroundVideoPlayer != null && isPaused)
            {
                backgroundVideoPlayer.Play();
                isPaused = false;
                wasPlayingBeforePause = false;
                
                DebugLog("▶️ Background video resumed");
                OnBackgroundVideoResumed?.Invoke();
            }
        }
        
        /// <summary>
        /// Callback khi skill video bắt đầu
        /// </summary>
        private void OnSkillVideoStarted()
        {
            if (pauseOnSkillAnimation && isPlaying)
            {
                PauseBackgroundVideo();
            }
        }
        
        /// <summary>
        /// Callback khi skill video kết thúc
        /// </summary>
        private void OnSkillVideoFinished()
        {
            if (resumeAfterSkillAnimation && isPaused)
            {
                ResumeBackgroundVideo();
            }
        }
        
        /// <summary>
        /// Callback khi skill animation bắt đầu
        /// </summary>
        private void OnSkillAnimationStarted()
        {
            if (pauseOnSkillAnimation && isPlaying)
            {
                PauseBackgroundVideo();
            }
        }
        
        /// <summary>
        /// Callback khi skill animation kết thúc
        /// </summary>
        private void OnSkillAnimationFinished()
        {
            if (resumeAfterSkillAnimation && isPaused)
            {
                ResumeBackgroundVideo();
            }
        }
        
        /// <summary>
        /// Callback khi video loop
        /// </summary>
        private void OnBackgroundVideoLoop(VideoPlayer vp)
        {
            DebugLog("🔄 Background video looped");
        }
        
        /// <summary>
        /// Callback khi video lỗi
        /// </summary>
        private void OnBackgroundVideoError(VideoPlayer source, string message)
        {
            DebugLogError($"❌ Background video error: {message}");
        }
        
        /// <summary>
        /// Setup từ code
        /// </summary>
        public void SetupBackgroundVideo(VideoPlayer player, VideoClip clip, GameObject canvas, UnityEngine.UI.RawImage image)
        {
            backgroundVideoPlayer = player;
            backgroundVideoClip = clip;
            backgroundVideoCanvas = canvas;
            backgroundVideoImage = image;
            
            SetupBackgroundVideo();
        }
        
        /// <summary>
        /// Set video clip mới
        /// </summary>
        public void SetBackgroundVideoClip(VideoClip newClip)
        {
            backgroundVideoClip = newClip;
            
            if (backgroundVideoPlayer != null)
            {
                backgroundVideoPlayer.clip = newClip;
            }
            
            DebugLog($"🎬 Background video clip changed to: {newClip?.name}");
        }
        
        /// <summary>
        /// Get trạng thái video
        /// </summary>
        public bool IsPlaying => isPlaying;
        public bool IsPaused => isPaused;
        
        private void DebugLog(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[BackgroundVideoController] {message}");
            }
        }
        
        private void DebugLogWarning(string message)
        {
            Debug.LogWarning($"[BackgroundVideoController] {message}");
        }
        
        private void DebugLogError(string message)
        {
            Debug.LogError($"[BackgroundVideoController] {message}");
        }
        
        // Context menu để test
        [ContextMenu("Test Play Background Video")]
        private void TestPlayBackgroundVideo()
        {
            PlayBackgroundVideo();
        }
        
        [ContextMenu("Test Stop Background Video")]
        private void TestStopBackgroundVideo()
        {
            StopBackgroundVideo();
        }
        
        [ContextMenu("Test Pause Background Video")]
        private void TestPauseBackgroundVideo()
        {
            PauseBackgroundVideo();
        }
        
        [ContextMenu("Test Resume Background Video")]
        private void TestResumeBackgroundVideo()
        {
            ResumeBackgroundVideo();
        }
    }
}
