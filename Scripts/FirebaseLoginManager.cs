using UnityEngine;
using System.Collections;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.UI;
using TMPro; // ✅ THÊM - Hỗ trợ TextMeshPro
using System.Collections.Generic;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Firebase;
using Newtonsoft.Json;


public class FirebaseLoginManager : MonoBehaviour
{
    //dang ky
    [Header("Dang ky tai khoan")]
    public InputField ipRegisterEmail;
    public InputField ipRegisterPassword;
    public InputField ipRegisterConfirmPassword; // Nhập lại mật khẩu
   
    public Button buttonRegister;
    
    [Header("Text Error (Chọn 1 trong 2)")]
    public Text txtRegisterError; // Legacy Text hiển thị lỗi đăng ký
    public TMP_Text tmpRegisterError; // TextMeshPro hiển thị lỗi đăng ký ✨
    
    //dang nhap
    [Header("Dang nhap tai khoan")]
    public InputField ipLoginEmail;
    public InputField ipLoginPassword;  

    public Button buttonLogin;
    
    [Header("Text Error (Chọn 1 trong 2)")]
    public Text txtLoginError; // Legacy Text hiển thị lỗi đăng nhập
    public TMP_Text tmpLoginError; // TextMeshPro hiển thị lỗi đăng nhập ✨
    
    [Header("⏳ LOADING ANIMATION")]
    public GameObject loadingAnimationRegister;
    public GameObject loadingAnimationLogin;
    
    // Dang ky dang nhap Auth

    private FirebaseAuth auth;

    //chuyen doi dang ky dang nhap
    [Header("Chuyen doi dang ky dang nhap")]
    public Button buttonMoveToSignIn;
    public Button buttonMoveToRegister;

    public GameObject loginForm;
    public GameObject registerForm;

    //Google Sign-In
    [Header("Google Sign-In")]
    public Button buttonGoogleSignIn; // Nút đăng nhập Google
    
    private FirebaseDatabaseManager databaseManager;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        
        // ✅ ẨN TẤT CẢ LOADING ANIMATION KHI KHỞI TẠO
        HideLoadingRegister();
        HideLoadingLogin();

        buttonRegister.onClick.AddListener(RegisterAccountWithFirebase);
        buttonLogin.onClick.AddListener(SignInAccountWithFirebase);
        
        // Thêm listener cho Google Sign-In
        if (buttonGoogleSignIn != null)
            buttonGoogleSignIn.onClick.AddListener(SignInWithGoogle);

        buttonMoveToSignIn.onClick.AddListener(SwitchForm);
        buttonMoveToRegister.onClick.AddListener(SwitchForm);
        
        databaseManager = GetComponent<FirebaseDatabaseManager>();
    }
    

    public void RegisterAccountWithFirebase()
    {
        string email = ipRegisterEmail.text.Trim();
        string password = ipRegisterPassword.text;
        string confirmPassword = ipRegisterConfirmPassword != null ? ipRegisterConfirmPassword.text : password;

        // ===== VALIDATION =====
        
        // 1. Kiểm tra rỗng
        if (string.IsNullOrEmpty(email))
        {
            ShowRegisterError("Vui lòng nhập email!");
            return;
        }
        
        if (string.IsNullOrEmpty(password))
        {
            ShowRegisterError("Vui lòng nhập mật khẩu!");
            return;
        }
        
        // 2. Kiểm tra định dạng email
        if (!IsValidEmail(email))
        {
            ShowRegisterError("Email không hợp lệ!\nVí dụ: user@example.com");
            return;
        }
        
        // 3. Kiểm tra độ dài mật khẩu (Firebase yêu cầu tối thiểu 6 ký tự)
        if (password.Length < 6)
        {
            ShowRegisterError("Mật khẩu quá ngắn!\nTối thiểu 6 ký tự");
            return;
        }
        
        // 4. Kiểm tra mật khẩu xác nhận
        if (ipRegisterConfirmPassword != null && password != confirmPassword)
        {
            ShowRegisterError("Mật khẩu xác nhận không khớp!");
            return;
        }
        
        // 5. Kiểm tra độ mạnh mật khẩu (khuyến nghị)
        if (!IsStrongPassword(password))
        {
            ShowRegisterError("Mật khẩu yếu!\nNên có: chữ hoa, chữ thường, số");
            // Không return, chỉ cảnh báo
        }
        
        // ===== ĐĂNG KÝ VỚI FIREBASE =====
        ClearRegisterError(); // Xóa lỗi cũ
        ShowRegisterError("Đang đăng ký...");
        
        // ✅ VÔ HIỆU HÓA NÚT ĐỂ TRÁNH SPAM
        SetRegisterButtonInteractable(false);
        
        // ✅ HIỂN THỊ LOADING ANIMATION
        ShowLoadingRegister();
        
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => 
        {
            if (task.IsCanceled)
            {
                ShowRegisterError("Đăng ký bị hủy!");
                Debug.Log("Dang ky bi huy.");
                SetRegisterButtonInteractable(true); // ✅ BẬT LẠI NÚT
                HideLoadingRegister(); // ✅ ẨN LOADING ANIMATION
                return;
            }

            if (task.IsFaulted)
            {
                // Parse Firebase error
                string errorMessage = ParseFirebaseError(task.Exception);
                ShowRegisterError(errorMessage);
                Debug.Log("Dang ky that bai: " + task.Exception);
                SetRegisterButtonInteractable(true); // ✅ BẬT LẠI NÚT
                HideLoadingRegister(); // ✅ ẨN LOADING ANIMATION
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Dang ky thanh cong!");

                FirebaseUser firebaseUser = task.Result.User;
                Debug.Log("Tai khoan moi duoc tao: " + firebaseUser.UserId + " Email: " + firebaseUser.Email);
                
                // Tạo cấu trúc mới ngay từ khi đăng ký (với email)
                CreateNewUserStructure(firebaseUser.UserId, firebaseUser.Email);

                // Hiển thị thông báo thành công
                ShowRegisterError("Đăng ký thành công! Đang chuyển...");
                
                // ✅ KHÔNG BẬT LẠI NÚT - Vì sắp chuyển scene rồi
                // ✅ KHÔNG ẨN LOADING - Để hiển thị trong lúc chuyển scene
                
                // chuyen man hinh khi dang ky thanh cong
                LoadingManager.NEXT_SCENE = "intromodau";
                SceneManager.LoadScene("Loading");
            }
        });
    }
    
    /// <summary>
    /// Tạo cấu trúc mới cho user mới đăng ký
    /// </summary>
    void CreateNewUserStructure(string userId, string email = "")
    {
        Debug.Log("[FirebaseLoginManager] Tạo cấu trúc mới cho user: " + userId + " Email: " + email);
        
        // SỬ DỤNG HELPER MỚI ĐỂ ĐẢM BẢO TÍNH NHẤT QUÁN
        // Name để trống cho người dùng nhập sau, Email riêng biệt
        FirebaseDataConsistencyHelper.CreateConsistentUserStructure(userId, "", email);
        
        Debug.Log("[FirebaseLoginManager] Đã tạo cấu trúc mới với FirebaseDataConsistencyHelper!");
    }

    public void SignInAccountWithFirebase()
    {
        string email = ipLoginEmail.text.Trim();
        string password = ipLoginPassword.text;

        // ===== VALIDATION =====
        
        // 1. Kiểm tra rỗng
        if (string.IsNullOrEmpty(email))
        {
            ShowLoginError("Vui lòng nhập email!");
            return;
        }
        
        if (string.IsNullOrEmpty(password))
        {
            ShowLoginError("Vui lòng nhập mật khẩu!");
            return;
        }
        
        // 2. Kiểm tra định dạng email
        if (!IsValidEmail(email))
        {
            ShowLoginError("Email không hợp lệ!");
            return;
        }
        
        // ===== ĐĂNG NHẬP VỚI FIREBASE =====
        ClearLoginError(); // Xóa lỗi cũ
        ShowLoginError("Đang đăng nhập...");

        // ✅ VÔ HIỆU HÓA NÚT ĐỂ TRÁNH SPAM
        SetLoginButtonInteractable(false);
        
        // ✅ HIỂN THỊ LOADING ANIMATION
        ShowLoadingLogin();

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                ShowLoginError("Đăng nhập bị hủy!");
                Debug.Log("Dang nhap bi huy.");
                SetLoginButtonInteractable(true); // ✅ BẬT LẠI NÚT
                HideLoadingLogin(); // ✅ ẨN LOADING ANIMATION
                return;
            }

            if (task.IsFaulted)
            {
                // Parse Firebase error
                string errorMessage = ParseFirebaseError(task.Exception);
                ShowLoginError(errorMessage);
                Debug.Log("Dang nhap that bai: " + task.Exception);
                SetLoginButtonInteractable(true); // ✅ BẬT LẠI NÚT
                HideLoadingLogin(); // ✅ ẨN LOADING ANIMATION
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Dang nhap thanh cong!");
                FirebaseUser user = task.Result.User;
                Debug.Log("Tai khoan da dang nhap: " + user.UserId + " Email: " + user.Email);
                
                // Hiển thị thông báo thành công
                ShowLoginError("Đăng nhập thành công! Đang chuyển...");
                
                // ✅ KHÔNG BẬT LẠI NÚT - Vì sắp chuyển scene rồi
                // ✅ KHÔNG ẨN LOADING - Để hiển thị trong lúc chuyển scene
                
                // chuyen man hinh khi dang nhap
                LoadingManager.NEXT_SCENE = "intromodau";
                SceneManager.LoadScene("Loading");
            }
        });
    }

    public void SwitchForm()
    {
        // ✅ XÓA TẤT CẢ THÔNG BÁO LỖI KHI CHUYỂN FORM
        ClearAllErrors();
        
        loginForm.SetActive(!loginForm.activeSelf);
        registerForm.SetActive(!registerForm.activeSelf);
    }

    // Google Sign-In Method - Simplified approach
    public void SignInWithGoogle()
    {
        Debug.Log("Google Sign-In đang được phát triển. Vui lòng sử dụng đăng nhập email/password.");
        
        // TODO: Implement Google Sign-In using Firebase Auth with Google Provider
        // This requires proper Google Sign-In SDK setup or web-based authentication
        
        // For now, show a message to user
        Debug.Log("Tính năng đăng nhập Google sẽ được cập nhật trong phiên bản tiếp theo.");
    }

    private void CreateOrUpdateGoogleUser(FirebaseUser firebaseUser)
    {
        // Kiểm tra xem user đã có dữ liệu trong database chưa
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("Users").Child(firebaseUser.UserId);
        
        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Lỗi khi kiểm tra user data: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;
            
            if (!snapshot.Exists)
            {
                // User mới - tạo dữ liệu mới SỬ DỤNG CÁCH NHẤT QUÁN
                Debug.Log("User Google mới - tạo dữ liệu...");
                CreateNewUserStructure(firebaseUser.UserId);
            }
            else
            {
                Debug.Log("User Google đã tồn tại - sử dụng dữ liệu cũ");
            }

            // Chuyển scene
            LoadingManager.NEXT_SCENE = "intromodau";
            SceneManager.LoadScene("Loading");
        });
    }
    
    // ===== VALIDATION HELPERS =====
    
    /// <summary>
    /// Kiểm tra email có hợp lệ không
    /// </summary>
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;
        
        // Kiểm tra cơ bản: phải có @ và dấu chấm
        if (!email.Contains("@") || !email.Contains("."))
            return false;
        
        // Kiểm tra @ không ở đầu/cuối
        int atIndex = email.IndexOf("@");
        if (atIndex <= 0 || atIndex >= email.Length - 1)
            return false;
        
        // Kiểm tra có dấu chấm sau @
        int dotIndex = email.LastIndexOf(".");
        if (dotIndex <= atIndex || dotIndex >= email.Length - 1)
            return false;
        
        // Kiểm tra không có khoảng trắng
        if (email.Contains(" "))
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Kiểm tra mật khẩu có đủ mạnh không (khuyến nghị)
    /// </summary>
    private bool IsStrongPassword(string password)
    {
        if (password.Length < 8)
            return false;
        
        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        
        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            if (char.IsLower(c)) hasLower = true;
            if (char.IsDigit(c)) hasDigit = true;
        }
        
        // Khuyến nghị có ít nhất 2 trong 3: chữ hoa, chữ thường, số
        int count = (hasUpper ? 1 : 0) + (hasLower ? 1 : 0) + (hasDigit ? 1 : 0);
        return count >= 2;
    }
    
    /// <summary>
    /// Parse lỗi từ Firebase thành message dễ hiểu
    /// </summary>
    private string ParseFirebaseError(System.Exception exception)
    {
        if (exception == null)
            return "Lỗi không xác định!";
        
        string errorMessage = exception.Message.ToLower();
        
        // Email đã tồn tại
        if (errorMessage.Contains("email") && errorMessage.Contains("already"))
            return "Email đã được đăng ký!\nVui lòng đăng nhập hoặc dùng email khác.";
        
        // Mật khẩu sai
        if (errorMessage.Contains("password") && errorMessage.Contains("wrong"))
            return "Mật khẩu không đúng!\nVui lòng kiểm tra lại.";
        
        // User không tồn tại
        if (errorMessage.Contains("user") && errorMessage.Contains("not found"))
            return "Tài khoản không tồn tại!\nVui lòng đăng ký trước.";
        
        // Email không hợp lệ
        if (errorMessage.Contains("email") && errorMessage.Contains("invalid"))
            return "Email không hợp lệ!\nVui lòng kiểm tra lại.";
        
        // Mật khẩu quá yếu
        if (errorMessage.Contains("weak") && errorMessage.Contains("password"))
            return "Mật khẩu quá yếu!\nTối thiểu 6 ký tự.";
        
        // Quá nhiều request
        if (errorMessage.Contains("too") && errorMessage.Contains("many"))
            return "Bạn đã thử quá nhiều lần!\nVui lòng đợi vài phút.";
        
        // Không có kết nối
        if (errorMessage.Contains("network") || errorMessage.Contains("connection"))
            return "Không có kết nối mạng!\nVui lòng kiểm tra internet.";
        
        // Lỗi chung
        return "Lỗi: " + exception.Message;
    }
    
    // ===== LOADING ANIMATION =====
    
    /// <summary>
    /// Hiển thị animation loading cho đăng ký
    /// </summary>
    private void ShowLoadingRegister()
    {
        if (loadingAnimationRegister != null)
        {
            loadingAnimationRegister.SetActive(true);
            Debug.Log("[FirebaseLoginManager] Hiển thị loading animation đăng ký");
        }
    }
    
    /// <summary>
    /// Ẩn animation loading cho đăng ký
    /// </summary>
    private void HideLoadingRegister()
    {
        if (loadingAnimationRegister != null)
        {
            loadingAnimationRegister.SetActive(false);
            Debug.Log("[FirebaseLoginManager] Ẩn loading animation đăng ký");
        }
    }
    
    /// <summary>
    /// Hiển thị animation loading cho đăng nhập
    /// </summary>
    private void ShowLoadingLogin()
    {
        if (loadingAnimationLogin != null)
        {
            loadingAnimationLogin.SetActive(true);
            Debug.Log("[FirebaseLoginManager] Hiển thị loading animation đăng nhập");
        }
    }
    
    /// <summary>
    /// Ẩn animation loading cho đăng nhập
    /// </summary>
    private void HideLoadingLogin()
    {
        if (loadingAnimationLogin != null)
        {
            loadingAnimationLogin.SetActive(false);
            Debug.Log("[FirebaseLoginManager] Ẩn loading animation đăng nhập");
        }
    }
    
    // ===== UI ERROR DISPLAY =====
    
    /// <summary>
    /// Xóa tất cả thông báo lỗi (cả đăng ký và đăng nhập)
    /// </summary>
    private void ClearAllErrors()
    {
        ClearRegisterError();
        ClearLoginError();
        Debug.Log("[FirebaseLoginManager] Đã xóa tất cả thông báo lỗi");
    }
    
    /// <summary>
    /// Hiển thị lỗi đăng ký (Hỗ trợ cả Text và TextMeshPro)
    /// </summary>
    private void ShowRegisterError(string message)
    {
        bool hasTextComponent = false;
        
        // ✅ XÓA THÔNG BÁO ĐĂNG NHẬP TRƯỚC KHI HIỂN THỊ ĐĂNG KÝ
        ClearLoginError();
        
        // Ưu tiên TextMeshPro
        if (tmpRegisterError != null)
        {
            tmpRegisterError.text = message;
            tmpRegisterError.gameObject.SetActive(true);
            hasTextComponent = true;
            
            // Tự động ẩn sau 2 giây nếu là thông báo thành công
            if (message.Contains("thành công"))
            {
                StartCoroutine(HideTextMeshProAfterDelay(tmpRegisterError, 2f));
            }
        }
        // Fallback: Legacy Text
        else if (txtRegisterError != null)
        {
            txtRegisterError.text = message;
            txtRegisterError.gameObject.SetActive(true);
            hasTextComponent = true;
            
            // Tự động ẩn sau 2 giây nếu là thông báo thành công
            if (message.Contains("thành công"))
            {
                StartCoroutine(HideTextAfterDelay(txtRegisterError, 2f));
            }
        }
        
        if (!hasTextComponent)
        {
            Debug.LogWarning("[FirebaseLoginManager] ⚠️ Chưa gán Text Error!\nGán tmpRegisterError (TextMeshPro) hoặc txtRegisterError (Legacy Text)");
        }
    }
    
    /// <summary>
    /// Xóa lỗi đăng ký (Hỗ trợ cả Text và TextMeshPro)
    /// </summary>
    private void ClearRegisterError()
    {
        if (tmpRegisterError != null)
        {
            tmpRegisterError.text = "";
            tmpRegisterError.gameObject.SetActive(false);
        }
        
        if (txtRegisterError != null)
        {
            txtRegisterError.text = "";
            txtRegisterError.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Hiển thị lỗi đăng nhập (Hỗ trợ cả Text và TextMeshPro)
    /// </summary>
    private void ShowLoginError(string message)
    {
        bool hasTextComponent = false;
        
        // ✅ XÓA THÔNG BÁO ĐĂNG KÝ TRƯỚC KHI HIỂN THỊ ĐĂNG NHẬP
        ClearRegisterError();
        
        // Ưu tiên TextMeshPro
        if (tmpLoginError != null)
        {
            tmpLoginError.text = message;
            tmpLoginError.gameObject.SetActive(true);
            hasTextComponent = true;
            
            // Tự động ẩn sau 2 giây nếu là thông báo thành công
            if (message.Contains("thành công"))
            {
                StartCoroutine(HideTextMeshProAfterDelay(tmpLoginError, 2f));
            }
        }
        // Fallback: Legacy Text
        else if (txtLoginError != null)
        {
            txtLoginError.text = message;
            txtLoginError.gameObject.SetActive(true);
            hasTextComponent = true;
            
            // Tự động ẩn sau 2 giây nếu là thông báo thành công
            if (message.Contains("thành công"))
            {
                StartCoroutine(HideTextAfterDelay(txtLoginError, 2f));
            }
        }
        
        if (!hasTextComponent)
        {
            Debug.LogWarning("[FirebaseLoginManager] ⚠️ Chưa gán Text Error!\nGán tmpLoginError (TextMeshPro) hoặc txtLoginError (Legacy Text)");
        }
    }
    
    /// <summary>
    /// Xóa lỗi đăng nhập (Hỗ trợ cả Text và TextMeshPro)
    /// </summary>
    private void ClearLoginError()
    {
        if (tmpLoginError != null)
        {
            tmpLoginError.text = "";
            tmpLoginError.gameObject.SetActive(false);
        }
        
        if (txtLoginError != null)
        {
            txtLoginError.text = "";
            txtLoginError.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Ẩn Legacy Text sau một khoảng thời gian
    /// </summary>
    private System.Collections.IEnumerator HideTextAfterDelay(Text errorText, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Ẩn TextMeshPro sau một khoảng thời gian
    /// </summary>
    private System.Collections.IEnumerator HideTextMeshProAfterDelay(TMP_Text errorText, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }
    
    // ===== BUTTON STATE MANAGEMENT =====
    
    /// <summary>
    /// Bật/tắt nút đăng ký và làm mờ/sáng
    /// </summary>
    private void SetRegisterButtonInteractable(bool interactable)
    {
        if (buttonRegister != null)
        {
            buttonRegister.interactable = interactable;
            
            // Làm mờ button khi disabled (alpha = 0.5)
            var colors = buttonRegister.colors;
            colors.disabledColor = new Color(
                colors.normalColor.r,
                colors.normalColor.g,
                colors.normalColor.b,
                0.5f // Alpha 50% khi disabled
            );
            buttonRegister.colors = colors;
            
            Debug.Log($"[FirebaseLoginManager] Nút Đăng ký: {(interactable ? "BẬT" : "TẮT")}");
        }
    }
    
    /// <summary>
    /// Bật/tắt nút đăng nhập và làm mờ/sáng
    /// </summary>
    private void SetLoginButtonInteractable(bool interactable)
    {
        if (buttonLogin != null)
        {
            buttonLogin.interactable = interactable;
            
            // Làm mờ button khi disabled (alpha = 0.5)
            var colors = buttonLogin.colors;
            colors.disabledColor = new Color(
                colors.normalColor.r,
                colors.normalColor.g,
                colors.normalColor.b,
                0.5f // Alpha 50% khi disabled
            );
            buttonLogin.colors = colors;
            
            Debug.Log($"[FirebaseLoginManager] Nút Đăng nhập: {(interactable ? "BẬT" : "TẮT")}");
        }
    }
}
