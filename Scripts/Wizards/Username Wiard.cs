using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using Newtonsoft.Json;
using UnityEngine.UI;  // cần thiết để dùng InputField và Button

public class UsernameWizard : MonoBehaviour
{
    public static bool IsUsernameDialogOpen { get; private set; } = false; // Trạng thái popup đặt tên
    
    public Text username;
    public Text gold;
    public Text diamond;

    public GameObject usernameWizard;
    public InputField ipUsername;   // sửa đúng tên InputField
    public Button buttonOK;

    private FirebaseDatabaseManager databaseManager;

    void Start()
    {
        databaseManager = GameObject.Find("Database Manager").GetComponent<FirebaseDatabaseManager>();

        // Kiểm tra LoadDataManager.userInGame có tồn tại không
        if (LoadDataManager.userInGame == null)
        {
            Debug.LogWarning("[UsernameWizard] LoadDataManager.userInGame is null! Chờ load data...");
            StartCoroutine(WaitForUserData());
            return;
        }

        if (LoadDataManager.userInGame.Name == "")
        {
            usernameWizard.SetActive(true);
            IsUsernameDialogOpen = true; // Đặt trạng thái popup mở
        }
        else
        {
            username.text = LoadDataManager.userInGame.Name;
            IsUsernameDialogOpen = false; // Đặt trạng thái popup đóng
        }

        gold.text = " " + LoadDataManager.userInGame.Gold.ToString();
        diamond.text = " " + LoadDataManager.userInGame.Diamond.ToString();

        buttonOK.onClick.AddListener(SetNewUsername);
    }
    
    /// <summary>
    /// Chờ LoadDataManager.userInGame được khởi tạo
    /// </summary>
    System.Collections.IEnumerator WaitForUserData()
    {
        while (LoadDataManager.userInGame == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("[UsernameWizard] LoadDataManager.userInGame đã sẵn sàng!");
        
        // Khởi tạo lại UI
        InitializeUI();
    }
    
    /// <summary>
    /// Khởi tạo UI sau khi có dữ liệu
    /// </summary>
    void InitializeUI()
    {
        if (LoadDataManager.userInGame.Name == "")
        {
            usernameWizard.SetActive(true);
            IsUsernameDialogOpen = true;
        }
        else
        {
            username.text = LoadDataManager.userInGame.Name;
            IsUsernameDialogOpen = false;
        }

        gold.text = " " + LoadDataManager.userInGame.Gold.ToString();
        diamond.text = " " + LoadDataManager.userInGame.Diamond.ToString();

        buttonOK.onClick.AddListener(SetNewUsername);
    }

    void Update()
    {
    }

    public void SetNewUsername()
    {
        if (ipUsername.text != "")
        {
            LoadDataManager.userInGame.Name = ipUsername.text;

            // Sử dụng PlayerDataSyncManager thay vì ghi đè trực tiếp
            if (PlayerDataSyncManager.Instance != null)
            {
                PlayerDataSyncManager.Instance.UpdateName(ipUsername.text);
                Debug.Log("[UsernameWizard] Đã cập nhật tên qua PlayerDataSyncManager");
            }
            else
            {
                Debug.LogWarning("[UsernameWizard] PlayerDataSyncManager.Instance is null!");
            }

            username.text = ipUsername.text;
            usernameWizard.SetActive(false);
            IsUsernameDialogOpen = false; // Đặt trạng thái popup đóng
        }
    }
}
