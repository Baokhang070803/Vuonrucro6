using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "caidat";
    
    [Header("UI References")]
    [SerializeField] private Button caidatButton;
    [SerializeField] private Button backToMap1Button;
    [SerializeField] private Button quitGameButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button reportButton;
    
    [Header("Report Panel")]
    [SerializeField] private GameObject panelReport;
    [SerializeField] private Button btnCloseReport;
    [SerializeField] private Button btnSendReport;
    
    [Header("Notification")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMPro.TMP_Text notificationText;
    [SerializeField] private Button notificationCloseButton;
    
    [Header("Firebase")]
    private DatabaseReference databaseReference;
    
    private void Start()
    {
        // Tự động gắn button caidat vào sự kiện OnClick nếu button được gán
        if (caidatButton != null)
        {
            caidatButton.onClick.AddListener(GoToCaidatScene);
            Debug.Log("Đã gắn button caidat vào sự kiện chuyển scene");
        }
        else
        {
            Debug.LogWarning("Button caidat chưa được gán! Vui lòng kéo button vào ô 'Caidat Button'");
        }
        
        // Tự động gắn button quay về map1 vào sự kiện OnClick nếu button được gán
        if (backToMap1Button != null)
        {
            backToMap1Button.onClick.AddListener(GoBackToMap1);
            Debug.Log("Đã gắn button quay về map1 vào sự kiện");
        }
        else
        {
            Debug.LogWarning("Button quay về map1 chưa được gán! Vui lòng kéo button vào ô 'Back To Map1 Button'");
        }
        
        // Tự động gắn button thoát game vào sự kiện OnClick nếu button được gán
        if (quitGameButton != null)
        {
            quitGameButton.onClick.AddListener(QuitGame);
            Debug.Log("Đã gắn button thoát game vào sự kiện");
        }
        else
        {
            Debug.LogWarning("Button thoát game chưa được gán! Vui lòng kéo button vào ô 'Quit Game Button'");
        }
        
        // Tự động gắn button đăng xuất vào sự kiện OnClick nếu button được gán
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(GoToLoginScene);
            Debug.Log("Đã gắn button đăng xuất vào sự kiện");
        }
        else
        {
            Debug.LogWarning("Button đăng xuất chưa được gán! Vui lòng kéo button vào ô 'Logout Button'");
        }
        
        // Tự động gắn button report vào sự kiện OnClick nếu button được gán
        if (reportButton != null)
        {
            reportButton.onClick.AddListener(ShowReportPanel);
            Debug.Log("Đã gắn button report vào sự kiện");
        }
        else
        {
            Debug.LogWarning("Button report chưa được gán! Vui lòng kéo button vào ô 'Report Button'");
        }
        
        // Tự động gắn button đóng report panel
        if (btnCloseReport != null)
        {
            btnCloseReport.onClick.AddListener(HideReportPanel);
            Debug.Log("Đã gắn button đóng report panel");
        }
        else
        {
            Debug.LogWarning("Button đóng report panel chưa được gán!");
        }
        
        // Tự động gắn button gửi report
        if (btnSendReport != null)
        {
            btnSendReport.onClick.AddListener(SendReport);
            Debug.Log("Đã gắn button gửi report");
        }
        else
        {
            Debug.LogWarning("Button gửi report chưa được gán!");
        }
        
        // Kiểm tra scene hiện tại và thiết lập phù hợp
        CheckCurrentSceneAndSetup();
        
        // Ẩn panel report ban đầu
        if (panelReport != null)
        {
            panelReport.SetActive(false);
        }
        
        
        // Khởi tạo Firebase Database
        InitializeFirebase();
        
        // Setup notification panel
        SetupNotificationPanel();
    }
    
    /// <summary>
    /// Chuyển từ scene hiện tại sang scene caidat
    /// </summary>
    public void GoToCaidatScene()
    {
        // Kiểm tra xem scene caidat có tồn tại không
        if (IsSceneInBuildSettings(targetSceneName))
        {
            Debug.Log($"Chuyển từ scene {SceneManager.GetActiveScene().name} sang scene {targetSceneName}");
            // Sử dụng LoadSceneMode.Additive để không hủy scene hiện tại
            SceneManager.LoadScene(targetSceneName, LoadSceneMode.Additive);
            
            // Đảm bảo scene caidat được set làm active scene để hiển thị đúng
            StartCoroutine(SetActiveSceneAfterLoad(targetSceneName));
        }
        else
        {
            Debug.LogError($"Scene '{targetSceneName}' không tồn tại trong Build Settings!");
        }
    }
    
    /// <summary>
    /// Chuyển sang scene với tên cụ thể
    /// </summary>
    /// <param name="sceneName">Tên scene cần chuyển đến</param>
    public void GoToScene(string sceneName)
    {
        if (IsSceneInBuildSettings(sceneName))
        {
            Debug.Log($"Chuyển từ scene {SceneManager.GetActiveScene().name} sang scene {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' không tồn tại trong Build Settings!");
        }
    }
    
    /// <summary>
    /// Kiểm tra xem scene có tồn tại trong Build Settings không
    /// </summary>
    /// <param name="sceneName">Tên scene cần kiểm tra</param>
    /// <returns>True nếu scene tồn tại</returns>
    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Chuyển về scene map1 (unload scene caidat)
    /// </summary>
    public void GoBackToMap1()
    {
        // Unload scene caidat để quay về scene map1
        Scene caidatScene = SceneManager.GetSceneByName("caidat");
        if (caidatScene.IsValid())
        {
            Debug.Log("Đóng scene caidat và quay về map1");
            SceneManager.UnloadSceneAsync(caidatScene);
            
            // Set scene map1 làm active scene
            Scene map1Scene = SceneManager.GetSceneByName("map1");
            if (map1Scene.IsValid())
            {
                SceneManager.SetActiveScene(map1Scene);
                Debug.Log("Đã set map1 làm active scene");
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy scene caidat để đóng");
        }
    }
    
    /// <summary>
    /// Coroutine để set active scene sau khi load xong
    /// </summary>
    /// <param name="sceneName">Tên scene cần set active</param>
    /// <returns></returns>
    private IEnumerator SetActiveSceneAfterLoad(string sceneName)
    {
        // Đợi một frame để đảm bảo scene đã load xong
        yield return null;
        
        Scene targetScene = SceneManager.GetSceneByName(sceneName);
        if (targetScene.IsValid())
        {
            SceneManager.SetActiveScene(targetScene);
            Debug.Log($"Đã set {sceneName} làm active scene");
            
            // Đảm bảo Camera của scene caidat được bật
            SetCameraActive(targetScene);
        }
    }
    
    /// <summary>
    /// Đảm bảo Camera của scene được bật
    /// </summary>
    /// <param name="scene">Scene cần bật Camera</param>
    private void SetCameraActive(Scene scene)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            Camera camera = obj.GetComponent<Camera>();
            if (camera != null)
            {
                camera.enabled = true;
                Debug.Log($"Đã bật Camera trong scene {scene.name}");
                break;
            }
            
            // Tìm Camera trong children
            Camera[] cameras = obj.GetComponentsInChildren<Camera>();
            foreach (Camera cam in cameras)
            {
                cam.enabled = true;
                Debug.Log($"Đã bật Camera trong scene {scene.name}");
                break;
            }
        }
    }
    
    /// <summary>
    /// Chuyển về scene Login (đăng xuất)
    /// </summary>
    public void GoToLoginScene()
    {
        GoToScene("Login");
    }
    
    /// <summary>
    /// Thoát game
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Thoát game...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Gắn button caidat vào sự kiện chuyển scene
    /// </summary>
    /// <param name="button">Button cần gắn</param>
    public void SetCaidatButton(Button button)
    {
        caidatButton = button;
        if (caidatButton != null)
        {
            caidatButton.onClick.AddListener(GoToCaidatScene);
            Debug.Log("Đã gắn button caidat vào sự kiện chuyển scene");
        }
    }
    
    /// <summary>
    /// Gắn button quay về map1 vào sự kiện
    /// </summary>
    /// <param name="button">Button cần gắn</param>
    public void SetBackToMap1Button(Button button)
    {
        backToMap1Button = button;
        if (backToMap1Button != null)
        {
            backToMap1Button.onClick.AddListener(GoBackToMap1);
            Debug.Log("Đã gắn button quay về map1 vào sự kiện");
        }
    }
    
    /// <summary>
    /// Gắn button thoát game vào sự kiện
    /// </summary>
    /// <param name="button">Button cần gắn</param>
    public void SetQuitGameButton(Button button)
    {
        quitGameButton = button;
        if (quitGameButton != null)
        {
            quitGameButton.onClick.AddListener(QuitGame);
            Debug.Log("Đã gắn button thoát game vào sự kiện");
        }
    }
    
    /// <summary>
    /// Gắn button đăng xuất vào sự kiện
    /// </summary>
    /// <param name="button">Button cần gắn</param>
    public void SetLogoutButton(Button button)
    {
        logoutButton = button;
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(GoToLoginScene);
            Debug.Log("Đã gắn button đăng xuất vào sự kiện");
        }
    }
    
    /// <summary>
    /// Kiểm tra scene hiện tại và thực hiện hành động phù hợp
    /// </summary>
    private void CheckCurrentSceneAndSetup()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"Scene hiện tại: {currentSceneName}");
        
        // Nếu đang ở scene caidat, chỉ hiển thị button quay về map1
        if (currentSceneName == "caidat")
        {
            Debug.Log("Đang ở scene caidat - chỉ hiển thị button quay về map1");
        }
        // Nếu đang ở scene map1, chỉ hiển thị button chuyển sang caidat
        else if (currentSceneName == "map1")
        {
            Debug.Log("Đang ở scene map1 - chỉ hiển thị button chuyển sang caidat");
        }
    }
    
    /// <summary>
    /// Hiển thị panel report
    /// </summary>
    public void ShowReportPanel()
    {
        if (panelReport != null)
        {
            panelReport.SetActive(true);
            Debug.Log("Đã hiển thị panel report");
        }
        else
        {
            Debug.LogWarning("Panel report chưa được gán!");
        }
    }
    
    /// <summary>
    /// Ẩn panel report
    /// </summary>
    public void HideReportPanel()
    {
        if (panelReport != null)
        {
            panelReport.SetActive(false);
            Debug.Log("Đã ẩn panel report");
        }
        else
        {
            Debug.LogWarning("Panel report chưa được gán!");
        }
    }
    
    /// <summary>
    /// Gửi report (có thể gửi lên server hoặc lưu local)
    /// </summary>
    public void SendReport()
    {
        Debug.Log("Đang gửi report...");
        
        if (panelReport != null)
        {
            // Tìm component nhập liệu trong panel report (không cần Scroll View)
            string reportContent = "";
            TMPro.TMP_InputField tmpInputField = null;
            InputField inputField = null;
            Text textComponent = null;
            
            // Thử tìm TMP_InputField trước (có thể ở bất kỳ đâu trong panel)
            tmpInputField = panelReport.GetComponentInChildren<TMPro.TMP_InputField>();
            if (tmpInputField != null)
            {
                reportContent = tmpInputField.text;
            }
            // Nếu không có TMP_InputField, thử tìm InputField thường
            else
            {
                inputField = panelReport.GetComponentInChildren<InputField>();
                if (inputField != null)
                {
                    reportContent = inputField.text;
                }
                // Nếu không có InputField, thử tìm Text component
                else
                {
                    textComponent = panelReport.GetComponentInChildren<Text>();
                    if (textComponent != null)
                    {
                        reportContent = textComponent.text;
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(reportContent))
            {
                // Gửi report lên Firebase Database
                SendReportToFirebase(reportContent);
            }
            else
            {
                Debug.LogWarning("Nội dung report trống! Vui lòng nhập nội dung trước khi gửi.");
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy component nhập liệu nào trong panel report!");
        }
    }
    
    /// <summary>
    /// Gắn button report vào sự kiện
    /// </summary>
    /// <param name="button">Button cần gắn</param>
    public void SetReportButton(Button button)
    {
        reportButton = button;
        if (reportButton != null)
        {
            reportButton.onClick.AddListener(ShowReportPanel);
            Debug.Log("Đã gắn button report vào sự kiện");
        }
    }
    
    
    /// <summary>
    /// Khởi tạo Firebase Database
    /// </summary>
    private void InitializeFirebase()
    {
        try
        {
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            Debug.Log("Đã khởi tạo Firebase Database thành công");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Lỗi khởi tạo Firebase: {e.Message}");
        }
    }
    
    /// <summary>
    /// Gửi report lên Firebase Database
    /// </summary>
    /// <param name="reportContent">Nội dung report</param>
    private void SendReportToFirebase(string reportContent)
    {
        if (databaseReference == null)
        {
            Debug.LogError("Firebase Database chưa được khởi tạo!");
            return;
        }
        
        // Lấy thông tin user hiện tại
        FirebaseUser currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        string userId = currentUser != null ? currentUser.UserId : "anonymous";
        string userEmail = currentUser != null ? currentUser.Email : "anonymous@example.com";
        
        // Tạo cấu trúc dữ liệu report (sử dụng Dictionary thay vì anonymous object)
        var reportData = new System.Collections.Generic.Dictionary<string, object>
        {
            {"content", reportContent},
            {"userId", userId},
            {"userEmail", userEmail},
            {"timestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
            {"status", "pending"} // pending, reviewed, resolved
        };
        
        // Tạo key duy nhất cho report
        string reportKey = databaseReference.Child("Reports").Push().Key;
        
        // Gửi lên Firebase
        databaseReference.Child("Reports").Child(reportKey).SetValueAsync(reportData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"Đã gửi report thành công lên Firebase! Key: {reportKey}");
                
                // Xóa nội dung sau khi gửi thành công
                ClearInputField();
                
                // Ẩn panel sau khi gửi
                HideReportPanel();
                
                // Hiển thị thông báo thành công
                ShowReportSuccessMessage();
            }
            else
            {
                Debug.LogError($"Lỗi khi gửi report lên Firebase: {task.Exception}");
                ShowReportErrorMessage("Lỗi khi gửi report! Vui lòng thử lại.");
            }
        });
    }
    
    /// <summary>
    /// Xóa nội dung InputField sau khi gửi thành công
    /// </summary>
    private void ClearInputField()
    {
        if (panelReport != null)
        {
            TMPro.TMP_InputField tmpInputField = panelReport.GetComponentInChildren<TMPro.TMP_InputField>();
            InputField inputField = panelReport.GetComponentInChildren<InputField>();
            Text textComponent = panelReport.GetComponentInChildren<Text>();
            
            if (tmpInputField != null)
                tmpInputField.text = "";
            else if (inputField != null)
                inputField.text = "";
            else if (textComponent != null)
                textComponent.text = "";
        }
    }
    
    /// <summary>
    /// Hiển thị thông báo thành công
    /// </summary>
    private void ShowReportSuccessMessage()
    {
        Debug.Log("✅ Report đã được gửi thành công!");
        
        // Hiển thị UI thông báo
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = "✅ Nội dung đã được gửi thành công!\nCảm ơn bạn đã phản hồi.";
            notificationPanel.SetActive(true);
            Debug.Log("✅ Đã hiển thị notification panel!");
            
            // Tự động ẩn sau 3 giây
            StartCoroutine(HideNotificationAfterDelay(3f));
        }
        else
        {
            Debug.LogWarning("❌ Notification Panel chưa được setup! Vui lòng kéo UI vào script.");
        }
    }
    
    /// <summary>
    /// Hiển thị thông báo lỗi
    /// </summary>
    /// <param name="message">Thông báo lỗi</param>
    private void ShowReportErrorMessage(string message)
    {
        Debug.LogError($"❌ {message}");
        
        // Hiển thị UI thông báo lỗi
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = $"❌ {message}";
            notificationPanel.SetActive(true);
            
            // Tự động ẩn sau 5 giây
            StartCoroutine(HideNotificationAfterDelay(5f));
        }
    }
    
    /// <summary>
    /// Setup notification panel
    /// </summary>
    private void SetupNotificationPanel()
    {
        // Ẩn notification panel ban đầu
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
        
        // Gắn button đóng notification
        if (notificationCloseButton != null)
        {
            notificationCloseButton.onClick.AddListener(HideNotification);
            Debug.Log("Đã gắn button đóng notification");
        }
    }
    
    /// <summary>
    /// Hiển thị notification
    /// </summary>
    /// <param name="message">Thông báo</param>
    public void ShowNotification(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true);
            Debug.Log($"Hiển thị notification: {message}");
        }
    }
    
    /// <summary>
    /// Ẩn notification
    /// </summary>
    public void HideNotification()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
            Debug.Log("Ẩn notification");
        }
    }
    
    /// <summary>
    /// Ẩn notification sau một khoảng thời gian
    /// </summary>
    /// <param name="delay">Thời gian delay (giây)</param>
    /// <returns></returns>
    private System.Collections.IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideNotification();
    }
}
