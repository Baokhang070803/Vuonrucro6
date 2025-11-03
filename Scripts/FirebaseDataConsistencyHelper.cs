using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;

/// <summary>
/// Helper class để đảm bảo tính nhất quán khi lưu dữ liệu lên Firebase
/// Tất cả dữ liệu user sẽ được lưu theo cấu trúc nhất quán:
/// Users/{userId}/
///   ├── Name: "string"
///   ├── Gold: number
///   ├── Diamond: number
///   ├── MapInGame: "JSON string"
///   ├── BagData: "JSON string"
///   └── BagUpgradeData: "JSON string"
/// </summary>
public static class FirebaseDataConsistencyHelper
{
    /// <summary>
    /// Đảm bảo cấu trúc dữ liệu user nhất quán khi tạo mới
    /// </summary>
    public static void CreateConsistentUserStructure(string userId, string userName = "", string email = "")
    {
        Debug.Log($"[FirebaseDataConsistencyHelper] Tạo cấu trúc nhất quán cho user: {userId}");
        
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance
            .GetReference("Users")
            .Child(userId);
        
        // Tạo MapInGame mặc định
        Map mapInGame = new Map();
        string mapJson = JsonConvert.SerializeObject(mapInGame);
        
        // Tạo BagData mặc định
        BagData bagData = new BagData();
        string bagJson = bagData.ToString();
        
        // Tạo BagUpgradeData mặc định (cấp 1)
        BagUpgradeData bagUpgradeData = new BagUpgradeData(1);
        string bagUpgradeJson = bagUpgradeData.ToString();
        
        // Tạo ExpData mặc định (cấp 1)
        ExpData expData = new ExpData();
        string expJson = expData.ToJson();
        
        // Lưu Name (để trống cho người dùng nhập sau)
        string nameToSave = !string.IsNullOrEmpty(userName) ? userName : "";
        userRef.Child("Name").SetValueAsync(nameToSave).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log($"[FirebaseDataConsistencyHelper] Đã lưu Name: '{nameToSave}' (để trống cho người dùng nhập sau)");
            else
                Debug.LogError($"[FirebaseDataConsistencyHelper] Lỗi lưu Name: {task.Exception}");
        });
        
        // Lưu Email (ưu tiên từ parameter, sau đó từ Firebase Auth)
        string emailToSave = !string.IsNullOrEmpty(email) ? email : 
                            (LoadDataManager.firebaseUser != null ? LoadDataManager.firebaseUser.Email : "");
        
        if (!string.IsNullOrEmpty(emailToSave))
        {
            userRef.Child("Email").SetValueAsync(emailToSave).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    Debug.Log($"[FirebaseDataConsistencyHelper] Đã lưu Email: {emailToSave}");
                else
                    Debug.LogError($"[FirebaseDataConsistencyHelper] Lỗi lưu Email: {task.Exception}");
            });
        }
        
        userRef.Child("Gold").SetValueAsync(0).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("[FirebaseDataConsistencyHelper] Đã lưu Gold: 0");
            else
                Debug.LogError($"[FirebaseDataConsistencyHelper] Lỗi lưu Gold: {task.Exception}");
        });
        
        userRef.Child("Diamond").SetValueAsync(0).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("[FirebaseDataConsistencyHelper] Đã lưu Diamond: 0");
            else
                Debug.LogError($"[FirebaseDataConsistencyHelper] Lỗi lưu Diamond: {task.Exception}");
        });
        
        userRef.Child("MapInGame").SetValueAsync(mapJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("[FirebaseDataConsistencyHelper] Đã lưu MapInGame");
            else
                Debug.LogError($"[FirebaseDataConsistencyHelper] Lỗi lưu MapInGame: {task.Exception}");
        });
        
        userRef.Child("BagData").SetValueAsync(bagJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("[FirebaseDataConsistencyHelper] Đã lưu BagData");
            else
                Debug.LogError($"[FirebaseDataConsistencyHelper] Lỗi lưu BagData: {task.Exception}");
        });
        
        userRef.Child("BagUpgradeData").SetValueAsync(bagUpgradeJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("[FirebaseDataConsistencyHelper] Đã lưu BagUpgradeData");
            else
                Debug.LogError($"[FirebaseDataConsistencyHelper] Lỗi lưu BagUpgradeData: {task.Exception}");
        });
        
        userRef.Child("ExpData").SetValueAsync(expJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("[FirebaseDataConsistencyHelper] Đã lưu ExpData");
            else
                Debug.LogError($"[FirebaseDataConsistencyHelper] Lỗi lưu ExpData: {task.Exception}");
        });
        
        Debug.Log("[FirebaseDataConsistencyHelper] Hoàn thành tạo cấu trúc user nhất quán!");
    }
    
    /// <summary>
    /// Kiểm tra và sửa cấu trúc dữ liệu user nếu bị lỗi
    /// </summary>
    public static void ValidateAndFixUserStructure(string userId)
    {
        Debug.Log($"[FirebaseDataConsistencyHelper] Kiểm tra cấu trúc user: {userId}");
        
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance
            .GetReference("Users")
            .Child(userId);
        
        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[FirebaseDataConsistencyHelper] Lỗi kiểm tra user: {task.Exception}");
                return;
            }
            
            DataSnapshot snapshot = task.Result;
            
            if (!snapshot.Exists)
            {
                Debug.Log("[FirebaseDataConsistencyHelper] User không tồn tại, tạo mới...");
                CreateConsistentUserStructure(userId);
                return;
            }
            
            // Kiểm tra từng field bắt buộc
            bool needsFix = false;
            
            if (!snapshot.Child("Name").Exists)
            {
                Debug.LogWarning("[FirebaseDataConsistencyHelper] Thiếu field Name, sẽ sửa...");
                needsFix = true;
            }
            
            if (!snapshot.Child("Gold").Exists)
            {
                Debug.LogWarning("[FirebaseDataConsistencyHelper] Thiếu field Gold, sẽ sửa...");
                needsFix = true;
            }
            
            if (!snapshot.Child("Diamond").Exists)
            {
                Debug.LogWarning("[FirebaseDataConsistencyHelper] Thiếu field Diamond, sẽ sửa...");
                needsFix = true;
            }
            
            if (!snapshot.Child("MapInGame").Exists)
            {
                Debug.LogWarning("[FirebaseDataConsistencyHelper] Thiếu field MapInGame, sẽ sửa...");
                needsFix = true;
            }
            
            if (!snapshot.Child("BagData").Exists)
            {
                Debug.LogWarning("[FirebaseDataConsistencyHelper] Thiếu field BagData, sẽ sửa...");
                needsFix = true;
            }
            
            if (!snapshot.Child("BagUpgradeData").Exists)
            {
                Debug.LogWarning("[FirebaseDataConsistencyHelper] Thiếu field BagUpgradeData, sẽ sửa...");
                needsFix = true;
            }
            
            if (needsFix)
            {
                Debug.Log("[FirebaseDataConsistencyHelper] Phát hiện cấu trúc không nhất quán, đang sửa...");
                
                // Lưu các field còn thiếu
                if (!snapshot.Child("Name").Exists)
                {
                    userRef.Child("Name").SetValueAsync("");
                }
                
                if (!snapshot.Child("Gold").Exists)
                {
                    userRef.Child("Gold").SetValueAsync(0);
                }
                
                if (!snapshot.Child("Diamond").Exists)
                {
                    userRef.Child("Diamond").SetValueAsync(0);
                }
                
                if (!snapshot.Child("MapInGame").Exists)
                {
                    Map mapInGame = new Map();
                    string mapJson = JsonConvert.SerializeObject(mapInGame);
                    userRef.Child("MapInGame").SetValueAsync(mapJson);
                }
                
                if (!snapshot.Child("BagData").Exists)
                {
                    BagData bagData = new BagData();
                    string bagJson = bagData.ToString();
                    userRef.Child("BagData").SetValueAsync(bagJson);
                }
                
                if (!snapshot.Child("BagUpgradeData").Exists)
                {
                    BagUpgradeData bagUpgradeData = new BagUpgradeData(1);
                    string bagUpgradeJson = bagUpgradeData.ToString();
                    userRef.Child("BagUpgradeData").SetValueAsync(bagUpgradeJson);
                }
                
                Debug.Log("[FirebaseDataConsistencyHelper] Đã sửa xong cấu trúc user!");
            }
            else
            {
                Debug.Log("[FirebaseDataConsistencyHelper] Cấu trúc user đã nhất quán!");
            }
        });
    }
    
    /// <summary>
    /// Debug: In ra cấu trúc dữ liệu user hiện tại
    /// </summary>
    public static void DebugUserStructure(string userId)
    {
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance
            .GetReference("Users")
            .Child(userId);
        
        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[FirebaseDataConsistencyHelper] Lỗi debug user: {task.Exception}");
                return;
            }
            
            DataSnapshot snapshot = task.Result;
            
            Debug.Log("=== USER STRUCTURE DEBUG ===");
            Debug.Log($"User ID: {userId}");
            
            if (snapshot.Exists)
            {
                foreach (DataSnapshot child in snapshot.Children)
                {
                    Debug.Log($"  {child.Key}: {child.Value} (Type: {child.Value.GetType().Name})");
                }
            }
            else
            {
                Debug.Log("User không tồn tại trong Firebase!");
            }
            Debug.Log("=== END DEBUG ===");
        });
    }
}
