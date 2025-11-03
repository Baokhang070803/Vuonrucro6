using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem; // dùng Input System (mới)
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class PlayerFarmController : MonoBehaviour
{
    [Header("Quest 2 Audio")]
    public AudioClip quest2CompletionVoice; // Giọng nói khi hoàn thành nhiệm vụ 2
    [Range(0f, 1f)] public float quest2VoiceVolume = 1f; // Âm lượng giọng nói
    private AudioSource questAudioSource; // Nguồn phát âm thanh cho quest
    
    // UI thông báo hoàn thành nhiệm vụ 10 hạt - ĐÃ CHUYỂN SANG DIALOGUE MANAGER
    [Header("Nhiệm vụ 10 hạt - Sử dụng DialogueManager")]
    // Không cần panel và text riêng nữa, sử dụng DialogueManager
    public Tilemap tm_Ground;
    public Tilemap tm_Grass;
    public Tilemap tm_Forest;
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = false; // TỐI ƯU: Tắt debug logs trong production
    
    [Header("Performance Settings")]
    [SerializeField] private bool enableFarmingOptimization = true; // Bật tối ưu farming
    [SerializeField] private int maxFarmingQueueSize = 10; // Giới hạn queue size
    
    /// <summary>
    /// TỐI ƯU: Debug log chỉ khi enableDebugLogs = true
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
    
    private void DebugLogWarning(string message)
    {
        if (enableDebugLogs)
        {
            Debug.LogWarning(message);
        }
    }

    public TileBase tb_Ground;
    public TileBase tb_Grass;
    public TileBase tb_Forest;

    public List<TileBase> lstTb_Sunflower; // danh sách các TileBase của hoa hướng dương
    public List<TileBase> lstTb_Pumpkin; // danh sách các TileBase của bí ngô
    public List<TileBase> lstTb_Pepper; // danh sách các TileBase của ớt
    public List<TileBase> lstTb_Eggplant; // danh sách các TileBase của cà tím


    // Đếm số cây đã thu hoạch
    private int harvestedCount = 0;
    private bool firstMissionCompleted = false;
    
    // Cooldown cho validation message
    private float lastValidationMessageTime = 0f;
    private float validationMessageCooldown = 3f;

    public TileMapManager tileMapManager;
    
    // Bag System Integration
    [Header("Bag System")]
    public bool enableBagSystem = true; // Bật/tắt hệ thống balo
    public bool showBagOnPlant = true; // Hiện balo khi trồng
    private GameObject bagCanvas;
    private GameObject bagPanel;
    
    // Cooldown để tránh spam
    private float lastPlantTime = 0f;
    private float plantCooldown = 2f; // 2 giây cooldown để tránh spam
    
    // ⏱️ THỜI GIAN PHÁT TRIỂN CÂY
    [Header("Plant Growth Settings")]
    [Tooltip("Thời gian mỗi giai đoạn phát triển (giây). Nếu cây có 4 giai đoạn → Tổng thời gian = stageGrowthTime × 4")]
    public float stageGrowthTime = 5f; // Mỗi giai đoạn chờ 5 giây (có thể điều chỉnh trong Inspector)
    
    // C# Farming cooldown
    private float lastCSharpUpdateTime = 0f;
    
    // C# Farming System - Không cần Python
    private bool useCSharpFarming = true; // Luôn dùng C# farming
    
    // TỐI ƯU: Batch farming operations để giảm lag
    private Queue<FarmingAction> farmingQueue = new Queue<FarmingAction>();
    private bool isProcessingFarmingQueue = false;
    
    [System.Serializable]
    public class FarmingAction
    {
        public Vector3Int position;
        public FarmingActionType actionType;
        public string seedName;
        
        public FarmingAction(Vector3Int pos, FarmingActionType type, string seed = "")
        {
            position = pos;
            actionType = type;
            seedName = seed;
        }
    }
    
    public enum FarmingActionType
    {
        ClearGrass,
        PlantSeed,
        Harvest
    }
    
    [Header("Item Icons")]
    public Sprite dâuXanhIcon; // Kéo icon Dâu Xanh vào đây
    public Sprite eggplantIcon; // Icon Cà tím
    
    [Header("Harvest Icons (Icon riêng cho thu hoạch)")]
    public Sprite harvestDâuXanhIcon; // Icon Dâu Xanh thu hoạch
    public Sprite harvestBíNgôIcon; // Icon Bí Ngô thu hoạch  
    public Sprite harvestỚtIcon; // Icon Ớt thu hoạch
    public Sprite harvestEggplantIcon; // Icon Cà tím thu hoạch


    private void Start()
    {
        // ĐÃ CHUYỂN SANG SỬ DỤNG DIALOGUE MANAGER
        // Không cần tìm GameObject cũ nữa
        Debug.Log("[PlayerFarmController] Đã chuyển sang sử dụng DialogueManager cho thông báo hoàn thành nhiệm vụ");
        
        // Chuẩn bị AudioSource cho quest audio
        questAudioSource = GetComponent<AudioSource>();
        if (questAudioSource == null)
        {
            questAudioSource = gameObject.AddComponent<AudioSource>();
            questAudioSource.playOnAwake = false;
        }
        
        // Khởi tạo C# farming system
        InitializeCSharpFarming();
    }
    
    private void InitializeCSharpFarming()
    {
        Debug.Log("✅ Khởi tạo C# farming system");
        useCSharpFarming = true;
        
        // Khởi tạo farming data nếu cần
        InitializeFarmingData();
    }
    
    private void InitializeFarmingData()
    {
        // Khởi tạo dữ liệu farming cơ bản
        Debug.Log("✅ C# farming system đã sẵn sàng");
    }

    void Update()
    {
        HandleFarmAction();
        
        // TỐI ƯU: Xử lý farming queue để giảm lag
        ProcessFarmingQueue();
        
        // Update plant growth với C# (TỐI ƯU: chỉ khi cần thiết)
        if (useCSharpFarming)
        {
            // TỐI ƯU: Chỉ update khi có cây trong scene
            if (Time.time - lastCSharpUpdateTime >= 1.0f)
            {
                UpdatePlantGrowthWithCSharp();
                lastCSharpUpdateTime = Time.time;
            }
        }
    }
    
    private void UpdatePlantGrowthWithCSharp()
    {
        // C# plant growth update logic
        // TODO: Implement plant growth stages in C#
        DebugLog("C# plant growth update - không cần Python");
    }
    
    /// <summary>
    /// TỐI ƯU: Xử lý farming queue để giảm lag
    /// </summary>
    private void ProcessFarmingQueue()
    {
        if (!enableFarmingOptimization) return;
        
        if (farmingQueue.Count > 0 && !isProcessingFarmingQueue)
        {
            StartCoroutine(ProcessFarmingQueueCoroutine());
        }
        
        // TỐI ƯU: Giới hạn queue size để tránh lag
        if (farmingQueue.Count > maxFarmingQueueSize)
        {
            Debug.LogWarning($"[PlayerFarmController] Queue quá lớn ({farmingQueue.Count}), xóa bớt...");
            while (farmingQueue.Count > maxFarmingQueueSize)
            {
                farmingQueue.Dequeue();
            }
        }
    }
    
    /// <summary>
    /// TỐI ƯU: Xử lý queue với delay để tránh lag
    /// </summary>
    private System.Collections.IEnumerator ProcessFarmingQueueCoroutine()
    {
        isProcessingFarmingQueue = true;
        
        while (farmingQueue.Count > 0)
        {
            FarmingAction action = farmingQueue.Dequeue();
            
            // Xử lý action
            switch (action.actionType)
            {
                case FarmingActionType.ClearGrass:
                    ClearGrassWithCSharp(action.position);
                    break;
                case FarmingActionType.PlantSeed:
                    PlantSeedWithCSharp(action.position);
                    break;
                case FarmingActionType.Harvest:
                    HarvestWithCSharp(action.position);
                    break;
            }
            
            // TỐI ƯU: Delay nhỏ giữa các action để tránh lag
            yield return new WaitForSeconds(0.1f);
        }
        
        isProcessingFarmingQueue = false;
    }

    public void HandleFarmAction()
    {
        var kb = Keyboard.current;
        if (kb == null) 
        {
            Debug.LogError("Keyboard.current is null!");
            return;
        }

        // CHỈ xử lý khi có input, không check validation mỗi frame
        bool hasInput = kb.cKey.wasPressedThisFrame || kb.vKey.wasPressedThisFrame || kb.mKey.wasPressedThisFrame;
        
        if (!hasInput) return; // Không có input thì không làm gì cả

        // Kiểm tra xem có thể làm farming quest không (CHỈ khi có input)
        bool canDoFarming = QuestManager.Instance != null && 
                           QuestManager.Instance.CanDoQuest("Những Hạt Mầm Đầu Tiên");

        // Sau khi hoàn thành quest "Những Hạt Mầm Đầu Tiên", cho phép farming thoải mái
        bool farmingQuestCompleted = QuestManager.Instance != null && 
                                   QuestManager.Instance.questList.Count > 1 && 
                                   QuestManager.Instance.questList[1].isCompleted; // Quest index 1 = "Những Hạt Mầm Đầu Tiên"

        Debug.Log($"🔍 Quest Check - canDoFarming: {canDoFarming}, farmingQuestCompleted: {farmingQuestCompleted}");
        Debug.Log($"🔍 Current Quest: {QuestManager.Instance?.GetCurrentQuest()?.title}");

        // SỬA: Cho phép farming sau khi hoàn thành quest "Những Hạt Mầm Đầu Tiên"
        if (!canDoFarming && !farmingQuestCompleted)
        {
            Debug.Log("❌ BỊ CHẶN: Chưa hoàn thành quest 'Những Hạt Mầm Đầu Tiên'!");
            // CHỈ hiển thị thông báo nếu đã đủ thời gian cooldown
            if (Time.time - lastValidationMessageTime >= validationMessageCooldown)
            {
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.ShowDependencyMessage("Những Hạt Mầm Đầu Tiên");
                }
                lastValidationMessageTime = Time.time;
            }
            return;
        }
        
        Debug.Log("✅ Được phép farming!");

        // Debug khi nhấn phím M
        if (kb.mKey.wasPressedThisFrame)
        {
            Debug.Log("Phím M được nhấn!");
        }

        if (kb.cKey.wasPressedThisFrame)
        {
            DebugLog("🔨 Phím C được nhấn - Bắt đầu đào đất!");
            Vector3Int cellPos = tm_Ground.WorldToCell(transform.position);
            DebugLog("Cell cell pos: " + cellPos);

            // TỐI ƯU: Thêm vào queue thay vì xử lý ngay
            farmingQueue.Enqueue(new FarmingAction(cellPos, FarmingActionType.ClearGrass));
        }

        if (kb.vKey.wasPressedThisFrame)
        {
            Vector3Int cellPos = tm_Ground.WorldToCell(transform.position);
            DebugLog("Cell cell pos: " + cellPos);

            // TỐI ƯU: Thêm vào queue thay vì xử lý ngay
            farmingQueue.Enqueue(new FarmingAction(cellPos, FarmingActionType.PlantSeed));
        }

        if(kb.mKey.wasPressedThisFrame)
        {
            Vector3Int cellPos = tm_Ground.WorldToCell(transform.position);
            DebugLog("Cell cell pos: " + cellPos);

            // TỐI ƯU: Thêm vào queue thay vì xử lý ngay
            farmingQueue.Enqueue(new FarmingAction(cellPos, FarmingActionType.Harvest));
        }
    }
    
    // ========== C# FARMING METHODS ==========
    
    // ClearGrassWithPython đã được xóa - chỉ dùng C#
    
    // PlantSeedWithPython đã được xóa - chỉ dùng C#
    
    /// <summary>
    /// Kiểm tra xem có cây nào ở vị trí này trong Unity tilemap không
    /// </summary>
    private bool HasPlantAtPosition(Vector3Int cellPos)
    {
        // Kiểm tra tm_Forest tilemap (nơi chứa tất cả cây)
        TileBase forestTile = tm_Forest.GetTile(cellPos);
        bool hasPlant = forestTile != null;
        
        Debug.Log($"[Unity] Checking plant at {cellPos}: Forest tile = {forestTile?.name ?? "NULL"}");
        
        return hasPlant;
    }
    
    /// <summary>
    /// Debug method để kiểm tra trạng thái cây tại vị trí
    /// </summary>
    [ContextMenu("Debug Plant Status")]
    private void DebugPlantStatus()
    {
        Vector3Int cellPos = tm_Ground.WorldToCell(transform.position);
        Debug.Log($"=== DEBUG PLANT STATUS AT {cellPos} ===");
        
        // Kiểm tra Unity tilemap
        bool hasUnityPlant = HasPlantAtPosition(cellPos);
        Debug.Log($"Unity has plant: {hasUnityPlant}");
        
        // Kiểm tra C# farming data
        DebugLog("C# farming debug - không cần Python");
        
        Debug.Log("=== END DEBUG ===");
    }
    
    // HarvestWithPython đã được xóa - chỉ dùng C#
    
    // ========== C# FARMING METHODS ==========
    
    private void ClearGrassWithCSharp(Vector3Int cellPos)
    {
        Debug.Log($"🔨 ClearGrassWithCSharp được gọi tại vị trí: {cellPos}");
        TileBase crrTileBase = tm_Grass.GetTile(cellPos);
        if (crrTileBase == tb_Grass)
        {
            Debug.Log($"✅ Tìm thấy cỏ tại {cellPos}, đang xóa...");
            tm_Grass.SetTile(cellPos, null);
            tileMapManager.SetStateForTilemapDetail(cellPos.x, cellPos.y, TilemapState.Ground);
            Debug.Log($"✅ Đã xóa cỏ và cập nhật trạng thái đất tại {cellPos}");
        }
        else
        {
            Debug.Log($"❌ Không có cỏ tại {cellPos}, tile: {crrTileBase}");
        }
    }
    
    private void PlantSeedWithCSharp(Vector3Int cellPos)
    {
        TileBase crrTileBase = tm_Grass.GetTile(cellPos);
        TileBase forestTileBase = tm_Forest.GetTile(cellPos);

        // Kiểm tra vị trí có thể trồng không
        if (CanPlantAtPosition(cellPos))
        {
            // Kiểm tra xem có nên hiện balo để chọn hạt giống không
            if (showBagOnPlant && enableBagSystem)
            {
                ShowBagForPlanting(cellPos);
            }
            else
            {
                // Trồng hạt giống mặc định
                PlantDefaultSeed(cellPos);
            }
        }
        else
        {
            // Hiển thị thông báo không thể trồng
            ShowCannotPlantMessage(cellPos);
        }
    }
    
    private void HarvestWithCSharp(Vector3Int cellPos)
    {
        TileBase crrTileBase = tm_Forest.GetTile(cellPos);
        Debug.Log("Current tile: " + (crrTileBase != null ? crrTileBase.name : "NULL"));

        // Kiểm tra thu hoạch cho tất cả loại cây
        bool canHarvest = false;
        string harvestItemName = "";
        
        // Kiểm tra hoa hướng dương
        if (lstTb_Sunflower.Count > 0 && crrTileBase == lstTb_Sunflower[lstTb_Sunflower.Count - 1])
        {
            canHarvest = true;
            harvestItemName = "Dâu Xanh";
        }
        // Kiểm tra bí ngô
        else if (lstTb_Pumpkin != null && lstTb_Pumpkin.Count > 0 && crrTileBase == lstTb_Pumpkin[lstTb_Pumpkin.Count - 1])
        {
            canHarvest = true;
            harvestItemName = "Bí Ngô";
        }
        // Kiểm tra ớt
        else if (lstTb_Pepper != null && lstTb_Pepper.Count > 0 && crrTileBase == lstTb_Pepper[lstTb_Pepper.Count - 1])
        {
            canHarvest = true;
            harvestItemName = "Ớt";
        }
        // Kiểm tra cà tím
        else if (lstTb_Eggplant != null && lstTb_Eggplant.Count > 0 && crrTileBase == lstTb_Eggplant[lstTb_Eggplant.Count - 1])
        {
            canHarvest = true;
            harvestItemName = "Cà tím";
        }
        
        if (canHarvest)
        {
            Debug.Log("Điều kiện thu hoạch đạt! Hiển thị tùy chọn...");
            
            // Hiển thị dialog lựa chọn thu hoạch
            ShowHarvestOptions(harvestItemName, cellPos);
        }
        
        if (!firstMissionCompleted && harvestedCount >= 10)
        {
            CompleteFarmingQuest();
        }
    }
    
    private void CompleteFarmingQuest()
    {
        firstMissionCompleted = true;
        Debug.Log("Đạt đủ 10 cây! Đang kiểm tra UI...");
        
        // Hoàn thành nhiệm vụ trong Quest Manager
        QuestManager.CompleteCurrentQuest("Những Hạt Mầm Đầu Tiên");
        
        // Phát âm thanh giọng nói khi hoàn thành nhiệm vụ 2
        if (quest2CompletionVoice != null && questAudioSource != null)
        {
            questAudioSource.PlayOneShot(quest2CompletionVoice, quest2VoiceVolume);
            Debug.Log("[PlayerFarmController] Đã phát âm thanh hoàn thành nhiệm vụ 2!");
        }
        
        // Hiển thị thông báo hoàn thành nhiệm vụ qua DialogueManager
        if (DialogueManager.I != null)
        {
            Debug.Log("Hiển thị thông báo hoàn thành nhiệm vụ qua DialogueManager!");
            DialogueManager.I.Show(new List<string> 
            { 
                "🎉 Chúc mừng!",
                "Bạn đã hoàn thành nhiệm vụ 'Những Hạt Mầm Đầu Tiên'!",
                "Đã thu hoạch đủ 10 cây!",
                "Hãy tiếp tục khám phá và hoàn thành các nhiệm vụ tiếp theo!"
            });
        }
        else
        {
            Debug.LogWarning("DialogueManager.I không tìm thấy! Sử dụng Debug.Log thay thế");
            Debug.Log("🎉 Bạn đã hoàn thành nhiệm vụ Những Hạt Mầm Đầu Tiên!");
        }
    }

    // ĐÃ XÓA: Không cần coroutine ẩn panel nữa vì sử dụng DialogueManager

    IEnumerator GrowPlant(Vector3Int cellPos, Tilemap tilemap, List<TileBase> lstTileBase)
    {
        int crrStage = 0;
        int totalStages = lstTileBase.Count;
        
        Debug.Log($"🌱 Bắt đầu trồng cây tại {cellPos} | {totalStages} giai đoạn | {stageGrowthTime}s/giai đoạn | Tổng: {totalStages * stageGrowthTime}s");

        while (crrStage < totalStages)
        {
            // TỐI ƯU: Chỉ set tile, không gọi SetStateForTilemapDetail trong coroutine
            tilemap.SetTile(cellPos, lstTileBase[crrStage]);
            
            Debug.Log($"🌿 Cây đang lớn... Giai đoạn {crrStage + 1}/{totalStages}");
            
            // ⏱️ THỜI GIAN PHÁT TRIỂN: Sử dụng biến stageGrowthTime (có thể điều chỉnh trong Inspector)
            // Ví dụ: stageGrowthTime = 5s, cây có 4 giai đoạn → Tổng: 20 giây để chín
            yield return new WaitForSeconds(stageGrowthTime);
            crrStage++;
        }
        
        Debug.Log($"🌾 Cây đã chín! Có thể thu hoạch tại {cellPos}");
        
        // TỐI ƯU: Chỉ gọi SetStateForTilemapDetail 1 lần ở cuối
        if (tileMapManager != null)
        {
            tileMapManager.SetStateForTilemapDetail(cellPos.x, cellPos.y, GetTilemapStateFromTileBase(lstTileBase[lstTileBase.Count - 1]));
        }
    }
    
    /// <summary>
    /// TỐI ƯU: Lấy TilemapState từ TileBase cuối cùng
    /// </summary>
    private TilemapState GetTilemapStateFromTileBase(TileBase tileBase)
    {
        // Kiểm tra loại cây dựa trên TileBase
        if (lstTb_Sunflower != null && lstTb_Sunflower.Count > 0 && tileBase == lstTb_Sunflower[lstTb_Sunflower.Count - 1])
            return TilemapState.Sunflower;
        else if (lstTb_Pumpkin != null && lstTb_Pumpkin.Count > 0 && tileBase == lstTb_Pumpkin[lstTb_Pumpkin.Count - 1])
            return TilemapState.Pumpkin;
        else if (lstTb_Pepper != null && lstTb_Pepper.Count > 0 && tileBase == lstTb_Pepper[lstTb_Pepper.Count - 1])
            return TilemapState.Pepper;
        else if (lstTb_Eggplant != null && lstTb_Eggplant.Count > 0 && tileBase == lstTb_Eggplant[lstTb_Eggplant.Count - 1])
            return TilemapState.Eggplant;
        else
            return TilemapState.Sunflower; // Fallback
    }
    
    // Hiện balo để chọn hạt giống
    void ShowBagForPlanting(Vector3Int cellPos)
    {
        // Tìm BagCanvas
        if (bagCanvas == null)
        {
            bagCanvas = GameObject.Find("BagCanvas");
        }
        
        // Tìm BagPanel bên trong BagCanvas
        if (bagPanel == null && bagCanvas != null)
        {
            // Tìm BagPanel với nhiều tên khác nhau
            bagPanel = bagCanvas.transform.Find("BagPanel")?.gameObject;
            if (bagPanel == null)
            {
                bagPanel = bagCanvas.transform.Find("Panel")?.gameObject;
            }
            if (bagPanel == null)
            {
                bagPanel = bagCanvas.transform.Find("Bag Panel")?.gameObject;
            }
            if (bagPanel == null)
            {
                // Tìm tất cả child objects có chứa "Panel"
                Transform[] children = bagCanvas.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    if (child.name.ToLower().Contains("panel"))
                    {
                        bagPanel = child.gameObject;
                        Debug.Log($"✓ Tìm thấy BagPanel: {child.name}");
                        break;
                    }
                }
            }
        }
        
        if (bagCanvas != null)
        {
            // Lưu vị trí để trồng
            PlayerPrefs.SetInt("PlantPosX", cellPos.x);
            PlayerPrefs.SetInt("PlantPosY", cellPos.y);
            
            // Debug thông tin
            Debug.Log($"BagCanvas found: {bagCanvas.name}");
            Debug.Log($"BagPanel found: {(bagPanel != null ? bagPanel.name : "NULL")}");
            
            // Hiện BagCanvas
            bagCanvas.SetActive(true);
            
            // Hiện BagPanel nếu có
            if (bagPanel != null)
            {
                bagPanel.SetActive(true);
                Debug.Log($"✓ Hiển thị BagPanel: {bagPanel.name}");
            }
            else
            {
                Debug.LogWarning("⚠ Không tìm thấy BagPanel trong BagCanvas!");
            }
            
            // Đóng ItemDetailPanel khi mở balo mới
            BagUI bagUI = FindObjectOfType<BagUI>();
            if (bagUI != null)
            {
                bagUI.CloseItemDetailOnBagOpen();
            }
            
            // Đảm bảo Canvas có sorting order cao
            Canvas canvas = bagCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 2000;
            }
            
            Debug.Log("✓ Hiển thị balo để chọn hạt giống!");
        }
        else
        {
            Debug.LogWarning("⚠ Không tìm thấy BagCanvas!");
            // Fallback: trồng hạt giống mặc định
            PlantDefaultSeed(cellPos);
        }
    }
    
    // Trồng hạt giống mặc định (hoa hướng dương)
    void PlantDefaultSeed(Vector3Int cellPos)
    {
        // Kiểm tra vị trí có thể trồng không
        if (!CanPlantAtPosition(cellPos))
        {
            ShowCannotPlantMessage(cellPos);
            return;
        }
        
        StartCoroutine(GrowPlant(cellPos, tm_Forest, lstTb_Sunflower));
        // TỐI ƯU: Không cần gọi SetStateForTilemapDetail ở đây nữa vì đã gọi trong GrowPlant
        
        // THÊM EXP TỪ FARMING
        if (PlayerExpManager.Instance != null)
        {
            PlayerExpManager.Instance.AddFarmingExp();
        }
        
        Debug.Log("✓ Đã trồng hoa hướng dương mặc định!");
    }
    
    // Trồng hạt giống từ balo (được gọi từ BagManager)
    public void PlantSeedFromBag(string seedName, int quantity)
    {
        // Kiểm tra cooldown
        if (Time.time - lastPlantTime < plantCooldown)
        {
            Debug.Log("⚠ Đang trong cooldown, vui lòng đợi!");
            return;
        }
        
        // Lấy vị trí đã lưu
        int posX = PlayerPrefs.GetInt("PlantPosX", 0);
        int posY = PlayerPrefs.GetInt("PlantPosY", 0);
        Vector3Int cellPos = new Vector3Int(posX, posY, 0);
        
        // Kiểm tra vị trí có thể trồng không
        if (!CanPlantAtPosition(cellPos))
        {
            ShowCannotPlantMessage(cellPos);
            return;
        }
        
        // Kiểm tra xem có hạt giống trong balo không
        if (BagManager.Instance != null && BagManager.Instance.HasItem(seedName))
        {
            // Sử dụng hạt giống từ balo
            bool seedUsed = BagManager.Instance.UseItem(seedName, 1);
            if (seedUsed)
            {
                // Cập nhật thời gian trồng
                lastPlantTime = Time.time;
                
                // Chọn đúng loại cây dựa trên tên hạt giống
                List<TileBase> plantTiles = GetPlantTilesForSeed(seedName);
                TilemapState plantState = GetPlantStateForSeed(seedName);
                
                // Kiểm tra plantTiles có null không
                if (plantTiles == null)
                {
                    Debug.LogError($"🌱 LỖI: Không thể trồng {seedName}! Vui lòng kiểm tra TileBase trong Inspector!");
                    return;
                }
                
                // Debug thông tin
                Debug.Log($"🌱 Trồng hạt giống: {seedName}");
                Debug.Log($"🌱 Sử dụng {plantTiles.Count} TileBase");
                Debug.Log($"🌱 TilemapState: {plantState}");
                if (plantTiles.Count > 0)
                {
                    Debug.Log($"🌱 TileBase đầu tiên: {plantTiles[0].name}");
                }
                
                // Trồng hạt giống
                StartCoroutine(GrowPlant(cellPos, tm_Forest, plantTiles));
                // TỐI ƯU: Không cần gọi SetStateForTilemapDetail ở đây nữa vì đã gọi trong GrowPlant
                
                // THÊM EXP TỪ FARMING
                if (PlayerExpManager.Instance != null)
                {
                    PlayerExpManager.Instance.AddFarmingExp();
                }
                
                // Đóng balo sau khi trồng xong (delay 1 giây)
                StartCoroutine(CloseBagAfterPlanting());
                
                Debug.Log($"✓ Đã trồng {seedName} từ balo!");
            }
            else
            {
                Debug.LogWarning($"⚠ Không thể sử dụng {seedName} từ balo!");
            }
        }
        else
        {
            Debug.LogWarning($"⚠ Không có {seedName} trong balo!");
        }
    }
    
    /// <summary>
    /// Đóng balo sau khi trồng xong
    /// </summary>
    IEnumerator CloseBagAfterPlanting()
    {
        yield return new WaitForSeconds(1f); // Đợi 1 giây
        
        // Tìm BagUI và đóng balo
        BagUI bagUI = FindObjectOfType<BagUI>();
        if (bagUI != null)
        {
            bagUI.CloseBagImmediately();
            Debug.Log("✓ Đã đóng balo sau khi trồng xong!");
        }
    }
    
    /// <summary>
    /// Lấy danh sách TileBase cho hạt giống
    /// </summary>
    List<TileBase> GetPlantTilesForSeed(string seedName)
    {
        Debug.Log($"🔍 GetPlantTilesForSeed được gọi với seedName: '{seedName}'");
        
        // Trim khoảng trắng và chuyển về lowercase
        string cleanSeedName = seedName.Trim().ToLower();
        Debug.Log($"🔍 seedName.Trim().ToLower(): '{cleanSeedName}'");
        
        // ✅ Xử lý tên hạt giống có tiền tố "hạt " (loại bỏ tiền tố để mapping)
        if (cleanSeedName.StartsWith("hạt "))
        {
            cleanSeedName = cleanSeedName.Substring(4); // Bỏ "hạt " → "bí ngô"
            Debug.Log($"🔍 Loại bỏ 'hạt ' → '{cleanSeedName}'");
        }
        
        switch (cleanSeedName)
        {
            case "bí ngô":
            case "pumpkin":
            case "giống bí ngô": // Nếu tên là "Hạt Giống Bí Ngô"
                Debug.Log($"🔍 Chọn bí ngô - lstTb_Pumpkin có {lstTb_Pumpkin?.Count ?? 0} phần tử");
                return lstTb_Pumpkin != null && lstTb_Pumpkin.Count > 0 ? lstTb_Pumpkin : lstTb_Sunflower;
            
            case "ớt":
            case "ớt đỏ":
            case "pepper":
            case "giống ớt":
                Debug.Log($"🔍 Chọn ớt - lstTb_Pepper có {lstTb_Pepper?.Count ?? 0} phần tử");
                return lstTb_Pepper != null && lstTb_Pepper.Count > 0 ? lstTb_Pepper : lstTb_Sunflower;
            
            case "cà tím":
            case "eggplant":
            case "giống cà tím":
                Debug.Log($"🔍 Chọn cà tím - lstTb_Eggplant có {lstTb_Eggplant?.Count ?? 0} phần tử");
                if (lstTb_Eggplant != null && lstTb_Eggplant.Count > 0)
                {
                    Debug.Log($"🔍 Sử dụng lstTb_Eggplant với {lstTb_Eggplant.Count} TileBase");
                    return lstTb_Eggplant;
                }
                else
                {
                    Debug.LogError($"🔍 LỖI: lstTb_Eggplant không có hoặc rỗng! Vui lòng gán TileBase cho cà tím trong Inspector!");
                    Debug.LogError($"🔍 lstTb_Eggplant = {lstTb_Eggplant}, Count = {lstTb_Eggplant?.Count ?? 0}");
                    return null;
                }
            
            case "hoa hướng dương":
            case "dâu xanh":
            case "sunflower":
            case "blueberry": // ✅ THÊM - Tên tiếng Anh trong shop
            case "giống cơ bản":
            case "giống hoa hướng dương":
            case "giống blueberry": // ✅ THÊM
            default:
                Debug.Log($"🔍 Chọn mặc định (sunflower) - lstTb_Sunflower có {lstTb_Sunflower?.Count ?? 0} phần tử");
                return lstTb_Sunflower;
        }
    }
    
    /// <summary>
    /// Lấy TilemapState cho hạt giống
    /// </summary>
    TilemapState GetPlantStateForSeed(string seedName)
    {
        string cleanName = seedName.Trim().ToLower();
        
        // ✅ Xử lý tên hạt giống có tiền tố "hạt "
        if (cleanName.StartsWith("hạt "))
        {
            cleanName = cleanName.Substring(4); // Bỏ "hạt " → "bí ngô"
        }
        
        switch (cleanName)
        {
            case "bí ngô":
            case "pumpkin":
            case "giống bí ngô":
                return TilemapState.Pumpkin;
            
            case "ớt":
            case "ớt đỏ":
            case "pepper":
            case "giống ớt":
                return TilemapState.Pepper;
            
            case "cà tím":
            case "eggplant":
            case "giống cà tím":
                return TilemapState.Eggplant;
            
            case "hoa hướng dương":
            case "dâu xanh":
            case "sunflower":
            case "blueberry": // ✅ THÊM
            case "giống cơ bản":
            case "giống hoa hướng dương":
            case "giống blueberry": // ✅ THÊM
            default:
                return TilemapState.Sunflower;
        }
    }
    
    /// <summary>
    /// Lấy icon cho sản phẩm thu hoạch
    /// </summary>
    Sprite GetHarvestIcon(string itemName)
    {
        switch (itemName.ToLower())
        {
            case "dâu xanh":
                return GetDâuXanhHarvestIcon();
            case "bí ngô":
                return GetBíNgôHarvestIcon();
            case "ớt":
                return GetỚtHarvestIcon();
            case "cà tím":
                return GetEggplantHarvestIcon();
            default:
                return GetDâuXanhHarvestIcon(); // Fallback
        }
    }
    
    /// <summary>
    /// Lấy icon cho Dâu Xanh
    /// </summary>
    Sprite GetDâuXanhIcon()
    {
        // Ưu tiên icon từ Inspector
        if (dâuXanhIcon != null)
        {
            return dâuXanhIcon;
        }
        
        // Thử lấy icon từ tilemap hiện tại
        if (lstTb_Sunflower != null && lstTb_Sunflower.Count > 0)
        {
            // Kiểm tra nếu là Tile (có sprite) thay vì TileBase
            if (lstTb_Sunflower[0] is Tile tile)
            {
                return tile.sprite;
            }
        }
        
        // Nếu không có, tìm trong Resources
        Sprite dâuXanhSprite = Resources.Load<Sprite>("Items/DâuXanh");
        if (dâuXanhSprite != null)
        {
            return dâuXanhSprite;
        }
        
        // Fallback: tìm sprite có tên chứa "blueberry" hoặc "dâu"
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        foreach (Sprite sprite in allSprites)
        {
            if (sprite.name.ToLower().Contains("blueberry") || 
                sprite.name.ToLower().Contains("dâu") ||
                sprite.name.ToLower().Contains("berry"))
            {
                return sprite;
            }
        }
        
        Debug.LogWarning("⚠ Không tìm thấy icon Dâu Xanh, sử dụng icon mặc định");
        return null; // Sẽ hiển thị icon trống
    }
    
    /// <summary>
    /// Lấy icon cho Bí Ngô
    /// </summary>
    Sprite GetPumpkinIcon()
    {
        // Thử lấy icon từ tilemap bí ngô
        if (lstTb_Pumpkin != null && lstTb_Pumpkin.Count > 0)
        {
            if (lstTb_Pumpkin[0] is Tile tile)
            {
                return tile.sprite;
            }
        }
        
        // Tìm trong Resources
        Sprite pumpkinSprite = Resources.Load<Sprite>("Items/BíNgô");
        if (pumpkinSprite != null)
        {
            return pumpkinSprite;
        }
        
        // Fallback: tìm sprite có tên chứa "pumpkin"
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        foreach (Sprite sprite in allSprites)
        {
            if (sprite.name.ToLower().Contains("pumpkin") || 
                sprite.name.ToLower().Contains("bí"))
            {
                return sprite;
            }
        }
        
        Debug.LogWarning("⚠ Không tìm thấy icon Bí Ngô, sử dụng icon mặc định");
        return null;
    }
    
    /// <summary>
    /// Lấy icon cho Ớt
    /// </summary>
    Sprite GetPepperIcon()
    {
        // Thử lấy icon từ tilemap ớt
        if (lstTb_Pepper != null && lstTb_Pepper.Count > 0)
        {
            if (lstTb_Pepper[0] is Tile tile)
            {
                return tile.sprite;
            }
        }
        
        // Tìm trong Resources
        Sprite pepperSprite = Resources.Load<Sprite>("Items/Ớt");
        if (pepperSprite != null)
        {
            return pepperSprite;
        }
        
        // Fallback: tìm sprite có tên chứa "pepper" hoặc "ớt"
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        foreach (Sprite sprite in allSprites)
        {
            if (sprite.name.ToLower().Contains("pepper") || 
                sprite.name.ToLower().Contains("ớt"))
            {
                return sprite;
            }
        }
        
        Debug.LogWarning("⚠ Không tìm thấy icon Ớt, sử dụng icon mặc định");
        return null;
    }
    
    /// <summary>
    /// Lấy icon thu hoạch cho Dâu Xanh
    /// </summary>
    Sprite GetDâuXanhHarvestIcon()
    {
        // Ưu tiên icon thu hoạch riêng biệt
        if (harvestDâuXanhIcon != null)
        {
            return harvestDâuXanhIcon;
        }
        
        // Fallback: sử dụng icon cũ
        return GetDâuXanhIcon();
    }
    
    /// <summary>
    /// Lấy icon thu hoạch cho Bí Ngô
    /// </summary>
    Sprite GetBíNgôHarvestIcon()
    {
        // Ưu tiên icon thu hoạch riêng biệt
        if (harvestBíNgôIcon != null)
        {
            return harvestBíNgôIcon;
        }
        
        // Fallback: sử dụng icon cũ
        return GetPumpkinIcon();
    }
    
    /// <summary>
    /// Lấy icon thu hoạch cho Ớt
    /// </summary>
    Sprite GetỚtHarvestIcon()
    {
        // Ưu tiên icon thu hoạch riêng biệt
        if (harvestỚtIcon != null)
        {
            return harvestỚtIcon;
        }
        
        // Fallback: sử dụng icon cũ
        return GetPepperIcon();
    }
    
    /// <summary>
    /// Lấy icon cho Cà tím
    /// </summary>
    Sprite GetEggplantIcon()
    {
        // Ưu tiên icon từ Inspector
        if (eggplantIcon != null)
        {
            return eggplantIcon;
        }
        
        // Thử lấy icon từ tilemap cà tím
        if (lstTb_Eggplant != null && lstTb_Eggplant.Count > 0)
        {
            if (lstTb_Eggplant[0] is Tile tile)
            {
                return tile.sprite;
            }
        }
        
        // Tìm trong Resources
        Sprite eggplantSprite = Resources.Load<Sprite>("Items/CàTím");
        if (eggplantSprite != null)
        {
            return eggplantSprite;
        }
        
        // Fallback: tìm sprite có tên chứa "eggplant" hoặc "cà tím"
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        foreach (Sprite sprite in allSprites)
        {
            if (sprite.name.ToLower().Contains("eggplant") || 
                sprite.name.ToLower().Contains("cà tím") ||
                sprite.name.ToLower().Contains("eggplant"))
            {
                return sprite;
            }
        }
        
        Debug.LogWarning("⚠ Không tìm thấy icon Cà tím, sử dụng icon mặc định");
        return null;
    }
    
    /// <summary>
    /// Lấy icon thu hoạch cho Cà tím
    /// </summary>
    Sprite GetEggplantHarvestIcon()
    {
        // Ưu tiên icon thu hoạch riêng biệt
        if (harvestEggplantIcon != null)
        {
            return harvestEggplantIcon;
        }
        
        // Fallback: sử dụng icon cũ
        return GetEggplantIcon();
    }
    
    /// <summary>
    /// Hiển thị tùy chọn thu hoạch (Bán/Bỏ/Lưu vào balo)
    /// </summary>
    void ShowHarvestOptions(string itemName, Vector3Int cellPos)
    {
        Debug.Log($"🌾 Thu hoạch {itemName}! Chọn hành động:");
        Debug.Log("1. Lưu vào balo");
        Debug.Log("2. Bán ngay");
        Debug.Log("3. Bỏ đi");
        
        // Tạm thời tự động lưu vào balo (có thể thay đổi thành UI dialog sau)
        ProcessHarvestChoice(itemName, cellPos, "save");
    }
    
    /// <summary>
    /// Xử lý lựa chọn thu hoạch
    /// </summary>
    void ProcessHarvestChoice(string itemName, Vector3Int cellPos, string choice)
    {
        // Xóa cây khỏi map
        tm_Grass.SetTile(cellPos, tb_Grass);
        tm_Forest.SetTile(cellPos, null);
        
        // Cập nhật trạng thái tilemap về Grass
        if (tileMapManager != null)
        {
            tileMapManager.SetStateForTilemapDetail(cellPos.x, cellPos.y, TilemapState.Grass);
        }
        
        // Đếm số cây đã thu hoạch
        harvestedCount++;
        Debug.Log($"Đã thu hoạch: {harvestedCount} cây");
        
        switch (choice.ToLower())
        {
            case "save":
                SaveToBag(itemName);
                break;
            case "sell":
                SellItem(itemName);
                break;
            case "drop":
                DropItem(itemName);
                break;
            default:
                SaveToBag(itemName); // Mặc định lưu vào balo
                break;
        }
        
        // Thêm EXP từ thu hoạch
        if (PlayerExpManager.Instance != null)
        {
            PlayerExpManager.Instance.AddHarvestingExp();
        }
    }
    
    /// <summary>
    /// Lưu item vào balo
    /// </summary>
    void SaveToBag(string itemName)
    {
        if (enableBagSystem && BagManager.Instance != null)
        {
            Sprite harvestIcon = GetHarvestIcon(itemName);
            int sellPrice = GetItemSellPrice(itemName);
            
            bool addedToBag = BagManager.Instance.AddItem(itemName, harvestIcon, 1, sellPrice);
            if (addedToBag)
            {
                Debug.Log($"✓ Đã lưu {itemName} vào balo!");
            }
            else
            {
                Debug.LogWarning($"⚠ Không thể lưu {itemName} vào balo (có thể balo đã đầy)!");
            }
        }
    }
    
    /// <summary>
    /// Bán item ngay lập tức
    /// </summary>
    void SellItem(string itemName)
    {
        int sellPrice = GetItemSellPrice(itemName);
        
        if (PlayerGoldManager.Instance != null)
        {
            PlayerGoldManager.Instance.AddGold(sellPrice);
            Debug.Log($"💰 Đã bán {itemName} với giá {sellPrice} vàng!");
        }
        else
        {
            Debug.LogWarning("⚠ Không thể bán item - PlayerGoldManager không tìm thấy!");
        }
    }
    
    /// <summary>
    /// Bỏ item (không làm gì)
    /// </summary>
    void DropItem(string itemName)
    {
        Debug.Log($"🗑️ Đã bỏ {itemName}!");
    }
    
    /// <summary>
    /// Lấy giá bán của item
    /// </summary>
    int GetItemSellPrice(string itemName)
    {
        switch (itemName.ToLower())
        {
            case "dâu xanh":
                return 30; // 30 vàng
            case "bí ngô":
                return 80; // 80 vàng
            case "ớt":
                return 100; // 100 vàng
            case "cà tím":
                return 120; // 120 vàng
            default:
                return 30; // Giá mặc định
        }
    }
    
    /// <summary>
    /// Kiểm tra vị trí có thể trồng cây hay không
    /// </summary>
    bool CanPlantAtPosition(Vector3Int cellPos)
    {
        TileBase grassTile = tm_Grass.GetTile(cellPos);
        TileBase forestTile = tm_Forest.GetTile(cellPos);
        
        // Chỉ có thể trồng khi không có cỏ và không có cây
        return grassTile == null && forestTile == null;
    }
    
    /// <summary>
    /// Hiển thị thông báo không thể trồng
    /// </summary>
    void ShowCannotPlantMessage(Vector3Int cellPos)
    {
        TileBase grassTile = tm_Grass.GetTile(cellPos);
        TileBase forestTile = tm_Forest.GetTile(cellPos);
        
        if (forestTile != null)
        {
            Debug.Log("⚠ Không thể trồng! Vị trí này đã có cây rồi!");
            
            if (DialogueManager.I != null)
            {
                DialogueManager.I.Show(new List<string> 
                { 
                    "⚠ Không thể trồng!",
                    "Vị trí này đã có cây rồi!",
                    "Hãy tìm vị trí trống khác để trồng."
                });
            }
        }
        else if (grassTile != null)
        {
            Debug.Log("⚠ Không thể trồng! Vị trí này chưa được dọn cỏ!");
            
            if (DialogueManager.I != null)
            {
                DialogueManager.I.Show(new List<string> 
                { 
                    "⚠ Không thể trồng!",
                    "Hãy dọn cỏ trước (nhấn C) rồi mới trồng!",
                    "Sử dụng: C (dọn cỏ) → V (trồng cây)"
                });
            }
        }
    }
}
