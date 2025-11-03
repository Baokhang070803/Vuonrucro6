using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script để sửa layout của shop items, đảm bảo text không bị che
/// </summary>
public class ShopItemLayoutFixer : MonoBehaviour
{
    [Header("Layout Settings")]
    public float itemWidth = 200f;
    public float itemHeight = 150f;
    
    [Header("Element Positions")]
    public float nameY = 60f;      // Vị trí Y của tên
    public float iconY = 0f;       // Vị trí Y của icon
    public float priceY = -40f;    // Vị trí Y của giá
    public float buttonY = -60f;   // Vị trí Y của button
    
    [Header("Element Sizes")]
    public float nameHeight = 30f;
    public float iconSize = 80f;
    public float priceHeight = 25f;
    public float buttonHeight = 30f;
    
    [ContextMenu("Fix Shop Item Layout")]
    public void FixShopItemLayout()
    {
        // Tìm các component
        var nameText = GetComponentInChildren<TextMeshProUGUI>();
        var iconImage = GetComponentInChildren<Image>();
        var button = GetComponentInChildren<Button>();
        
        // Sửa layout chính
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(itemWidth, itemHeight);
        
        // Sửa vị trí tên
        if (nameText != null)
        {
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, nameY);
            nameRect.sizeDelta = new Vector2(itemWidth - 20, nameHeight);
            
            // Đảm bảo text alignment
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontSize = 14f;
        }
        
        // Sửa vị trí icon
        if (iconImage != null)
        {
            var iconRect = iconImage.GetComponent<RectTransform>();
            iconRect.anchoredPosition = new Vector2(0, iconY);
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        }
        
        // Sửa vị trí button
        if (button != null)
        {
            var buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(0, buttonY);
            buttonRect.sizeDelta = new Vector2(itemWidth - 20, buttonHeight);
            
            // Sửa text trong button
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.fontSize = 12f;
            }
        }
        
        // Tìm và sửa price text
        var allTexts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            if (text != nameText && text != button.GetComponentInChildren<TextMeshProUGUI>())
            {
                var priceRect = text.GetComponent<RectTransform>();
                priceRect.anchoredPosition = new Vector2(0, priceY);
                priceRect.sizeDelta = new Vector2(itemWidth - 20, priceHeight);
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 12f;
            }
        }
        
        Debug.Log("Đã sửa layout cho shop item!");
    }
    
    [ContextMenu("Fix All Shop Items")]
    public void FixAllShopItems()
    {
        var shopItems = FindObjectsOfType<SimpleShopItem>();
        foreach (var item in shopItems)
        {
            var fixer = item.GetComponent<ShopItemLayoutFixer>();
            if (fixer == null)
            {
                fixer = item.gameObject.AddComponent<ShopItemLayoutFixer>();
            }
            fixer.FixShopItemLayout();
        }
        
        Debug.Log($"Đã sửa layout cho {shopItems.Length} shop items!");
    }
    
    private void Start()
    {
        // Tự động sửa layout khi start
        if (Application.isPlaying)
        {
            FixShopItemLayout();
        }
    }
}
