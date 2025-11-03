using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using Newtonsoft.Json;

public class LoadDataManager : MonoBehaviour
{

    public static FirebaseUser firebaseUser;
    public static Users userInGame;

    private DatabaseReference reference;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
        GetUserInGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetUserInGame()
    {
        reference.Child("Users").Child(firebaseUser.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("Dữ liệu đọc được: " + snapshot.Value.ToString());
                
                // KIỂM TRA VÀ SỬA CẤU TRÚC DỮ LIỆU TRƯỚC KHI LOAD
                FirebaseDataConsistencyHelper.ValidateAndFixUserStructure(firebaseUser.UserId);
                
                // Chỉ sử dụng cấu trúc mới
                Debug.Log("[LoadDataManager] Load từ cấu trúc mới!");
                LoadFromNewStructure(snapshot);

                // Hiển thị thông báo hướng dẫn đầu game (chỉ khi không quay về từ PvP)
                // Delay nhỏ để đảm bảo nvnu1dituyen chạy trước
                StartCoroutine(ShowDialogueIfNeeded());
                
                // Đồng bộ toàn bộ dữ liệu người chơi từ Firebase
                if (PlayerDataSyncManager.Instance != null)
                {
                    Debug.Log("[LoadDataManager] Đang gọi LoadAllPlayerData...");
                    PlayerDataSyncManager.Instance.LoadAllPlayerData();
                }
                else
                {
                    Debug.LogWarning("[LoadDataManager] PlayerDataSyncManager.Instance is null!");
                }
            }
            else
            {
                Debug.Log("Đọc dữ liệu thất bại: " + task.Exception);
                
                // Nếu không load được, tạo user mới với cấu trúc nhất quán
                Debug.Log("[LoadDataManager] Tạo user mới với cấu trúc nhất quán...");
                FirebaseDataConsistencyHelper.CreateConsistentUserStructure(firebaseUser.UserId, "");
                CreateDefaultUser();
            }
        });
    }
    
    /// <summary>
    /// Load từ cấu trúc mới (từng field riêng biệt)
    /// </summary>
    void LoadFromNewStructure(DataSnapshot snapshot)
    {
        try
        {
            // Tạo userInGame mới
            userInGame = new Users();
            
            // Load từng field riêng biệt
            if (snapshot.Child("Name").Exists)
            {
                userInGame.Name = snapshot.Child("Name").Value.ToString();
            }
            
            if (snapshot.Child("Gold").Exists)
            {
                userInGame.Gold = int.Parse(snapshot.Child("Gold").Value.ToString());
            }
            
            if (snapshot.Child("Diamond").Exists)
            {
                userInGame.Diamond = int.Parse(snapshot.Child("Diamond").Value.ToString());
            }
            
            if (snapshot.Child("MapInGame").Exists)
            {
                string mapJson = snapshot.Child("MapInGame").Value.ToString();
                userInGame.MapInGame = JsonConvert.DeserializeObject<Map>(mapJson);
            }
            else
            {
                userInGame.MapInGame = new Map();
            }
            
            Debug.Log($"[LoadDataManager] Đã load từ cấu trúc mới: Name={userInGame.Name}, Gold={userInGame.Gold}, Diamond={userInGame.Diamond}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LoadDataManager] Lỗi load cấu trúc mới: {e.Message}");
            CreateDefaultUser();
        }
    }
    
    /// <summary>
    /// Tạo user mặc định khi có lỗi
    /// </summary>
    void CreateDefaultUser()
    {
        Debug.Log("[LoadDataManager] Tạo user mặc định...");
        userInGame = new Users("", 0, 0, new Map());
    }
    
    /// <summary>
    /// Hiển thị dialogue nếu cần (với delay để đảm bảo nvnu1dituyen chạy trước)
    /// </summary>
    System.Collections.IEnumerator ShowDialogueIfNeeded()
    {
        // Đợi 0.2 giây để đảm bảo nvnu1dituyen chạy trước
        yield return new WaitForSeconds(0.2f);
        
        string combatFlag = PlayerPrefs.GetString("JustFinishedCombat", "false");
        if (combatFlag != "true" && DialogueManager.I != null)
        {
            DialogueManager.I.Show(new List<string> { "Hãy đi thẳng về phía trước và tham quan!" });
        }
    }
}
