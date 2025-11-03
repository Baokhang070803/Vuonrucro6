using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

namespace PVP
{
    /// <summary>
    /// Helper script để setup Background Video trong PVP Scene
    /// Attach vào GameObject chứa background video
    /// </summary>
    public class BackgroundVideoSetup : MonoBehaviour
    {
        [Header("Background Video Components")]
        [Tooltip("BackgroundVideoController component")]
        public BackgroundVideoController backgroundVideoController;
        
        [Tooltip("VideoPlayer component")]
        public VideoPlayer videoPlayer;
        
        [Tooltip("Canvas chứa background video")]
        public GameObject videoCanvas;
        
        [Tooltip("RawImage hiển thị video")]
        public RawImage videoImage;
        
        [Header("Video Settings")]
        [Tooltip("Video clip nền")]
        public VideoClip backgroundVideoClip;
        
        [Tooltip("Render texture cho video")]
        public RenderTexture renderTexture;
        
        [Header("Integration")]
        [Tooltip("Tích hợp với VideoSkillPlayer")]
        public VideoSkillPlayer skillVideoPlayer;
        
        [Tooltip("Tích hợp với SkillAnimationController")]
        public SkillAnimationController skillAnimationController;
        
        [ContextMenu("Auto Setup Background Video")]
        public void AutoSetupBackgroundVideo()
        {
            // Tìm hoặc tạo BackgroundVideoController
            if (backgroundVideoController == null)
            {
                backgroundVideoController = GetComponent<BackgroundVideoController>();
                if (backgroundVideoController == null)
                {
                    backgroundVideoController = gameObject.AddComponent<BackgroundVideoController>();
                    Debug.Log("✅ Added BackgroundVideoController component");
                }
            }
            
            // Tìm VideoPlayer
            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
                if (videoPlayer == null)
                {
                    videoPlayer = gameObject.AddComponent<VideoPlayer>();
                    Debug.Log("✅ Added VideoPlayer component");
                }
            }
            
            // Tìm Video Canvas
            if (videoCanvas == null)
            {
                videoCanvas = GameObject.Find("BackgroundVideoCanvas");
                if (videoCanvas == null)
                {
                    // Tạo Canvas mới
                    videoCanvas = CreateBackgroundVideoCanvas();
                }
            }
            
            // Tìm Video Image
            if (videoImage == null && videoCanvas != null)
            {
                videoImage = videoCanvas.GetComponentInChildren<RawImage>();
                if (videoImage == null)
                {
                    // Tạo RawImage mới
                    videoImage = CreateBackgroundVideoImage();
                }
            }
            
            // Tìm skill components
            if (skillVideoPlayer == null)
            {
                skillVideoPlayer = FindObjectOfType<VideoSkillPlayer>();
            }
            
            if (skillAnimationController == null)
            {
                skillAnimationController = FindObjectOfType<SkillAnimationController>();
            }
            
            // Setup BackgroundVideoController
            if (backgroundVideoController != null)
            {
                backgroundVideoController.SetupBackgroundVideo(
                    videoPlayer, 
                    backgroundVideoClip, 
                    videoCanvas, 
                    videoImage
                );
                
                // Set integration components
                var controllerType = backgroundVideoController.GetType();
                var skillVideoField = controllerType.GetField("skillVideoPlayer", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var skillAnimField = controllerType.GetField("skillAnimationController", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (skillVideoField != null)
                    skillVideoField.SetValue(backgroundVideoController, skillVideoPlayer);
                if (skillAnimField != null)
                    skillAnimField.SetValue(backgroundVideoController, skillAnimationController);
                
                Debug.Log("✅ Background video setup complete!");
            }
        }
        
        /// <summary>
        /// Tạo Background Video Canvas
        /// </summary>
        private GameObject CreateBackgroundVideoCanvas()
        {
            // Tạo Canvas
            GameObject canvas = new GameObject("BackgroundVideoCanvas");
            canvas.transform.SetParent(transform, false);
            
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = -100; // Ở dưới cùng
            
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvas.AddComponent<GraphicRaycaster>();
            
            Debug.Log("✅ Created BackgroundVideoCanvas");
            return canvas;
        }
        
        /// <summary>
        /// Tạo Background Video Image
        /// </summary>
        private RawImage CreateBackgroundVideoImage()
        {
            if (videoCanvas == null) return null;
            
            // Tạo RawImage
            GameObject imageObj = new GameObject("BackgroundVideoImage");
            imageObj.transform.SetParent(videoCanvas.transform, false);
            
            RawImage image = imageObj.AddComponent<RawImage>();
            
            // Setup RectTransform để phủ toàn màn hình
            RectTransform rectTransform = image.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            Debug.Log("✅ Created BackgroundVideoImage");
            return image;
        }
        
        /// <summary>
        /// Setup từ Inspector
        /// </summary>
        [ContextMenu("Setup from Inspector Values")]
        public void SetupFromInspector()
        {
            if (backgroundVideoController != null)
            {
                backgroundVideoController.SetupBackgroundVideo(
                    videoPlayer, 
                    backgroundVideoClip, 
                    videoCanvas, 
                    videoImage
                );
                
                Debug.Log("✅ Background video setup from Inspector values");
            }
        }
        
        /// <summary>
        /// Test background video
        /// </summary>
        [ContextMenu("Test Background Video")]
        public void TestBackgroundVideo()
        {
            if (backgroundVideoController != null)
            {
                backgroundVideoController.PlayBackgroundVideo();
            }
        }
        
        private void Start()
        {
            // Auto setup nếu chưa setup
            if (backgroundVideoController == null || videoPlayer == null)
            {
                AutoSetupBackgroundVideo();
            }
        }
    }
}
