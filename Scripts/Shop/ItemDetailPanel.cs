using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDetailPanel : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject detailPanel;
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemPriceText;
    public TextMeshProUGUI totalPriceText;
    public TMP_InputField quantityInputField;
    public Button increaseQuantityButton;
    public Button decreaseQuantityButton;
    public Button purchaseButton;
    
    [Header("Tham chiếu")]
    public ShopManager shopManager;
    public PlayerGoldManager playerGoldManager;
    
    private SeedData currentSeed;
    private int currentQuantity = 1;
    private int maxQuantity = 99;
    
    private void Start()
    {
        // Gán sự kiện cho các nút
        if (increaseQuantityButton != null)
            increaseQuantityButton.onClick.AddListener(IncreaseQuantity);
        
        if (decreaseQuantityButton != null)
            decreaseQuantityButton.onClick.AddListener(DecreaseQuantity);
        
        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(PurchaseItems);
        
        // Gán sự kiện cho input field
        if (quantityInputField != null)
        {
            quantityInputField.onValueChanged.AddListener(OnQuantityInputChanged);
            quantityInputField.onEndEdit.AddListener(OnQuantityInputEndEdit);
        }
        
        // Ẩn panel ban đầu
        if (detailPanel != null)
            detailPanel.SetActive(false);
    }
    
    public void ShowItemDetail(SeedData seed)
    {
        Debug.Log($"[ItemDetailPanel] ShowItemDetail được gọi cho {seed?.seedName}");
        
        if (seed == null) 
        {
            Debug.LogError("[ItemDetailPanel] seed là null!");
            return;
        }
        
        if (detailPanel == null)
        {
            Debug.LogError("[ItemDetailPanel] detailPanel là null!");
            return;
        }
        
        currentSeed = seed;
        currentQuantity = 1;
        
        Debug.Log($"[ItemDetailPanel] Hiển thị panel cho {seed.seedName}");
        
        // Hiển thị panel
        detailPanel.SetActive(true);
        Debug.Log($"[ItemDetailPanel] Panel đã được SetActive(true) cho {seed.seedName}");
        
        // Đảm bảo Canvas có sorting order cao
        var canvas = detailPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 10000; // Cao hơn tất cả
            canvas.overrideSorting = true;
            Debug.Log($"[ItemDetailPanel] Đã set Canvas sorting order: {canvas.sortingOrder}");
        }
        else
        {
            Debug.LogWarning("[ItemDetailPanel] Không tìm thấy Canvas cho ItemDetailPanel!");
        }
        
        // Force enable tất cả parent objects
        Transform parent = detailPanel.transform.parent;
        while (parent != null)
        {
            parent.gameObject.SetActive(true);
            parent = parent.parent;
        }
        
        // Kiểm tra xem panel có thực sự active không
        if (detailPanel.activeInHierarchy)
        {
            Debug.Log($"[ItemDetailPanel] Panel đã active trong hierarchy cho {seed.seedName}");
        }
        else
        {
            Debug.LogError($"[ItemDetailPanel] Panel KHÔNG active trong hierarchy cho {seed.seedName}!");
        }
        
        // Cập nhật thông tin
        UpdateItemDisplay();
        UpdateQuantityDisplay();
        UpdatePurchaseButton();
        
        // Force refresh UI
        Canvas.ForceUpdateCanvases();
        
        Debug.Log($"[ItemDetailPanel] Đã hiển thị chi tiết cho {seed.seedName}");
    }
    
    private void UpdateItemDisplay()
    {
        if (currentSeed == null) return;
        
        // Icon
        if (itemIcon != null && currentSeed.seedIcon != null)
        {
            itemIcon.sprite = currentSeed.seedIcon;
        }
        
        // Tên
        if (itemNameText != null)
        {
            itemNameText.text = currentSeed.seedName;
        }
        
        // Mô tả
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = currentSeed.description;
        }
        
        // Giá đơn vị
        if (itemPriceText != null)
        {
            string currencySymbol = GetCurrencySymbol(currentSeed.currencyType);
            itemPriceText.text = $"{currencySymbol}{currentSeed.price}";
        }
    }
    
    private void UpdateQuantityDisplay()
    {
        if (quantityInputField != null)
        {
            quantityInputField.text = currentQuantity.ToString();
        }
        
        if (totalPriceText != null && currentSeed != null)
        {
            int totalPrice = currentSeed.price * currentQuantity;
            string currencySymbol = GetCurrencySymbol(currentSeed.currencyType);
            totalPriceText.text = $"Tổng: {currencySymbol}{totalPrice}";
        }
        
        // Cập nhật trạng thái nút tăng/giảm
        if (increaseQuantityButton != null)
        {
            increaseQuantityButton.interactable = currentQuantity < maxQuantity;
        }
        
        if (decreaseQuantityButton != null)
        {
            decreaseQuantityButton.interactable = currentQuantity > 1;
        }
    }
    
    private void UpdatePurchaseButton()
    {
        if (currentSeed == null || shopManager == null) return;
        
        bool canPurchase = CanPurchaseCurrentQuantity();
        bool isUnlocked = currentSeed.isUnlocked;
        
        if (purchaseButton != null)
        {
            purchaseButton.interactable = canPurchase && isUnlocked;
        }
    }
    
    private bool CanPurchaseCurrentQuantity()
    {
        if (currentSeed == null) return false;
        
        int totalCost = currentSeed.price * currentQuantity;
        
        switch (currentSeed.currencyType)
        {
            case SeedData.CurrencyType.Gold:
                return playerGoldManager != null && playerGoldManager.GetGold() >= totalCost;
            case SeedData.CurrencyType.Gems:
                // TODO: Implement gem system
                return true;
            case SeedData.CurrencyType.Coins:
                // TODO: Implement coin system
                return true;
            default:
                return false;
        }
    }
    
    private void IncreaseQuantity()
    {
        if (currentQuantity < maxQuantity)
        {
            currentQuantity++;
            UpdateQuantityDisplay();
            UpdatePurchaseButton();
        }
    }
    
    private void DecreaseQuantity()
    {
        if (currentQuantity > 1)
        {
            currentQuantity--;
            UpdateQuantityDisplay();
            UpdatePurchaseButton();
        }
    }
    
    private void PurchaseItems()
    {
        if (currentSeed == null || shopManager == null) return;
        
        // Mua từng item một (vì ShopManager hiện tại chỉ hỗ trợ mua 1 item)
        bool allPurchased = true;
        for (int i = 0; i < currentQuantity; i++)
        {
            if (!shopManager.PurchaseSeed(currentSeed))
            {
                allPurchased = false;
                break;
            }
        }
        
        if (allPurchased)
        {
            Debug.Log($"Đã mua thành công {currentQuantity} {currentSeed.seedName}");
            CloseDetailPanel();
        }
        else
        {
            Debug.LogWarning($"Không thể mua {currentQuantity} {currentSeed.seedName}");
        }
    }
    
    public void CloseDetailPanel()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
        
        currentSeed = null;
        currentQuantity = 1;
        
        Debug.Log("[ItemDetailPanel] Đã đóng ItemDetailPanel");
    }
    
    /// <summary>
    /// Đóng panel từ bên ngoài (được gọi từ ShopUI)
    /// </summary>
    public void ForceClosePanel()
    {
        CloseDetailPanel();
    }
    
    private string GetCurrencySymbol(SeedData.CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case SeedData.CurrencyType.Gold:
                return ""; // Bỏ icon vàng
            case SeedData.CurrencyType.Gems:
                return "💎";
            case SeedData.CurrencyType.Coins:
                return "🪙";
            default:
                return "";
        }
    }
    
    // Xử lý khi người dùng thay đổi input
    private void OnQuantityInputChanged(string value)
    {
        // Không cần xử lý gì ở đây, chỉ khi end edit
    }
    
    // Xử lý khi người dùng kết thúc nhập
    private void OnQuantityInputEndEdit(string value)
    {
        if (int.TryParse(value, out int newQuantity))
        {
            // Giới hạn số lượng trong khoảng hợp lệ
            newQuantity = Mathf.Clamp(newQuantity, 1, maxQuantity);
            currentQuantity = newQuantity;
            UpdateQuantityDisplay();
            UpdatePurchaseButton();
        }
        else
        {
            // Nếu nhập không hợp lệ, reset về giá trị hiện tại
            UpdateQuantityDisplay();
        }
    }
    
    // Cập nhật khi tiền thay đổi
    public void OnGoldChanged(int newGold)
    {
        UpdatePurchaseButton();
    }
}
