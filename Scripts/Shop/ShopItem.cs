using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour
{
    [Header("UI Components")]
    public Image seedIcon;
    public TextMeshProUGUI seedNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;
    public Button purchaseButton;
    public TextMeshProUGUI purchaseButtonText;
    public GameObject lockedOverlay;
    
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
        currentSeed = seed;
        shopManager = manager;
        shopUI = ui;
        
        UpdateDisplay();
        AddItemClickHandler();
    }
    
    private void UpdateDisplay()
    {
        if (currentSeed == null) return;
        
        // Hiển thị thông tin hạt giống
        if (seedIcon != null && currentSeed.seedIcon != null)
        {
            seedIcon.sprite = currentSeed.seedIcon;
        }
        
        if (seedNameText != null)
        {
            seedNameText.text = currentSeed.seedName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = currentSeed.description;
        }
        
        if (priceText != null)
        {
            string currencySymbol = GetCurrencySymbol(currentSeed.currencyType);
            priceText.text = $"{currencySymbol}{currentSeed.price}";
        }
        
        UpdateAvailability();
    }
    
    public void UpdateAvailability()
    {
        if (currentSeed == null || shopManager == null) return;
        
        bool canPurchase = shopManager.CanPurchaseSeed(currentSeed);
        bool isUnlocked = currentSeed.isUnlocked;
        
        // Cập nhật trạng thái nút mua
        if (purchaseButton != null)
        {
            purchaseButton.interactable = canPurchase && isUnlocked;
        }
        
        if (purchaseButtonText != null)
        {
            if (!isUnlocked)
            {
                purchaseButtonText.text = "CHƯA MỞ KHÓA";
            }
            else if (!canPurchase)
            {
                purchaseButtonText.text = "KHÔNG ĐỦ TIỀN";
            }
            else
            {
                purchaseButtonText.text = "MUA";
            }
        }
        
        // Hiển thị overlay khóa
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!isUnlocked);
        }
    }
    
    private string GetCurrencySymbol(SeedData.CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case SeedData.CurrencyType.Gold:
                return "💰";
            case SeedData.CurrencyType.Gems:
                return "💎";
            case SeedData.CurrencyType.Coins:
                return "🪙";
            default:
                return "";
        }
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
        // Thêm Button component nếu chưa có
        Button itemButton = GetComponent<Button>();
        if (itemButton == null)
        {
            itemButton = gameObject.AddComponent<Button>();
        }
        
        // Gán sự kiện click để hiển thị chi tiết
        itemButton.onClick.AddListener(OnItemClicked);
    }
    
    private void OnItemClicked()
    {
        if (currentSeed != null && shopUI != null)
        {
            shopUI.ShowItemDetail(currentSeed);
        }
    }
}
