// Wait for Firebase to be initialized
function waitForFirebase() {
    return new Promise(resolve => {
        const checkFirebase = () => {
            if (window.firebaseAuth && window.firebaseRTDB) {
                resolve();
            } else {
                setTimeout(checkFirebase, 100);
            }
        };
        checkFirebase();
    });
}

// Initialize Firebase Auth functions
async function initializeFirebaseAuth() {
    await waitForFirebase();
    
    // Import Firebase modules dynamically
    const { 
        createUserWithEmailAndPassword,
        signInWithEmailAndPassword,
        signOut,
        sendPasswordResetEmail,
        GoogleAuthProvider,
        signInWithPopup,
        updateProfile,
        onAuthStateChanged
    } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-auth.js');
    
    const { 
        doc, 
        setDoc, 
        getDoc,
        collection,
        query,
        where,
        getDocs
    } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');

    const auth = window.firebaseAuth;
    const db = window.firebaseRTDB;
    
    // Expose auth globally for recharge.html
    window.auth = auth;
    
    // Also expose firebase globally for recharge.html
    if (typeof window.firebase === 'undefined') {
        window.firebase = {
            database: () => window.firebaseRTDB
        };
    }
    
    // Debug: Log what we're exposing
    console.log('🔧 Exposing Firebase objects:');
    console.log('  - window.auth:', typeof window.auth);
    console.log('  - window.firebase:', typeof window.firebase);
    console.log('  - window.firebaseRTDB:', typeof window.firebaseRTDB);
    console.log('  - window.firebaseRTDB.ref:', typeof window.firebaseRTDB?.ref);

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
        if (form) {
            form.insertBefore(errorDiv, form.firstChild);
        }
        
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
        if (form) {
            form.insertBefore(successDiv, form.firstChild);
        }
        
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
    const firebaseAuth = {
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
            <div class="user-menu-item" onclick="goToRecharge()">
                <i class="fas fa-coins"></i> Nạp Tiền
            </div>
            <hr>
            <div class="user-menu-item logout" onclick="handleLogout()">
                <i class="fas fa-sign-out-alt"></i> Đăng xuất
            </div>
        `;
        
        const loginBtn = document.querySelector('.login-btn');
        if (loginBtn && loginBtn.parentNode) {
            loginBtn.parentNode.appendChild(menu);
        }
        
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

    // Promo Code Functions
    async function showPromoCodes() {
        // Check if promo code system is available
        if (typeof window.promoCodeSystem === 'undefined') {
            Swal.fire({
                title: 'Hệ thống mã khuyến mãi',
                text: 'Đang tải hệ thống mã khuyến mãi...',
                icon: 'info',
                allowOutsideClick: false,
                showConfirmButton: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });
            
            // Wait for promo code system to load
            setTimeout(() => {
                if (typeof window.promoCodeSystem !== 'undefined') {
                    Swal.close();
                    showPromoCodeInterface();
                } else {
                    Swal.fire({
                        title: 'Lỗi hệ thống',
                        text: 'Không thể tải hệ thống mã khuyến mãi. Vui lòng thử lại sau.',
                        icon: 'error',
                        confirmButtonText: 'Đóng'
                    });
                }
            }, 2000);
            return;
        }
        
        showPromoCodeInterface();
    }
    
    function showPromoCodeInterface() {
        Swal.fire({
            title: '🎁 Mã Khuyến Mãi',
            html: `
                <div class="promo-container">
                    <div class="promo-header">
                        <div class="promo-icon">
                            <i class="fas fa-gift"></i>
                        </div>
                        <h3>Nhập mã khuyến mãi</h3>
                        <p>Nhập mã để nhận phần thưởng đặc biệt</p>
                    </div>
                    
                    <div class="promo-input-section">
                        <div class="input-group">
                            <label for="promoCodeInput" class="input-label">
                                <i class="fas fa-ticket-alt"></i>
                                Mã khuyến mãi
                            </label>
                            <input type="text" id="promoCodeInput" class="promo-input" 
                                   placeholder="Nhập mã khuyến mãi..." maxlength="20">
                            <div class="input-hint">VD: LANGHOARUC888</div>
                        </div>
                        
                        <button type="button" class="promo-btn primary" onclick="usePromoCode()">
                            <i class="fas fa-gift"></i>
                            <span>Sử dụng mã</span>
                        </button>
                    </div>
                    
                    <div class="promo-actions">
                        <button type="button" class="promo-btn secondary" onclick="showPromoHistory()">
                            <i class="fas fa-history"></i>
                            <span>Lịch sử</span>
                        </button>
                        
                        <button type="button" class="promo-btn info" onclick="showAvailablePromos()">
                            <i class="fas fa-list"></i>
                            <span>Mã có sẵn</span>
                        </button>
                    </div>
                    
                    <div class="promo-footer">
                        <div class="promo-tips">
                            <i class="fas fa-lightbulb"></i>
                            <span>Mẹo: Mã khuyến mãi thường có thời hạn sử dụng</span>
                        </div>
                    </div>
                </div>
            `,
            showCancelButton: true,
            cancelButtonText: 'Đóng',
            confirmButtonText: false,
            showConfirmButton: false,
            width: '600px',
            customClass: {
                popup: 'promo-code-popup',
                title: 'promo-title'
            },
            showCloseButton: true,
            focusConfirm: false,
            allowOutsideClick: true
        });
        
        // Focus on input
        setTimeout(() => {
            const input = document.getElementById('promoCodeInput');
            if (input) {
                input.focus();
                input.addEventListener('keypress', function(e) {
                    if (e.key === 'Enter') {
                        usePromoCode();
                    }
                });
            }
        }, 100);
    }
    
    async function usePromoCode() {
        const promoCode = document.getElementById('promoCodeInput').value.trim().toUpperCase();
        
        if (!promoCode) {
            Swal.fire({
                title: '⚠️ Lỗi!',
                text: 'Vui lòng nhập mã khuyến mãi',
                icon: 'warning',
                confirmButtonText: 'Thử lại',
                confirmButtonColor: '#ffc107'
            });
            return;
        }
        
        // Show loading
        Swal.fire({
            title: 'Đang xử lý...',
            text: 'Vui lòng chờ trong giây lát',
            allowOutsideClick: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
        
        try {
            const result = await window.promoCodeSystem.usePromoCode(promoCode);
            
            if (result.success) {
                Swal.fire({
                    title: '🎉 Thành công!',
                    html: `
                        <div class="success-container">
                            <div class="success-icon">
                                <i class="fas fa-gift"></i>
                            </div>
                            <h3>Chúc mừng!</h3>
                            <p class="success-message">${result.message}</p>
                            <div class="reward-card">
                                <div class="reward-icon">
                                    <i class="fas fa-${result.reward.type === 'diamond' ? 'gem' : 'coins'}"></i>
                                </div>
                                <div class="reward-info">
                                    <h4>Phần thưởng nhận được</h4>
                                    <div class="reward-amount">
                                        ${result.reward.value} ${result.reward.type === 'diamond' ? 'Kim cương' : 'Vàng'}
                                    </div>
                                    <p class="reward-description">${result.reward.description}</p>
                                </div>
                            </div>
                        </div>
                    `,
                    icon: 'success',
                    confirmButtonText: 'Tuyệt vời!',
                    confirmButtonColor: '#667eea',
                    customClass: {
                        popup: 'success-popup'
                    }
                });
                
                // Update user stats if available
                if (window.updateStatsDisplay) {
                    window.updateStatsDisplay();
                }
            } else {
                Swal.fire({
                    title: 'Lỗi!',
                    text: result.message,
                    icon: 'error',
                    confirmButtonText: 'Thử lại'
                });
            }
        } catch (error) {
            console.error('Error using promo code:', error);
            Swal.fire({
                title: 'Lỗi hệ thống!',
                text: 'Đã có lỗi xảy ra khi sử dụng mã khuyến mãi. Vui lòng thử lại sau.',
                icon: 'error',
                confirmButtonText: 'Đóng'
            });
        }
    }
    
    async function showPromoHistory() {
        try {
            const history = await window.promoCodeSystem.getUserPromoHistory();
            
            if (history.length === 0) {
                Swal.fire({
                    title: 'Lịch sử mã khuyến mãi',
                    text: 'Bạn chưa sử dụng mã khuyến mãi nào',
            icon: 'info',
            confirmButtonText: 'Đóng'
        });
                return;
            }
            
            let historyHtml = '<div style="text-align: left; max-height: 300px; overflow-y: auto;">';
            history.forEach((promo, index) => {
                historyHtml += `
                    <div style="margin-bottom: 15px; padding: 10px; background: #f8f9fa; border-radius: 5px; border-left: 4px solid #4a90e2;">
                        <p><strong>Mã:</strong> ${promo.code}</p>
                        <p><strong>Phần thưởng:</strong> ${promo.rewardValue} ${promo.rewardType === 'diamond' ? 'Kim cương' : 'Vàng'}</p>
                        <p><strong>Ngày sử dụng:</strong> ${new Date(promo.usedAt).toLocaleDateString('vi-VN')}</p>
                        <p><strong>Mô tả:</strong> ${promo.promoDescription}</p>
                    </div>
                `;
            });
            historyHtml += '</div>';
            
            Swal.fire({
                title: '📋 Lịch sử mã khuyến mãi',
                html: historyHtml,
                width: '600px',
                confirmButtonText: 'Đóng'
            });
        } catch (error) {
            console.error('Error getting promo history:', error);
            Swal.fire({
                title: 'Lỗi!',
                text: 'Không thể tải lịch sử mã khuyến mãi',
                icon: 'error',
                confirmButtonText: 'Đóng'
            });
        }
    }
    
    async function showAvailablePromos() {
        try {
            const availablePromos = await window.promoCodeSystem.getAvailablePromoCodes();
            
            if (availablePromos.length === 0) {
                Swal.fire({
                    title: 'Mã khuyến mãi có sẵn',
                    text: 'Hiện tại không có mã khuyến mãi nào',
                    icon: 'info',
                    confirmButtonText: 'Đóng'
                });
                return;
            }
            
            let promosHtml = '<div style="text-align: left; max-height: 300px; overflow-y: auto;">';
            availablePromos.forEach((promo, index) => {
                promosHtml += `
                    <div style="margin-bottom: 15px; padding: 10px; background: #f8f9fa; border-radius: 5px; border-left: 4px solid #28a745;">
                        <p><strong>Mã:</strong> ${promo.code}</p>
                        <p><strong>Phần thưởng:</strong> ${promo.rewardValue} ${promo.rewardType === 'diamond' ? 'Kim cương' : 'Vàng'}</p>
                        <p><strong>Mô tả:</strong> ${promo.description}</p>
                        <p><strong>Đã sử dụng:</strong> ${promo.usedCount}/${promo.usageLimit === 0 ? '∞' : promo.usageLimit}</p>
                        <p><strong>Hạn sử dụng:</strong> ${new Date(promo.expiryDate).toLocaleDateString('vi-VN')}</p>
                    </div>
                `;
            });
            promosHtml += '</div>';
            
            Swal.fire({
                title: '📋 Mã khuyến mãi có sẵn',
                html: promosHtml,
                width: '600px',
                confirmButtonText: 'Đóng'
            });
        } catch (error) {
            console.error('Error getting available promos:', error);
            Swal.fire({
                title: 'Lỗi!',
                text: 'Không thể tải danh sách mã khuyến mãi',
                icon: 'error',
                confirmButtonText: 'Đóng'
            });
        }
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

    // Go to recharge page
    function goToRecharge() {
        window.location.href = 'recharge.html';
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
    window.showPromoCodeInterface = showPromoCodeInterface;
    window.usePromoCode = usePromoCode;
    window.showPromoHistory = showPromoHistory;
    window.showAvailablePromos = showAvailablePromos;
    window.goToRecharge = goToRecharge;

    console.log('🔥 Firebase Auth initialized successfully!');
}

// Initialize Firebase Auth
initializeFirebaseAuth();