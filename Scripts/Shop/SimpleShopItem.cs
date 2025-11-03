using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleShopItem : MonoBehaviour
{
    [Header("UI Components - Đơn giản")]
    public Image seedIcon;
    public TextMeshProUGUI seedNameText;
    public TextMeshProUGUI priceText;
    public Button purchaseButton;
    public TextMeshProUGUI purchaseButtonText;
    
    private SeedData currentSeed;
    private ShopManager shopManager;
    private ShopUI shopUI;
    
    private void Start()
    {
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }
    }
    
    public void SetupItem(SeedData seed, ShopManager manager, ShopUI ui = null)
    {
        Debug.Log($"[SimpleShopItem] SetupItem được gọi cho {seed.seedName}");
        
        currentSeed = seed;
        shopManager = manager;
        shopUI = ui;
        UpdateDisplay();
        
        // Tự động sửa layout để tránh text bị che
        FixLayout();
        
        // Thêm sự kiện click cho toàn bộ item để hiển thị chi tiết
        AddItemClickHandler();
        
        Debug.Log($"[SimpleShopItem] SetupItem hoàn thành cho {seed.seedName}");
    }
    
    private void FixLayout()
    {
        // Đảm bảo item có kích thước đúng
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(180, 150);
        
        // Sửa vị trí các element
        if (seedNameText != null)
        {
            var nameRect = seedNameText.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, 60);
            nameRect.sizeDelta = new Vector2(160, 30);
            seedNameText.alignment = TextAlignmentOptions.Center;
        }
        
        if (seedIcon != null)
        {
            var iconRect = seedIcon.GetComponent<RectTransform>();
            iconRect.anchoredPosition = new Vector2(0, 0);
            iconRect.sizeDelta = new Vector2(80, 80);
        }
        
        if (priceText != null)
        {
            var priceRect = priceText.GetComponent<RectTransform>();
            priceRect.anchoredPosition = new Vector2(0, -40);
            priceRect.sizeDelta = new Vector2(160, 25);
            priceText.alignment = TextAlignmentOptions.Center;
        }
        
        if (purchaseButton != null)
        {
            var buttonRect = purchaseButton.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(0, -60);
            buttonRect.sizeDelta = new Vector2(160, 30);
            
            // Sửa text trong button
            if (purchaseButtonText != null)
            {
                purchaseButtonText.alignment = TextAlignmentOptions.Center;
            }
        }
    }
    
    private void UpdateDisplay()
    {
        if (currentSeed == null) return;
        
        // Hiển thị icon
        if (seedIcon != null && currentSeed.seedIcon != null)
        {
            seedIcon.sprite = currentSeed.seedIcon;
        }
        
        // Hiển thị tên
        if (seedNameText != null)
        {
            seedNameText.text = currentSeed.seedName;
        }
        
        // Hiển thị giá
        if (priceText != null)
        {
            string currencySymbol = GetCurrencySymbol(currentSeed.currencyType);
            priceText.text = $"{currencySymbol}{currentSeed.price}";
        }
        
        UpdateAvailability();
    }
    
    private void UpdateAvailability()
    {
        if (currentSeed == null || shopManager == null) return;
        
        bool canPurchase = shopManager.CanPurchaseSeed(currentSeed);
        bool isUnlocked = currentSeed.isUnlocked;
        
        // Cập nhật nút mua
        if (purchaseButton != null)
        {
            purchaseButton.interactable = canPurchase && isUnlocked;
        }
        
        if (purchaseButtonText != null)
        {
            if (!isUnlocked)
            {
                purchaseButtonText.text = "KHÓA";
            }
            else if (!canPurchase)
            {
                purchaseButtonText.text = "HẾT TIỀN";
            }
            else
            {
                purchaseButtonText.text = "MUA";
            }
        }
    }
    
    private string GetCurrencySymbol(SeedData.CurrencyType currencyType)
    {
        return "";  // Không hiển thị icon
    }
    
    private void OnPurchaseClicked()
    {
        if (currentSeed != null && shopManager != null)
        {
            shopManager.PurchaseSeed(currentSeed);
        }
    }
    
    public void RefreshItem()
    {
        UpdateAvailability();
    }
    
    private void AddItemClickHandler()
    {
        Debug.Log($"[SimpleShopItem] Bắt đầu setup click handler cho {gameObject.name}");
        
        // Thêm Button component nếu chưa có
        Button itemButton = GetComponent<Button>();
        if (itemButton == null)
        {
            itemButton = gameObject.AddComponent<Button>();
            Debug.Log($"[SimpleShopItem] Đã thêm Button component cho {gameObject.name}");
        }
        
        // Xóa listener cũ trước khi thêm mới (tránh duplicate)
        itemButton.onClick.RemoveListener(OnItemClicked);
        
        // Gán sự kiện click để hiển thị chi tiết
        itemButton.onClick.AddListener(OnItemClicked);
        
        Debug.Log($"[SimpleShopItem] Đã setup click handler cho {gameObject.name}");
    }
    
    private void OnItemClicked()
    {
        Debug.Log($"[SimpleShopItem] OnItemClicked được gọi cho {gameObject.name}");
        
        if (currentSeed == null)
        {
            Debug.LogError($"[SimpleShopItem] currentSeed là null cho {gameObject.name}!");
            return;
        }
        
        if (shopUI == null)
        {
            Debug.LogError($"[SimpleShopItem] shopUI là null cho {gameObject.name}!");
            return;
        }
        
        Debug.Log($"[SimpleShopItem] Hiển thị chi tiết cho {currentSeed.seedName}");
        
        // Đợi một frame để đảm bảo ItemDetailPanel sẵn sàng
        StartCoroutine(ShowItemDetailWithDelay());
    }
    
    private System.Collections.IEnumerator ShowItemDetailWithDelay()
    {
        // Đợi 2 frame để đảm bảo ItemDetailPanel sẵn sàng
        yield return null;
        yield return null;
        
        if (shopUI != null && currentSeed != null)
        {
            Debug.Log($"[SimpleShopItem] Gọi ShowItemDetail cho {currentSeed.seedName}");
            shopUI.ShowItemDetail(currentSeed);
        }
        else
        {
            Debug.LogError($"[SimpleShopItem] shopUI hoặc currentSeed vẫn null sau delay!");
        }
    }
}
