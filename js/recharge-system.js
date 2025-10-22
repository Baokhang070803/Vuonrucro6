class RechargeSystem {
    constructor() {
        this.rtdb = null;
        this.auth = null;
        this.selectedPackage = null;
        this.selectedPaymentMethod = null;
        this.packages = {
            diamond: [
                { id: 'diamond_100', name: '100 Kim Cương', amount: 100, price: 10000, bonus: 0, icon: 'fas fa-gem' },
                { id: 'diamond_500', name: '500 Kim Cương', amount: 500, price: 45000, bonus: 50, icon: 'fas fa-gem' },
                { id: 'diamond_1000', name: '1000 Kim Cương', amount: 1000, price: 85000, bonus: 150, icon: 'fas fa-gem' },
                { id: 'diamond_2000', name: '2000 Kim Cương', amount: 2000, price: 160000, bonus: 400, icon: 'fas fa-gem' },
                { id: 'diamond_5000', name: '5000 Kim Cương', amount: 5000, price: 380000, bonus: 1200, icon: 'fas fa-gem' }
            ],
            gold: [
                { id: 'gold_1000', name: '1000 Vàng', amount: 1000, price: 5000, bonus: 0, icon: 'fas fa-coins' },
                { id: 'gold_5000', name: '5000 Vàng', amount: 5000, price: 22000, bonus: 500, icon: 'fas fa-coins' },
                { id: 'gold_10000', name: '10000 Vàng', amount: 10000, price: 42000, bonus: 1000, icon: 'fas fa-coins' },
                { id: 'gold_20000', name: '20000 Vàng', amount: 20000, price: 80000, bonus: 2000, icon: 'fas fa-coins' },
                { id: 'gold_50000', name: '50000 Vàng', amount: 50000, price: 190000, bonus: 5000, icon: 'fas fa-coins' }
            ]
        };
        this.paymentMethods = [
            { id: 'vnpay', name: 'VNPay', icon: 'fas fa-credit-card', color: '#1f4e79' }
        ];
        
        this.initializeFirebase();
    }

    async initializeFirebase() {
        // Wait for Firebase to be available
        let attempts = 0;
        while (attempts < 50 && (!window.firebaseRTDB || !window.firebaseAuth)) {
            await new Promise(resolve => setTimeout(resolve, 100));
            attempts++;
        }

        if (window.firebaseRTDB && window.firebaseAuth) {
            this.rtdb = window.firebaseRTDB;
            this.auth = window.firebaseAuth;
            console.log('✅ Recharge System: Firebase initialized');
        } else {
            console.error('❌ Recharge System: Firebase not available');
        }
    }

    getCurrentUser() {
        if (!this.auth) return null;
        
        // Try different ways to get current user
        if (typeof this.auth.getCurrentUser === 'function') {
            return this.auth.getCurrentUser();
        } else if (this.auth.currentUser) {
            return this.auth.currentUser;
        } else if (this.auth.auth && this.auth.auth.currentUser) {
            return this.auth.auth.currentUser;
        }
        return null;
    }

    async initialize() {
        await this.loadUserData();
        this.renderPackages();
        this.renderPaymentMethods();
        this.setupEventListeners();
        
        // Auto-select VNPay
        this.selectedPaymentMethod = 'vnpay';
        const vnpayMethod = document.querySelector('[data-method-id="vnpay"]');
        if (vnpayMethod) {
            vnpayMethod.classList.add('selected');
        }
        this.updateRechargeButton();
    }

    async loadUserData() {
        const user = this.getCurrentUser();
        if (!user) {
            console.log('User not logged in');
            return;
        }

        try {
            if (!this.rtdb) {
                console.error('RTDB not initialized');
                return;
            }

            const { ref, get } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            const userRef = ref(this.rtdb, `Users/${user.uid}`);
            const snapshot = await get(userRef);
            
            if (snapshot.exists()) {
                const userData = snapshot.val();
                this.updateUserDisplay(userData);
            }
        } catch (error) {
            console.error('Error loading user data:', error);
        }
    }

    updateUserDisplay(userData) {
        // Update gold display
        const goldElement = document.getElementById('userGold');
        if (goldElement) {
            goldElement.textContent = userData.Gold || 0;
        }

        // Update diamond display
        const diamondElement = document.getElementById('userDiamond');
        if (diamondElement) {
            diamondElement.textContent = userData.Diamond || 0;
        }

        // Update level display
        const levelElement = document.getElementById('userLevel');
        if (levelElement) {
            const expData = userData.ExpData ? JSON.parse(userData.ExpData) : { currentLevel: 1 };
            levelElement.textContent = expData.currentLevel || 1;
        }
    }

    renderPackages() {
        // Render diamond packages
        const diamondContainer = document.getElementById('diamondPackages');
        if (diamondContainer) {
            diamondContainer.innerHTML = this.packages.diamond.map(pkg => `
                <div class="package-item" data-package-id="${pkg.id}" data-package-type="diamond" onclick="selectPackage('${pkg.id}', 'diamond')">
                    <div class="package-info">
                        <div class="package-icon" style="background: linear-gradient(135deg, #667eea, #764ba2);">
                            <i class="${pkg.icon}"></i>
                        </div>
                        <div class="package-details">
                            <h4>${pkg.name}</h4>
                            <p>${pkg.bonus > 0 ? `+${pkg.bonus} bonus` : 'Không có bonus'}</p>
                        </div>
                    </div>
                    <div class="package-price">${pkg.price.toLocaleString()}đ</div>
                </div>
            `).join('');
        }

        // Render gold packages
        const goldContainer = document.getElementById('goldPackages');
        if (goldContainer) {
            goldContainer.innerHTML = this.packages.gold.map(pkg => `
                <div class="package-item" data-package-id="${pkg.id}" data-package-type="gold" onclick="selectPackage('${pkg.id}', 'gold')">
                    <div class="package-info">
                        <div class="package-icon" style="background: linear-gradient(135deg, #ffd700, #ffb347);">
                            <i class="${pkg.icon}"></i>
                        </div>
                        <div class="package-details">
                            <h4>${pkg.name}</h4>
                            <p>${pkg.bonus > 0 ? `+${pkg.bonus} bonus` : 'Không có bonus'}</p>
                        </div>
                    </div>
                    <div class="package-price">${pkg.price.toLocaleString()}đ</div>
                </div>
            `).join('');
        }
    }

    renderPaymentMethods() {
        const container = document.getElementById('paymentMethods');
        if (container) {
            container.innerHTML = this.paymentMethods.map(method => `
                <div class="payment-method" data-method-id="${method.id}" onclick="selectPaymentMethod('${method.id}')">
                    <div class="payment-icon" style="background: ${method.color};">
                        <i class="${method.icon}"></i>
                    </div>
                    <div class="payment-name">${method.name}</div>
                </div>
            `).join('');
        }
    }

    setupEventListeners() {
        // Package selection
        window.selectPackage = (packageId, type) => {
            // Remove previous selection
            document.querySelectorAll('.package-item').forEach(item => {
                item.classList.remove('selected');
            });

            // Add selection to clicked item
            const selectedItem = document.querySelector(`[data-package-id="${packageId}"]`);
            if (selectedItem) {
                selectedItem.classList.add('selected');
            }

            // Store selection
            this.selectedPackage = { id: packageId, type };
            this.updateRechargeButton();
        };

        // Payment method selection
        window.selectPaymentMethod = (methodId) => {
            // Remove previous selection
            document.querySelectorAll('.payment-method').forEach(method => {
                method.classList.remove('selected');
            });

            // Add selection to clicked method
            const selectedMethod = document.querySelector(`[data-method-id="${methodId}"]`);
            if (selectedMethod) {
                selectedMethod.classList.add('selected');
            }

            // Store selection
            this.selectedPaymentMethod = methodId;
            this.updateRechargeButton();
        };
    }

    updateRechargeButton() {
        const btn = document.getElementById('rechargeBtn');
        if (btn) {
            if (this.selectedPackage && this.selectedPaymentMethod) {
                btn.disabled = false;
                const packageData = this.getPackageData(this.selectedPackage.id, this.selectedPackage.type);
                btn.innerHTML = `<i class="fas fa-shopping-cart"></i> Nạp ${packageData.name} - ${packageData.price.toLocaleString()}đ`;
            } else {
                btn.disabled = true;
                btn.innerHTML = '<i class="fas fa-shopping-cart"></i> Nạp tiền ngay';
            }
        }
    }

    getPackageData(packageId, type) {
        const packages = this.packages[type];
        return packages.find(pkg => pkg.id === packageId);
    }

    async processRecharge() {
        if (!this.selectedPackage || !this.selectedPaymentMethod) {
            Swal.fire({
                icon: 'warning',
                title: 'Chưa chọn gói',
                text: 'Vui lòng chọn gói nạp và phương thức thanh toán'
            });
            return;
        }

        const user = this.getCurrentUser();
        if (!user) {
            Swal.fire({
                icon: 'error',
                title: 'Chưa đăng nhập',
                text: 'Vui lòng đăng nhập để nạp tiền'
            });
            return;
        }

        const packageData = this.getPackageData(this.selectedPackage.id, this.selectedPackage.type);
        
        // Show confirmation
        const result = await Swal.fire({
            title: 'Xác nhận nạp tiền',
            html: `
                <div style="text-align: left;">
                    <p><strong>Gói:</strong> ${packageData.name}</p>
                    <p><strong>Giá:</strong> ${packageData.price.toLocaleString()}đ</p>
                    <p><strong>Phương thức:</strong> ${this.paymentMethods.find(m => m.id === this.selectedPaymentMethod).name}</p>
                    ${packageData.bonus > 0 ? `<p><strong>Bonus:</strong> +${packageData.bonus}</p>` : ''}
                </div>
            `,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Xác nhận',
            cancelButtonText: 'Hủy',
            confirmButtonColor: '#667eea'
        });

        if (result.isConfirmed) {
            await this.executeRecharge(packageData);
        }
    }

    async executeRecharge(packageData) {
        try {
            if (!this.rtdb) {
                throw new Error('Firebase RTDB not initialized');
            }

            const user = this.getCurrentUser();
            const { ref, get, set } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            
            // Get current user data
            const userRef = ref(this.rtdb, `Users/${user.uid}`);
            const snapshot = await get(userRef);
            
            if (!snapshot.exists()) {
                throw new Error('User data not found');
            }

            const userData = snapshot.val();
            const totalAmount = packageData.amount + packageData.bonus;

            // Update user data
            if (this.selectedPackage.type === 'diamond') {
                userData.Diamond = (userData.Diamond || 0) + totalAmount;
            } else {
                userData.Gold = (userData.Gold || 0) + totalAmount;
            }

            // Save updated data
            await set(userRef, userData);

            // Record transaction
            await this.recordTransaction(packageData, userData);

            // Show success message
            Swal.fire({
                icon: 'success',
                title: 'Nạp tiền thành công!',
                text: `Bạn đã nhận được ${totalAmount} ${this.selectedPackage.type === 'diamond' ? 'Kim Cương' : 'Vàng'}`,
                confirmButtonColor: '#667eea'
            });

            // Update display
            this.updateUserDisplay(userData);

            // Reset selections
            this.resetSelections();

        } catch (error) {
            console.error('Error processing recharge:', error);
            Swal.fire({
                icon: 'error',
                title: 'Lỗi nạp tiền',
                text: error.message || 'Có lỗi xảy ra khi nạp tiền'
            });
        }
    }

    async recordTransaction(packageData, userData) {
        try {
            const { ref, push } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            const transactionRef = ref(this.rtdb, `Transactions/${this.getCurrentUser().uid}`);
            
            const transaction = {
                timestamp: Date.now(),
                date: new Date().toISOString().split('T')[0],
                time: new Date().toLocaleTimeString('vi-VN'),
                packageId: packageData.id,
                packageName: packageData.name,
                packageType: this.selectedPackage.type,
                amount: packageData.amount,
                bonus: packageData.bonus,
                totalAmount: packageData.amount + packageData.bonus,
                price: packageData.price,
                paymentMethod: this.selectedPaymentMethod,
                status: 'completed',
                userAgent: navigator.userAgent,
                ipAddress: 'unknown', // Would need server-side implementation
                sessionId: this.generateSessionId(),
                deviceType: this.getDeviceType()
            };

            await push(transactionRef, transaction);
        } catch (error) {
            console.error('Error recording transaction:', error);
        }
    }

    resetSelections() {
        this.selectedPackage = null;
        this.selectedPaymentMethod = null;
        
        document.querySelectorAll('.package-item, .payment-method').forEach(item => {
            item.classList.remove('selected');
        });
        
        this.updateRechargeButton();
    }

    generateSessionId() {
        return 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    getDeviceType() {
        const userAgent = navigator.userAgent;
        if (/Mobile|Android|iPhone|iPad/.test(userAgent)) {
            return 'mobile';
        } else if (/Tablet|iPad/.test(userAgent)) {
            return 'tablet';
        } else {
            return 'desktop';
        }
    }
}

// Initialize system
window.rechargeSystem = new RechargeSystem();

// Export functions
window.processRecharge = function() {
    return window.rechargeSystem.processRecharge();
};

console.log('💰 Recharge System initialized!');