using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Manager tập trung để đồng bộ dữ liệu người chơi lên Firebase
/// Tránh xung đột giữa các hệ thống khác nhau
/// </summary>
public class PlayerDataSyncManager : MonoBehaviour
{
    public static PlayerDataSyncManager Instance;
    
    private DatabaseReference userReference;
    private bool isInitialized = false;
    
    // TỐI ƯU: Debounce để tránh lưu quá nhiều lần
    private float lastMapUpdateTime = 0f;
    private float mapUpdateCooldown = 3f; // Tăng lên 3 giây để giảm lag
    private bool isMapUpdatePending = false;
    
    // TỐI ƯU: Batch tile updates để giảm Firebase calls
    private Dictionary<string, TilemapState> pendingTileUpdates = new Dictionary<string, TilemapState>();
    private float lastTileBatchTime = 0f;
    private float tileBatchCooldown = 2f; // Batch tiles mỗi 2 giây
    
    // TỐI ƯU: Tắt Firebase khi farming để test
    [Header("Debug Settings")]
    public bool disableFirebaseSaving = false; // Tắt lưu Firebase để test
    
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
        InitializeFirebase();
        SetupRealTimeListeners();
        
        // Đảm bảo load dữ liệu ngay khi khởi tạo
        if (LoadDataManager.firebaseUser != null)
        {
            Debug.Log("[PlayerDataSyncManager] Tự động load dữ liệu khi khởi tạo...");
            LoadAllPlayerData();
        }
    }
    
    void Update()
    {
        // TỐI ƯU: Kiểm tra pending map update
        if (isMapUpdatePending && Time.time - lastMapUpdateTime >= mapUpdateCooldown)
        {
            if (LoadDataManager.userInGame?.MapInGame != null)
            {
                Debug.Log("[PlayerDataSyncManager] Lưu map pending...");
                UpdateMapInGame(LoadDataManager.userInGame.MapInGame);
            }
        }
        
        // TỐI ƯU: Batch tile updates để giảm Firebase calls
        if (pendingTileUpdates.Count > 0 && Time.time - lastTileBatchTime >= tileBatchCooldown)
        {
            ProcessBatchTileUpdates();
        }
    }
    
    /// <summary>
    /// Khởi tạo Firebase reference
    /// </summary>
    void InitializeFirebase()
    {
        if (LoadDataManager.firebaseUser != null)
        {
            userReference = FirebaseDatabase.DefaultInstance
                .GetReference("Users")
                .Child(LoadDataManager.firebaseUser.UserId);
            
            isInitialized = true;
            Debug.Log("[PlayerDataSyncManager] Đã khởi tạo Firebase reference!");
        }
        else
        {
            Debug.LogWarning("[PlayerDataSyncManager] FirebaseUser is null!");
        }
    }
    
    /// <summary>
    /// Thiết lập real-time listeners cho vàng và kim cương
    /// </summary>
    void SetupRealTimeListeners()
    {
        if (!isInitialized || userReference == null) 
        {
            Debug.LogWarning("[PlayerDataSyncManager] Chưa khởi tạo Firebase! Không thể setup listeners.");
            return;
        }
        
        Debug.Log("[PlayerDataSyncManager] Đang thiết lập real-time listeners...");
        
        // Listener cho Gold
        userReference.Child("Gold").ValueChanged += OnGoldChanged;
        
        // Listener cho Diamond
        userReference.Child("Diamond").ValueChanged += OnDiamondChanged;
        
        // TỐI ƯU: Thêm real-time listeners cho EXP và Quest
        userReference.Child("ExpData").ValueChanged += OnExpDataChanged;
        userReference.Child("QuestData").ValueChanged += OnQuestDataChanged;
        
        Debug.Log("[PlayerDataSyncManager] ✅ Đã thiết lập real-time listeners cho Gold, Diamond, EXP và Quest!");
    }
    
    /// <summary>
    /// Xử lý khi Gold thay đổi từ Firebase
    /// </summary>
    void OnGoldChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.Snapshot.Exists)
        {
            try
            {
                int newGold = int.Parse(args.Snapshot.Value.ToString());
                
                Debug.Log($"[Real-time] Gold thay đổi từ Firebase: {newGold}");
                
                // Cập nhật LoadDataManager
                if (LoadDataManager.userInGame != null)
                {
                    LoadDataManager.userInGame.Gold = newGold;
                }
                
                // Cập nhật PlayerGoldManager (không trigger save lại)
                if (PlayerGoldManager.Instance != null)
                {
                    PlayerGoldManager.Instance.SetGoldFromFirebase(newGold);
                }
                
                Debug.Log($"[Real-time] ✅ Đã cập nhật Gold trong game: {newGold}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Real-time] Lỗi parse Gold: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Xử lý khi Diamond thay đổi từ Firebase
    /// </summary>
    void OnDiamondChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.Snapshot.Exists)
        {
            try
            {
                int newDiamond = int.Parse(args.Snapshot.Value.ToString());
                
                Debug.Log($"[Real-time] Diamond thay đổi từ Firebase: {newDiamond}");
                
                // Cập nhật LoadDataManager
                if (LoadDataManager.userInGame != null)
                {
                    LoadDataManager.userInGame.Diamond = newDiamond;
                }
                
                // Cập nhật PlayerGoldManager (không trigger save lại)
                if (PlayerGoldManager.Instance != null)
                {
                    PlayerGoldManager.Instance.SetDiamondFromFirebase(newDiamond);
                }
                
                Debug.Log($"[Real-time] ✅ Đã cập nhật Diamond trong game: {newDiamond}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Real-time] Lỗi parse Diamond: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Cập nhật vàng (không ghi đè dữ liệu khác)
    /// </summary>
    public void UpdateGold(int gold)
    {
        if (!isInitialized || userReference == null) return;
        
        userReference.Child("Gold").SetValueAsync(gold).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"[PlayerDataSyncManager] Đã cập nhật Gold: {gold}");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi cập nhật Gold: {task.Exception}");
            }
        });
    }
    
    /// <summary>
    /// Cập nhật kim cương (không ghi đè dữ liệu khác)
    /// </summary>
    public void UpdateDiamond(int diamond)
    {
        if (!isInitialized || userReference == null) return;
        
        userReference.Child("Diamond").SetValueAsync(diamond).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"[PlayerDataSyncManager] Đã cập nhật Diamond: {diamond}");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi cập nhật Diamond: {task.Exception}");
            }
        });
    }
    
    /// <summary>
    /// Cập nhật MapInGame (không ghi đè dữ liệu khác)
    /// TỐI ƯU: Sử dụng debounce để tránh lag
    /// </summary>
    public void UpdateMapInGame(Map mapData)
    {
        if (!isInitialized || userReference == null) return;
        
        // TỐI ƯU: Debounce - chỉ lưu mỗi 2 giây
        if (Time.time - lastMapUpdateTime < mapUpdateCooldown)
        {
            Debug.Log("[PlayerDataSyncManager] Debounce: Chờ lưu map...");
            isMapUpdatePending = true;
            return;
        }
        
        lastMapUpdateTime = Time.time;
        isMapUpdatePending = false;
        
        // TỐI ƯU: Sử dụng coroutine để tránh lag
        StartCoroutine(UpdateMapInGameAsync(mapData));
    }
    
    /// <summary>
    /// Cập nhật MapInGame bất đồng bộ để tránh lag
    /// TỐI ƯU: Chia nhỏ việc serialize và upload
    /// </summary>
    private System.Collections.IEnumerator UpdateMapInGameAsync(Map mapData)
    {
        // TỐI ƯU: Chia nhỏ việc serialize để tránh lag
        yield return null; // Đợi 1 frame
        
        string mapJson = JsonConvert.SerializeObject(mapData);
        
        yield return null; // Đợi 1 frame nữa
        
        // TỐI ƯU: Kiểm tra kích thước JSON
        if (mapJson.Length > 1000000) // Nếu > 1MB
        {
            Debug.LogWarning($"[PlayerDataSyncManager] Map quá lớn ({mapJson.Length} bytes), có thể gây lag!");
        }
        
        // Upload bất đồng bộ
        var task = userReference.Child("MapInGame").SetValueAsync(mapJson);
        
        // Đợi upload hoàn thành (không block game)
        while (!task.IsCompleted)
        {
            yield return null;
        }
        
        if (task.IsCompleted)
        {
            Debug.Log($"[PlayerDataSyncManager] Đã cập nhật MapInGame ({mapJson.Length} bytes)");
        }
        else if (task.IsFaulted)
        {
            Debug.LogError($"[PlayerDataSyncManager] Lỗi cập nhật MapInGame: {task.Exception}");
        }
    }
    
    /// <summary>
    /// TỐI ƯU: Cập nhật chỉ 1 tile thay vì toàn bộ map
    /// SỬA: Sử dụng batch updates để giảm Firebase calls
    /// </summary>
    public void UpdateSingleTile(int x, int y, TilemapState state)
    {
        if (!isInitialized || userReference == null) return;
        
        // TỐI ƯU: Tắt Firebase để test lag
        if (disableFirebaseSaving)
        {
            Debug.Log($"[PlayerDataSyncManager] Firebase tắt - Không lưu tile ({x},{y}) = {state}");
            return;
        }
        
        // TỐI ƯU: Thêm vào batch thay vì lưu ngay
        string tileKey = $"{x}_{y}";
        pendingTileUpdates[tileKey] = state;
        
        Debug.Log($"[PlayerDataSyncManager] Đã thêm tile ({x},{y}) = {state} vào batch. Tổng: {pendingTileUpdates.Count}");
    }
    
    /// <summary>
    /// TỐI ƯU: Xử lý batch tile updates để giảm Firebase calls
    /// </summary>
    private void ProcessBatchTileUpdates()
    {
        if (pendingTileUpdates.Count == 0) return;
        
        Debug.Log($"[PlayerDataSyncManager] Đang xử lý batch {pendingTileUpdates.Count} tiles...");
        
        // Tạo batch update
        var batchData = new Dictionary<string, object>();
        foreach (var kvp in pendingTileUpdates)
        {
            string[] coords = kvp.Key.Split('_');
            int x = int.Parse(coords[0]);
            int y = int.Parse(coords[1]);
            
            var tileData = new { x = x, y = y, tilemapState = kvp.Value.ToString() };
            batchData[$"MapInGame/lstTilemapDetail/{kvp.Key}"] = JsonConvert.SerializeObject(tileData);
        }
        
        // Lưu batch
        userReference.UpdateChildrenAsync(batchData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"[PlayerDataSyncManager] Đã lưu batch {pendingTileUpdates.Count} tiles!");
                pendingTileUpdates.Clear();
                lastTileBatchTime = Time.time;
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi batch update: {task.Exception}");
            }
        });
    }
    
    /// <summary>
    /// Cập nhật BagData (không ghi đè dữ liệu khác)
    /// </summary>
    public void UpdateBagData(BagData bagData)
    {
        if (!isInitialized || userReference == null) return;
        
        string bagJson = bagData.ToString();
        userReference.Child("BagData").SetValueAsync(bagJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("[PlayerDataSyncManager] Đã cập nhật BagData");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi cập nhật BagData: {task.Exception}");
            }
        });
    }
    
    /// <summary>
    /// Cập nhật Name (không ghi đè dữ liệu khác)
    /// </summary>
    public void UpdateName(string name)
    {
        if (!isInitialized || userReference == null) return;
        
        userReference.Child("Name").SetValueAsync(name).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"[PlayerDataSyncManager] Đã cập nhật Name: {name}");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi cập nhật Name: {task.Exception}");
            }
        });
    }
    
    /// <summary>
    /// Cập nhật Email (không ghi đè dữ liệu khác)
    /// </summary>
    public void UpdateEmail(string email)
    {
        if (!isInitialized || userReference == null) return;
        
        userReference.Child("Email").SetValueAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"[PlayerDataSyncManager] Đã cập nhật Email: {email}");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi cập nhật Email: {task.Exception}");
            }
        });
    }
    
    /// <summary>
    /// Load toàn bộ dữ liệu người chơi từ Firebase
    /// </summary>
    public void LoadAllPlayerData()
    {
        if (!isInitialized || userReference == null) return;
        
        Debug.Log("[PlayerDataSyncManager] Đang load toàn bộ dữ liệu người chơi...");
        
        userReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi load dữ liệu: {task.Exception}");
                return;
            }
            
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                
                if (snapshot.Exists)
                {
                    try
                    {
                        // Load từng field riêng biệt
                        if (snapshot.Child("Name").Exists)
                        {
                            LoadDataManager.userInGame.Name = snapshot.Child("Name").Value.ToString();
                        }
                        
                        if (snapshot.Child("Email").Exists)
                        {
                            LoadDataManager.userInGame.Email = snapshot.Child("Email").Value.ToString();
                            Debug.Log($"[PlayerDataSyncManager] Đã load Email: {LoadDataManager.userInGame.Email}");
                        }
                        
                        if (snapshot.Child("Gold").Exists)
                        {
                            LoadDataManager.userInGame.Gold = int.Parse(snapshot.Child("Gold").Value.ToString());
                        }
                        
                        if (snapshot.Child("Diamond").Exists)
                        {
                            LoadDataManager.userInGame.Diamond = int.Parse(snapshot.Child("Diamond").Value.ToString());
                        }
                        
                        if (snapshot.Child("MapInGame").Exists)
                        {
                            string mapJson = snapshot.Child("MapInGame").Value.ToString();
                            LoadDataManager.userInGame.MapInGame = JsonConvert.DeserializeObject<Map>(mapJson);
                        }
                        
                        if (snapshot.Child("BagData").Exists)
                        {
                            string bagJson = snapshot.Child("BagData").Value.ToString();
                            BagData bagData = JsonConvert.DeserializeObject<BagData>(bagJson);
                            
                            // Cập nhật BagManager
                            if (BagManager.Instance != null)
                            {
                                BagManager.Instance.LoadBagFromData(bagData);
                            }
                        }
                        
                        if (snapshot.Child("BagUpgradeData").Exists)
                        {
                            string bagUpgradeJson = snapshot.Child("BagUpgradeData").Value.ToString();
                            BagUpgradeData bagUpgradeData = JsonConvert.DeserializeObject<BagUpgradeData>(bagUpgradeJson);
                            
                            // Cập nhật BagUpgradeManager nếu có
                            if (BagUpgradeManager.Instance != null)
                            {
                                BagUpgradeManager.Instance.LoadUpgradeData(bagUpgradeData);
                            }
                        }
                        
                        if (snapshot.Child("ExpData").Exists)
                        {
                            string expJson = snapshot.Child("ExpData").Value.ToString();
                            ExpData expData = ExpData.FromJson(expJson);
                            
                            // Cập nhật PlayerExpManager
                            if (PlayerExpManager.Instance != null)
                            {
                                PlayerExpManager.Instance.expData = expData;
                                Debug.Log($"[PlayerDataSyncManager] Đã load ExpData: Level {expData.currentLevel}, EXP {expData.currentExp}, Stat Points {expData.statPoints}");
                                
                                // Trigger event để UI cập nhật
                                PlayerExpManager.Instance.OnExpChanged?.Invoke(expData);
                            }
                        }
                        
                        if (snapshot.Child("QuestData").Exists)
                        {
                            string questJson = snapshot.Child("QuestData").Value.ToString();
                            QuestData questData = JsonConvert.DeserializeObject<QuestData>(questJson);
                            
                            // Cập nhật QuestManager
                            if (QuestManager.Instance != null)
                            {
                                QuestManager.Instance.LoadQuestDataFromFirebase(questData);
                                Debug.Log($"[PlayerDataSyncManager] Đã load QuestData: Current quest index {questData.currentQuestIndex}, Total quests {questData.questList.Count}");
                            }
                        }
                        
                        if (snapshot.Child("PlayerPosition").Exists)
                        {
                            string positionJson = snapshot.Child("PlayerPosition").Value.ToString();
                            Vector3 savedPosition = JsonConvert.DeserializeObject<Vector3>(positionJson);
                            
                            // Khôi phục vị trí player
                            GameObject player = GameObject.FindGameObjectWithTag("Player");
                            if (player != null)
                            {
                                player.transform.position = savedPosition;
                                Debug.Log($"[PlayerDataSyncManager] Đã khôi phục vị trí player: {savedPosition}");
                            }
                            else
                            {
                                Debug.LogWarning("[PlayerDataSyncManager] Không tìm thấy Player object!");
                            }
                        }
                        
                        if (snapshot.Child("TutorialCompleted").Exists)
                        {
                            bool tutorialCompleted = bool.Parse(snapshot.Child("TutorialCompleted").Value.ToString());
                            
                            // Cập nhật tutorial state
                            PlayerPrefs.SetString("TutorialCompleted", tutorialCompleted.ToString());
                            PlayerPrefs.Save();
                            Debug.Log($"[PlayerDataSyncManager] Đã load TutorialCompleted: {tutorialCompleted}");
                            
                            // Nếu tutorial đã hoàn thành, skip tutorial
                            if (tutorialCompleted)
                            {
                                var tutorialSaver = FindObjectOfType<TutorialProgressSaver>();
                                if (tutorialSaver != null)
                                {
                                    tutorialSaver.SkipTutorialIfNeeded();
                                }
                            }
                        }
                        
                        Debug.Log("[PlayerDataSyncManager] Đã load toàn bộ dữ liệu người chơi!");
                        
                        // Đảm bảo MapInGame được khởi tạo sau khi load dữ liệu
                        if (TileMapManager.Instance != null)
                        {
                            TileMapManager.Instance.EnsureMapInitialized();
                        }
                        
                        // Trigger events để UI cập nhật
                        if (PlayerGoldManager.Instance != null)
                        {
                            PlayerGoldManager.Instance.SetGold(LoadDataManager.userInGame.Gold);
                            PlayerGoldManager.Instance.SetDiamond(LoadDataManager.userInGame.Diamond);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[PlayerDataSyncManager] Lỗi parse dữ liệu: {e.Message}");
                    }
                }
                else
                {
                    Debug.Log("[PlayerDataSyncManager] Không có dữ liệu người chơi trong Firebase");
                }
            }
        });
    }
    
    /// <summary>
    /// Cập nhật ExpData (không ghi đè dữ liệu khác)
    /// </summary>
    public void UpdateExpData(ExpData expData)
    {
        if (!isInitialized || userReference == null) return;
        
        string expJson = expData.ToJson();
        userReference.Child("ExpData").SetValueAsync(expJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("[PlayerDataSyncManager] Đã cập nhật ExpData");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi cập nhật ExpData: {task.Exception}");
            }
        });
    }
    
    /// <summary>
    /// Cập nhật dữ liệu nâng cấp balo
    /// </summary>
    public void UpdateBagUpgradeData(BagUpgradeData upgradeData)
    {
        if (!isInitialized || userReference == null) return;
        
        string jsonData = JsonConvert.SerializeObject(upgradeData);
        userReference.Child("BagUpgradeData").SetValueAsync(jsonData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted) 
            {
                Debug.Log("[PlayerDataSyncManager] BagUpgradeData đã được cập nhật!");
            }
            else if (task.IsFaulted) 
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi cập nhật BagUpgradeData: {task.Exception}");
            }
        });
    }
    
    /// <summary>
    /// Cập nhật quest data
    /// </summary>
    public void UpdateQuestData(QuestData questData)
    {
        if (!isInitialized || userReference == null) return;
        
        string jsonData = JsonConvert.SerializeObject(questData);
        userReference.Child("QuestData").SetValueAsync(jsonData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("[PlayerDataSyncManager] QuestData đã được cập nhật!");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi cập nhật QuestData: {task.Exception}");
            }
        });
    }
    
    /// <summary>
    /// Cập nhật player position
    /// </summary>
    public void UpdatePlayerPosition(Vector3 position)
    {
        if (!isInitialized || userReference == null) return;
        
        string positionJson = JsonConvert.SerializeObject(position);
        userReference.Child("PlayerPosition").SetValueAsync(positionJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"[PlayerDataSyncManager] PlayerPosition đã được cập nhật: {position}");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi cập nhật PlayerPosition: {task.Exception}");
            }
        });
    }
    
    /// <summary>
    /// Cập nhật tutorial progress
    /// </summary>
    public void UpdateTutorialProgress(bool tutorialCompleted)
    {
        if (!isInitialized || userReference == null) return;
        
        userReference.Child("TutorialCompleted").SetValueAsync(tutorialCompleted).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"[PlayerDataSyncManager] TutorialCompleted đã được cập nhật: {tutorialCompleted}");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"[PlayerDataSyncManager] Lỗi cập nhật TutorialCompleted: {task.Exception}");
            }
        });
    }
    
    /// <summary>
    /// Xử lý khi ExpData thay đổi từ Firebase
    /// </summary>
    void OnExpDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.Snapshot.Exists)
        {
            try
            {
                string expJson = args.Snapshot.Value.ToString();
                ExpData expData = ExpData.FromJson(expJson);
                
                Debug.Log($"[Real-time] ExpData thay đổi từ Firebase: Level {expData.currentLevel}, EXP {expData.currentExp}");
                
                // Cập nhật PlayerExpManager
                if (PlayerExpManager.Instance != null)
                {
                    PlayerExpManager.Instance.expData = expData;
                    
                    // Trigger event để UI cập nhật
                    PlayerExpManager.Instance.OnExpChanged?.Invoke(expData);
                }
                
                Debug.Log($"[Real-time] ✅ Đã cập nhật ExpData trong game!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Real-time] Lỗi parse ExpData: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Xử lý khi QuestData thay đổi từ Firebase
    /// </summary>
    void OnQuestDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.Snapshot.Exists)
        {
            try
            {
                string questJson = args.Snapshot.Value.ToString();
                QuestData questData = JsonConvert.DeserializeObject<QuestData>(questJson);
                
                Debug.Log($"[Real-time] QuestData thay đổi từ Firebase: Quest {questData.currentQuestIndex}");
                
                // Cập nhật QuestManager
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.LoadQuestDataFromFirebase(questData);
                }
                
                Debug.Log($"[Real-time] ✅ Đã cập nhật QuestData trong game!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Real-time] Lỗi parse QuestData: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Hủy listeners khi destroy
    /// </summary>
    void OnDestroy()
    {
        if (userReference != null)
        {
            userReference.Child("Gold").ValueChanged -= OnGoldChanged;
            userReference.Child("Diamond").ValueChanged -= OnDiamondChanged;
            userReference.Child("ExpData").ValueChanged -= OnExpDataChanged;
            userReference.Child("QuestData").ValueChanged -= OnQuestDataChanged;
            Debug.Log("[PlayerDataSyncManager] Đã hủy real-time listeners");
        }
    }
}
