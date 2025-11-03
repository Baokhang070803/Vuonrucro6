using Newtonsoft.Json;

[System.Serializable]
public class BagUpgradeData
{
    public int bagLevel;
    public int bagCapacity;
    
    public BagUpgradeData()
    {
        bagLevel = 1;
        bagCapacity = 20; // Cấp 1 có 20 ô
    }
    
    public BagUpgradeData(int level)
    {
        bagLevel = level;
        bagCapacity = 20 + (level - 1) * 2; // Mỗi cấp tăng 2 ô
    }
    
    public BagUpgradeData(int level, int capacity)
    {
        bagLevel = level;
        bagCapacity = capacity;
    }
    
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
