using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject shopPanel;
    public Transform shopItemContainer;
    public GameObject shopItemPrefab;
    public Button closeButton;
    public TextMeshProUGUI playerGoldText;
    
    [Header("Tham chiếu")]
    public ShopManager shopManager;
    public PlayerGoldManager playerGoldManager;
    public ItemDetailPanel itemDetailPanel;
    public ShopScrollRectConfig scrollRectConfig;
    
    private List<SimpleShopItem> shopItems = new List<SimpleShopItem>();
    
    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
        
        if (shopManager != null)
        {
            shopManager.OnSeedPurchased += OnSeedPurchased;
            shopManager.OnPurchaseFailed += OnPurchaseFailed;
        }
        
        if (playerGoldManager != null)
        {
            playerGoldManager.OnGoldChanged += UpdateGoldDisplay;
        }
        
        // Kết nối với ItemDetailPanel
        if (itemDetailPanel != null)
        {
            itemDetailPanel.shopManager = shopManager;
            itemDetailPanel.playerGoldManager = playerGoldManager;
        }
    }
    
    public void SetupShop(ShopManager manager)
    {
        shopManager = manager;
        CreateShopItems();
        if (playerGoldManager != null)
        {
            UpdateGoldDisplay(playerGoldManager.GetGold());
        }
    }
    
    private void CreateShopItems()
    {
        Debug.Log("[ShopUI] Bắt đầu tạo shop items...");
        
        if (shopManager == null)
        {
            Debug.LogError("[ShopUI] ShopManager is null!");
            return;
        }
        
        if (shopItemContainer == null)
        {
            Debug.LogError("[ShopUI] ShopItemContainer is null!");
            return;
        }
        
        if (shopItemPrefab == null)
        {
            Debug.LogError("[ShopUI] ShopItemPrefab is null!");
            return;
        }
            
        // Xóa các item cũ
        foreach (Transform child in shopItemContainer)
        {
            Destroy(child.gameObject);
        }
        shopItems.Clear();
        
        // Kiểm tra và thêm Layout Group nếu cần
        EnsureLayoutGroup();
        
        // Tạo các item mới
        List<SeedData> availableSeeds = shopManager.GetAvailableSeeds();
        Debug.Log($"[ShopUI] Tìm thấy {availableSeeds.Count} hạt giống");
        
        foreach (SeedData seed in availableSeeds)
        {
            Debug.Log($"[ShopUI] Tạo item cho: {seed.seedName}");
            GameObject itemObj = Instantiate(shopItemPrefab, shopItemContainer);
            SimpleShopItem shopItem = itemObj.GetComponent<SimpleShopItem>();
            
            if (shopItem != null)
            {
                shopItem.SetupItem(seed, shopManager, this);
                shopItems.Add(shopItem);
                Debug.Log($"[ShopUI] Đã tạo thành công: {seed.seedName}");
            }
            else
            {
                Debug.LogError($"[ShopUI] Không tìm thấy SimpleShopItem component trong {seed.seedName}");
            }
        }
        
        Debug.Log($"[ShopUI] Hoàn thành tạo {shopItems.Count} shop items");
        
        // Cập nhật hiển thị thanh cuộn sau khi tạo items
        UpdateScrollbarVisibility();
    }
    
    private void EnsureLayoutGroup()
    {
        // Kiểm tra xem đã có Layout Group chưa
        var gridLayout = shopItemContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        var verticalLayout = shopItemContainer.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        var horizontalLayout = shopItemContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        
        if (gridLayout == null && verticalLayout == null && horizontalLayout == null)
        {
            // Thêm Grid Layout Group với cấu hình 4x4
            var layoutGroup = shopItemContainer.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            layoutGroup.cellSize = new Vector2(180, 150);
            layoutGroup.spacing = new Vector2(8, 10);
            layoutGroup.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
            layoutGroup.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            layoutGroup.constraintCount = 4; // 4 cột cho layout 4x4
            
            Debug.Log("[ShopUI] Đã thêm Grid Layout Group 4x4 vào ShopItemContainer");
        }
        
        // Cấu hình ScrollRect để ẩn thanh cuộn khi không cần
        SetupScrollRect();
    }
    
    private void SetupScrollRect()
    {
        // Tìm ScrollRect component
        var scrollRect = shopItemContainer.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            // Cấu hình ScrollRect
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            
            // Cấu hình Content Size Fitter để tự động điều chỉnh kích thước
            var contentSizeFitter = shopItemContainer.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                contentSizeFitter = shopItemContainer.gameObject.AddComponent<ContentSizeFitter>();
            }
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            Debug.Log("[ShopUI] Đã cấu hình ScrollRect với thanh cuộn tự động ẩn");
        }
    }
    
    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            CreateShopItems(); // Tạo shop items khi mở
            UpdateShopItems();
            UpdateScrollbarVisibility(); // Cập nhật hiển thị thanh cuộn
            
            // Đảm bảo ItemDetailPanel được đóng khi mở shop
            if (itemDetailPanel != null)
            {
                itemDetailPanel.ForceClosePanel();
            }
        }
    }
    
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        
        // Đóng ItemDetailPanel khi đóng shop
        if (itemDetailPanel != null)
        {
            itemDetailPanel.ForceClosePanel();
        }
        
        Debug.Log("[ShopUI] Đã đóng shop và ItemDetailPanel");
    }
    
    private void UpdateShopItems()
    {
        foreach (SimpleShopItem item in shopItems)
        {
            item.RefreshItem();
        }
    }
    
    private void UpdateGoldDisplay(int gold)
    {
        if (playerGoldText != null)
        {
            playerGoldText.text = $"Vàng: {gold}";
        }
    }
    
    private void OnSeedPurchased(SeedData seed)
    {
        Debug.Log($"Mua thành công: {seed.seedName}");
        UpdateShopItems();
        if (playerGoldManager != null)
        {
            UpdateGoldDisplay(playerGoldManager.GetGold());
        }
        
        // Hiển thị thông báo mua thành công
        ShowPurchaseSuccess(seed.seedName);
    }
    
    private void ShowPurchaseSuccess(string seedName)
    {
        // TODO: Hiển thị popup thông báo mua thành công
        Debug.Log($"🎉 Đã mua {seedName} và thêm vào túi!");
        
        // Có thể thêm hiệu ứng UI ở đây
        // VD: Popup text, animation, sound effect
    }
    
    private void OnPurchaseFailed(string message)
    {
        Debug.LogWarning($"Mua thất bại: {message}");
        
        // TODO: Hiển thị thông báo lỗi cho người chơi
    }
    
    // Phương thức để hiển thị chi tiết item
    public void ShowItemDetail(SeedData seed)
    {
        Debug.Log($"[ShopUI] ShowItemDetail được gọi cho {seed?.seedName}");
        
        if (itemDetailPanel == null)
        {
            Debug.LogError("[ShopUI] itemDetailPanel là null! Không thể hiển thị chi tiết!");
            return;
        }
        
        if (seed == null)
        {
            Debug.LogError("[ShopUI] seed là null! Không thể hiển thị chi tiết!");
            return;
        }
        
        Debug.Log($"[ShopUI] Gọi ItemDetailPanel.ShowItemDetail cho {seed.seedName}");
        itemDetailPanel.ShowItemDetail(seed);
    }
    
    /// <summary>
    /// Cập nhật hiển thị thanh cuộn dựa trên số lượng items
    /// </summary>
    private void UpdateScrollbarVisibility()
    {
        int totalItems = shopItems.Count;
        
        // Sử dụng ShopScrollRectConfig nếu có
        if (scrollRectConfig != null)
        {
            scrollRectConfig.UpdateScrollbarVisibility(totalItems);
        }
        else
        {
            // Fallback: sử dụng logic cũ
            var scrollRect = shopItemContainer.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                int maxItemsWithoutScroll = 16; // 4x4 = 16 items
                
                if (totalItems <= maxItemsWithoutScroll)
                {
                    scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                    Debug.Log($"[ShopUI] Có {totalItems} items (≤16), ẩn thanh cuộn");
                }
                else
                {
                    scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                    Debug.Log($"[ShopUI] Có {totalItems} items (>16), hiện thanh cuộn khi cần");
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        if (shopManager != null)
        {
            shopManager.OnSeedPurchased -= OnSeedPurchased;
            shopManager.OnPurchaseFailed -= OnPurchaseFailed;
        }
        
        if (playerGoldManager != null)
        {
            playerGoldManager.OnGoldChanged -= UpdateGoldDisplay;
        }
    }
}
