// Import Firebase modules from CDN
import { 
    createUserWithEmailAndPassword,
    signInWithEmailAndPassword,
    signOut,
    sendPasswordResetEmail,
    GoogleAuthProvider,
    signInWithPopup,
    updateProfile,
    onAuthStateChanged
} from 'https://www.gstatic.com/firebasejs/10.7.1/firebase-auth.js';
import { 
    doc, 
    setDoc, 
    getDoc,
    collection,
    query,
    where,
    getDocs
} from 'https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js';

// Wait for Firebase to be initialized
await new Promise(resolve => {
    const checkFirebase = () => {
        if (window.firebaseAuth && window.firebaseDb) {
            resolve();
        } else {
            setTimeout(checkFirebase, 100);
        }
    };
    checkFirebase();
});

const auth = window.firebaseAuth;
const db = window.firebaseDb;

// Google Auth Provider
const googleProvider = new GoogleAuthProvider();
googleProvider.setCustomParameters({
    prompt: 'select_account'
});

// Auth State Management
let currentUser = null;

// Listen for auth state changes
onAuthStateChanged(auth, (user) => {
    currentUser = user;
    if (user) {
        console.log('User signed in:', user.email);
        updateUIForLoggedInUser(user);
    } else {
        console.log('User signed out');
        updateUIForLoggedOutUser();
    }
});

// Show loading state
function showLoading(button) {
    button.classList.add('loading');
    button.disabled = true;
}

// Hide loading state
function hideLoading(button) {
    button.classList.remove('loading');
    button.disabled = false;
}

// Show error message
function showError(message, formId) {
    // Remove existing error messages
    const existingError = document.querySelector(`#${formId} .error-message`);
    if (existingError) {
        existingError.remove();
    }
    
    // Create new error message
    const errorDiv = document.createElement('div');
    errorDiv.className = 'error-message';
    errorDiv.innerHTML = `<i class="fas fa-exclamation-circle"></i> ${message}`;
    
    // Insert at the top of the form
    const form = document.querySelector(`#${formId}`);
    form.insertBefore(errorDiv, form.firstChild);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        if (errorDiv.parentNode) {
            errorDiv.remove();
        }
    }, 5000);
}

// Show success message
function showSuccess(message, formId) {
    const successDiv = document.createElement('div');
    successDiv.className = 'success-message';
    successDiv.innerHTML = `<i class="fas fa-check-circle"></i> ${message}`;
    
    const form = document.querySelector(`#${formId}`);
    form.insertBefore(successDiv, form.firstChild);
    
    setTimeout(() => {
        if (successDiv.parentNode) {
            successDiv.remove();
        }
    }, 3000);
}

// Check if username exists
async function checkUsernameExists(username) {
    try {
        const usersRef = collection(db, 'users');
        const q = query(usersRef, where('username', '==', username));
        const querySnapshot = await getDocs(q);
        return !querySnapshot.empty;
    } catch (error) {
        console.error('Error checking username:', error);
        return false;
    }
}

// Save user data to Firestore
async function saveUserData(user, additionalData = {}) {
    try {
        const userRef = doc(db, 'users', user.uid);
        const userData = {
            uid: user.uid,
            email: user.email,
            displayName: user.displayName || additionalData.fullname || '',
            username: additionalData.username || '',
            createdAt: new Date().toISOString(),
            lastLogin: new Date().toISOString(),
            provider: additionalData.provider || 'email',
            ...additionalData
        };
        
        await setDoc(userRef, userData, { merge: true });
        console.log('User data saved successfully');
    } catch (error) {
        console.error('Error saving user data:', error);
    }
}

// Firebase Auth Functions
export const firebaseAuth = {
    // Register with email and password
    async register(email, password, fullname, username) {
        try {
            // Check if username already exists
            const usernameExists = await checkUsernameExists(username);
            if (usernameExists) {
                throw new Error('Tên đăng nhập đã tồn tại!');
            }

            const userCredential = await createUserWithEmailAndPassword(auth, email, password);
            const user = userCredential.user;
            
            // Update user profile
            await updateProfile(user, {
                displayName: fullname
            });
            
            // Save additional user data
            await saveUserData(user, {
                fullname,
                username,
                provider: 'email'
            });
            
            return {
                success: true,
                user: user,
                message: 'Đăng ký thành công!'
            };
        } catch (error) {
            console.error('Registration error:', error);
            let message = 'Đã có lỗi xảy ra khi đăng ký!';
            
            switch (error.code) {
                case 'auth/email-already-in-use':
                    message = 'Email này đã được sử dụng!';
                    break;
                case 'auth/invalid-email':
                    message = 'Email không hợp lệ!';
                    break;
                case 'auth/weak-password':
                    message = 'Mật khẩu quá yếu! Vui lòng chọn mật khẩu mạnh hơn.';
                    break;
                case 'auth/operation-not-allowed':
                    message = 'Đăng ký bằng email chưa được kích hoạt!';
                    break;
            }
            
            return {
                success: false,
                message: error.message || message
            };
        }
    },

    // Login with email and password
    async login(email, password) {
        try {
            const userCredential = await signInWithEmailAndPassword(auth, email, password);
            const user = userCredential.user;
            
            // Update last login
            await saveUserData(user, {
                lastLogin: new Date().toISOString()
            });
            
            return {
                success: true,
                user: user,
                message: 'Đăng nhập thành công!'
            };
        } catch (error) {
            console.error('Login error:', error);
            let message = 'Đã có lỗi xảy ra khi đăng nhập!';
            
            switch (error.code) {
                case 'auth/user-not-found':
                    message = 'Không tìm thấy tài khoản với email này!';
                    break;
                case 'auth/wrong-password':
                    message = 'Mật khẩu không đúng!';
                    break;
                case 'auth/invalid-email':
                    message = 'Email không hợp lệ!';
                    break;
                case 'auth/user-disabled':
                    message = 'Tài khoản đã bị khóa!';
                    break;
                case 'auth/too-many-requests':
                    message = 'Quá nhiều lần thử. Vui lòng thử lại sau!';
                    break;
            }
            
            return {
                success: false,
                message: message
            };
        }
    },

    // Login with Google
    async loginWithGoogle() {
        try {
            const result = await signInWithPopup(auth, googleProvider);
            const user = result.user;
            
            // Save user data
            await saveUserData(user, {
                provider: 'google',
                lastLogin: new Date().toISOString()
            });
            
            return {
                success: true,
                user: user,
                message: 'Đăng nhập Google thành công!'
            };
        } catch (error) {
            console.error('Google login error:', error);
            let message = 'Đã có lỗi xảy ra khi đăng nhập Google!';
            
            switch (error.code) {
                case 'auth/popup-closed-by-user':
                    message = 'Đăng nhập bị hủy!';
                    break;
                case 'auth/popup-blocked':
                    message = 'Popup bị chặn! Vui lòng cho phép popup và thử lại.';
                    break;
                case 'auth/account-exists-with-different-credential':
                    message = 'Tài khoản đã tồn tại với phương thức đăng nhập khác!';
                    break;
            }
            
            return {
                success: false,
                message: message
            };
        }
    },

    // Reset password
    async resetPassword(email) {
        try {
            await sendPasswordResetEmail(auth, email);
            return {
                success: true,
                message: 'Email khôi phục mật khẩu đã được gửi!'
            };
        } catch (error) {
            console.error('Password reset error:', error);
            let message = 'Đã có lỗi xảy ra khi gửi email!';
            
            switch (error.code) {
                case 'auth/user-not-found':
                    message = 'Không tìm thấy tài khoản với email này!';
                    break;
                case 'auth/invalid-email':
                    message = 'Email không hợp lệ!';
                    break;
            }
            
            return {
                success: false,
                message: message
            };
        }
    },

    // Logout
    async logout() {
        try {
            await signOut(auth);
            return {
                success: true,
                message: 'Đăng xuất thành công!'
            };
        } catch (error) {
            console.error('Logout error:', error);
            return {
                success: false,
                message: 'Đã có lỗi xảy ra khi đăng xuất!'
            };
        }
    },

    // Get current user
    getCurrentUser() {
        return currentUser;
    },

    // Check if user is logged in
    isLoggedIn() {
        return currentUser !== null;
    }
};

// UI Update functions
function updateUIForLoggedInUser(user) {
    // Update login button to show user info
    const loginBtn = document.querySelector('.login-btn');
    if (loginBtn) {
        loginBtn.innerHTML = `
            <img src="${user.photoURL || 'img/logo/ChatGPT Image 13_27_39 3 thg 9, 2025.png'}" 
                 alt="Avatar" class="user-avatar">
            <span>${user.displayName || user.email}</span>
            <i class="fas fa-chevron-down"></i>
        `;
        loginBtn.onclick = () => showUserMenu();
    }
    
    // Close any open modals
    closeAllModals();
}

function updateUIForLoggedOutUser() {
    // Reset login button
    const loginBtn = document.querySelector('.login-btn');
    if (loginBtn) {
        loginBtn.innerHTML = `
            <i class="fas fa-user"></i>
            Đăng nhập
        `;
        loginBtn.onclick = () => window.openLoginModal();
    }
}

function showUserMenu() {
    // Create dropdown menu for logged in user
    const existingMenu = document.querySelector('.user-menu');
    if (existingMenu) {
        existingMenu.remove();
        return;
    }
    
    const menu = document.createElement('div');
    menu.className = 'user-menu';
    menu.innerHTML = `
        <div class="user-menu-item" onclick="showPromoCodes()">
            <i class="fas fa-gift"></i> Mã khuyến mãi
        </div>
        <hr>
        <div class="user-menu-item logout" onclick="handleLogout()">
            <i class="fas fa-sign-out-alt"></i> Đăng xuất
        </div>
    `;
    
    const loginBtn = document.querySelector('.login-btn');
    loginBtn.parentNode.appendChild(menu);
    
    // Close menu when clicking outside
    setTimeout(() => {
        document.addEventListener('click', function closeMenu(e) {
            if (!menu.contains(e.target) && !loginBtn.contains(e.target)) {
                menu.remove();
                document.removeEventListener('click', closeMenu);
            }
        });
    }, 0);
}

function closeAllModals() {
    const modals = ['loginModal', 'registerModal', 'forgotPasswordModal', 'resetSuccessModal'];
    modals.forEach(modalId => {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = 'none';
        }
    });
    document.body.style.overflow = 'auto';
}

// Placeholder functions for user menu items
function showPromoCodes() {
    Swal.fire({
        title: '<div style="font-size: 1.8rem; font-weight: 700; color: #2c3e50; margin-bottom: 10px;">Mã Khuyến Mãi</div>',
        html: `
            <div style="margin: 25px 0;">
                <!-- Description -->
                <p style="font-size: 1rem; 
                          color: #5a6c7d; 
                          margin-bottom: 30px; 
                          line-height: 1.6;
                          font-weight: 400;">
                    Nhập mã để nhận Kim Cương, Vàng và nhiều phần thưởng khác
                </p>
                
                <!-- Input -->
                <div style="margin: 0 auto 25px; max-width: 420px;">
                    <input type="text" 
                           id="promoCodeInput" 
                           placeholder="NHẬP MÃ TẠI ĐÂY"
                           style="width: 100%; 
                                  padding: 16px 20px; 
                                  font-size: 1.05rem; 
                                  border: 2px solid #e1e8ed; 
                                  border-radius: 12px; 
                                  outline: none;
                                  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
                                  text-transform: uppercase;
                                  letter-spacing: 3px;
                                  font-weight: 600;
                                  color: #2c3e50;
                                  background: #f8f9fa;
                                  text-align: center;"
                           onkeyup="this.value = this.value.toUpperCase()"
                           onfocus="this.style.borderColor='#667eea'; 
                                    this.style.background='#ffffff'; 
                                    this.style.boxShadow='0 0 0 4px rgba(102, 126, 234, 0.1)'"
                           onblur="this.style.borderColor='#e1e8ed'; 
                                   this.style.background='#f8f9fa'; 
                                   this.style.boxShadow='none'">
                </div>
                
                <!-- Note -->
                <div style="background: #f8f9fa; 
                            padding: 16px 20px; 
                            border-radius: 10px; 
                            border-left: 4px solid #667eea;
                            margin-top: 25px;">
                    <p style="margin: 0; 
                              color: #5a6c7d; 
                              font-size: 0.92rem; 
                              line-height: 1.5;">
                        <span style="color: #667eea; font-weight: 600;">💡 Lưu ý:</span> 
                        Theo dõi Fanpage và Discord để nhận mã mới nhất
                    </p>
                </div>
            </div>
        `,
        showCancelButton: true,
        confirmButtonText: 'Nhận Quà',
        cancelButtonText: 'Đóng',
        confirmButtonColor: '#667eea',
        cancelButtonColor: '#95a5a6',
        buttonsStyling: true,
        width: '500px',
        padding: '35px',
        background: '#ffffff',
        backdrop: 'rgba(44, 62, 80, 0.4)',
        customClass: {
            confirmButton: 'swal2-promo-confirm',
            cancelButton: 'swal2-promo-cancel',
            popup: 'swal2-promo-popup'
        },
        didOpen: () => {
            const style = document.createElement('style');
            style.innerHTML = `
                .swal2-promo-popup {
                    border-radius: 16px !important;
                    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15) !important;
                }
                .swal2-promo-confirm {
                    padding: 13px 35px !important;
                    border-radius: 10px !important;
                    font-weight: 600 !important;
                    font-size: 1rem !important;
                    transition: all 0.3s ease !important;
                    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3) !important;
                }
                .swal2-promo-confirm:hover {
                    transform: translateY(-2px);
                    box-shadow: 0 6px 16px rgba(102, 126, 234, 0.4) !important;
                }
                .swal2-promo-cancel {
                    padding: 13px 30px !important;
                    border-radius: 10px !important;
                    font-weight: 500 !important;
                    font-size: 0.95rem !important;
                    transition: all 0.3s ease !important;
                }
                .swal2-promo-cancel:hover {
                    background: #7f8c8d !important;
                }
            `;
            document.head.appendChild(style);
            
            setTimeout(() => {
                document.getElementById('promoCodeInput').focus();
            }, 100);
        },
        preConfirm: () => {
            const code = document.getElementById('promoCodeInput').value.trim();
            if (!code) {
                Swal.showValidationMessage('Vui lòng nhập mã khuyến mãi');
                return false;
            }
            return code;
        }
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire({
                title: 'Đang xử lý...',
                html: '<div style="color: #667eea; font-size: 2.5rem;">⏳</div>',
                showConfirmButton: false,
                allowOutsideClick: false,
                timer: 1000,
                didOpen: () => {
                    Swal.showLoading();
                }
            }).then(() => {
                // TODO: Implement promo code validation and reward
                Swal.fire({
                    icon: 'error',
                    title: 'Mã không hợp lệ',
                    html: `
                        <p style="font-size: 1rem; color: #5a6c7d; margin: 15px 0; line-height: 1.5;">
                            Mã <strong style="color: #e74c3c;">${result.value}</strong> không tồn tại hoặc đã hết hạn
                        </p>
                    `,
                    confirmButtonText: 'Đã hiểu',
                    confirmButtonColor: '#667eea',
                    width: '450px'
                });
            });
        }
    });
    
    const menu = document.querySelector('.user-menu');
    if (menu) menu.remove();
}


async function handleLogout() {
    Swal.fire({
        icon: 'question',
        title: 'Xác nhận đăng xuất',
        text: 'Bạn có chắc chắn muốn đăng xuất khỏi Vườn Rực Rỡ?',
        showCancelButton: true,
        confirmButtonText: 'Đăng xuất',
        cancelButtonText: 'Ở lại',
        confirmButtonColor: '#ff6b6b',
        cancelButtonColor: '#6c757d'
    }).then(async (result) => {
        if (result.isConfirmed) {
            const logoutResult = await firebaseAuth.logout();
            if (logoutResult.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Đăng xuất thành công!',
                    text: 'Hẹn gặp lại bạn trong Vườn Rực Rỡ!',
                    confirmButtonText: 'Tạm biệt!',
                    confirmButtonColor: '#4a90e2',
                    timer: 2000,
                    timerProgressBar: true
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi đăng xuất!',
                    text: logoutResult.message,
                    confirmButtonText: 'Thử lại',
                    confirmButtonColor: '#ff6b6b'
                });
            }
        }
    });
    
    const menu = document.querySelector('.user-menu');
    if (menu) menu.remove();
}

// Make functions globally available
window.firebaseAuth = firebaseAuth;
window.showLoading = showLoading;
window.hideLoading = hideLoading;
window.showError = showError;
window.showSuccess = showSuccess;
window.showUserMenu = showUserMenu;
window.handleLogout = handleLogout;
window.showPromoCodes = showPromoCodes;

console.log('🔥 Firebase Auth initialized successfully!');