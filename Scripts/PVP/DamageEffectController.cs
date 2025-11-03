using UnityEngine;
using System.Collections;

namespace PVP
{
    /// <summary>
    /// Điều khiển hiệu ứng khi nhân vật bị trúng đòn:
    /// - Flash màu đỏ
    /// - Rung lắc
    /// </summary>
    public class DamageEffectController : MonoBehaviour
    {
        [Header("Flash Settings")]
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.2f;
        [SerializeField] private int flashCount = 2; // Số lần nhấp nháy
        
        [Header("Shake Settings")]
        [SerializeField] private float shakeIntensity = 0.3f;
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float shakeSpeed = 30f;
        
        [Header("References")]
        private SpriteRenderer spriteRenderer;
        private UnityEngine.UI.Image uiImage; // Thêm support cho UI Image
        private Color originalColor;
        private Vector3 originalPosition;
        private Transform targetTransform;
        
        private Coroutine damageEffectCoroutine;
        
        private void Awake()
        {
            Debug.Log($"[DamageEffectController] Awake() on {gameObject.name}");
            
            InitializeSpriteRenderer();
            
            // Lưu vị trí gốc
            if (targetTransform == null)
            {
                targetTransform = transform;
            }
            
            if (targetTransform != null)
            {
                originalPosition = targetTransform.localPosition;
                Debug.Log($"[DamageEffectController] targetTransform: {targetTransform.name}, originalPosition: {originalPosition}");
            }
            else
            {
                Debug.LogError($"[DamageEffectController] {gameObject.name} transform is null!");
            }
        }
        
        private void Start()
        {
            // Đảm bảo originalPosition được lưu sau khi layout hoàn tất
            if (targetTransform != null)
            {
                originalPosition = targetTransform.localPosition;
                Debug.Log($"[DamageEffectController] Start() - Updated originalPosition: {originalPosition}");
            }
        }
        
        /// <summary>
        /// Kích hoạt hiệu ứng bị damage
        /// </summary>
        public void TriggerDamageEffect()
        {
            Debug.Log($"[DamageEffectController] TriggerDamageEffect() called on {gameObject.name}");
            
            // Validate và re-initialize nếu cần
            if (spriteRenderer == null && uiImage == null)
            {
                Debug.LogWarning($"[DamageEffectController] Both spriteRenderer and uiImage are NULL! Trying to find it now...");
                InitializeSpriteRenderer();
            }
            
            if (targetTransform == null)
            {
                Debug.LogWarning($"[DamageEffectController] {gameObject.name} - targetTransform is null! Re-initializing...");
                targetTransform = transform;
                if (targetTransform != null)
                {
                    originalPosition = targetTransform.localPosition;
                }
            }
            
            // Log status (with null checks)
            string spriteInfo = (spriteRenderer != null) ? spriteRenderer.gameObject.name : 
                                (uiImage != null) ? $"UI Image on {uiImage.gameObject.name}" : "NULL";
            string transformInfo = (targetTransform != null) ? targetTransform.name : "NULL";
            Debug.Log($"[DamageEffectController] targetTransform: {transformInfo}, visual: {spriteInfo}");
            
            // Dừng effect cũ nếu đang chạy
            if (damageEffectCoroutine != null)
            {
                StopCoroutine(damageEffectCoroutine);
            }
            
            damageEffectCoroutine = StartCoroutine(DamageEffectCoroutine());
        }
        
        /// <summary>
        /// Initialize/Re-initialize SpriteRenderer hoặc UI Image
        /// </summary>
        private void InitializeSpriteRenderer()
        {
            Debug.Log($"[DamageEffectController] Searching for SpriteRenderer/Image from {gameObject.name}...");
            
            // Tìm SpriteRenderer - thử nhiều cách
            // 1. Tìm trong children
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Debug.Log($"  → Found SpriteRenderer in children: {spriteRenderer.gameObject.name}");
            }
            
            // 2. Nếu không có, thử tìm trong parent
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInParent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Debug.Log($"  → Found SpriteRenderer in parent: {spriteRenderer.gameObject.name}");
                }
            }
            
            // 3. Nếu vẫn không có, thử tìm trên chính GameObject này
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Debug.Log($"  → Found SpriteRenderer on this GameObject");
                }
            }
            
            // 4. Fallback: Tìm trong toàn bộ hierarchy của parent
            if (spriteRenderer == null && transform.parent != null)
            {
                Debug.Log($"  → Searching in parent.children of {transform.parent.name}...");
                spriteRenderer = transform.parent.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Debug.Log($"  → Found SpriteRenderer in parent's children: {spriteRenderer.gameObject.name}");
                }
            }
            
            // 5. Nếu không có SpriteRenderer, thử tìm UI Image
            if (spriteRenderer == null)
            {
                Debug.Log($"  → No SpriteRenderer found, trying UI Image...");
                
                uiImage = GetComponentInChildren<UnityEngine.UI.Image>();
                if (uiImage == null)
                {
                    uiImage = GetComponent<UnityEngine.UI.Image>();
                }
                if (uiImage == null && transform.parent != null)
                {
                    uiImage = transform.parent.GetComponentInChildren<UnityEngine.UI.Image>();
                }
                
                if (uiImage != null)
                {
                    Debug.Log($"  → ✅ Found UI Image on {uiImage.gameObject.name}");
                    originalColor = uiImage.color;
                    targetTransform = uiImage.transform;
                    originalPosition = targetTransform.localPosition;
                    return;
                }
            }
            
            // Debug: List all children nếu không tìm thấy gì
            if (spriteRenderer == null && uiImage == null)
            {
                Debug.LogWarning($"  ❌ Neither SpriteRenderer nor UI Image found! GameObject hierarchy:");
                PrintHierarchy(transform, 0);
            }
            
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
                Debug.Log($"[DamageEffectController] ✅ Found SpriteRenderer on {spriteRenderer.gameObject.name}, Color: {originalColor}");
                
                // Dùng transform của SpriteRenderer để rung
                targetTransform = spriteRenderer.transform;
                originalPosition = targetTransform.localPosition;
            }
            else if (uiImage == null)
            {
                Debug.LogWarning($"[DamageEffectController] ❌ {gameObject.name} không tìm thấy SpriteRenderer hoặc Image!");
                originalColor = Color.white;
            }
        }
        
        /// <summary>
        /// Debug: In ra hierarchy
        /// </summary>
        private void PrintHierarchy(Transform t, int level)
        {
            string indent = new string(' ', level * 2);
            var components = t.GetComponents<Component>();
            string componentList = string.Join(", ", System.Array.ConvertAll(components, c => c.GetType().Name));
            Debug.Log($"{indent}- {t.name} [{componentList}]");
            
            foreach (Transform child in t)
            {
                PrintHierarchy(child, level + 1);
            }
        }
        
        /// <summary>
        /// Coroutine chạy cả flash và shake cùng lúc
        /// </summary>
        private IEnumerator DamageEffectCoroutine()
        {
            Debug.Log($"[DamageEffectController] DamageEffectCoroutine started!");
            
            // Chạy flash và shake song song
            Coroutine flashCoroutine = StartCoroutine(FlashEffect());
            Coroutine shakeCoroutine = StartCoroutine(ShakeEffect());
            
            // Đợi cả 2 xong
            yield return flashCoroutine;
            yield return shakeCoroutine;
            
            Debug.Log($"[DamageEffectController] DamageEffectCoroutine finished!");
            damageEffectCoroutine = null;
        }
        
        /// <summary>
        /// Hiệu ứng flash màu đỏ
        /// </summary>
        private IEnumerator FlashEffect()
        {
            Debug.Log($"[DamageEffectController] FlashEffect started!");
            
            if (spriteRenderer == null && uiImage == null)
            {
                Debug.LogWarning("[DamageEffectController] No SpriteRenderer or Image! Cannot flash.");
                yield break;
            }
            
            for (int i = 0; i < flashCount; i++)
            {
                Debug.Log($"[DamageEffectController] Flash {i + 1}/{flashCount}");
                
                // Flash đỏ
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = damageFlashColor;
                }
                else if (uiImage != null)
                {
                    uiImage.color = damageFlashColor;
                }
                
                yield return new WaitForSeconds(flashDuration / 2);
                
                // Trở về màu gốc
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
                else if (uiImage != null)
                {
                    uiImage.color = originalColor;
                }
                
                yield return new WaitForSeconds(flashDuration / 2);
            }
            
            // Đảm bảo trở về màu gốc
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
            else if (uiImage != null)
            {
                uiImage.color = originalColor;
            }
            
            Debug.Log($"[DamageEffectController] FlashEffect finished!");
        }
        
        /// <summary>
        /// Hiệu ứng rung lắc
        /// </summary>
        private IEnumerator ShakeEffect()
        {
            Debug.Log($"[DamageEffectController] ShakeEffect started!");
            
            if (targetTransform == null)
            {
                Debug.LogWarning("[DamageEffectController] targetTransform is null! Cannot shake.");
                yield break;
            }
            
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                // Random offset
                float x = Random.Range(-1f, 1f) * shakeIntensity;
                float y = Random.Range(-1f, 1f) * shakeIntensity;
                
                targetTransform.localPosition = originalPosition + new Vector3(x, y, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Trở về vị trí gốc
            targetTransform.localPosition = originalPosition;
            Debug.Log($"[DamageEffectController] ShakeEffect finished!");
        }
        
        /// <summary>
        /// Hiệu ứng rung lắc theo hướng (từ trái/phải)
        /// </summary>
        public void TriggerDamageEffectFromDirection(Vector3 attackerPosition)
        {
            if (damageEffectCoroutine != null)
            {
                StopCoroutine(damageEffectCoroutine);
            }
            
            damageEffectCoroutine = StartCoroutine(DamageEffectWithDirectionCoroutine(attackerPosition));
        }
        
        private IEnumerator DamageEffectWithDirectionCoroutine(Vector3 attackerPosition)
        {
            // Tính hướng từ attacker đến nhân vật
            Vector3 direction = (transform.position - attackerPosition).normalized;
            
            // Chạy flash và shake có hướng song song
            Coroutine flashCoroutine = StartCoroutine(FlashEffect());
            Coroutine shakeCoroutine = StartCoroutine(DirectionalShakeEffect(direction));
            
            yield return flashCoroutine;
            yield return shakeCoroutine;
            
            damageEffectCoroutine = null;
        }
        
        /// <summary>
        /// Rung lắc theo hướng bị đánh
        /// </summary>
        private IEnumerator DirectionalShakeEffect(Vector3 direction)
        {
            if (targetTransform == null)
            {
                Debug.LogWarning("[DamageEffectController] targetTransform is null! Cannot shake.");
                yield break;
            }
            
            float elapsed = 0f;
            
            // Push back một chút
            Vector3 pushPosition = originalPosition + direction * shakeIntensity * 2;
            
            while (elapsed < shakeDuration)
            {
                float t = elapsed / shakeDuration;
                
                // Dao động quanh vị trí push
                float oscillation = Mathf.Sin(t * shakeSpeed) * shakeIntensity * (1 - t);
                Vector3 offset = direction * oscillation;
                
                // Lerp từ push position về original
                Vector3 targetPos = Vector3.Lerp(pushPosition, originalPosition, t);
                targetTransform.localPosition = targetPos + offset;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Trở về vị trí gốc
            targetTransform.localPosition = originalPosition;
        }
        
        /// <summary>
        /// Reset về trạng thái ban đầu
        /// </summary>
        public void ResetEffects()
        {
            if (damageEffectCoroutine != null)
            {
                StopCoroutine(damageEffectCoroutine);
                damageEffectCoroutine = null;
            }
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
            
            if (uiImage != null)
            {
                uiImage.color = originalColor;
            }
            
            if (targetTransform != null)
            {
                targetTransform.localPosition = originalPosition;
            }
        }
        
        /// <summary>
        /// Test effect trong Editor
        /// </summary>
        [ContextMenu("Test Damage Effect")]
        private void TestDamageEffect()
        {
            TriggerDamageEffect();
        }
        
        [ContextMenu("Test Directional Effect (Left)")]
        private void TestDirectionalEffectLeft()
        {
            Vector3 fakeAttacker = transform.position + Vector3.left * 2;
            TriggerDamageEffectFromDirection(fakeAttacker);
        }
        
        [ContextMenu("Test Directional Effect (Right)")]
        private void TestDirectionalEffectRight()
        {
            Vector3 fakeAttacker = transform.position + Vector3.right * 2;
            TriggerDamageEffectFromDirection(fakeAttacker);
        }
    }
}
