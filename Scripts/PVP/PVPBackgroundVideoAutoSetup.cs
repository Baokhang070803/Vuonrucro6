using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Linq;

namespace PVP
{
    /// <summary>
    /// Auto setup Background Video cho PVP Scene
    /// Chạy trong Editor để tự động tạo background video system
    /// </summary>
    public class PVPBackgroundVideoAutoSetup : MonoBehaviour
    {
        [Header("Background Video Settings")]
        [Tooltip("Video clip nền cho PVP")]
        public VideoClip backgroundVideoClip;
        
        [Tooltip("Tự động setup khi Start")]
        public bool autoSetupOnStart = true;
        
        [Tooltip("Hiện debug logs")]
        public bool showDebugLogs = true;
        
        [ContextMenu("Auto Setup Background Video System")]
        public void AutoSetupBackgroundVideoSystem()
        {
            DebugLog("🎬 Starting auto setup background video system...");
            
            // 1. Tạo GameObject chứa background video
            GameObject backgroundVideoObject = CreateBackgroundVideoObject();
            
            // 2. Setup BackgroundVideoController
            BackgroundVideoController controller = SetupBackgroundVideoController(backgroundVideoObject);
            
            // 3. Setup VideoPlayer
            VideoPlayer videoPlayer = SetupVideoPlayer(backgroundVideoObject);
            
            // 4. Tạo Canvas và UI
            GameObject canvas = CreateBackgroundVideoCanvas();
            RawImage videoImage = CreateBackgroundVideoImage(canvas);
            
            // 5. Tạo RenderTexture
            RenderTexture renderTexture = CreateRenderTexture();
            
            // 6. Kết nối tất cả components
            ConnectComponents(controller, videoPlayer, canvas, videoImage, renderTexture);
            
            // 7. Tích hợp với Turn3v3Manager
            IntegrateWithTurnManager(controller);
            
            // 8. Tích hợp với GameSpeedToggle
            IntegrateWithGameSpeedToggle(controller);
            
            DebugLog("✅ Background video system setup complete!");
        }
        
        /// <summary>
        /// Tạo GameObject chứa background video
        /// </summary>
        private GameObject CreateBackgroundVideoObject()
        {
            GameObject bgVideoObj = GameObject.Find("BackgroundVideo");
            if (bgVideoObj == null)
            {
                bgVideoObj = new GameObject("BackgroundVideo");
                bgVideoObj.transform.SetParent(transform, false);
                DebugLog("✅ Created BackgroundVideo GameObject");
            }
            else
            {
                DebugLog("📁 Found existing BackgroundVideo GameObject");
            }
            
            return bgVideoObj;
        }
        
        /// <summary>
        /// Setup BackgroundVideoController
        /// </summary>
        private BackgroundVideoController SetupBackgroundVideoController(GameObject parent)
        {
            BackgroundVideoController controller = parent.GetComponent<BackgroundVideoController>();
            if (controller == null)
            {
                controller = parent.AddComponent<BackgroundVideoController>();
                DebugLog("✅ Added BackgroundVideoController component");
            }
            else
            {
                DebugLog("📁 Found existing BackgroundVideoController");
            }
            
            return controller;
        }
        
        /// <summary>
        /// Setup VideoPlayer
        /// </summary>
        private VideoPlayer SetupVideoPlayer(GameObject parent)
        {
            VideoPlayer videoPlayer = parent.GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                videoPlayer = parent.AddComponent<VideoPlayer>();
                DebugLog("✅ Added VideoPlayer component");
            }
            else
            {
                DebugLog("📁 Found existing VideoPlayer");
            }
            
            return videoPlayer;
        }
        
        /// <summary>
        /// Tạo Background Video Canvas
        /// </summary>
        private GameObject CreateBackgroundVideoCanvas()
        {
            GameObject canvas = GameObject.Find("BackgroundVideoCanvas");
            if (canvas == null)
            {
                canvas = new GameObject("BackgroundVideoCanvas");
                canvas.transform.SetParent(transform, false);
                
                // Setup Canvas component
                Canvas canvasComponent = canvas.AddComponent<Canvas>();
                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasComponent.sortingOrder = -100; // Ở dưới cùng
                
                // Setup CanvasScaler
                CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                
                canvas.AddComponent<GraphicRaycaster>();
                
                DebugLog("✅ Created BackgroundVideoCanvas");
            }
            else
            {
                DebugLog("📁 Found existing BackgroundVideoCanvas");
            }
            
            return canvas;
        }
        
        /// <summary>
        /// Tạo Background Video Image
        /// </summary>
        private RawImage CreateBackgroundVideoImage(GameObject canvas)
        {
            RawImage videoImage = canvas.GetComponentInChildren<RawImage>();
            if (videoImage == null)
            {
                GameObject imageObj = new GameObject("BackgroundVideoImage");
                imageObj.transform.SetParent(canvas.transform, false);
                
                videoImage = imageObj.AddComponent<RawImage>();
                
                // Setup RectTransform để phủ toàn màn hình
                RectTransform rectTransform = videoImage.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                
                DebugLog("✅ Created BackgroundVideoImage");
            }
            else
            {
                DebugLog("📁 Found existing BackgroundVideoImage");
            }
            
            return videoImage;
        }
        
        /// <summary>
        /// Tạo RenderTexture
        /// </summary>
        private RenderTexture CreateRenderTexture()
        {
            RenderTexture renderTexture = Resources.FindObjectsOfTypeAll<RenderTexture>()
                .FirstOrDefault(rt => rt.name == "BackgroundVideo_RenderTexture");
            
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(1920, 1080, 0);
                renderTexture.name = "BackgroundVideo_RenderTexture";
                DebugLog("✅ Created RenderTexture");
            }
            else
            {
                DebugLog("📁 Found existing RenderTexture");
            }
            
            return renderTexture;
        }
        
        /// <summary>
        /// Kết nối tất cả components
        /// </summary>
        private void ConnectComponents(BackgroundVideoController controller, VideoPlayer videoPlayer, 
            GameObject canvas, RawImage videoImage, RenderTexture renderTexture)
        {
            // Setup BackgroundVideoController
            controller.SetupBackgroundVideo(videoPlayer, backgroundVideoClip, canvas, videoImage);
            
            // Setup VideoPlayer với RenderTexture
            videoPlayer.targetTexture = renderTexture;
            videoImage.texture = renderTexture;
            
            DebugLog("✅ Connected all components");
        }
        
        /// <summary>
        /// Tích hợp với Turn3v3Manager
        /// </summary>
        private void IntegrateWithTurnManager(BackgroundVideoController controller)
        {
            Turn3v3Manager turnManager = FindObjectOfType<Turn3v3Manager>();
            if (turnManager != null)
            {
                // Set background video controller vào Turn3v3Manager
                var turnManagerType = turnManager.GetType();
                var backgroundVideoField = turnManagerType.GetField("backgroundVideoController", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (backgroundVideoField != null)
                {
                    backgroundVideoField.SetValue(turnManager, controller);
                    DebugLog("✅ Integrated with Turn3v3Manager");
                }
                else
                {
                    DebugLogWarning("⚠️ Could not find backgroundVideoController field in Turn3v3Manager");
                }
            }
            else
            {
                DebugLogWarning("⚠️ Turn3v3Manager not found in scene");
            }
        }
        
        /// <summary>
        /// Tích hợp với GameSpeedToggle
        /// </summary>
        private void IntegrateWithGameSpeedToggle(BackgroundVideoController controller)
        {
            GameSpeedToggle gameSpeedToggle = FindObjectOfType<GameSpeedToggle>();
            if (gameSpeedToggle != null)
            {
                // Set GameSpeedToggle vào BackgroundVideoController
                var controllerType = controller.GetType();
                var gameSpeedField = controllerType.GetField("gameSpeedToggle", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (gameSpeedField != null)
                {
                    gameSpeedField.SetValue(controller, gameSpeedToggle);
                    DebugLog("✅ Integrated with GameSpeedToggle");
                }
                else
                {
                    DebugLogWarning("⚠️ Could not find gameSpeedToggle field in BackgroundVideoController");
                }
            }
            else
            {
                DebugLogWarning("⚠️ GameSpeedToggle not found in scene");
            }
        }
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                AutoSetupBackgroundVideoSystem();
            }
        }
        
        private void DebugLog(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[PVPBackgroundVideoAutoSetup] {message}");
            }
        }
        
        private void DebugLogWarning(string message)
        {
            Debug.LogWarning($"[PVPBackgroundVideoAutoSetup] {message}");
        }
    }
}
