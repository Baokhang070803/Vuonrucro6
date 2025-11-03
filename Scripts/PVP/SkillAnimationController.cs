using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace PVP
{
    /// <summary>
    /// Controller cho animation skill bằng frame ảnh
    /// Hiển thị animation sau khi video skill kết thúc
    /// </summary>
    public class SkillAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Image animationImage; // Image để hiển thị frame
        [SerializeField] private GameObject animationCanvas; // Canvas chứa animation
        [SerializeField] private float frameDuration = 0.1f; // Thời gian mỗi frame
        [SerializeField] private bool loopAnimation = false; // Có lặp lại không
        [SerializeField] private bool autoHideAfterPlay = true; // Tự động ẩn sau khi chạy xong
        
        [Header("Animation Effects")]
        [SerializeField] private bool enableScaleEffect = true; // Hiệu ứng scale
        [SerializeField] private Vector3 startScale = Vector3.zero;
        [SerializeField] private Vector3 maxScale = Vector3.one * 1.2f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [SerializeField] private bool enableFadeEffect = true; // Hiệu ứng fade
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        
        [SerializeField] private bool enableMoveEffect = true; // Hiệu ứng di chuyển
        [SerializeField] private Vector3 moveOffset = new Vector3(0, 50f, 0);
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Position Settings")]
        [SerializeField] private AnimationPositionMode positionMode = AnimationPositionMode.TargetPosition; // Mặc định ở vị trí target
        [SerializeField] private Transform targetPosition; // Vị trí mục tiêu (nếu dùng TargetPosition)
        
        public enum AnimationPositionMode
        {
            Center,           // Giữa màn hình
            TargetPosition,   // Vị trí của target
            UserPosition,     // Vị trí của người dùng skill
            CustomPosition    // Vị trí tùy chỉnh
        }
        
        // Events
        public System.Action OnAnimationStarted;
        public System.Action OnAnimationFinished;
        
        // State
        private bool isPlaying = false;
        private Coroutine animationCoroutine;
        private Vector3 originalPosition;
        private Vector3 originalScale;
        private Color originalColor;
        
        private void Awake()
        {
            // Tìm animation image nếu chưa gán
            if (animationImage == null)
            {
                animationImage = GetComponent<Image>();
            }
            
            // Tìm animation canvas nếu chưa gán
            if (animationCanvas == null)
            {
                animationCanvas = gameObject;
            }
            
            // Lưu giá trị gốc
            if (animationImage != null)
            {
                originalPosition = animationImage.transform.localPosition;
                originalScale = animationImage.transform.localScale;
                originalColor = animationImage.color;
            }
            
            // Ẩn ban đầu
            HideAnimation();
        }
        
        /// <summary>
        /// Phát animation skill với danh sách frame ảnh - LUÔN ở vị trí target
        /// </summary>
        public void PlaySkillAnimation(Sprite[] frames, CharacterData user = null, CharacterData target = null)
        {
            if (frames == null || frames.Length == 0)
            {
                Debug.LogWarning("[SkillAnimationController] Không có frame nào để phát!");
                OnAnimationFinished?.Invoke();
                return;
            }
            
            if (isPlaying)
            {
                StopAnimation();
            }
            
            // ✅ FORCE: Luôn dùng TargetPosition mode
            AnimationPositionMode originalMode = positionMode;
            positionMode = AnimationPositionMode.TargetPosition;
            
            animationCoroutine = StartCoroutine(PlayAnimationCoroutine(frames, user, target));
            
            // Restore original mode sau khi hoàn thành
            StartCoroutine(RestorePositionModeAfterAnimation(originalMode));
        }
        
        /// <summary>
        /// Restore position mode sau khi animation hoàn thành
        /// </summary>
        private IEnumerator RestorePositionModeAfterAnimation(AnimationPositionMode originalMode)
        {
            yield return new WaitUntil(() => !isPlaying);
            positionMode = originalMode;
        }
        
        /// <summary>
        /// Phát animation ở vị trí cụ thể (override position mode)
        /// </summary>
        public void PlaySkillAnimationAtPosition(Sprite[] frames, Vector3 worldPosition, CharacterData user = null)
        {
            if (frames == null || frames.Length == 0)
            {
                Debug.LogWarning("[SkillAnimationController] Không có frame nào để phát!");
                OnAnimationFinished?.Invoke();
                return;
            }
            
            if (isPlaying)
            {
                StopAnimation();
            }
            
            // Tạo fake target để force position
            CharacterData fakeTarget = new CharacterData();
            fakeTarget.characterName = "CustomPosition";
            fakeTarget.characterObject = new GameObject("FakeTarget");
            fakeTarget.characterObject.transform.position = worldPosition;
            
            animationCoroutine = StartCoroutine(PlayAnimationCoroutine(frames, user, fakeTarget));
            
            // Cleanup fake target sau animation
            StartCoroutine(CleanupFakeTargetAfterAnimation(fakeTarget));
        }
        
        /// <summary>
        /// Cleanup fake target sau animation
        /// </summary>
        private IEnumerator CleanupFakeTargetAfterAnimation(CharacterData fakeTarget)
        {
            yield return new WaitUntil(() => !isPlaying);
            if (fakeTarget?.characterObject != null)
            {
                DestroyImmediate(fakeTarget.characterObject);
            }
        }
        
        /// <summary>
        /// Phát animation với Sprite array
        /// </summary>
        private IEnumerator PlayAnimationCoroutine(Sprite[] frames, CharacterData user, CharacterData target)
        {
            isPlaying = true;
            OnAnimationStarted?.Invoke();
            
            Debug.Log($"[SkillAnimationController] Bắt đầu animation với {frames.Length} frames");
            
             // Setup vị trí animation
             SetupAnimationPosition(user, target);
             Debug.Log($"[SkillAnimationController] 🔍 Vị trí sau SetupAnimationPosition: {animationImage.transform.localPosition}");
             
             // Hiện animation
             ShowAnimation();
             Debug.Log($"[SkillAnimationController] 🔍 Vị trí sau ShowAnimation: {animationImage.transform.localPosition}");
             
             // Reset về trạng thái ban đầu
             ResetAnimationState();
             Debug.Log($"[SkillAnimationController] 🔍 Vị trí sau ResetAnimationState: {animationImage.transform.localPosition}");
            
            // Hiệu ứng xuất hiện
            yield return StartCoroutine(PlayEntranceEffect());
            
            // Phát từng frame
            for (int i = 0; i < frames.Length; i++)
            {
                if (animationImage != null)
                {
                    animationImage.sprite = frames[i];
                }
                
                yield return new WaitForSeconds(frameDuration);
            }
            
            // Hiệu ứng kết thúc
            if (autoHideAfterPlay)
            {
                yield return StartCoroutine(PlayExitEffect());
                HideAnimation();
            }
            
            isPlaying = false;
            OnAnimationFinished?.Invoke();
            
            Debug.Log("[SkillAnimationController] Animation hoàn thành!");
        }
        
        /// <summary>
        /// Setup vị trí animation dựa trên mode
        /// </summary>
        private void SetupAnimationPosition(CharacterData user, CharacterData target)
        {
            if (animationImage == null) return;
            
            Vector3 targetPos = Vector3.zero;
            
             // ✅ ƯU TIÊN: Luôn hiển thị ở vị trí target nếu có
             if (target?.characterObject != null)
             {
                 // 🔥 TÌM VỊ TRÍ VISUAL THỰC TẾ
                 Vector3 worldPos = GetTargetVisualPosition(target);
                 
                 // Kiểm tra loại Canvas
                 Canvas canvas = animationCanvas.GetComponent<Canvas>();
                 if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                 {
                     // 🔥 Screen Space Overlay: Dùng RectTransformUtility trực tiếp
                     RectTransform targetRect = target.characterObject.GetComponentInChildren<Image>()?.rectTransform;
                     RectTransform canvasRect = animationCanvas.GetComponent<RectTransform>();
                     
                     if (targetRect != null && canvasRect != null)
                     {
                         // Chuyển đổi trực tiếp từ UI element sang canvas local position
                         Vector2 localPos;
                         RectTransformUtility.ScreenPointToLocalPointInRectangle(
                             canvasRect, targetRect.position, null, out localPos);
                         
                         targetPos = localPos;
                         
                         Debug.Log($"[SkillAnimationController] Screen Space Overlay - Target: {target.characterName}");
                         Debug.Log($"  Target Rect Position: {targetRect.position} → Local: {localPos}");
                     }
                     else
                     {
                         Debug.LogWarning("[SkillAnimationController] Không tìm thấy RectTransform!");
                         targetPos = Vector3.zero;
                     }
                 }
                 else
                 {
                     // World Space hoặc Screen Space Camera: Dùng InverseTransformPoint
                     targetPos = animationCanvas.transform.InverseTransformPoint(worldPos);
                     
                     Debug.Log($"[SkillAnimationController] World/Screen Space Camera - Target: {target.characterName}");
                     Debug.Log($"  Visual World: {worldPos} → Local: {targetPos}");
                 }
                 
                 // 🔥 DỊCH CHUYỂN AnimationImage đến vị trí target với offset nhỏ
                 Vector3 adjustedPos = targetPos + new Vector3(0, 30f, 0); // Offset 30px lên trên để animation hiện trên đầu nhân vật
                 animationImage.transform.localPosition = adjustedPos;
                 Debug.Log($"[SkillAnimationController] ✅ Đã dịch chuyển AnimationImage đến: {adjustedPos} (gốc: {targetPos})");
             }
            else
            {
                // Fallback theo mode nếu không có target
                switch (positionMode)
                {
                    case AnimationPositionMode.Center:
                        targetPos = Vector3.zero; // Giữa màn hình
                        break;
                        
                    case AnimationPositionMode.UserPosition:
                        if (user?.characterObject != null)
                        {
                            Vector3 worldPos = GetTargetVisualPosition(user);
                            Canvas canvas = animationCanvas.GetComponent<Canvas>();
                            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                            {
                                RectTransform userRect = user.characterObject.GetComponentInChildren<Image>()?.rectTransform;
                                RectTransform canvasRect = animationCanvas.GetComponent<RectTransform>();
                                
                                if (userRect != null && canvasRect != null)
                                {
                                    Vector2 localPos;
                                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                                        canvasRect, userRect.position, null, out localPos);
                                    targetPos = localPos;
                                }
                            }
                            else
                            {
                                targetPos = animationCanvas.transform.InverseTransformPoint(worldPos);
                            }
                        }
                        break;
                        
                    case AnimationPositionMode.CustomPosition:
                        if (targetPosition != null)
                        {
                            targetPos = animationCanvas.transform.InverseTransformPoint(targetPosition.position);
                        }
                        break;
                        
                    case AnimationPositionMode.TargetPosition:
                    default:
                        targetPos = Vector3.zero; // Fallback về center
                        break;
                }
                
                 Debug.Log($"[SkillAnimationController] Animation tại vị trí fallback: {positionMode}");
                 
                 // 🔥 DỊCH CHUYỂN AnimationImage đến vị trí fallback
                 animationImage.transform.localPosition = targetPos;
                 Debug.Log($"[SkillAnimationController] ✅ Đã dịch chuyển AnimationImage đến fallback: {targetPos}");
             }
        }
        
        /// <summary>
        /// 🔥 Tìm vị trí visual thực tế của target (Image component)
        /// </summary>
        private Vector3 GetTargetVisualPosition(CharacterData target)
        {
            if (target?.characterObject == null) return Vector3.zero;
            
            // Tìm Image component trong hierarchy
            Image targetImage = target.characterObject.GetComponentInChildren<Image>();
            if (targetImage != null)
            {
                Vector3 visualPos = targetImage.transform.position;
                Debug.Log($"[SkillAnimationController] Tìm thấy Image visual tại: {visualPos}");
                return visualPos;
            }
            
            // Fallback: Dùng vị trí characterObject
            Vector3 fallbackPos = target.characterObject.transform.position;
            Debug.Log($"[SkillAnimationController] Fallback dùng characterObject tại: {fallbackPos}");
            return fallbackPos;
        }
        
         /// <summary>
         /// Reset animation về trạng thái ban đầu (KHÔNG reset vị trí)
         /// </summary>
         private void ResetAnimationState()
         {
             if (animationImage == null) return;
             
             // 🔥 KHÔNG reset vị trí - giữ nguyên vị trí target đã setup
             // animationImage.transform.localPosition = originalPosition; // ❌ REMOVED
             
             // Reset scale
             if (enableScaleEffect)
             {
                 animationImage.transform.localScale = startScale;
             }
             else
             {
                 animationImage.transform.localScale = originalScale;
             }
             
             // Reset color/alpha
             if (enableFadeEffect)
             {
                 Color color = originalColor;
                 color.a = 0f;
                 animationImage.color = color;
             }
             else
             {
                 animationImage.color = originalColor;
             }
             
             Debug.Log($"[SkillAnimationController] ResetAnimationState - Vị trí giữ nguyên: {animationImage.transform.localPosition}");
         }
        
        /// <summary>
        /// Hiệu ứng xuất hiện
        /// </summary>
        private IEnumerator PlayEntranceEffect()
        {
            if (animationImage == null) yield break;
            
            float elapsed = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInDuration;
                
                // Scale effect
                if (enableScaleEffect)
                {
                    Vector3 scale = Vector3.Lerp(startScale, maxScale, scaleCurve.Evaluate(t));
                    animationImage.transform.localScale = scale;
                }
                
                // Fade effect
                if (enableFadeEffect)
                {
                    Color color = originalColor;
                    color.a = Mathf.Lerp(0f, originalColor.a, t);
                    animationImage.color = color;
                }
                
                 // Move effect - 🔥 TẠM THỜI TẮT để animation hiện đúng vị trí target
                 // if (enableMoveEffect)
                 // {
                 //     Vector3 currentPos = animationImage.transform.localPosition;
                 //     Vector3 movePos = currentPos + moveOffset * moveCurve.Evaluate(t);
                 //     animationImage.transform.localPosition = movePos;
                 // }
                
                yield return null;
            }
            
            // Đảm bảo về trạng thái cuối
            if (enableScaleEffect)
                animationImage.transform.localScale = maxScale;
            if (enableFadeEffect)
                animationImage.color = originalColor;
        }
        
        /// <summary>
        /// Hiệu ứng kết thúc
        /// </summary>
        private IEnumerator PlayExitEffect()
        {
            if (animationImage == null) yield break;
            
            float elapsed = 0f;
            Vector3 startPos = animationImage.transform.localPosition;
            Vector3 startScale = animationImage.transform.localScale;
            Color startColor = animationImage.color;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;
                
                // Scale effect
                if (enableScaleEffect)
                {
                    Vector3 scale = Vector3.Lerp(startScale, Vector3.zero, t);
                    animationImage.transform.localScale = scale;
                }
                
                // Fade effect
                if (enableFadeEffect)
                {
                    Color color = startColor;
                    color.a = Mathf.Lerp(startColor.a, 0f, t);
                    animationImage.color = color;
                }
                
                 // Move effect - 🔥 TẠM THỜI TẮT để animation hiện đúng vị trí target
                 // if (enableMoveEffect)
                 // {
                 //     Vector3 movePos = Vector3.Lerp(startPos, animationImage.transform.localPosition, t);
                 //     animationImage.transform.localPosition = movePos;
                 // }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Hiện animation
        /// </summary>
        private void ShowAnimation()
        {
            if (animationCanvas != null)
            {
                animationCanvas.SetActive(true);
            }
        }
        
        /// <summary>
        /// Ẩn animation
        /// </summary>
        private void HideAnimation()
        {
            if (animationCanvas != null)
            {
                animationCanvas.SetActive(false);
            }
        }
        
        /// <summary>
        /// Dừng animation
        /// </summary>
        public void StopAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            
            isPlaying = false;
            HideAnimation();
        }
        
        /// <summary>
        /// Kiểm tra có đang phát animation không
        /// </summary>
        public bool IsPlaying => isPlaying;
        
        /// <summary>
        /// Setup animation từ code
        /// </summary>
        public void SetupAnimation(Image image, GameObject canvas)
        {
            animationImage = image;
            animationCanvas = canvas;
            
            if (animationImage != null)
            {
                originalPosition = animationImage.transform.localPosition;
                originalScale = animationImage.transform.localScale;
                originalColor = animationImage.color;
            }
        }
        
        /// <summary>
        /// Debug: Kiểm tra vị trí target trong Console
        /// </summary>
        [ContextMenu("Debug Target Position")]
        private void DebugTargetPosition()
        {
            // Tìm CharacterSkills components (MonoBehaviour) để lấy CharacterData
            var characterSkills = FindObjectsOfType<CharacterSkills>();
            if (characterSkills.Length > 0)
            {
                // Lấy CharacterData từ Team3v3Manager
                var teamManager = FindObjectOfType<Team3v3Manager>();
                if (teamManager != null)
                {
                    var allCharacters = teamManager.GetAllCharacters();
                    if (allCharacters.Count > 0)
                    {
                        var target = allCharacters[0]; // Lấy character đầu tiên
                        Debug.Log($"=== DEBUG TARGET POSITION ===");
                        Debug.Log($"Target: {target.characterName}");
                        Debug.Log($"CharacterObject Position: {target.characterObject.transform.position}");
                        
                        // 🔥 Test vị trí visual thực tế
                        Vector3 visualPos = GetTargetVisualPosition(target);
                        Debug.Log($"Visual Position: {visualPos}");
                        
                        if (animationCanvas != null)
                        {
                            Canvas canvas = animationCanvas.GetComponent<Canvas>();
                            Debug.Log($"Canvas Render Mode: {canvas.renderMode}");
                            
                            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                            {
                                // 🔥 Test cách mới: Dùng RectTransform trực tiếp
                                RectTransform targetRect = target.characterObject.GetComponentInChildren<Image>()?.rectTransform;
                                RectTransform canvasRect = animationCanvas.GetComponent<RectTransform>();
                                
                                if (targetRect != null && canvasRect != null)
                                {
                                    Debug.Log($"Target Rect Position: {targetRect.position}");
                                    
                                    Vector2 localPos;
                                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                                        canvasRect, targetRect.position, null, out localPos);
                                    Debug.Log($"Canvas Local Position (NEW): {localPos}");
                                }
                                else
                                {
                                    Debug.LogWarning("Không tìm thấy RectTransform!");
                                }
                            }
                            else
                            {
                                Vector3 localPos = animationCanvas.transform.InverseTransformPoint(visualPos);
                                Debug.Log($"Canvas Local Position: {localPos}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Không tìm thấy characters trong Team3v3Manager!");
                    }
                }
                else
                {
                    Debug.LogWarning("Không tìm thấy Team3v3Manager!");
                }
            }
            else
            {
                Debug.LogWarning("Không tìm thấy CharacterSkills components!");
            }
        }
        
        /// <summary>
        /// Test animation trong Editor
        /// </summary>
        [ContextMenu("Test Animation")]
        private void TestAnimation()
        {
            // Tạo test frames
            Sprite[] testFrames = new Sprite[5];
            for (int i = 0; i < testFrames.Length; i++)
            {
                // Tạo sprite đơn giản để test
                Texture2D texture = new Texture2D(64, 64);
                Color[] colors = new Color[64 * 64];
                for (int j = 0; j < colors.Length; j++)
                {
                    colors[j] = Color.Lerp(Color.red, Color.blue, (float)i / testFrames.Length);
                }
                texture.SetPixels(colors);
                texture.Apply();
                
                testFrames[i] = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
            }
            
            PlaySkillAnimation(testFrames);
        }
    }
}
