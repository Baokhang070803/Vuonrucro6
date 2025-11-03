using UnityEngine;

/// <summary>
/// Script test hệ thống đồng bộ balo Firebase
/// </summary>
public class BagSyncTestScript : MonoBehaviour
{
    [Header("Test Controls")]
    public bool enableTestControls = true;
    
    void Update()
    {
        if (!enableTestControls) return;
        
        // Test thêm item và đồng bộ
        if (Input.GetKeyDown(KeyCode.B))
        {
            TestAddItemAndSync();
        }
        
        // Test đồng bộ từ Firebase
        if (Input.GetKeyDown(KeyCode.L))
        {
            TestLoadFromFirebase();
        }
        
        // Test đồng bộ lên Firebase
        if (Input.GetKeyDown(KeyCode.S))
        {
            TestSaveToFirebase();
        }
        
        // Test debug balo
        if (Input.GetKeyDown(KeyCode.M))
        {
            TestDebugBag();
        }
        
        // Test clear balo
        if (Input.GetKeyDown(KeyCode.C))
        {
            TestClearBag();
        }
    }
    
    void TestAddItemAndSync()
    {
        if (BagManager.Instance != null)
        {
            bool success = BagManager.Instance.AddSunflower(1);
            Debug.Log($"[BagSyncTest] Thêm hoa hướng dương: {(success ? "Thành công" : "Thất bại")}");
            
            if (success)
            {
                Debug.Log("[BagSyncTest] Item đã được tự động lưu lên Firebase!");
            }
        }
        else
        {
            Debug.LogError("[BagSyncTest] BagManager.Instance is null!");
        }
    }
    
    void TestLoadFromFirebase()
    {
        if (BagManager.Instance != null)
        {
            BagManager.Instance.SyncBagFromFirebase();
            Debug.Log("[BagSyncTest] Đang load balo từ Firebase...");
        }
        else
        {
            Debug.LogError("[BagSyncTest] BagManager.Instance is null!");
        }
    }
    
    void TestSaveToFirebase()
    {
        if (BagManager.Instance != null)
        {
            BagManager.Instance.SyncBagToFirebase();
            Debug.Log("[BagSyncTest] Đang lưu balo lên Firebase...");
        }
        else
        {
            Debug.LogError("[BagSyncTest] BagManager.Instance is null!");
        }
    }
    
    void TestDebugBag()
    {
        if (BagManager.Instance != null)
        {
            BagManager.Instance.DebugBagInfo();
        }
        else
        {
            Debug.LogError("[BagSyncTest] BagManager.Instance is null!");
        }
    }
    
    void TestClearBag()
    {
        if (BagManager.Instance != null)
        {
            // Test xóa tất cả items
            var items = BagManager.Instance.GetAllItems();
            foreach (var item in items)
            {
                BagManager.Instance.RemoveItem(item.itemName, item.quantity);
            }
            Debug.Log("[BagSyncTest] Đã xóa tất cả items khỏi balo!");
        }
        else
        {
            Debug.LogError("[BagSyncTest] BagManager.Instance is null!");
        }
    }
    
    void OnGUI()
    {
        if (!enableTestControls) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 250, 150));
        GUILayout.Label("=== BAG SYNC TEST CONTROLS ===");
        GUILayout.Label("B - Thêm item + Sync");
        GUILayout.Label("L - Load từ Firebase");
        GUILayout.Label("S - Save lên Firebase");
        GUILayout.Label("M - Debug balo");
        GUILayout.Label("C - Clear balo");
        
        if (BagManager.Instance != null)
        {
            int sunflowerCount = BagManager.Instance.GetItemQuantity("Hoa Hướng Dương");
            GUILayout.Label($"Hoa hướng dương: {sunflowerCount}");
        }
        
        GUILayout.EndArea();
    }
}
