using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using Newtonsoft.Json;


public class TileMapManager : MonoBehaviour
{
    public static TileMapManager Instance { get; private set; }
    
    public Tilemap tm_Ground;
    public Tilemap tm_Grass;
    public Tilemap tm_Forest;

    public TileBase tb_Forest;
    public TileBase tb_Sunflower;
    
    // Tham chiếu đến PlayerFarmController để lấy danh sách TileBase
    private PlayerFarmController playerFarmController;

    // TỐI ƯU: Dictionary để lookup nhanh tile theo (x,y) - O(1) thay vì O(n)
    private Dictionary<string, TilemapDetail> tileLookup = new Dictionary<string, TilemapDetail>();

    private FirebaseDatabaseManager databaseManager;

    private DatabaseReference reference;

    private     void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Tìm PlayerFarmController
        playerFarmController = FindObjectOfType<PlayerFarmController>();
        if (playerFarmController == null)
        {
            Debug.LogWarning("PlayerFarmController không tìm thấy!");
        }
        
        // Kiểm tra Database Manager
        GameObject dbManager = GameObject.Find("Database Manager");
        if (dbManager != null)
        {
            databaseManager = dbManager.GetComponent<FirebaseDatabaseManager>();
        }
        else
        {
            Debug.LogError("Database Manager không tìm thấy!");
            return;
        }

        // Kiểm tra LoadDataManager và userInGame
        if (LoadDataManager.userInGame != null && 
            LoadDataManager.userInGame.MapInGame != null && 
            LoadDataManager.userInGame.MapInGame.lstTilemapDetail != null)
        {
            LoadMapForUser();
        }
        else
        {
            Debug.Log("Không có dữ liệu map, tạo map mới...");
            WriteAllTileMapToFirebase();
        }

        FirebaseApp app = FirebaseApp.DefaultInstance;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }
    
    /// <summary>
    /// Kiểm tra và khởi tạo lại map nếu bị null (được gọi sau khi PlayerDataSyncManager load xong)
    /// </summary>
    public void EnsureMapInitialized()
    {
        // Kiểm tra nếu userInGame hoặc MapInGame bị null
        if (LoadDataManager.userInGame == null)
        {
            Debug.LogWarning("[TileMapManager] LoadDataManager.userInGame is null! Đang khởi tạo user mới...");
            LoadDataManager.userInGame = new Users("Player", 1000000, 1000000, null);
        }
        
        if (LoadDataManager.userInGame.MapInGame == null || 
            LoadDataManager.userInGame.MapInGame.lstTilemapDetail == null)
        {
            Debug.Log("[TileMapManager] MapInGame bị null sau khi load từ Firebase, tạo lại map...");
            WriteAllTileMapToFirebase();
        }
        else
        {
            Debug.Log($"[TileMapManager] ✅ Map đã tồn tại với {LoadDataManager.userInGame.MapInGame.GetLength()} tiles");
        }
    }

    public void WriteAllTileMapToFirebase()
    {
        // Kiểm tra tm_Ground trước khi sử dụng
        if (tm_Ground == null)
        {
            Debug.LogError("tm_Ground is null! Không thể tạo map. Vui lòng gán Tilemap trong Inspector.");
            return;
        }

        List<TilemapDetail> tilemaps = new List<TilemapDetail>();
        
        // TỐI ƯU: Clear dictionary trước khi tạo map mới
        tileLookup.Clear();
        
        for (int x = tm_Ground.cellBounds.min.x; x < tm_Ground.cellBounds.max.x; x++)
        {
            for (int y = tm_Ground.cellBounds.min.y; y < tm_Ground.cellBounds.max.y; y++)
            {
                TilemapDetail tm_detail = new TilemapDetail(x, y, TilemapState.Grass);
                tilemaps.Add(tm_detail);
                
                // TỐI ƯU: Thêm vào dictionary ngay khi tạo
                string key = $"{x}_{y}";
                tileLookup[key] = tm_detail;
            }
        }

        // Tạo MapInGame mới
        Map newMap = new Map(tilemaps);
        
        // Kiểm tra LoadDataManager.userInGame trước khi gán
        if (LoadDataManager.userInGame == null)
        {
            Debug.LogWarning("LoadDataManager.userInGame is null! Đang khởi tạo user mới...");
            // Khởi tạo user mới nếu chưa có
            LoadDataManager.userInGame = new Users("Player", 1000000, 1000000, null);
        }
        
        LoadDataManager.userInGame.MapInGame = newMap;
        Debug.Log($"[TileMapManager] ✅ Đã gán MapInGame vào LoadDataManager.userInGame!");
        Debug.Log($"[TileMapManager] ✅ MapInGame có {newMap.GetLength()} tiles");
        Debug.Log($"[TileMapManager] ✅ tileLookup có {tileLookup.Count} tiles");

        // SỬA: Sử dụng PlayerDataSyncManager để lưu từng field riêng biệt
        // Thay vì ghi đè toàn bộ user object
        Debug.Log($"[TileMapManager] PlayerDataSyncManager.Instance: {PlayerDataSyncManager.Instance != null}");
        if (PlayerDataSyncManager.Instance != null)
        {
            PlayerDataSyncManager.Instance.UpdateMapInGame(newMap);
            Debug.Log("[TileMapManager] Đã gửi MapInGame để lưu qua PlayerDataSyncManager!");
        }
        else
        {
            Debug.LogError("[TileMapManager] PlayerDataSyncManager.Instance is null! Không thể lưu MapInGame.");
        }
    }
    public void LoadMapForUser()
    {
        if (LoadDataManager.userInGame?.MapInGame == null)
        {
            Debug.LogError("LoadDataManager.userInGame.MapInGame là null! Không thể load map.");
            return;
        }
        
        Debug.Log("Bắt đầu load map cho user...");
        MapToUI(LoadDataManager.userInGame.MapInGame);
        Debug.Log("Hoàn thành load map cho user.");
    }
    public void TilemapDetailToTileBase(TilemapDetail tilemapDetail)
    {
        Vector3Int cellPos = new Vector3Int(tilemapDetail.x, tilemapDetail.y, 0);
      
       if (tilemapDetail.tilemapState == TilemapState.Ground)
       {
          tm_Grass.SetTile(cellPos, null);
          tm_Forest.SetTile(cellPos, null);  
       }
       else if (tilemapDetail.tilemapState == TilemapState.Grass)
       {
          tm_Forest.SetTile(cellPos, null);
       }
       else if (tilemapDetail.tilemapState == TilemapState.Forest)
       {
          tm_Grass.SetTile(cellPos, null);
          tm_Forest.SetTile(cellPos, tb_Forest);
       }
       else if (tilemapDetail.tilemapState == TilemapState.Sunflower)
       {
          tm_Grass.SetTile(cellPos, null);
          tm_Forest.SetTile(cellPos, tb_Sunflower);
       }
        else if (tilemapDetail.tilemapState == TilemapState.Pepper)
        {
           tm_Grass.SetTile(cellPos, null);
           // Sử dụng logic từ PlayerFarmController để hiển thị đúng cây ớt
           SetPepperTile(cellPos);
        }
        else if (tilemapDetail.tilemapState == TilemapState.Pumpkin)
        {
           tm_Grass.SetTile(cellPos, null);
           // Sử dụng logic từ PlayerFarmController để hiển thị đúng cây bí ngô
           SetPumpkinTile(cellPos);
        }
        else if (tilemapDetail.tilemapState == TilemapState.Eggplant)
        {
           tm_Grass.SetTile(cellPos, null);
           // Sử dụng logic từ PlayerFarmController để hiển thị đúng cây cà tím
           SetEggplantTile(cellPos);
        }
      
    
    }

    public void MapToUI(Map map)
    {
        if (map?.lstTilemapDetail == null)
        {
            Debug.LogWarning("Map hoặc lstTilemapDetail là null!");
            return;
        }

        int mapLength = map.GetLength();
        Debug.Log($"Load map to UI - Tổng số tiles: {mapLength}");
        
        // TỐI ƯU: Xây dựng Dictionary lookup ngay khi load map
        tileLookup.Clear();
        
        // Giới hạn số lượng tiles để tránh vòng lặp vô hạn
        int maxTiles = Mathf.Min(mapLength, 10000); // Giới hạn 10,000 tiles
        
        for (int i = 0; i < maxTiles; i++)
        {
            if (i % 1000 == 0) // Log tiến trình mỗi 1000 tiles
            {
                Debug.Log($"Processing tile {i}/{maxTiles}");
            }
            
            TilemapDetail tile = map.lstTilemapDetail[i];
            TilemapDetailToTileBase(tile);
            
            // TỐI ƯU: Thêm vào dictionary để lookup nhanh sau này
            string key = $"{tile.x}_{tile.y}";
            if (!tileLookup.ContainsKey(key))
            {
                tileLookup[key] = tile;
            }
        }
        
        if (mapLength > maxTiles)
        {
            Debug.LogWarning($"Đã giới hạn xử lý {maxTiles}/{mapLength} tiles để tránh lag!");
        }
        
        Debug.Log($"[TileMapManager] ✅ Đã xây dựng tileLookup với {tileLookup.Count} tiles");
    }
    public void SetStateForTilemapDetail(int x, int y, TilemapState state)
    {
        // Kiểm tra null references
        if (LoadDataManager.userInGame == null)
        {
            Debug.LogWarning("[TileMapManager] LoadDataManager.userInGame is null!");
            return;
        }
        
        if (LoadDataManager.userInGame.MapInGame == null)
        {
            Debug.LogWarning("[TileMapManager] MapInGame is null!");
            return;
        }
        
        if (LoadDataManager.userInGame.MapInGame.lstTilemapDetail == null)
        {
            Debug.LogError("[TileMapManager] ❌ lstTilemapDetail is NULL! Không thể cập nhật tilemap state.");
            Debug.LogError("[TileMapManager] ❌ Map chưa được khởi tạo hoặc bị lỗi khi load từ Firebase!");
            return;
        }

        // ✅ TỐI ƯU: Sử dụng Dictionary lookup O(1) thay vì vòng lặp O(n)
        string key = $"{x}_{y}";
        if (tileLookup.ContainsKey(key))
        {
            TilemapDetail tile = tileLookup[key];
            tile.tilemapState = state;
            
            // TỐI ƯU: Sử dụng UpdateSingleTile thay vì UpdateMapInGame
            if (PlayerDataSyncManager.Instance != null)
            {
                // TỐI ƯU: Chỉ lưu 1 tile thay vì toàn bộ map
                PlayerDataSyncManager.Instance.UpdateSingleTile(x, y, state);
            }
            else
            {
                Debug.LogError("[TileMapManager] PlayerDataSyncManager.Instance is null! Không thể lưu tilemap state.");
            }
        }
        else
        {
            // Fallback: Nếu không tìm thấy trong dictionary, dùng vòng lặp (trường hợp hiếm)
            Debug.LogWarning($"[TileMapManager] Tile ({x}, {y}) không có trong tileLookup, dùng vòng lặp fallback...");
            
            bool tileFound = false;
            for (int i = 0; i < LoadDataManager.userInGame.MapInGame.GetLength(); i++)
            {
                if (LoadDataManager.userInGame.MapInGame.lstTilemapDetail[i].x == x &&  
                    LoadDataManager.userInGame.MapInGame.lstTilemapDetail[i].y == y)
                {
                    LoadDataManager.userInGame.MapInGame.lstTilemapDetail[i].tilemapState = state;
                    tileFound = true;
                    
                    // Thêm vào dictionary để lần sau nhanh hơn
                    tileLookup[key] = LoadDataManager.userInGame.MapInGame.lstTilemapDetail[i];
                    
                    if (PlayerDataSyncManager.Instance != null)
                    {
                        PlayerDataSyncManager.Instance.UpdateSingleTile(x, y, state);
                    }
                    break;
                }
            }
            
            if (!tileFound)
            {
                Debug.LogWarning($"[TileMapManager] ❌ KHÔNG TÌM THẤY tile tại ({x}, {y})!");
            }
        }
    }
     
     /// <summary>
     /// Hiển thị cây ớt sử dụng danh sách TileBase từ PlayerFarmController
     /// </summary>
     private void SetPepperTile(Vector3Int cellPos)
     {
         if (playerFarmController == null)
         {
             Debug.LogWarning("PlayerFarmController không tìm thấy, sử dụng tb_Sunflower làm fallback");
             tm_Forest.SetTile(cellPos, tb_Sunflower);
             return;
         }
         
         // Lấy danh sách TileBase ớt từ PlayerFarmController
         var pepperTiles = GetPepperTilesFromController();
         if (pepperTiles != null && pepperTiles.Count > 0)
         {
             // Hiển thị giai đoạn cuối cùng của ớt
             tm_Forest.SetTile(cellPos, pepperTiles[pepperTiles.Count - 1]);
             Debug.Log($"Đã hiển thị cây ớt tại {cellPos}");
         }
         else
         {
             Debug.LogWarning("Không tìm thấy TileBase ớt, sử dụng tb_Sunflower làm fallback");
             tm_Forest.SetTile(cellPos, tb_Sunflower);
         }
     }
     
     /// <summary>
     /// Lấy danh sách TileBase ớt từ PlayerFarmController
     /// </summary>
     private System.Collections.Generic.List<UnityEngine.Tilemaps.TileBase> GetPepperTilesFromController()
     {
         if (playerFarmController == null) return null;
         
         // Sử dụng reflection để lấy lstTb_Pepper từ PlayerFarmController
         var field = typeof(PlayerFarmController).GetField("lstTb_Pepper", 
             System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
         
         if (field != null)
         {
             return field.GetValue(playerFarmController) as System.Collections.Generic.List<UnityEngine.Tilemaps.TileBase>;
         }
         
         return null;
     }
     
     /// <summary>
     /// Hiển thị cây bí ngô sử dụng danh sách TileBase từ PlayerFarmController
     /// </summary>
     private void SetPumpkinTile(Vector3Int cellPos)
     {
         if (playerFarmController == null)
         {
             Debug.LogWarning("PlayerFarmController không tìm thấy, sử dụng tb_Sunflower làm fallback");
             tm_Forest.SetTile(cellPos, tb_Sunflower);
             return;
         }
         
         // Lấy danh sách TileBase bí ngô từ PlayerFarmController
         var pumpkinTiles = GetPumpkinTilesFromController();
         if (pumpkinTiles != null && pumpkinTiles.Count > 0)
         {
             // Hiển thị giai đoạn cuối cùng của bí ngô
             tm_Forest.SetTile(cellPos, pumpkinTiles[pumpkinTiles.Count - 1]);
             Debug.Log($"Đã hiển thị cây bí ngô tại {cellPos}");
         }
         else
         {
             Debug.LogWarning("Không tìm thấy TileBase bí ngô, sử dụng tb_Sunflower làm fallback");
             tm_Forest.SetTile(cellPos, tb_Sunflower);
         }
     }
     
     /// <summary>
     /// Lấy danh sách TileBase bí ngô từ PlayerFarmController
     /// </summary>
     private System.Collections.Generic.List<UnityEngine.Tilemaps.TileBase> GetPumpkinTilesFromController()
     {
         if (playerFarmController == null) return null;
         
         // Sử dụng reflection để lấy lstTb_Pumpkin từ PlayerFarmController
         var field = typeof(PlayerFarmController).GetField("lstTb_Pumpkin", 
             System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
         
         if (field != null)
         {
             return field.GetValue(playerFarmController) as System.Collections.Generic.List<UnityEngine.Tilemaps.TileBase>;
         }
         
         return null;
     }
     
     /// <summary>
     /// Hiển thị cây cà tím sử dụng danh sách TileBase từ PlayerFarmController
     /// </summary>
     private void SetEggplantTile(Vector3Int cellPos)
     {
         if (playerFarmController == null)
         {
             Debug.LogWarning("PlayerFarmController không tìm thấy, sử dụng tb_Sunflower làm fallback");
             tm_Forest.SetTile(cellPos, tb_Sunflower);
             return;
         }
         
         // Lấy danh sách TileBase cà tím từ PlayerFarmController
         var eggplantTiles = GetEggplantTilesFromController();
         if (eggplantTiles != null && eggplantTiles.Count > 0)
         {
             // Hiển thị giai đoạn cuối cùng của cà tím
             tm_Forest.SetTile(cellPos, eggplantTiles[eggplantTiles.Count - 1]);
             Debug.Log($"Đã hiển thị cây cà tím tại {cellPos}");
         }
         else
         {
             Debug.LogWarning("Không tìm thấy TileBase cà tím, sử dụng tb_Sunflower làm fallback");
             tm_Forest.SetTile(cellPos, tb_Sunflower);
         }
     }
     
     /// <summary>
     /// Lấy danh sách TileBase cà tím từ PlayerFarmController
     /// </summary>
     private System.Collections.Generic.List<UnityEngine.Tilemaps.TileBase> GetEggplantTilesFromController()
     {
         if (playerFarmController == null) return null;
         
         // Sử dụng reflection để lấy lstTb_Eggplant từ PlayerFarmController
         var field = typeof(PlayerFarmController).GetField("lstTb_Eggplant", 
             System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
         
         if (field != null)
         {
             return field.GetValue(playerFarmController) as System.Collections.Generic.List<UnityEngine.Tilemaps.TileBase>;
         }
         
         return null;
     }
}