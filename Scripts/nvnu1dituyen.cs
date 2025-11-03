using UnityEngine;
using UnityEngine.InputSystem; // Thêm dòng này
using UnityEngine.EventSystems;
using Python.Runtime;
using System;
using System.IO;

public class nvnu1dituyen : MonoBehaviour
{
    public float moveSpeed = 5f; // tốc độ di chuyển
    private Vector2 moveInput;
    private bool isSprinting;
    private Vector2 movement; // Thêm biến này
    private Animator animator; // Thêm biến này
    private Vector3? mouseTarget = null; // Vị trí cần đến khi nhấn chuột
    private Rigidbody2D rb; // Thêm biến này
    private Collider2D col; // collider của người chơi

    [Header("Village Entry Quest")]
    public Transform villageEntryTarget; // Kéo GameObject QuestManager vào đây
    public Vector3 villageEntryPosition = new Vector3(0, 0, 0); // Hoặc nhập tọa độ thủ công
    public float entryDistance = 2f; // Khoảng cách để hoàn thành nhiệm vụ
    private bool hasEnteredVillage = false;

    // Python.NET integration (Optional - fallback to C# if not available)
    private static PyObject playerMovementModule;
    private bool usePython = false; // Chuyển sang false nếu Python không khả dụng
    
    // C# Movement variables (BACKUP khi không có Python)
    private Vector2 lastMoveDirection;
    private float horizontal = 0f;
    private float vertical = 0f;
    private float speed = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>(); // Lấy animator từ GameObject
        rb = GetComponent<Rigidbody2D>(); // Lấy Rigidbody2D
        col = GetComponent<Collider2D>(); // Lấy Collider2D
        if (rb != null)
        {
            rb.freezeRotation = true; // Khóa xoay Rigidbody2D
            rb.gravityScale = 0;      // Không bị rơi
        }

        // Kiểm tra xem có cần khôi phục vị trí từ combat không - PHIÊN BẢN AN TOÀN
        CheckReturnFromCombat();

        Debug.Log("nvnu1dituyen Start hoàn thành, player có thể di chuyển");

        // Thử khởi tạo Python, nếu không được thì dùng C#
        TryInitializePython();
    }
    
    void CheckReturnFromCombat()
    {
        try
        {
            // CHỈ check khi scene là map1 và có flag rõ ràng
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "map1")
            {
                string combatFlag = PlayerPrefs.GetString("JustFinishedCombat", "false");
                Debug.Log($"Combat flag: {combatFlag}");
                
                if (combatFlag == "true")
                {
                    Debug.Log("Vừa hoàn thành combat, khôi phục vị trí...");
                    
                    // KHÔNG clear flag ngay - để các script khác kiểm tra
                    // Flag sẽ được clear sau khi tất cả script đã kiểm tra
                    
                    // Khôi phục vị trí nếu có
                    if (PlayerPrefs.HasKey("SavedPlayerX"))
                    {
                        float x = PlayerPrefs.GetFloat("SavedPlayerX");
                        float y = PlayerPrefs.GetFloat("SavedPlayerY");
                        float z = PlayerPrefs.GetFloat("SavedPlayerZ");
                        
                        Vector3 savedPosition = new Vector3(x, y, z);
                        transform.position = savedPosition;
                        Debug.Log($"[nvnu1dituyen] Đã khôi phục vị trí từ combat: {savedPosition}");
                    }
                    else
                    {
                        Debug.LogWarning("[nvnu1dituyen] Không có vị trí đã lưu từ combat! Player sẽ ở vị trí hiện tại.");
                    }
                    
                    // Destroy tất cả slime trong scene
                    DestroyAllSlimes();
                    
                    // Đánh dấu đã xem intro để không phát lại nữa
                    MarkIntroAsWatched();
                    
                    // Clear flag sau 1 frame để các script khác có thể kiểm tra
                    StartCoroutine(ClearCombatFlagDelayed());
                    
                    Debug.Log("CheckReturnFromCombat hoàn thành!");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Lỗi trong CheckReturnFromCombat: {e.Message}");
            // Clear flag để tránh lỗi lặp lại
            PlayerPrefs.DeleteKey("JustFinishedCombat");
            PlayerPrefs.Save();
        }
    }
    
    void DestroyAllSlimes()
    {
        try
        {
            // Cách 1: Destroy theo tên cụ thể
            GameObject slimeGreen = GameObject.Find("Slime_Green_0");
            if (slimeGreen != null)
            {
                Debug.Log("Destroying Slime_Green_0");
                Destroy(slimeGreen);
            }
            
            // Cách 2: Tìm và destroy tất cả GameObject có tên chứa "Slime"
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.name.ToLower().Contains("slime"))
                {
                    Debug.Log($"Destroying slime: {obj.name}");
                    Destroy(obj);
                }
            }
            
            // Cách 3: Tìm theo component SlimeAttack
            SlimeAttack[] slimeAttacks = FindObjectsOfType<SlimeAttack>();
            foreach (SlimeAttack slimeAttack in slimeAttacks)
            {
                if (slimeAttack != null)
                {
                    Debug.Log($"Destroying slime with SlimeAttack: {slimeAttack.gameObject.name}");
                    Destroy(slimeAttack.gameObject);
                }
            }
            
            // Cách 4: Tìm theo component SlimeRandomJump
            SlimeRandomJump[] slimeJumps = FindObjectsOfType<SlimeRandomJump>();
            foreach (SlimeRandomJump slimeJump in slimeJumps)
            {
                if (slimeJump != null)
                {
                    Debug.Log($"Destroying slime with SlimeRandomJump: {slimeJump.gameObject.name}");
                    Destroy(slimeJump.gameObject);
                }
            }
            
            Debug.Log("Đã destroy tất cả slimes sau khi hoàn thành nhiệm vụ!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Lỗi khi destroy slimes: {e.Message}");
        }
    }
    
        System.Collections.IEnumerator ClearCombatFlagDelayed()
        {
            // Đợi 2 giây để các script khác có thể kiểm tra flag
            yield return new WaitForSeconds(2f);
            
            // Clear flag và vị trí đã lưu
            PlayerPrefs.DeleteKey("JustFinishedCombat");
            PlayerPrefs.DeleteKey("SavedPlayerX");
            PlayerPrefs.DeleteKey("SavedPlayerY");
            PlayerPrefs.DeleteKey("SavedPlayerZ");
            PlayerPrefs.Save();
            
            Debug.Log("[nvnu1dituyen] Đã clear combat flag sau 2 giây!");
        }
    
    void MarkIntroAsWatched()
    {
        try
        {
            // Đánh dấu intro đã được xem
            PlayerPrefs.SetString("IntroWatched", "true");
            PlayerPrefs.Save();
            
            Debug.Log("Đã đánh dấu intro là đã xem - sẽ không phát lại nữa");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Lỗi khi mark intro watched: {e.Message}");
        }
    }
    
    void CheckAndRestorePosition()
    {
        string returnFlag = PlayerPrefs.GetString("ReturnFromCombat", "false");
        Debug.Log($"Player checking ReturnFromCombat flag: {returnFlag}");
        
        // CHỈ restore khi flag = "true" VÀ đang ở scene map1 VÀ có vị trí đã lưu
        if (returnFlag == "true" && 
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "map1" &&
            PlayerPrefs.HasKey("PlayerPosX"))
        {
            // Restore vị trí
            float x = PlayerPrefs.GetFloat("PlayerPosX", transform.position.x);
            float y = PlayerPrefs.GetFloat("PlayerPosY", transform.position.y);
            float z = PlayerPrefs.GetFloat("PlayerPosZ", transform.position.z);
            
            Vector3 savedPosition = new Vector3(x, y, z);
            
            // CHỈ restore nếu vị trí khác với vị trí hiện tại (tránh restore không cần thiết)
            if (Vector3.Distance(transform.position, savedPosition) > 0.1f)
            {
                transform.position = savedPosition;
                Debug.Log($"Player đã khôi phục vị trí từ {transform.position} về: {savedPosition}");
                
                // Tắt intro video NGAY LẬP TỨC
                StartCoroutine(DisableIntroVideoDelayed());
            }
            
            // Clear flag và dữ liệu đã lưu
            PlayerPrefs.SetString("ReturnFromCombat", "false");
            PlayerPrefs.DeleteKey("PlayerPosX");
            PlayerPrefs.DeleteKey("PlayerPosY");
            PlayerPrefs.DeleteKey("PlayerPosZ");
            PlayerPrefs.Save();
            
            Debug.Log("Đã clear flag và position data");
        }
    }
    
    System.Collections.IEnumerator DisableIntroVideoDelayed()
    {
        // Chờ một frame để tất cả objects được load
        yield return null;
        
        // Tắt intro video
        var introVideoController = FindObjectOfType<IntroVideoController>();
        if (introVideoController != null)
        {
            introVideoController.gameObject.SetActive(false);
            Debug.Log("Player đã tắt IntroVideoController");
        }
        
        var introVideoPlayer = FindObjectOfType<IntroVideoPlayer>();
        if (introVideoPlayer != null)
        {
            introVideoPlayer.gameObject.SetActive(false);
            Debug.Log("Player đã tắt IntroVideoPlayer");
        }
        
        // Tắt tất cả video đang chạy
        var allVideoPlayers = FindObjectsOfType<UnityEngine.Video.VideoPlayer>();
        foreach (var vp in allVideoPlayers)
        {
            if (vp.gameObject.name.ToLower().Contains("intro"))
            {
                vp.gameObject.SetActive(false);
                Debug.Log($"Đã tắt video: {vp.gameObject.name}");
            }
        }
    }

    private void TryInitializePython()
    {
        try
        {
            var pythonManager = PythonManager.Instance;
            
            if (!PythonManager.IsPythonInitialized())
            {
                Debug.LogWarning("Python không khả dụng - sử dụng C# fallback");
                usePython = false;
                return;
            }

            playerMovementModule = PythonManager.ImportModule("nvnu1dituyen_logic");
            if (playerMovementModule == null)
            {
                Debug.LogWarning("Không load được Python module - sử dụng C# fallback");
                usePython = false;
                return;
            }

            // Thử khởi tạo Python
            using (Py.GIL())
            {
                PyObject result = playerMovementModule.InvokeMethod("initialize_player_movement",
                    new PyObject[] {
                        new PyFloat(moveSpeed),
                        new PyFloat(villageEntryPosition.x),
                        new PyFloat(villageEntryPosition.y),
                        new PyFloat(villageEntryPosition.z),
                        new PyFloat(entryDistance)
                    });
                usePython = result.As<bool>();
                
                if (usePython)
                {
                    Debug.Log("✅ Sử dụng Python cho movement logic");
                }
                else
                {
                    Debug.LogWarning("Python init failed - sử dụng C# fallback");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Lỗi khởi tạo Python: {e.Message} - sử dụng C# fallback");
            usePython = false;
        }
    }

    void Update()
    {
        if (usePython && playerMovementModule != null)
        {
            UpdateWithPython();
        }
        else
        {
            UpdateWithCSharp(); // ✅ FALLBACK C#
        }
    }

    void UpdateWithPython()
    {
        try
        {
            using (Py.GIL())
            {
                // Kiểm tra nhiệm vụ vào làng
                CheckVillageEntry();

                // Chặn toàn bộ điều khiển khi đang đặt tên user
                if (UsernameWizard.IsUsernameDialogOpen)
                {
                    // Dừng hết di chuyển và animation khi đang đặt tên
                    if (animator != null)
                    {
                        animator.SetFloat("Horizontal", 0);
                        animator.SetFloat("Vertical", 0);
                        animator.SetFloat("Speed", 0);
                    }
                    return; // Không xử lý input gì cả
                }

                // Chặn toàn bộ điều khiển khi đang mở hội thoại
                PyObject dialogueResult = playerMovementModule.InvokeMethod("handle_player_dialogue_state",
                    new PyObject[] { new PyInt(DialogueManager.IsDialogueOpen ? 1 : 0) });
                
                bool shouldReturn = dialogueResult.As<bool>();
                if (shouldReturn)
                {
                    // Update animator with Python values
                    PyObject animParams = playerMovementModule.InvokeMethod("get_player_animation_parameters");
                    var animList = animParams.As<float[]>();
                    
                    if (animator != null)
                    {
                        animator.SetFloat("Horizontal", animList[0]);
                        animator.SetFloat("Vertical", animList[1]);
                        animator.SetFloat("Speed", animList[2]);
                    }
                    return;
                }

                // Đọc input từ Input System package (chỉ ghi nhận input, KHÔNG di chuyển trực tiếp ở đây)
                moveInput = Vector2.zero;
                isSprinting = false;

                var keyboard = Keyboard.current;
                bool aPressed = false, dPressed = false, wPressed = false, sPressed = false, shiftPressed = false;
                
                if (keyboard != null)
                {
                    aPressed = keyboard.aKey.isPressed;
                    dPressed = keyboard.dKey.isPressed;
                    wPressed = keyboard.wKey.isPressed;
                    sPressed = keyboard.sKey.isPressed;
                    shiftPressed = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
                }

                // Process input in Python
                playerMovementModule.InvokeMethod("process_player_input",
                    new PyObject[] {
                        new PyInt(aPressed ? 1 : 0),
                        new PyInt(dPressed ? 1 : 0), 
                        new PyInt(wPressed ? 1 : 0),
                        new PyInt(sPressed ? 1 : 0),
                        new PyInt(shiftPressed ? 1 : 0)
                    });

                // Bắt click chuột để đặt mục tiêu di chuyển, nhưng KHÔNG di chuyển ở Update - xử lý trong FixedUpdate bằng Rigidbody2D
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    // Bỏ qua click nếu đang trên UI
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }
                    Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                    mouseWorldPos.z = 0f;
                    
                    // Process mouse click in Python
                    playerMovementModule.InvokeMethod("process_mouse_click_target",
                        new PyObject[] {
                            new PyFloat(mouseWorldPos.x),
                            new PyFloat(mouseWorldPos.y),
                            new PyFloat(mouseWorldPos.z)
                        });
                }

                // Update animation states in Python
                playerMovementModule.InvokeMethod("update_player_animation_states",
                    new PyObject[] {
                        new PyFloat(transform.position.x),
                        new PyFloat(transform.position.y),
                        new PyFloat(transform.position.z)
                    });

                // Get animation parameters from Python and update animator
                PyObject animResult = playerMovementModule.InvokeMethod("get_player_animation_parameters");
                var animArray = animResult.As<float[]>();
                
                if (animator != null)
                {
                    animator.SetFloat("Horizontal", animArray[0]);
                    animator.SetFloat("Vertical", animArray[1]);
                    animator.SetFloat("Speed", animArray[2]);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi Python Update: {e.Message} - Chuyển sang C#");
            usePython = false;
        }
    }

    // ✅ C# FALLBACK LOGIC - CHẠY KHI PYTHON KHÔNG KHẢ DỤNG
    void UpdateWithCSharp()
    {
        CheckVillageEntryCSharp();

        // Chặn di chuyển khi đang đặt tên
        if (UsernameWizard.IsUsernameDialogOpen)
        {
            if (animator != null)
            {
                animator.SetFloat("Horizontal", 0);
                animator.SetFloat("Vertical", 0);
                animator.SetFloat("Speed", 0);
            }
            return;
        }

        // Chặn di chuyển khi dialogue mở
        if (DialogueManager.IsDialogueOpen)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (animator != null)
            {
                animator.SetFloat("Horizontal", 0);
                animator.SetFloat("Vertical", 0);
                animator.SetFloat("Speed", 0);
            }
            return;
        }

        // Đọc input
        moveInput = Vector2.zero;
        isSprinting = false;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed) moveInput.x -= 1;
            if (keyboard.dKey.isPressed) moveInput.x += 1;
            if (keyboard.wKey.isPressed) moveInput.y += 1;
            if (keyboard.sKey.isPressed) moveInput.y -= 1;
            isSprinting = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        }

        // Click chuột để đặt mục tiêu
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                mouseWorldPos.z = 0f;
                mouseTarget = mouseWorldPos;
            }
        }

        // Xử lý di chuyển đến mouse target
        if (mouseTarget.HasValue)
        {
            Vector3 direction = mouseTarget.Value - transform.position;
            float distance = direction.magnitude;
            
            if (distance > 0.1f)
            {
                moveInput = new Vector2(direction.x, direction.y).normalized;
            }
            else
            {
                mouseTarget = null;
                moveInput = Vector2.zero;
            }
        }

        // Tính toán movement
        movement = moveInput.normalized;
        
        // Cập nhật animation
        if (movement.sqrMagnitude > 0.01f)
        {
            horizontal = movement.x;
            vertical = movement.y;
            speed = movement.magnitude;
            lastMoveDirection = movement;
        }
        else
        {
            speed = 0;
        }

        if (animator != null)
        {
            animator.SetFloat("Horizontal", horizontal);
            animator.SetFloat("Vertical", vertical);
            animator.SetFloat("Speed", speed);
        }
    }

    void FixedUpdate()
    {
        if (usePython && playerMovementModule != null)
        {
            FixedUpdateWithPython();
        }
        else
        {
            FixedUpdateWithCSharp(); // ✅ FALLBACK C#
        }
    }

    void FixedUpdateWithPython()
    {
        if (rb == null) return;

        // Chặn di chuyển vật lý khi đang đặt tên
        if (UsernameWizard.IsUsernameDialogOpen)
        {
            rb.linearVelocity = Vector2.zero; // Dừng hết di chuyển
            return;
        }

        try
        {
            using (Py.GIL())
            {
                // Calculate movement direction and speed using Python
                PyObject movementResult = playerMovementModule.InvokeMethod("calculate_player_movement_direction",
                    new PyObject[] {
                        new PyFloat(transform.position.x),
                        new PyFloat(transform.position.y),
                        new PyFloat(transform.position.z),
                        new PyFloat(Time.fixedDeltaTime)
                    });

                var movementArray = movementResult.As<float[]>();
                Vector2 moveDir = new Vector2(movementArray[0], movementArray[1]);
                float currentSpeed = movementArray[2];
                float distance = movementArray[3];

                if (moveDir.sqrMagnitude > 0.0001f)
                {
                    // Kiểm tra va chạm theo hướng di chuyển trước khi di chuyển
                    RaycastHit2D[] hits = new RaycastHit2D[6];
                    int hitCount = rb.Cast(moveDir, hits, distance + 0.01f);
                    bool blocked = false;
                    for (int i = 0; i < hitCount; i++)
                    {
                        var h = hits[i];
                        if (h.collider == null) continue;
                        if (h.collider.isTrigger) continue; // bỏ qua trigger
                        if (col != null && h.collider == col) continue; // bỏ qua chính mình

                        // Nếu va chạm với collider khác (ví dụ NPC Mụ Thảo), coi như bị chặn
                        blocked = true;
                        break;
                    }

                    if (!blocked)
                    {
                        rb.MovePosition(rb.position + moveDir * distance);
                    }
                    else
                    {
                        // Handle collision in Python
                        playerMovementModule.InvokeMethod("handle_player_collision");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi Python FixedUpdate: {e.Message} - Chuyển sang C#");
            usePython = false;
        }
    }

    // ✅ C# FALLBACK PHYSICS
    void FixedUpdateWithCSharp()
    {
        if (rb == null) return;

        if (UsernameWizard.IsUsernameDialogOpen || DialogueManager.IsDialogueOpen)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (movement.sqrMagnitude > 0.01f)
        {
            float currentSpeed = isSprinting ? moveSpeed * 1.5f : moveSpeed;
            Vector2 targetPosition = rb.position + movement * currentSpeed * Time.fixedDeltaTime;
            
            // Kiểm tra va chạm
            RaycastHit2D[] hits = new RaycastHit2D[6];
            int hitCount = rb.Cast(movement, hits, currentSpeed * Time.fixedDeltaTime + 0.01f);
            bool blocked = false;
            
            for (int i = 0; i < hitCount; i++)
            {
                var h = hits[i];
                if (h.collider == null) continue;
                if (h.collider.isTrigger) continue;
                if (col != null && h.collider == col) continue;
                
                blocked = true;
                break;
            }

            if (!blocked)
            {
                rb.MovePosition(targetPosition);
            }
            else
            {
                // Dừng lại khi va chạm
                mouseTarget = null;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void CheckVillageEntry()
    {
        if (hasEnteredVillage || !usePython || playerMovementModule == null) return;

        // CHỈ check khi đã hoàn thành 2 nhiệm vụ trước
        if (!ShouldCheckVillageEntry()) return;

        try
        {
            using (Py.GIL())
            {
                PyObject result;
                
                // Ưu tiên sử dụng villageEntryTarget nếu có, nếu không thì dùng villageEntryPosition
                if (villageEntryTarget != null)
                {
                    result = playerMovementModule.InvokeMethod("check_player_village_entry",
                        new PyObject[] {
                            new PyFloat(transform.position.x),
                            new PyFloat(transform.position.y),
                            new PyFloat(transform.position.z),
                            new PyFloat(villageEntryTarget.position.x),
                            new PyFloat(villageEntryTarget.position.y),
                            new PyFloat(villageEntryTarget.position.z)
                        });
                }
                else
                {
                    result = playerMovementModule.InvokeMethod("check_player_village_entry",
                        new PyObject[] {
                            new PyFloat(transform.position.x),
                            new PyFloat(transform.position.y),
                            new PyFloat(transform.position.z)
                        });
                }
                
                bool enteredVillage = result.As<bool>();
                if (enteredVillage)
                {
                    hasEnteredVillage = true;
                    
                    // Dừng di chuyển ngay lập tức
                    StopMovement();
                    
                    // Hoàn thành nhiệm vụ "Tìm đường vào làng"
                    QuestManager.CompleteCurrentQuest("Tìm đường vào làng");
                    
                    Debug.Log("Đã đến lối vào làng! Hoàn thành nhiệm vụ.");
                    
                    // Thông báo đã được xử lý trong QuestManager.CompleteQuest()
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in CheckVillageEntry: {e.Message}");
        }
    }

    // ✅ C# VERSION
    void CheckVillageEntryCSharp()
    {
        if (hasEnteredVillage) return;
        if (!ShouldCheckVillageEntry()) return;

        Vector3 targetPos = villageEntryTarget != null ? villageEntryTarget.position : villageEntryPosition;
        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance <= entryDistance)
        {
            hasEnteredVillage = true;
            StopMovementCSharp();
            QuestManager.CompleteCurrentQuest("Tìm đường vào làng");
            Debug.Log("Đã đến lối vào làng! Hoàn thành nhiệm vụ.");
        }
    }
    
    bool ShouldCheckVillageEntry()
    {
        // Kiểm tra xem có cần check village entry không
        if (QuestManager.Instance == null) return false;
        
        // Lấy quest hiện tại
        Quest currentQuest = QuestManager.Instance.GetCurrentQuest();
        if (currentQuest == null) return false;
        
        // CHỈ check khi quest hiện tại là "Tìm đường vào làng"
        if (currentQuest.title != "Tìm đường vào làng") 
        {
            // Debug.Log($"Quest hiện tại: '{currentQuest.title}' - Chưa đến lúc tìm đường vào làng");
            return false;
        }
        
        // Kiểm tra 2 quest trước đã hoàn thành chưa
        int completedQuests = 0;
        var questList = QuestManager.Instance.questList;
        
        // Check quest 1: "Gặp Mụ Thảo"
        if (questList.Count > 0 && questList[0].isCompleted)
            completedQuests++;
            
        // Check quest 2: "Những Hạt Mầm Đầu Tiên"  
        if (questList.Count > 1 && questList[1].isCompleted)
            completedQuests++;
        
        if (completedQuests < 2)
        {
            // Debug.Log($"Chỉ hoàn thành {completedQuests}/2 nhiệm vụ trước. Chưa thể tìm đường vào làng.");
            return false;
        }
        
        // Debug.Log("Đã hoàn thành 2 nhiệm vụ trước. Có thể tìm đường vào làng!");
        return true;
    }

    void StopMovement()
    {
        if (usePython && playerMovementModule != null)
        {
            try
            {
                using (Py.GIL())
                {
                    playerMovementModule.InvokeMethod("stop_player_movement");
                    
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                    }
                    
                    PyObject animResult = playerMovementModule.InvokeMethod("get_player_animation_parameters");
                    var animArray = animResult.As<float[]>();
                    
                    if (animator != null)
                    {
                        animator.SetFloat("Horizontal", animArray[0]);
                        animator.SetFloat("Vertical", animArray[1]);
                        animator.SetFloat("Speed", animArray[2]);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in StopMovement: {e.Message}");
            }
        }
        else
        {
            StopMovementCSharp();
        }
    }

    void StopMovementCSharp()
    {
        mouseTarget = null;
        moveInput = Vector2.zero;
        movement = Vector2.zero;
        speed = 0;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        if (animator != null)
        {
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
            animator.SetFloat("Speed", 0);
        }
    }
}
