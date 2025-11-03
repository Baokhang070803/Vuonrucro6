using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// UI Controller cho hệ thống balo
/// </summary>
public class BagUI : MonoBehaviour
{
    [Header("Bag Panel")]
    public GameObject bagPanel; // Panel chứa toàn bộ UI balo
    public Button openBagButton; // Nút mở balo (kéo vào Inspector)
    
    [Header("Bag Grid")]
    public Transform bagGridParent; // Parent chứa các slot
    public GameObject bagSlotPrefab; // Prefab cho mỗi slot
    
    [Header("Item Detail Panel")]
    public GameObject itemDetailPanel; // Panel hiển thị chi tiết item
    public Image itemDetailIcon;
    public TextMeshProUGUI itemDetailName;
    public TextMeshProUGUI itemDetailDescription;
    public TextMeshProUGUI itemDetailQuantity;
    public TextMeshProUGUI itemDetailPrice;
    public Button sellButton;
    public Button dropButton;
    public Button useButton; // Nút Dùng cho hạt giống
    public Button closeDetailButton;
    
    [Header("Bag Info")]
    public TextMeshProUGUI bagInfoText; // Hiển thị "Slots: 5/20"
    public Button upgradeBagButton;     // Nút nâng cấp balo
    
    // Private variables
    private List<GameObject> bagSlots = new List<GameObject>();
    private BagItem selectedItem;
    private bool isBagOpen = false;
    
    private bool isInitialized = false;
    private bool isInitializing = false;
    
    void Start()
    {
        if (isInitialized || isInitializing) 
        {
            Debug.Log("[BagUI] BagUI đã được khởi tạo hoặc đang khởi tạo, bỏ qua!");
            return;
        }
        
        Debug.Log("[BagUI] Bắt đầu khởi tạo BagUI...");
        
        // Đảm bảo BagPanel active trước khi khởi tạo
        if (bagPanel != null && !bagPanel.activeInHierarchy)
        {
            bagPanel.SetActive(true);
            Debug.Log("[BagUI] Đã kích hoạt BagPanel trong Start()");
        }
        
        isInitializing = true;
        
        // Đợi một frame để đảm bảo tất cả managers đã khởi tạo
        StartCoroutine(InitializeWithDelay());
    }
    
    System.Collections.IEnumerator InitializeWithDelay()
    {
        // Đợi 1 frame để đảm bảo BagManager đã khởi tạo
        yield return null;
        
        Debug.Log("[BagUI] Đang kiểm tra BagManager...");
        
        // Đợi cho đến khi BagManager.Instance sẵn sàng
        int attempts = 0;
        while (BagManager.Instance == null && attempts < 50) // Tối đa 50 frame (khoảng 1 giây)
        {
            yield return null;
            attempts++;
            Debug.Log($"[BagUI] Đang đợi BagManager... Attempt {attempts}/50");
        }
        
        if (BagManager.Instance == null)
        {
            Debug.LogError("[BagUI] BagManager.Instance vẫn null sau 50 frame! Khởi tạo thất bại!");
            yield break;
        }
        
        Debug.Log("[BagUI] BagManager đã sẵn sàng! Tiếp tục khởi tạo...");
        
        InitializeUI();
        SetupEvents();
        
        // Đăng ký event từ BagManager
        BagManager.Instance.OnBagChanged += RefreshBagUI;
        Debug.Log("[BagUI] Đã đăng ký event OnBagChanged");
        
        // Đăng ký event từ BagUpgradeManager
        if (BagUpgradeManager.Instance != null)
        {
            BagUpgradeManager.Instance.OnBagUpgraded += OnBagUpgraded;
            Debug.Log("[BagUI] Đã đăng ký event OnBagUpgraded");
        }
        else
        {
            Debug.LogWarning("[BagUI] BagUpgradeManager.Instance is null!");
        }
        
        Debug.Log("[BagUI] Khởi tạo BagUI hoàn thành!");
        isInitialized = true;
        isInitializing = false;
        
        // Nếu balo đang mở, refresh UI
        if (isBagOpen)
        {
            RefreshBagUI();
            Debug.Log("[BagUI] Balo đang mở, đã refresh UI sau khởi tạo!");
        }
    }
    
    void Update()
    {
        // Đóng ItemDetailPanel khi ấn Escape (sử dụng Input System)
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && itemDetailPanel != null && itemDetailPanel.activeInHierarchy)
        {
            CloseItemDetail();
        }
        
        // Đóng ItemDetailPanel khi click chuột trái bên ngoài (sử dụng Input System)
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && itemDetailPanel != null && itemDetailPanel.activeInHierarchy)
        {
            // Kiểm tra xem click có nằm ngoài panel không
            if (!IsMouseOverPanel())
            {
                CloseItemDetail();
            }
        }
    }
    
    bool IsMouseOverPanel()
    {
        if (itemDetailPanel == null) return false;
        
        // Lấy RectTransform của panel
        RectTransform panelRect = itemDetailPanel.GetComponent<RectTransform>();
        if (panelRect == null) return false;
        
        // Lấy mouse position từ Input System
        var mouse = Mouse.current;
        if (mouse == null) return false;
        
        Vector2 mousePosition = mouse.position.ReadValue();
        
        // Chuyển đổi mouse position sang local position của panel
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect, 
            mousePosition, 
            null, 
            out localMousePosition
        );
        
        // Kiểm tra xem mouse có nằm trong bounds của panel không
        return panelRect.rect.Contains(localMousePosition);
    }
    
    void InitializeUI()
    {
        // Ẩn panel ban đầu (chỉ ẩn nếu balo chưa được mở)
        if (bagPanel != null && !isBagOpen)
            bagPanel.SetActive(false);
            
        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);
        
        // Setup nút nâng cấp balo
        if (upgradeBagButton != null)
        {
            upgradeBagButton.onClick.RemoveAllListeners(); // Xóa listeners cũ
            upgradeBagButton.onClick.AddListener(OpenUpgradePanel);
        }
        
        // Tạo grid slots
        CreateBagSlots();
        
        Debug.Log("[BagUI] Đã khởi tạo UI balo!");
    }
    
    void SetupEvents()
    {
        // ✅ XÓA TẤT CẢ LISTENERS CŨ TRƯỚC KHI THÊM MỚI (Tránh duplicate listeners)
        
        // Setup nút đóng detail panel
        if (closeDetailButton != null)
        {
            closeDetailButton.onClick.RemoveAllListeners();
            closeDetailButton.onClick.AddListener(CloseItemDetail);
        }
        
        // Setup nút bán
        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(SellSelectedItem);
        }
        
        // Setup nút bỏ
        if (dropButton != null)
        {
            dropButton.onClick.RemoveAllListeners();
            dropButton.onClick.AddListener(DropSelectedItem);
        }
        
        // Setup nút dùng
        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(UseSelectedItem);
        }
        
        Debug.Log("[BagUI] Đã setup tất cả button events (đã xóa listeners cũ)");
    }
    
    void CreateBagSlots()
    {
        if (bagGridParent == null || bagSlotPrefab == null) 
        {
            Debug.LogError("[BagUI] bagGridParent hoặc bagSlotPrefab là null!");
            return;
        }
        
        if (BagManager.Instance == null)
        {
            Debug.LogError("[BagUI] BagManager.Instance là null! Không thể tạo slots!");
            return;
        }
        
        Debug.Log($"[BagUI] Bắt đầu tạo {BagManager.Instance.maxBagSlots} slots...");
        
        // Xóa slots cũ
        foreach (Transform child in bagGridParent)
        {
            Destroy(child.gameObject);
        }
        bagSlots.Clear();
        
        // Tạo slots mới
        for (int i = 0; i < BagManager.Instance.maxBagSlots; i++)
        {
            GameObject slot = Instantiate(bagSlotPrefab, bagGridParent);
            bagSlots.Add(slot);
            
            // Setup slot
            SetupBagSlot(slot, i);
        }
        
        Debug.Log($"[BagUI] Đã tạo {bagSlots.Count} slots!");
    }
    
    void SetupBagSlot(GameObject slot, int index)
    {
        // Tìm các component trong slot
        Button slotButton = slot.GetComponent<Button>();
        Image slotIcon = slot.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI slotQuantity = slot.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
        
        if (slotButton != null)
        {
            int slotIndex = index; // Capture index
            slotButton.onClick.AddListener(() => OnSlotClicked(slotIndex));
        }
        
        // Ẩn icon và quantity ban đầu
        if (slotIcon != null)
        {
            slotIcon.gameObject.SetActive(false);
        }
        if (slotQuantity != null)
        {
            slotQuantity.gameObject.SetActive(false);
        }
    }
    
    void OnSlotClicked(int slotIndex)
    {
        if (BagManager.Instance == null) return;
        
        List<BagItem> items = BagManager.Instance.GetAllItems();
        
        if (slotIndex < items.Count)
        {
            selectedItem = items[slotIndex];
            ShowItemDetail(selectedItem);
        }
    }
    
    void ShowItemDetail(BagItem item)
    {
        if (itemDetailPanel == null || item == null) return;
        
        selectedItem = item;
        
        // Hiển thị thông tin item
        if (itemDetailIcon != null)
        {
            itemDetailIcon.sprite = item.itemIcon;
            itemDetailIcon.gameObject.SetActive(true);
        }
        
        if (itemDetailName != null)
            itemDetailName.text = item.itemName;
            
        if (itemDetailDescription != null)
            itemDetailDescription.text = item.description;
            
        if (itemDetailQuantity != null)
            itemDetailQuantity.text = $"Số lượng: {item.quantity}";
            
        if (itemDetailPrice != null)
            itemDetailPrice.text = $"Giá bán: {item.sellPrice}/cây";
        
        // Hiển thị/ẩn nút Dùng dựa trên loại item
        if (useButton != null)
        {
            bool isSeed = IsSeedItem(item.itemName);
            useButton.gameObject.SetActive(isSeed);
            
            if (isSeed)
            {
                useButton.GetComponentInChildren<TextMeshProUGUI>().text = "Trồng";
            }
        }
        
        // Hiển thị/ẩn nút Bán cho tất cả item
        if (sellButton != null)
        {
            sellButton.gameObject.SetActive(true);
            sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Bán";
        }
        
        // Hiển thị/ẩn nút Bỏ cho tất cả item
        if (dropButton != null)
        {
            dropButton.gameObject.SetActive(true);
            dropButton.GetComponentInChildren<TextMeshProUGUI>().text = "Bỏ";
        }
        
        // Hiển thị panel
        itemDetailPanel.SetActive(true);
        
        Debug.Log($"[BagUI] Hiển thị chi tiết: {item.itemName} x{item.quantity}");
    }
    
    /// <summary>
    /// Cập nhật ItemDetailPanel với thông tin mới
    /// </summary>
    void UpdateItemDetailDisplay()
    {
        if (selectedItem == null || itemDetailPanel == null) return;
        
        // Cập nhật số lượng
        if (itemDetailQuantity != null)
            itemDetailQuantity.text = $"Số lượng: {selectedItem.quantity}";
            
        // Cập nhật giá bán
        if (itemDetailPrice != null)
            itemDetailPrice.text = $"Giá bán: {selectedItem.sellPrice}/cây";
            
        Debug.Log($"[BagUI] Đã cập nhật ItemDetailPanel: {selectedItem.itemName} x{selectedItem.quantity}");
    }
    
    void CloseItemDetail()
    {
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
            selectedItem = null;
            Debug.Log("[BagUI] Đã đóng ItemDetailPanel!");
        }
        else
        {
            Debug.LogWarning("[BagUI] ItemDetailPanel is null!");
        }
    }
    
    void SellSelectedItem()
    {
        if (selectedItem == null || BagManager.Instance == null) return;
        
        string itemName = selectedItem.itemName;
        int goldEarned = BagManager.Instance.SellItem(itemName, 1);
        
        if (goldEarned > 0)
        {
            Debug.Log($"[BagUI] Đã bán 1 {itemName} và nhận {goldEarned} vàng!");
            
            // Hiển thị thông báo nhận vàng
            ShowGoldNotification(goldEarned);
            
            // Refresh UI trước
            RefreshBagUI();
            
            // Cập nhật selectedItem với dữ liệu mới
            UpdateSelectedItemAfterSell();
            
            // Đóng detail nếu item hết
            if (selectedItem == null || selectedItem.IsEmpty())
            {
                CloseItemDetail();
            }
            else
            {
                // Cập nhật ItemDetailPanel với thông tin mới
                UpdateItemDetailDisplay();
            }
        }
    }
    
    /// <summary>
    /// Cập nhật selectedItem sau khi bán
    /// </summary>
    void UpdateSelectedItemAfterSell()
    {
        if (selectedItem == null || BagManager.Instance == null) return;
        
        // Lấy item mới từ BagManager
        BagItem updatedItem = BagManager.Instance.GetItem(selectedItem.itemName);
        
        if (updatedItem != null)
        {
            // Cập nhật selectedItem với dữ liệu mới
            selectedItem = updatedItem;
            Debug.Log($"[BagUI] Đã cập nhật selectedItem: {selectedItem.itemName} x{selectedItem.quantity}");
        }
        else
        {
            // Item không còn trong túi
            selectedItem = null;
            Debug.Log("[BagUI] Item đã hết trong túi!");
        }
    }
    
    void DropSelectedItem()
    {
        if (selectedItem == null || BagManager.Instance == null) return;
        
        string itemName = selectedItem.itemName;
        
        // Xóa 1 item khỏi balo
        if (BagManager.Instance.RemoveItem(itemName, 1))
        {
            Debug.Log($"[BagUI] Đã bỏ 1 {itemName}!");
            
            // Refresh UI
            RefreshBagUI();
            
            // Cập nhật selectedItem
            UpdateSelectedItemAfterSell();
            
            // Đóng detail nếu item hết
            if (selectedItem == null || selectedItem.IsEmpty())
            {
                CloseItemDetail();
            }
            else
            {
                // Cập nhật ItemDetailPanel
                UpdateItemDetailDisplay();
            }
        }
    }
    
    void UseSelectedItem()
    {
        // Kiểm tra nếu đang trong chế độ nâng cấp balo
        if (selectedItem == null && itemDetailPanel != null && itemDetailPanel.activeInHierarchy)
        {
            // Kiểm tra xem có phải đang hiển thị thông tin nâng cấp không
            if (itemDetailName != null && itemDetailName.text == "Nâng Cấp Balo")
            {
                // Thực hiện nâng cấp balo
                PerformBagUpgrade();
                return;
            }
        }
        
        if (selectedItem == null || BagManager.Instance == null) return;
        
        string itemName = selectedItem.itemName;
        
        // Kiểm tra xem có phải hạt giống không
        if (IsSeedItem(itemName))
        {
            // Gọi PlayerFarmController để trồng hạt giống (KHÔNG sử dụng hạt giống ở đây)
            PlayerFarmController farmController = FindObjectOfType<PlayerFarmController>();
            if (farmController != null)
            {
                // Chỉ gọi trồng hạt giống, PlayerFarmController sẽ tự sử dụng hạt giống
                farmController.PlantSeedFromBag(itemName, 1);
                
                // KHÔNG đóng balo ngay lập tức - để người chơi có thể chọn hạt giống khác
                // CloseBagImmediately(); // ← ĐÃ XÓA DÒNG NÀY
                
                // Refresh UI
                RefreshBagUI();
                
                // Cập nhật selectedItem sau khi sử dụng
                UpdateSelectedItemAfterSell();
                
                // Đóng detail nếu item hết
                if (selectedItem == null || selectedItem.IsEmpty())
                {
                    CloseItemDetail();
                }
            }
            else
            {
                Debug.LogWarning("[BagUI] Không tìm thấy PlayerFarmController!");
            }
        }
        else
        {
            Debug.LogWarning($"[BagUI] {itemName} không phải hạt giống!");
        }
    }
    
    /// <summary>
    /// Thực hiện nâng cấp balo
    /// </summary>
    void PerformBagUpgrade()
    {
        if (BagUpgradeManager.Instance != null)
        {
            BagUpgradeManager.Instance.UpgradeBag();
            Debug.Log("[BagUI] Đã thực hiện nâng cấp balo!");
            
            // Cập nhật thông tin trong ItemDetailPanel
            ShowUpgradeItemDetail();
        }
        else
        {
            Debug.LogWarning("[BagUI] BagUpgradeManager.Instance is null!");
        }
    }
    
    bool IsSeedItem(string itemName)
    {
        // Kiểm tra xem item có phải hạt giống không
        string[] seedNames = { 
            "Hạt Giống Cơ Bản", "Hạt Giống", "Seed", 
            "Blueberry", "Strawberry", "Tomato",
            "Bí ngô", "Cà chua", "Cà rốt", "Khoai tây",
            "Bắp cải", "Ớt", "Dưa chuột", "Hành tây",
            "Tỏi", "Rau diếp", "Cà tím", "Ớt đỏ"
        };
        
        foreach (string seedName in seedNames)
        {
            if (itemName.Contains(seedName) || itemName.ToLower().Contains("seed") || itemName.ToLower().Contains("hạt"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    bool IsHarvestableItem(string itemName)
    {
        // Kiểm tra xem item có phải sản phẩm thu hoạch không (có thể bán)
        string[] harvestableNames = { 
            "Dâu Xanh", "Hoa Hướng Dương", "Blueberry", "Strawberry", "Tomato", "Carrot", "Potato",
            "Bí Ngô", "Ớt", "Dâu xanh", "Bí ngô", "Ớt đỏ"
        };
        
        foreach (string harvestName in harvestableNames)
        {
            if (itemName.Contains(harvestName) || itemName.ToLower().Contains("hoa") || itemName.ToLower().Contains("dâu"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    public void ToggleBag()
    {
        if (bagPanel == null) 
        {
            Debug.LogError("[BagUI] bagPanel là null! Không thể toggle balo!");
            return;
        }
        
        if (BagManager.Instance == null)
        {
            Debug.LogError("[BagUI] BagManager.Instance là null! Không thể toggle balo!");
            return;
        }
        
        // Nếu chưa khởi tạo, khởi tạo ngay
        if (!isInitialized && !isInitializing)
        {
            Debug.Log("[BagUI] Chưa khởi tạo, khởi tạo ngay...");
            
            // Đảm bảo BagPanel active trước khi start coroutine
            if (bagPanel != null && !bagPanel.activeInHierarchy)
            {
                bagPanel.SetActive(true);
                isBagOpen = true; // Đánh dấu balo đã mở
                Debug.Log("[BagUI] Đã kích hoạt BagPanel để khởi tạo");
            }
            
            StartCoroutine(InitializeWithDelay());
            return;
        }
        
        // Nếu đang khởi tạo, đợi
        if (isInitializing)
        {
            Debug.Log("[BagUI] Đang khởi tạo, bỏ qua toggle...");
            return;
        }
        
        Debug.Log($"[BagUI] ToggleBag được gọi! Current state: {isBagOpen}");
        
        isBagOpen = !isBagOpen;
        bagPanel.SetActive(isBagOpen);
        
        if (isBagOpen)
        {
            // Đóng ItemDetailPanel khi mở balo
            CloseItemDetail();
            RefreshBagUI();
            Debug.Log("[BagUI] Balo đã mở và refresh UI!");
        }
        else
        {
            Debug.Log("[BagUI] Balo đã đóng!");
        }
        
        Debug.Log($"[BagUI] Balo {(isBagOpen ? "mở" : "đóng")}!");
    }
    
    /// <summary>
    /// Đóng balo ngay lập tức (không toggle)
    /// </summary>
    public void CloseBagImmediately()
    {
        if (bagPanel != null)
        {
            bagPanel.SetActive(false);
            isBagOpen = false;
            
            // Đóng ItemDetailPanel khi đóng balo
            CloseItemDetail();
            
            Debug.Log("[BagUI] Đã đóng balo ngay lập tức!");
        }
    }
    
    void RefreshBagUI()
    {
        if (BagManager.Instance == null) return;
        
        List<BagItem> items = BagManager.Instance.GetAllItems();
        
        // Cập nhật slots
        for (int i = 0; i < bagSlots.Count; i++)
        {
            GameObject slot = bagSlots[i];
            
            // Tìm components
            Image slotIcon = slot.transform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI slotQuantity = slot.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
            
            if (i < items.Count)
            {
                // Có item trong slot này
                BagItem item = items[i];
                
                if (slotIcon != null)
                {
                    slotIcon.sprite = item.itemIcon;
                    slotIcon.gameObject.SetActive(true);
                }
                
                if (slotQuantity != null)
                {
                    slotQuantity.text = item.quantity.ToString();
                    slotQuantity.gameObject.SetActive(true);
                }
            }
            else
            {
                // Slot trống
                if (slotIcon != null)
                    slotIcon.gameObject.SetActive(false);
                    
                if (slotQuantity != null)
                    slotQuantity.gameObject.SetActive(false);
            }
        }
        
        // Cập nhật thông tin balo
        if (bagInfoText != null)
        {
            bagInfoText.text = $"Slots: {items.Count}/{BagManager.Instance.maxBagSlots}";
        }
        
        Debug.Log($"[BagUI] Đã refresh UI balo! Items: {items.Count}");
    }
    
    /// <summary>
    /// Đóng ItemDetailPanel khi mở balo mới
    /// </summary>
    public void CloseItemDetailOnBagOpen()
    {
        CloseItemDetail();
        Debug.Log("[BagUI] Đã đóng ItemDetailPanel khi mở balo mới!");
    }
    
    /// <summary>
    /// Hiển thị thông báo nhận vàng
    /// </summary>
    void ShowGoldNotification(int goldAmount)
    {
        Debug.Log($"💰 Nhận được {goldAmount} vàng!");
        
        // TODO: Có thể thêm popup UI đẹp hơn ở đây
        // VD: Floating text, popup panel, animation
        
        // Hiển thị thông báo trong Console
        if (DialogueManager.I != null)
        {
            var messages = new List<string> 
            { 
                $"💰 Nhận được {goldAmount} vàng!",
                "Vàng đã được cộng vào tài khoản."
            };
            DialogueManager.I.Show(messages);
        }
    }
    
    /// <summary>
    /// Mở panel nâng cấp balo
    /// </summary>
    public void OpenUpgradePanel()
    {
        if (BagUpgradeManager.Instance != null)
        {
            BagUpgradeManager.Instance.OpenUpgradePanel();
            Debug.Log("[BagUI] Đã mở panel nâng cấp balo!");
        }
        else
        {
            Debug.LogWarning("[BagUI] BagUpgradeManager.Instance is null!");
        }
        
        // Hiện ItemDetailPanel khi ấn nút nâng cấp
        ShowUpgradeItemDetail();
    }
    
    /// <summary>
    /// Hiện ItemDetailPanel với thông tin nâng cấp balo
    /// </summary>
    void ShowUpgradeItemDetail()
    {
        if (itemDetailPanel == null) return;
        
        // Hiện panel
        itemDetailPanel.SetActive(true);
        
        // Cập nhật thông tin nâng cấp
        if (itemDetailIcon != null)
        {
            // Có thể set icon nâng cấp ở đây
            itemDetailIcon.gameObject.SetActive(false);
        }
        
        if (itemDetailName != null)
            itemDetailName.text = "Nâng Cấp Balo";
            
        if (itemDetailDescription != null)
        {
            int currentCapacity = BagUpgradeManager.Instance != null ? BagUpgradeManager.Instance.GetBagCapacity() : 20;
            int nextCapacity = BagUpgradeManager.Instance != null ? BagUpgradeManager.Instance.GetNextBagCapacity() : 22;
            int upgradeCost = BagUpgradeManager.Instance != null ? BagUpgradeManager.Instance.GetUpgradeCost() : 100;
            
            itemDetailDescription.text = $"Tăng số ô từ {currentCapacity} lên {nextCapacity}\nChi phí: {upgradeCost} vàng";
        }
            
        if (itemDetailQuantity != null)
            itemDetailQuantity.text = $"Cấp độ hiện tại: {(BagUpgradeManager.Instance != null ? BagUpgradeManager.Instance.currentBagLevel : 1)}";
            
        if (itemDetailPrice != null)
        {
            int upgradeCost = BagUpgradeManager.Instance != null ? BagUpgradeManager.Instance.GetUpgradeCost() : 100;
            itemDetailPrice.text = $"Chi phí nâng cấp: {upgradeCost} vàng";
        }
        
        // Ẩn các nút không cần thiết cho nâng cấp
        if (sellButton != null)
            sellButton.gameObject.SetActive(false);
            
        if (dropButton != null)
            dropButton.gameObject.SetActive(false);
            
        if (useButton != null)
        {
            useButton.gameObject.SetActive(true);
            useButton.GetComponentInChildren<TextMeshProUGUI>().text = "Nâng Cấp";
        }
        
        Debug.Log("[BagUI] Đã hiện ItemDetailPanel cho nâng cấp balo!");
    }
    
    /// <summary>
    /// Xử lý khi balo được nâng cấp
    /// </summary>
    void OnBagUpgraded(int newCapacity)
    {
        Debug.Log($"[BagUI] Balo đã được nâng cấp! Số ô mới: {newCapacity}");
        
        // Tạo lại các slot với số lượng mới
        CreateBagSlots();
        
        // Refresh UI
        RefreshBagUI();
        
        Debug.Log($"[BagUI] Đã tạo lại {newCapacity} slots!");
    }
    
    void OnDestroy()
    {
        // Hủy đăng ký events
        if (BagManager.Instance != null)
        {
            BagManager.Instance.OnBagChanged -= RefreshBagUI;
        }
        
        if (BagUpgradeManager.Instance != null)
        {
            BagUpgradeManager.Instance.OnBagUpgraded -= OnBagUpgraded;
        }
    }
}
