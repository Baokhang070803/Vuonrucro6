using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script để cấu hình layout cho shop items
/// </summary>
public class ShopLayoutHelper : MonoBehaviour
{
    [Header("Layout Settings")]
    public Vector2 cellSize = new Vector2(200, 150);
    public Vector2 spacing = new Vector2(10, 10);
    public int columns = 3;
    
    [Header("Layout Type")]
    public LayoutType layoutType = LayoutType.Grid;
    
    public enum LayoutType
    {
        Grid,
        Vertical,
        Horizontal
    }
    
    [ContextMenu("Setup Layout")]
    public void SetupLayout()
    {
        Transform container = transform;
        
        // Xóa layout cũ
        var oldLayouts = container.GetComponents<LayoutGroup>();
        foreach (var layout in oldLayouts)
        {
            if (Application.isPlaying)
                Destroy(layout);
            else
                DestroyImmediate(layout);
        }
        
        // Thêm layout mới
        switch (layoutType)
        {
            case LayoutType.Grid:
                SetupGridLayout(container);
                break;
            case LayoutType.Vertical:
                SetupVerticalLayout(container);
                break;
            case LayoutType.Horizontal:
                SetupHorizontalLayout(container);
                break;
        }
        
        Debug.Log($"Đã setup {layoutType} layout cho shop container");
    }
    
    private void SetupGridLayout(Transform container)
    {
        var gridLayout = container.gameObject.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = cellSize;
        gridLayout.spacing = spacing;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
    }
    
    private void SetupVerticalLayout(Transform container)
    {
        var verticalLayout = container.gameObject.AddComponent<VerticalLayoutGroup>();
        verticalLayout.spacing = spacing.y;
        verticalLayout.childAlignment = TextAnchor.UpperCenter;
        verticalLayout.childControlHeight = true;
        verticalLayout.childControlWidth = true;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = true;
    }
    
    private void SetupHorizontalLayout(Transform container)
    {
        var horizontalLayout = container.gameObject.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.spacing = spacing.x;
        horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
        horizontalLayout.childControlHeight = true;
        horizontalLayout.childControlWidth = true;
        horizontalLayout.childForceExpandHeight = true;
        horizontalLayout.childForceExpandWidth = false;
    }
    
    private void Start()
    {
        // Tự động setup layout khi start
        if (GetComponent<LayoutGroup>() == null)
        {
            SetupLayout();
        }
    }
}
