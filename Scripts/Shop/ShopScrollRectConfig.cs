using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script cấu hình ScrollRect cho cửa hàng với layout 4x4
/// </summary>
public class ShopScrollRectConfig : MonoBehaviour
{
    [Header("Layout Settings")]
    public int maxItemsWithoutScroll = 16; // 4x4 = 16 items
    public int columnsPerRow = 4;
    
    [Header("ScrollRect References")]
    public ScrollRect scrollRect;
    public RectTransform contentTransform;
    
    private void Start()
    {
        // Tự động tìm ScrollRect nếu chưa gán
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
        
        // Tự động tìm content transform
        if (contentTransform == null && scrollRect != null)
        {
            contentTransform = scrollRect.content;
        }
        
        // Cấu hình ScrollRect
        ConfigureScrollRect();
    }
    
    /// <summary>
    /// Cấu hình ScrollRect với thiết lập tối ưu cho layout 4x4
    /// </summary>
    public void ConfigureScrollRect()
    {
        if (scrollRect == null) return;
        
        // Cấu hình cơ bản
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.elasticity = 0.1f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;
        scrollRect.scrollSensitivity = 20f;
        
        // Cấu hình thanh cuộn
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        
        // Cấu hình Content Size Fitter
        SetupContentSizeFitter();
        
        Debug.Log("[ShopScrollRectConfig] Đã cấu hình ScrollRect cho layout 4x4");
    }
    
    /// <summary>
    /// Thiết lập Content Size Fitter để tự động điều chỉnh kích thước
    /// </summary>
    private void SetupContentSizeFitter()
    {
        if (contentTransform == null) return;
        
        var contentSizeFitter = contentTransform.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
        }
        
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
    
    /// <summary>
    /// Cập nhật hiển thị thanh cuộn dựa trên số lượng items
    /// </summary>
    public void UpdateScrollbarVisibility(int itemCount)
    {
        if (scrollRect == null) return;
        
        // Tính toán số hàng
        int rows = Mathf.CeilToInt((float)itemCount / columnsPerRow);
        int maxRowsWithoutScroll = maxItemsWithoutScroll / columnsPerRow; // 4 hàng cho 4x4
        
        if (rows <= maxRowsWithoutScroll)
        {
            // Ẩn thanh cuộn hoàn toàn khi không cần
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            Debug.Log($"[ShopScrollRectConfig] {itemCount} items ({rows} hàng) - Ẩn thanh cuộn");
        }
        else
        {
            // Hiện thanh cuộn khi cần
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            Debug.Log($"[ShopScrollRectConfig] {itemCount} items ({rows} hàng) - Hiện thanh cuộn khi cần");
        }
    }
    
    /// <summary>
    /// Kiểm tra xem có cần thanh cuộn không
    /// </summary>
    public bool NeedsScrollbar(int itemCount)
    {
        int rows = Mathf.CeilToInt((float)itemCount / columnsPerRow);
        int maxRowsWithoutScroll = maxItemsWithoutScroll / columnsPerRow;
        return rows > maxRowsWithoutScroll;
    }
    
    /// <summary>
    /// Lấy số hàng tối đa không cần cuộn
    /// </summary>
    public int GetMaxRowsWithoutScroll()
    {
        return maxItemsWithoutScroll / columnsPerRow;
    }
    
    [ContextMenu("Test Scrollbar Visibility")]
    public void TestScrollbarVisibility()
    {
        // Test với các số lượng items khác nhau
        int[] testCounts = { 4, 8, 12, 16, 20, 24 };
        
        foreach (int count in testCounts)
        {
            bool needsScroll = NeedsScrollbar(count);
            Debug.Log($"Test {count} items: Cần thanh cuộn = {needsScroll}");
        }
    }
}
