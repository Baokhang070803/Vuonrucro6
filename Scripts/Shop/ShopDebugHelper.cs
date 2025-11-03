using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper script để debug và sửa layout shop items
/// </summary>
public class ShopDebugHelper : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    public Color debugColor = Color.red;
    
    [ContextMenu("Debug All Shop Items")]
    public void DebugAllShopItems()
    {
        var shopItems = FindObjectsOfType<SimpleShopItem>();
        Debug.Log($"=== DEBUG SHOP ITEMS ({shopItems.Length} items) ===");
        
        for (int i = 0; i < shopItems.Length; i++)
        {
            var item = shopItems[i];
            Debug.Log($"Item {i + 1}: {item.name}");
            
            // Debug RectTransform
            var rect = item.GetComponent<RectTransform>();
            Debug.Log($"  Position: {rect.anchoredPosition}");
            Debug.Log($"  Size: {rect.sizeDelta}");
            
            // Debug Text components
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                var textRect = text.GetComponent<RectTransform>();
                Debug.Log($"  Text '{text.text}': Pos({textRect.anchoredPosition}) Size({textRect.sizeDelta})");
            }
            
            // Debug Images
            var images = item.GetComponentsInChildren<Image>();
            foreach (var image in images)
            {
                var imgRect = image.GetComponent<RectTransform>();
                Debug.Log($"  Image: Pos({imgRect.anchoredPosition}) Size({imgRect.sizeDelta})");
            }
        }
    }
    
    [ContextMenu("Fix All Shop Items Layout")]
    public void FixAllShopItemsLayout()
    {
        var shopItems = FindObjectsOfType<SimpleShopItem>();
        
        foreach (var item in shopItems)
        {
            // Thêm ShopItemLayoutFixer nếu chưa có
            var fixer = item.GetComponent<ShopItemLayoutFixer>();
            if (fixer == null)
            {
                fixer = item.gameObject.AddComponent<ShopItemLayoutFixer>();
            }
            
            // Sửa layout
            fixer.FixShopItemLayout();
        }
        
        Debug.Log($"Đã sửa layout cho {shopItems.Length} shop items!");
    }
    
    [ContextMenu("Reset Shop Items Position")]
    public void ResetShopItemsPosition()
    {
        var shopItems = FindObjectsOfType<SimpleShopItem>();
        
        foreach (var item in shopItems)
        {
            var rect = item.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(200, 150);
        }
        
        Debug.Log($"Đã reset position cho {shopItems.Length} shop items!");
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        var shopItems = FindObjectsOfType<SimpleShopItem>();
        
        foreach (var item in shopItems)
        {
            var rect = item.GetComponent<RectTransform>();
            
            // Vẽ border cho shop item
            Gizmos.color = debugColor;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            
            // Vẽ 4 cạnh của rectangle
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            }
        }
    }
}
