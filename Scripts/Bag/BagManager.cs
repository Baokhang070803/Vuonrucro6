using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;

/// <summary>
/// Manager quản lý hệ thống balo
/// </summary>
public class BagManager : MonoBehaviour
{
    public static BagManager Instance;
    
    [Header("Bag Settings")]
    public int maxBagSlots = 20; // Số slot tối đa trong balo (có thể được nâng cấp)
    
    [Header("🌾 HẠT GIỐNG (Seed Icons) - Dùng trong Shop")]
    public Sprite seedSunflowerIcon;    // Icon hạt giống hoa hướng dương
    public Sprite seedPumpkinIcon;      // Icon hạt giống bí ngô
    public Sprite seedPepperIcon;       // Icon hạt giống ớt
    public Sprite seedEggplantIcon;     // Icon hạt giống cà tím
    
    [Header("🍎 SẢN PHẨM THU HOẠCH (Harvest Icons) - Dùng trong Balo")]
    public Sprite harvestDâuXanhIcon;   // Icon dâu xanh (sản phẩm thu hoạch)
    public Sprite harvestBíNgôIcon;     // Icon bí ngô (sản phẩm thu hoạch)
    public Sprite harvestỚtIcon;        // Icon ớt (sản phẩm thu hoạch)
    public Sprite harvestCàTímIcon;     // Icon cà tím (sản phẩm thu hoạch)
    
    [Header("⚠️ FALLBACK - Icon mặc định nếu không tìm thấy")]
    public Sprite defaultIcon;          // Icon mặc định (thường là dâu xanh)
    
    // Danh sách items trong balo
    private List<BagItem> bagItems = new List<BagItem>();
    
    // Events
    public System.Action<BagItem> OnItemAdded;
    public System.Action<BagItem> OnItemRemoved;
    public System.Action OnBagChanged;
    
    // Firebase sync
    private DatabaseReference bagReference;
    private bool isInitialized = false;
    
    // Firebase sync cooldown để tránh spam
    private float lastSaveTime = 0f;
    private float saveCooldown = 1f; // 1 giây cooldown
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Khởi tạo Firebase reference
        InitializeFirebase();
        
        // Khởi tạo balo rỗng
        bagItems.Clear();
        
        // Load dữ liệu từ Firebase
        LoadBagFromFirebase();
        
        Debug.Log("[BagManager] Đã khởi tạo balo!");
    }
    
    /// <summary>
    /// Khởi tạo Firebase reference
    /// </summary>
    void InitializeFirebase()
    {
        if (LoadDataManager.firebaseUser != null)
        {
            bagReference = FirebaseDatabase.DefaultInstance
                .GetReference("Users")
                .Child(LoadDataManager.firebaseUser.UserId)
                .Child("BagData");
            
            Debug.Log("[BagManager] Đã khởi tạo Firebase reference!");
        }
        else
        {
            Debug.LogWarning("[BagManager] FirebaseUser is null! Không thể đồng bộ balo.");
        }
    }
    
    /// <summary>
    /// Thêm item vào balo
    /// </summary>
    public bool AddItem(string itemName, Sprite itemIcon, int quantity = 1, int sellPrice = 10)
    {
        // Tìm item đã có trong balo
        BagItem existingItem = bagItems.FirstOrDefault(item => item.itemName == itemName);
        
        if (existingItem != null)
        {
            // Item đã có, chỉ cần tăng số lượng
            existingItem.AddQuantity(quantity);
            Debug.Log($"[BagManager] Đã thêm {quantity} {itemName} vào balo. Tổng: {existingItem.quantity}");
            
            OnItemAdded?.Invoke(existingItem);
            OnBagChanged?.Invoke();
            
            // Tự động lưu vào Firebase
            SaveBagToFirebase();
            
            return true;
        }
        else
        {
            // Item mới, kiểm tra còn slot không
            if (bagItems.Count >= maxBagSlots)
            {
                Debug.LogWarning($"[BagManager] Balo đã đầy! Không thể thêm {itemName}");
                return false;
            }
            
            // Tạo item mới
            BagItem newItem = new BagItem(itemName, itemIcon, quantity, sellPrice);
            bagItems.Add(newItem);
            
            Debug.Log($"[BagManager] Đã thêm item mới: {newItem}");
            
            OnItemAdded?.Invoke(newItem);
            OnBagChanged?.Invoke();
            
            // Tự động lưu vào Firebase
            SaveBagToFirebase();
            
            return true;
        }
    }
    
    /// <summary>
    /// Thêm hoa hướng dương vào balo
    /// </summary>
    public bool AddSunflower(int quantity = 1)
    {
        // Sử dụng harvest icon (sản phẩm) khi thêm vào balo
        Sprite icon = harvestDâuXanhIcon != null ? harvestDâuXanhIcon : seedSunflowerIcon;
        return AddItem("Hoa Hướng Dương", icon, quantity, 15); // Giá 15 vàng mỗi cây
    }
    
    /// <summary>
    /// Xóa item khỏi balo
    /// </summary>
    public bool RemoveItem(string itemName, int quantity = 1)
    {
        BagItem item = bagItems.FirstOrDefault(i => i.itemName == itemName);
        
        if (item != null)
        {
            // Kiểm tra có đủ số lượng để xóa không
            if (item.quantity >= quantity)
            {
                item.RemoveQuantity(quantity);
                Debug.Log($"[BagManager] Đã xóa {quantity} {itemName} khỏi balo. Còn lại: {item.quantity}");
                
                // Nếu item hết thì xóa khỏi danh sách
                if (item.IsEmpty())
                {
                    bagItems.Remove(item);
                    Debug.Log($"[BagManager] Đã xóa hoàn toàn {itemName} khỏi balo");
                }
                
                OnItemRemoved?.Invoke(item);
                OnBagChanged?.Invoke();
                
                // Tự động lưu vào Firebase
                SaveBagToFirebase();
                
                return true;
            }
            else
            {
                Debug.LogWarning($"[BagManager] Không đủ {itemName} để xóa!");
                return false;
            }
        }
        
        Debug.LogWarning($"[BagManager] Không tìm thấy {itemName} trong balo!");
        return false;
    }
    
    /// <summary>
    /// Bán item
    /// </summary>
    public int SellItem(string itemName, int quantity = 1)
    {
        BagItem item = bagItems.FirstOrDefault(i => i.itemName == itemName);
        
        if (item != null && item.quantity >= quantity)
        {
            int totalPrice = item.sellPrice * quantity;
            
            // Xóa item khỏi balo
            if (RemoveItem(itemName, quantity))
            {
                Debug.Log($"[BagManager] Đã bán {quantity} {itemName} với giá {totalPrice} vàng!");
                
                // Thêm vàng vào tài khoản người chơi
                if (PlayerGoldManager.Instance != null)
                {
                    PlayerGoldManager.Instance.AddGold(totalPrice);
                }
                else
                {
                    Debug.LogWarning("[BagManager] PlayerGoldManager.Instance is null! Không thể cộng vàng.");
                }
                
                return totalPrice;
            }
        }
        
        return 0;
    }
    
    /// <summary>
    /// Lấy danh sách tất cả items trong balo
    /// </summary>
    public List<BagItem> GetAllItems()
    {
        return new List<BagItem>(bagItems);
    }
    
    /// <summary>
    /// Lấy item theo tên
    /// </summary>
    public BagItem GetItem(string itemName)
    {
        return bagItems.FirstOrDefault(item => item.itemName == itemName);
    }
    
    /// <summary>
    /// Kiểm tra có item trong balo không
    /// </summary>
    public bool HasItem(string itemName)
    {
        return bagItems.Any(item => item.itemName == itemName && !item.IsEmpty());
    }
    
    /// <summary>
    /// Lấy số lượng item
    /// </summary>
    public int GetItemQuantity(string itemName)
    {
        BagItem item = GetItem(itemName);
        return item != null ? item.quantity : 0;
    }
    
    /// <summary>
    /// Sử dụng item (giảm số lượng)
    /// </summary>
    public bool UseItem(string itemName, int quantity = 1)
    {
        return RemoveItem(itemName, quantity);
    }
    
    /// <summary>
    /// Kiểm tra balo có đầy không
    /// </summary>
    public bool IsBagFull()
    {
        return bagItems.Count >= maxBagSlots;
    }
    
    /// <summary>
    /// Lấy số slot còn trống
    /// </summary>
    public int GetEmptySlots()
    {
        return maxBagSlots - bagItems.Count;
    }
    
    /// <summary>
    /// Load balo từ BagData (được gọi bởi PlayerDataSyncManager)
    /// </summary>
    public void LoadBagFromData(BagData bagData)
    {
        if (bagData == null) return;
        
        // Clear balo hiện tại
        bagItems.Clear();
        
        // Load items từ BagData
        foreach (var itemData in bagData.items)
        {
            // ✅ FIX: Lấy icon đúng theo tên item thay vì luôn dùng sunflowerIcon
            Sprite correctIcon = GetIconForItem(itemData.itemName);
            BagItem item = itemData.ToBagItem(correctIcon);
            bagItems.Add(item);
        }
        
        Debug.Log($"[BagManager] Đã load {bagItems.Count} items từ BagData!");
        
        // Trigger event để UI cập nhật
        OnBagChanged?.Invoke();
    }
    
    /// <summary>
    /// Load balo từ Firebase (deprecated - sử dụng PlayerDataSyncManager)
    /// </summary>
    public void LoadBagFromFirebase()
    {
        if (bagReference == null)
        {
            Debug.LogWarning("[BagManager] BagReference is null! Không thể load từ Firebase.");
            return;
        }
        
        Debug.Log("[BagManager] Đang load balo từ Firebase...");
        
        bagReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[BagManager] Lỗi khi load balo từ Firebase: {task.Exception}");
                return;
            }
            
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                
                if (snapshot.Exists && !string.IsNullOrEmpty(snapshot.Value.ToString()))
                {
                    try
                    {
                        string jsonData = snapshot.Value.ToString();
                        BagData bagData = JsonConvert.DeserializeObject<BagData>(jsonData);
                        
                        // Clear balo hiện tại
                        bagItems.Clear();
                        
                        // Load items từ Firebase
                        foreach (var itemData in bagData.items)
                        {
                            // ✅ FIX: Lấy icon đúng theo tên item
                            Sprite correctIcon = GetIconForItem(itemData.itemName);
                            BagItem item = itemData.ToBagItem(correctIcon);
                            bagItems.Add(item);
                        }
                        
                        Debug.Log($"[BagManager] Đã load {bagItems.Count} items từ Firebase!");
                        
                        // Trigger event để UI cập nhật
                        OnBagChanged?.Invoke();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[BagManager] Lỗi khi parse dữ liệu balo: {e.Message}");
                    }
                }
                else
                {
                    Debug.Log("[BagManager] Không có dữ liệu balo trong Firebase. Tạo balo mới.");
                }
                
                isInitialized = true;
            }
        });
    }
    
    /// <summary>
    /// Lưu balo lên Firebase (sử dụng PlayerDataSyncManager)
    /// </summary>
    public void SaveBagToFirebase()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[BagManager] Chưa khởi tạo xong! Không thể lưu.");
            return;
        }
        
        // Kiểm tra cooldown để tránh spam Firebase
        if (Time.time - lastSaveTime < saveCooldown)
        {
            Debug.Log("[BagManager] Đang trong cooldown, bỏ qua lưu Firebase!");
            return;
        }
        
        try
        {
            // Convert bagItems thành BagData
            BagData bagData = new BagData();
            foreach (var item in bagItems)
            {
                bagData.items.Add(new BagItemData(item));
            }
            
            // Sử dụng PlayerDataSyncManager để tránh xung đột dữ liệu
            if (PlayerDataSyncManager.Instance != null)
            {
                PlayerDataSyncManager.Instance.UpdateBagData(bagData);
                lastSaveTime = Time.time; // Cập nhật thời gian lưu cuối
                Debug.Log($"[BagManager] Đã gửi {bagItems.Count} items để lưu lên Firebase!");
            }
            else
            {
                Debug.LogWarning("[BagManager] PlayerDataSyncManager.Instance is null!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[BagManager] Lỗi khi serialize dữ liệu balo: {e.Message}");
        }
    }
    
    /// <summary>
    /// Đồng bộ balo (load từ Firebase)
    /// </summary>
    public void SyncBagFromFirebase()
    {
        Debug.Log("[BagManager] Đang đồng bộ balo từ Firebase...");
        LoadBagFromFirebase();
    }
    
    /// <summary>
    /// Đồng bộ balo (lưu lên Firebase)
    /// </summary>
    public void SyncBagToFirebase()
    {
        Debug.Log("[BagManager] Đang đồng bộ balo lên Firebase...");
        SaveBagToFirebase();
    }
    
    /// <summary>
    /// Thiết lập số slot tối đa của balo (dùng cho nâng cấp)
    /// </summary>
    public void SetMaxBagSlots(int newMaxSlots)
    {
        maxBagSlots = newMaxSlots;
        Debug.Log($"[BagManager] Đã cập nhật số slot tối đa: {maxBagSlots}");
        
        // Trigger event để UI cập nhật
        OnBagChanged?.Invoke();
    }
    
    /// <summary>
    /// Lấy số slot tối đa hiện tại
    /// </summary>
    public int GetMaxBagSlots()
    {
        return maxBagSlots;
    }
    
    /// <summary>
    /// Lấy số slot đã sử dụng
    /// </summary>
    public int GetUsedBagSlots()
    {
        return bagItems.Count;
    }
    
    /// <summary>
    /// Lấy số slot còn trống
    /// </summary>
    public int GetEmptyBagSlots()
    {
        return maxBagSlots - bagItems.Count;
    }
    
    /// <summary>
    /// Lấy icon đúng cho item dựa trên tên
    /// PHÂN BIỆT: Hạt giống vs Sản phẩm thu hoạch
    /// </summary>
    private Sprite GetIconForItem(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            return defaultIcon != null ? defaultIcon : harvestDâuXanhIcon;
        }
        
        // Chuẩn hóa tên item (lowercase, trim)
        string normalizedName = itemName.Trim().ToLower();
        
        // ✅ KIỂM TRA XEM LÀ HẠT GIỐNG HAY SẢN PHẨM
        bool isSeed = normalizedName.Contains("hạt") || normalizedName.Contains("seed") || normalizedName.Contains("giống");
        
        // Mapping tên item → icon
        switch (normalizedName)
        {
            // ===== DÂU XANH / HOA HƯỚNG DƯƠNG / BLUEBERRY =====
            case "dâu xanh":
            case "hoa hướng dương":
            case "sunflower":
            case "blueberry": // ✅ Sản phẩm thu hoạch
                return harvestDâuXanhIcon != null ? harvestDâuXanhIcon : defaultIcon;
            
            case "hạt giống cơ bản":
            case "hạt dâu xanh":
            case "hạt hoa hướng dương":
            case "hạt giống hoa hướng dương":
            case "hạt blueberry": // ✅ THÊM - Hạt giống từ shop
            case "hạt giống blueberry": // ✅ THÊM
            case "seed sunflower":
            case "seed blueberry": // ✅ THÊM
                return seedSunflowerIcon != null ? seedSunflowerIcon : defaultIcon;
            
            // ===== BÍ NGÔ / PUMPKIN =====
            case "bí ngô": // Sản phẩm thu hoạch (thu hoạch từ cây)
            case "pumpkin":
                return harvestBíNgôIcon != null ? harvestBíNgôIcon : defaultIcon;
            
            case "hạt bí ngô": // Hạt giống (mua từ shop)
            case "hạt giống bí ngô":
            case "hạt pumpkin": // ✅ THÊM
            case "hạt giống pumpkin": // ✅ THÊM
            case "seed pumpkin":
                return seedPumpkinIcon != null ? seedPumpkinIcon : defaultIcon;
            
            // ===== ỚT =====
            case "ớt": // Sản phẩm thu hoạch (thu hoạch từ cây)
            case "ớt đỏ":
            case "pepper":
                return harvestỚtIcon != null ? harvestỚtIcon : defaultIcon;
            
            case "hạt ớt": // Hạt giống (mua từ shop)
            case "hạt ớt đỏ": // ✅ THÊM case này!
            case "hạt giống ớt":
            case "hạt giống ớt đỏ": // ✅ THÊM case này!
            case "seed pepper":
                return seedPepperIcon != null ? seedPepperIcon : defaultIcon;
            
            // ===== CÀ TÍM / EGGPLANT =====
            case "cà tím": // Sản phẩm thu hoạch (thu hoạch từ cây)
            case "eggplant":
                return harvestCàTímIcon != null ? harvestCàTímIcon : defaultIcon;
            
            case "hạt cà tím": // Hạt giống (mua từ shop)
            case "hạt giống cà tím":
            case "hạt eggplant": // ✅ THÊM
            case "hạt giống eggplant": // ✅ THÊM
            case "seed eggplant":
                return seedEggplantIcon != null ? seedEggplantIcon : defaultIcon;
            
            // ===== DEFAULT =====
            default:
                Debug.LogWarning($"[BagManager] ⚠️ Không tìm thấy icon cho '{itemName}', dùng defaultIcon");
                return defaultIcon != null ? defaultIcon : harvestDâuXanhIcon;
        }
    }
    
    /// <summary>
    /// Debug: In thông tin balo
    /// </summary>
    [ContextMenu("Debug Bag Info")]
    public void DebugBagInfo()
    {
        Debug.Log("=== BAG INFO ===");
        Debug.Log($"Số items: {bagItems.Count}");
        Debug.Log($"Số slot tối đa: {maxBagSlots}");
        Debug.Log($"Số slot còn trống: {maxBagSlots - bagItems.Count}");
        
        foreach (var item in bagItems)
        {
            Debug.Log($"- {item.itemName}: {item.quantity} (Giá: {item.sellPrice})");
        }
    }
}
