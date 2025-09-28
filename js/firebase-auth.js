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
        <div class="user-menu-item" onclick="showUserProfile()">
            <i class="fas fa-user-circle"></i> Thông tin cá nhân
        </div>
        <div class="user-menu-item" onclick="showGameStats()">
            <i class="fas fa-trophy"></i> Thành tích
        </div>
        <div class="user-menu-item" onclick="showSettings()">
            <i class="fas fa-cog"></i> Cài đặt
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
function showUserProfile() {
    Swal.fire({
        icon: 'info',
        title: 'Thông tin cá nhân',
        text: 'Tính năng quản lý thông tin cá nhân sẽ sớm có! Bạn sẽ có thể xem và chỉnh sửa profile, avatar và thông tin tài khoản.',
        confirmButtonText: 'Đã hiểu',
        confirmButtonColor: '#4a90e2'
    });
    document.querySelector('.user-menu').remove();
}

function showGameStats() {
    Swal.fire({
        icon: 'info',
        title: 'Thành tích game',
        text: 'Tính năng xem thành tích và bảng xếp hạng sẽ sớm có! Theo dõi điểm số, level và các thành tựu của bạn.',
        confirmButtonText: 'Tuyệt vời!',
        confirmButtonColor: '#4a90e2'
    });
    document.querySelector('.user-menu').remove();
}

function showSettings() {
    Swal.fire({
        icon: 'info',
        title: 'Cài đặt',
        text: 'Tính năng cài đặt sẽ sớm có! Bạn sẽ có thể tùy chỉnh âm thanh, đồ họa và các tùy chọn game khác.',
        confirmButtonText: 'OK',
        confirmButtonColor: '#4a90e2'
    });
    document.querySelector('.user-menu').remove();
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
window.showUserProfile = showUserProfile;
window.showGameStats = showGameStats;
window.showSettings = showSettings;

console.log('🔥 Firebase Auth initialized successfully!');