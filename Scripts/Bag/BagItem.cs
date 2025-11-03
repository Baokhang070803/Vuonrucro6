using UnityEngine;

/// <summary>
/// Class đại diện cho một item trong balo
/// </summary>
[System.Serializable]
public class BagItem
{
    public string itemName;
    public Sprite itemIcon;
    public int quantity;
    public int sellPrice;
    public string description;

    public BagItem()
    {
        itemName = "";
        itemIcon = null;
        quantity = 0;
        sellPrice = 0;
        description = "";
    }

    public BagItem(string name, Sprite icon, int qty, int price)
    {
        itemName = name;
        itemIcon = icon;
        quantity = qty;
        sellPrice = price;
        description = "";
    }

    public BagItem(string name, Sprite icon, int qty, int price, string desc)
    {
        itemName = name;
        itemIcon = icon;
        quantity = qty;
        sellPrice = price;
        description = desc;
    }

    /// <summary>
    /// Thêm số lượng
    /// </summary>
    public void AddQuantity(int amount)
    {
        quantity += amount;
    }

    /// <summary>
    /// Giảm số lượng
    /// </summary>
    public void RemoveQuantity(int amount)
    {
        quantity = Mathf.Max(0, quantity - amount);
    }

    /// <summary>
    /// Kiểm tra item có rỗng không
    /// </summary>
    public bool IsEmpty()
    {
        return quantity <= 0;
    }

    /// <summary>
    /// Lấy tổng giá trị bán
    /// </summary>
    public int GetTotalSellPrice()
    {
        return sellPrice * quantity;
    }

    /// <summary>
    /// Lấy giá trị bán cho số lượng cụ thể
    /// </summary>
    public int GetSellPrice(int amount)
    {
        return sellPrice * amount;
    }

    public override string ToString()
    {
        return $"{itemName} x{quantity} (Giá: {sellPrice} vàng/cây)";
    }
}
