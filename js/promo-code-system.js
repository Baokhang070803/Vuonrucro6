// Promo Code System - Hệ thống mã khuyến mãi
// Tác giả: Làng Hoa Rực Team
// Ngày: 2025

class PromoCodeSystem {
    constructor() {
        this.rtdb = null;
        this.auth = null;
        this.dailyRewards = {
            1: 10, 2: 10, 3: 15, 4: 15, 5: 20, 6: 20, 7: 100,
            8: 25, 9: 25, 10: 30, 11: 30, 12: 35, 13: 35, 14: 200,
            15: 40, 16: 40, 17: 45, 18: 45, 19: 50, 20: 50, 21: 300,
            22: 55, 23: 55, 24: 60, 25: 60, 26: 65, 27: 65, 28: 70, 29: 75, 30: 500
        };
        
        // Initialize Firebase connections
        this.initializeFirebase();
    }

    // Initialize Firebase connections
    async initializeFirebase() {
        try {
            // Wait for Firebase to be available
            let attempts = 0;
            const maxAttempts = 50; // 5 seconds max wait
            
            while (attempts < maxAttempts) {
                if (window.firebaseRTDB) {
                    this.rtdb = window.firebaseRTDB;
                    console.log('✅ Promo Code System - Firebase RTDB initialized');
                    break;
                }
                await new Promise(resolve => setTimeout(resolve, 100));
                attempts++;
            }
            
            if (!this.rtdb) {
                console.error('❌ Promo Code System - Firebase RTDB not available after 5 seconds');
            }
        } catch (error) {
            console.error('Error initializing Promo Code System Firebase:', error);
        }
    }

    // Lấy user hiện tại
    getCurrentUser() {
        if (window.firebaseAuth && typeof window.firebaseAuth.getCurrentUser === 'function') {
            const user = window.firebaseAuth.getCurrentUser();
            if (user) return user;
        }
        return null;
    }

    // Kiểm tra mã khuyến mãi có hợp lệ không
    async validatePromoCode(code) {
        try {
            // Wait for RTDB to be initialized
            if (!this.rtdb) {
                console.log('⏳ Waiting for Firebase RTDB...');
                await this.initializeFirebase();
                if (!this.rtdb) {
                    throw new Error('Firebase RTDB not initialized');
                }
            }

            const user = this.getCurrentUser();
            if (!user) {
                throw new Error('User not logged in');
            }

            const { ref, get } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            
            // Lấy thông tin mã khuyến mãi
            const promoRef = ref(this.rtdb, `PromoCodes/${code}`);
            const snapshot = await get(promoRef);
            
            if (!snapshot.exists()) {
                return {
                    valid: false,
                    message: 'Mã khuyến mãi không tồn tại'
                };
            }

            const promoData = snapshot.val();
            
            // Kiểm tra mã có active không
            if (!promoData.isActive) {
                return {
                    valid: false,
                    message: 'Mã khuyến mãi đã bị vô hiệu hóa'
                };
            }

            // Kiểm tra ngày hết hạn
            const now = new Date();
            const expiryDate = new Date(promoData.expiryDate);
            if (now > expiryDate) {
                return {
                    valid: false,
                    message: 'Mã khuyến mãi đã hết hạn'
                };
            }

            // Kiểm tra ngày bắt đầu
            const startDate = new Date(promoData.startDate);
            if (now < startDate) {
                return {
                    valid: false,
                    message: 'Mã khuyến mãi chưa có hiệu lực'
                };
            }

            // Kiểm tra giới hạn sử dụng
            if (promoData.usedCount >= promoData.usageLimit) {
                return {
                    valid: false,
                    message: 'Mã khuyến mãi đã hết lượt sử dụng'
                };
            }

            // Kiểm tra user đã sử dụng mã này chưa
            const userPromoRef = ref(this.rtdb, `UserPromoCodes/${user.uid}/${code}`);
            const userPromoSnapshot = await get(userPromoRef);
            
            if (userPromoSnapshot.exists()) {
                return {
                    valid: false,
                    message: 'Bạn đã sử dụng mã này rồi'
                };
            }

            return {
                valid: true,
                promoData: promoData,
                message: 'Mã khuyến mãi hợp lệ'
            };

        } catch (error) {
            console.error('Error validating promo code:', error);
            return {
                valid: false,
                message: 'Lỗi hệ thống khi kiểm tra mã'
            };
        }
    }

    // Sử dụng mã khuyến mãi
    async usePromoCode(code) {
        try {
            // Validate mã trước
            const validation = await this.validatePromoCode(code);
            if (!validation.valid) {
                return {
                    success: false,
                    message: validation.message
                };
            }

            const user = this.getCurrentUser();
            const promoData = validation.promoData;

            const { ref, get, set, update } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');

            // Lấy dữ liệu user hiện tại
            const userRef = ref(this.rtdb, `Users/${user.uid}`);
            const userSnapshot = await get(userRef);
            
            if (!userSnapshot.exists()) {
                return {
                    success: false,
                    message: 'Không tìm thấy dữ liệu người dùng'
                };
            }

            const rawUserData = userSnapshot.val();
            const userData = this.safeParseFirebaseData(rawUserData);

            // Cập nhật phần thưởng
            if (promoData.rewardType === 'diamond') {
                userData.Diamond = (userData.Diamond || 0) + promoData.rewardValue;
            } else if (promoData.rewardType === 'gold') {
                userData.Gold = (userData.Gold || 0) + promoData.rewardValue;
            }

            // Lưu dữ liệu user
            await set(userRef, userData);

            // Cập nhật số lần sử dụng mã
            const promoRef = ref(this.rtdb, `PromoCodes/${code}`);
            await update(promoRef, {
                usedCount: promoData.usedCount + 1
            });

            // Lưu lịch sử sử dụng mã của user
            const userPromoRef = ref(this.rtdb, `UserPromoCodes/${user.uid}/${code}`);
            await set(userPromoRef, {
                usedAt: new Date().toISOString(),
                rewardType: promoData.rewardType,
                rewardValue: promoData.rewardValue,
                promoDescription: promoData.description
            });

            return {
                success: true,
                message: `Nhận thành công ${promoData.rewardValue} ${promoData.rewardType === 'diamond' ? 'kim cương' : 'vàng'}!`,
                reward: {
                    type: promoData.rewardType,
                    value: promoData.rewardValue,
                    description: promoData.description
                },
                newBalance: {
                    diamond: userData.Diamond,
                    gold: userData.Gold
                }
            };

        } catch (error) {
            console.error('Error using promo code:', error);
            return {
                success: false,
                message: 'Lỗi hệ thống khi sử dụng mã'
            };
        }
    }

    // Lấy lịch sử sử dụng mã của user
    async getUserPromoHistory() {
        try {
            const user = this.getCurrentUser();
            if (!user) return [];

            const { ref, get } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            
            const userPromoRef = ref(this.rtdb, `UserPromoCodes/${user.uid}`);
            const snapshot = await get(userPromoRef);
            
            if (!snapshot.exists()) return [];
            
            const promos = snapshot.val();
            return Object.entries(promos).map(([code, data]) => ({
                code: code,
                ...data
            })).sort((a, b) => new Date(b.usedAt) - new Date(a.usedAt));

        } catch (error) {
            console.error('Error getting user promo history:', error);
            return [];
        }
    }

    // Lấy danh sách mã khuyến mãi có sẵn
    async getAvailablePromoCodes() {
        try {
            if (!this.rtdb) return [];

            const { ref, get } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            
            const promosRef = ref(this.rtdb, 'PromoCodes');
            const snapshot = await get(promosRef);
            
            if (!snapshot.exists()) return [];
            
            const promos = snapshot.val();
            const now = new Date();
            
            return Object.entries(promos)
                .map(([code, data]) => ({ code, ...data }))
                .filter(promo => {
                    const startDate = new Date(promo.startDate);
                    const expiryDate = new Date(promo.expiryDate);
                    return promo.isActive && 
                           now >= startDate && 
                           now <= expiryDate && 
                           promo.usedCount < promo.usageLimit;
                })
                .sort((a, b) => b.rewardValue - a.rewardValue);

        } catch (error) {
            console.error('Error getting available promo codes:', error);
            return [];
        }
    }

    // Utility function to safely parse Firebase data
    safeParseFirebaseData(data) {
        try {
            if (typeof data === 'string') {
                return JSON.parse(data);
            } else if (typeof data === 'object' && data !== null) {
                return data;
            } else {
                console.warn('Unexpected data type:', typeof data, data);
                return {};
            }
        } catch (error) {
            console.error('Error parsing Firebase data:', error);
            console.error('Data:', data);
            return {};
        }
    }
}

// Khởi tạo hệ thống
window.promoCodeSystem = new PromoCodeSystem();

// Export functions để sử dụng trong HTML
window.validatePromoCode = async function(code) {
    return await window.promoCodeSystem.validatePromoCode(code);
};

window.usePromoCode = async function(code) {
    return await window.promoCodeSystem.usePromoCode(code);
};

window.getUserPromoHistory = async function() {
    return await window.promoCodeSystem.getUserPromoHistory();
};

window.getAvailablePromoCodes = async function() {
    return await window.promoCodeSystem.getAvailablePromoCodes();
};

// Debug function for Promo Code System
window.checkPromoSystemStatus = function() {
    const status = {
        rtdb: !!window.promoCodeSystem.rtdb,
        auth: !!window.promoCodeSystem.auth,
        windowFirebaseRTDB: !!window.firebaseRTDB,
        windowFirebaseAuth: !!window.firebaseAuth,
        user: !!window.promoCodeSystem.getCurrentUser()
    };
    
    console.log('🎁 Promo Code System Status:', status);
    return status;
};

console.log('🎁 Promo Code System initialized!');
