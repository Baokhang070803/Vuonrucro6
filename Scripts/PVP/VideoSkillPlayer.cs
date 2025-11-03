using UnityEngine;
using UnityEngine.Video;
using System.Collections;

namespace PVP
{
    /// <summary>
    /// Quản lý việc phát video animation cho skills
    /// Dựa trên cấu trúc: Canvas/RawImage/VideoPlayer
    /// </summary>
    public class VideoSkillPlayer : MonoBehaviour
    {
        [Header("Video References")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private GameObject videoCanvas; // Canvas chứa video
        [SerializeField] private RenderTexture renderTexture; // Tùy chọn: dùng RenderTexture
        
        [Header("Animation Integration")]
        [SerializeField] private SkillAnimationController skillAnimationController; // Controller cho frame animation
        
        [Header("Settings")]
        [SerializeField] private bool autoHideAfterPlay = true;
        [SerializeField] private float delayAfterVideo = 0.5f; // Delay sau khi video kết thúc
        [SerializeField] private bool syncWithGameSpeed = true; // Video có tăng tốc theo game không
        
        // Events
        public System.Action OnVideoStarted;
        public System.Action OnVideoFinished;
        
        private bool isPlaying = false;
        private Coroutine playCoroutine;
        
        private void Awake()
        {
            // Tìm VideoPlayer nếu chưa gán
            if (videoPlayer == null)
            {
                videoPlayer = GetComponentInChildren<VideoPlayer>();
            }
            
            // Tìm SkillAnimationController nếu chưa gán
            if (skillAnimationController == null)
            {
                skillAnimationController = GetComponent<SkillAnimationController>();
            }
            
            // Setup video player
            if (videoPlayer != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = false;
                videoPlayer.skipOnDrop = true;
                
                // Subscribe to events
                videoPlayer.loopPointReached += OnVideoEnded;
            }
            
            // Ẩn video canvas ban đầu
            HideVideo();
        }
        
        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoEnded;
            }
        }
        
        /// <summary>
        /// Phát skill video với animation frames sau khi video kết thúc
        /// </summary>
        public void PlaySkillVideoWithAnimation(VideoClip videoClip, Sprite[] animationFrames, CharacterData user = null, CharacterData target = null, System.Action onComplete = null)
        {
            if (isPlaying)
            {
                Debug.LogWarning("[VideoSkillPlayer] Đang phát video khác!");
                return;
            }
            
            Debug.Log($"[VideoSkillPlayer] Phát video + animation: {videoClip?.name}");
            
            playCoroutine = StartCoroutine(PlayVideoWithAnimationCoroutine(videoClip, animationFrames, user, target, onComplete));
        }
        
        /// <summary>
        /// Coroutine phát video rồi animation
        /// </summary>
        private IEnumerator PlayVideoWithAnimationCoroutine(VideoClip videoClip, Sprite[] animationFrames, CharacterData user, CharacterData target, System.Action onComplete)
        {
            isPlaying = true;
            OnVideoStarted?.Invoke();
            
            // Phát video trước
            if (videoClip != null)
            {
                yield return StartCoroutine(PlayVideoCoroutine(videoClip, null));
            }
            
            // Đợi một chút sau video
            yield return new WaitForSeconds(delayAfterVideo);
            
            // Phát animation frames tại vị trí target
            if (animationFrames != null && animationFrames.Length > 0 && skillAnimationController != null)
            {
                Debug.Log($"[VideoSkillPlayer] Bắt đầu animation với {animationFrames.Length} frames tại vị trí target: {target?.characterName}");
                
                bool animationFinished = false;
                
                skillAnimationController.OnAnimationFinished += () => animationFinished = true;
                skillAnimationController.PlaySkillAnimation(animationFrames, user, target);
                
                // Chờ animation kết thúc
                yield return new WaitUntil(() => animationFinished);
                
                Debug.Log("[VideoSkillPlayer] Animation hoàn thành!");
            }
            
            isPlaying = false;
            OnVideoFinished?.Invoke();
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// Phát video skill
        /// </summary>
        public void PlaySkillVideo(VideoClip videoClip, System.Action onComplete = null)
        {
            if (videoClip == null)
            {
                Debug.LogWarning("Video clip null! Bỏ qua phát video.");
                onComplete?.Invoke();
                return;
            }
            
            if (isPlaying)
            {
                Debug.LogWarning("Video đang phát! Chờ video hiện tại kết thúc.");
                return;
            }
            
            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
            }
            
            playCoroutine = StartCoroutine(PlayVideoCoroutine(videoClip, onComplete));
        }
        
        /// <summary>
        /// Coroutine phát video
        /// </summary>
        private IEnumerator PlayVideoCoroutine(VideoClip videoClip, System.Action onComplete)
        {
            isPlaying = true;
            
            // Hiển thị video canvas
            ShowVideo();
            
            // Set video clip
            if (videoPlayer != null)
            {
                videoPlayer.clip = videoClip;
                
                // Đồng bộ tốc độ video với tốc độ game
                if (syncWithGameSpeed)
                {
                    videoPlayer.playbackSpeed = Time.timeScale;
                    Debug.Log($"Video playback speed: {Time.timeScale}x");
                }
                else
                {
                    videoPlayer.playbackSpeed = 1f;
                }
                
                videoPlayer.Prepare();
                
                // Chờ video prepare xong
                while (!videoPlayer.isPrepared)
                {
                    yield return null;
                }
                
                // Phát video
                videoPlayer.Play();
                OnVideoStarted?.Invoke();
                
                Debug.Log($"Đang phát video skill: {videoClip.name}");
                
                // Chờ video phát xong
                while (videoPlayer.isPlaying)
                {
                    yield return null;
                }
                
                // Delay thêm một chút sau khi video kết thúc
                if (delayAfterVideo > 0)
                {
                    yield return new WaitForSeconds(delayAfterVideo);
                }
            }
            
            // Ẩn video
            if (autoHideAfterPlay)
            {
                HideVideo();
            }
            
            isPlaying = false;
            
            Debug.Log("Video skill đã phát xong!");
            OnVideoFinished?.Invoke();
            
            // Gọi callback
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// Callback khi video kết thúc
        /// </summary>
        private void OnVideoEnded(VideoPlayer vp)
        {
            Debug.Log("Video reached end point");
        }
        
        /// <summary>
        /// Hiển thị video canvas
        /// </summary>
        private void ShowVideo()
        {
            if (videoCanvas != null)
            {
                videoCanvas.SetActive(true);
            }
        }
        
        /// <summary>
        /// Ẩn video canvas
        /// </summary>
        private void HideVideo()
        {
            if (videoCanvas != null)
            {
                videoCanvas.SetActive(false);
            }
        }
        
        /// <summary>
        /// Dừng video đang phát
        /// </summary>
        public void StopVideo()
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            
            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
                playCoroutine = null;
            }
            
            isPlaying = false;
            HideVideo();
        }
        
        /// <summary>
        /// Setup video player từ code
        /// </summary>
        public void SetupVideoPlayer(VideoPlayer player, GameObject canvas)
        {
            videoPlayer = player;
            videoCanvas = canvas;
            
            if (videoPlayer != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = false;
                videoPlayer.loopPointReached -= OnVideoEnded;
                videoPlayer.loopPointReached += OnVideoEnded;
            }
            
            HideVideo();
        }
        
        /// <summary>
        /// Cập nhật tốc độ video (gọi khi game speed thay đổi)
        /// </summary>
        public void UpdateVideoSpeed(float speed)
        {
            if (videoPlayer != null && syncWithGameSpeed)
            {
                videoPlayer.playbackSpeed = speed;
                Debug.Log($"📹 Video speed updated to {speed}x");
            }
        }
        
        // Properties
        public bool IsPlaying => isPlaying;
    }
}