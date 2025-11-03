using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Class để serialize/deserialize dữ liệu balo
/// </summary>
[System.Serializable]
public class BagData
{
    public List<BagItemData> items;
    
    public BagData()
    {
        items = new List<BagItemData>();
    }
    
    public BagData(List<BagItemData> itemList)
    {
        items = itemList ?? new List<BagItemData>();
    }
    
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

/// <summary>
/// Class để serialize/deserialize BagItem
/// </summary>
[System.Serializable]
public class BagItemData
{
    public string itemName;
    public int quantity;
    public int sellPrice;
    public string description;
    
    public BagItemData()
    {
        itemName = "";
        quantity = 0;
        sellPrice = 0;
        description = "";
    }
    
    public BagItemData(BagItem bagItem)
    {
        itemName = bagItem.itemName;
        quantity = bagItem.quantity;
        sellPrice = bagItem.sellPrice;
        description = bagItem.description;
    }
    
    /// <summary>
    /// Convert BagItemData thành BagItem
    /// </summary>
    public BagItem ToBagItem(Sprite itemIcon)
    {
        BagItem item = new BagItem(itemName, itemIcon, quantity, sellPrice);
        item.description = description;
        return item;
    }
}
