// Firebase Check-in System - Chỉ dùng Firebase, không dùng localStorage
// Tác giả: Làng Hoa Rực Team
// Ngày: 2025

class FirebaseCheckinSystem {
    constructor() {
        // Wait for Firebase to be initialized
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
                    console.log('✅ Firebase RTDB initialized');
                    break;
                }
                await new Promise(resolve => setTimeout(resolve, 100));
                attempts++;
            }
            
            if (!this.rtdb) {
                console.error('❌ Firebase RTDB not available after 5 seconds');
            }
        } catch (error) {
            console.error('Error initializing Firebase:', error);
        }
    }

    // Lấy user hiện tại
    getCurrentUser() {
        try {
            // Try multiple ways to get current user
            if (window.firebaseAuth && typeof window.firebaseAuth.getCurrentUser === 'function') {
                const user = window.firebaseAuth.getCurrentUser();
                if (user) return user;
            }
            
            // Try global Firebase auth instance
            if (window.firebaseAuth && window.firebaseAuth.auth && window.firebaseAuth.auth.currentUser) {
                return window.firebaseAuth.auth.currentUser;
            }
            
            // Try direct currentUser property
            if (window.firebaseAuth && window.firebaseAuth.currentUser) {
                return window.firebaseAuth.currentUser;
            }
            
            return null;
        } catch (error) {
            console.error('Error getting current user:', error);
            return null;
        }
    }

    // Kiểm tra đã điểm danh hôm nay chưa
    async hasCheckedInToday() {
        try {
            const user = this.getCurrentUser();
            if (!user) return false;

            // Check if RTDB is available
            if (!this.rtdb) {
                console.error('Firebase RTDB not initialized');
                return false;
            }

            const { ref, get } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            const today = new Date().toISOString().split('T')[0];
            
            const checkinRef = ref(this.rtdb, `CheckinHistory/${user.uid}/checkins`);
            const snapshot = await get(checkinRef);
            
            if (!snapshot.exists()) return false;
            
            const checkins = snapshot.val();
            return Object.keys(checkins).some(key => checkins[key].date === today);
        } catch (error) {
            console.error('Error checking today checkin:', error);
            return false;
        }
    }

    // Lấy thông tin điểm danh của user
    async getUserCheckinData() {
        try {
            const user = this.getCurrentUser();
            if (!user) return null;

            const { ref, get } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            
            // Lấy thông tin user
            const userRef = ref(this.rtdb, `Users/${user.uid}`);
            const userSnapshot = await get(userRef);
            
            if (!userSnapshot.exists()) return null;
            
            const rawData = userSnapshot.val();
            const userData = this.safeParseFirebaseData(rawData);
            
            // Lấy thống kê điểm danh
            const checkinStatsRef = ref(this.rtdb, `Users/${user.uid}/CheckinStats`);
            const checkinStatsSnapshot = await get(checkinStatsRef);
            
            const checkinStats = checkinStatsSnapshot.exists() ? checkinStatsSnapshot.val() : {
                totalCheckins: 0,
                totalDiamondsEarned: 0,
                currentStreak: 0,
                lastCheckinDate: null,
                longestStreak: 0
            };

            return {
                name: userData.Name,
                gold: userData.Gold || 0,
                diamond: userData.Diamond || 0,
                checkinStats: checkinStats
            };
        } catch (error) {
            console.error('Error getting user checkin data:', error);
            return null;
        }
    }

    // Thực hiện điểm danh
    async performCheckin() {
        try {
            const user = this.getCurrentUser();
            console.log('FirebaseCheckinSystem - Current user:', user);
            console.log('FirebaseCheckinSystem - RTDB object:', this.rtdb);
            console.log('FirebaseCheckinSystem - Window firebaseRTDB:', window.firebaseRTDB);
            
            if (!user) {
                throw new Error('User not logged in');
            }

            // Check if RTDB is available
            if (!this.rtdb) {
                throw new Error('Firebase RTDB not initialized');
            }

            // Kiểm tra đã điểm danh hôm nay chưa
            const alreadyCheckedIn = await this.hasCheckedInToday();
            if (alreadyCheckedIn) {
                throw new Error('Already checked in today');
            }

            const now = new Date();
            const today = now.toISOString().split('T')[0];
            const time = now.toTimeString().split(' ')[0];
            const dayOfMonth = now.getDate();
            const timestamp = now.toISOString();
            
            // Tính phần thưởng
            const reward = this.dailyRewards[dayOfMonth] || 10;
            const isSpecialDay = [7, 14, 21, 30].includes(dayOfMonth);

            // Tạo key cho lần điểm danh này
            const checkinKey = `${today}_${time.replace(/:/g, '-')}`;

            // Thông tin chi tiết lần điểm danh
            const checkinData = {
                timestamp: timestamp,
                date: today,
                time: time,
                dayOfMonth: dayOfMonth,
                diamondReward: reward,
                isSpecialDay: isSpecialDay,
                userAgent: navigator.userAgent,
                ipAddress: 'unknown', // Cần backend để lấy IP thật
                sessionId: this.generateSessionId(),
                deviceType: this.getDeviceType(),
                userId: user.uid,
                userName: user.displayName || user.email?.split('@')[0] || 'User'
            };

            const { ref, set, get, update } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');

            // 1. Lưu chi tiết lần điểm danh
            const checkinHistoryRef = ref(this.rtdb, `CheckinHistory/${user.uid}/checkins/${checkinKey}`);
            await set(checkinHistoryRef, checkinData);

            // 2. Cập nhật thống kê user
            const userRef = ref(this.rtdb, `Users/${user.uid}`);
            const userSnapshot = await get(userRef);
            
            const rawUserData = userSnapshot.val();
            const userData = this.safeParseFirebaseData(rawUserData);
            
            // Cập nhật kim cương
            userData.Diamond = (userData.Diamond || 0) + reward;
            
            // Cập nhật thống kê điểm danh
            const checkinStatsRef = ref(this.rtdb, `Users/${user.uid}/CheckinStats`);
            const checkinStatsSnapshot = await get(checkinStatsRef);
            const currentStats = checkinStatsSnapshot.exists() ? checkinStatsSnapshot.val() : {
                totalCheckins: 0,
                totalDiamondsEarned: 0,
                currentStreak: 0,
                lastCheckinDate: null,
                longestStreak: 0
            };

            // Tính streak
            const yesterday = new Date(now);
            yesterday.setDate(yesterday.getDate() - 1);
            const yesterdayStr = yesterday.toISOString().split('T')[0];
            
            let newStreak = 1;
            if (currentStats.lastCheckinDate === yesterdayStr) {
                newStreak = currentStats.currentStreak + 1;
            }

            const updatedStats = {
                totalCheckins: currentStats.totalCheckins + 1,
                totalDiamondsEarned: currentStats.totalDiamondsEarned + reward,
                currentStreak: newStreak,
                lastCheckinDate: today,
                longestStreak: Math.max(currentStats.longestStreak, newStreak)
            };

            // Lưu tất cả cập nhật
            // Save user data as object (not JSON string) for consistency
            await set(userRef, userData);
            await set(checkinStatsRef, updatedStats);

            // 3. Cập nhật thống kê tổng quan
            await this.updateAnalytics(today, reward);

            return {
                success: true,
                reward: reward,
                isSpecialDay: isSpecialDay,
                newDiamondTotal: userData.Diamond,
                streak: newStreak,
                checkinData: checkinData
            };

        } catch (error) {
            console.error('Error performing checkin:', error);
            return {
                success: false,
                error: error.message
            };
        }
    }

    // Cập nhật thống kê tổng quan
    async updateAnalytics(date, reward) {
        try {
            const { ref, get, set, update } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            
            const today = date;
            const month = date.substring(0, 7); // YYYY-MM

            // Cập nhật thống kê ngày
            const dailyStatsRef = ref(this.rtdb, `CheckinAnalytics/dailyStats/${today}`);
            const dailySnapshot = await get(dailyStatsRef);
            const dailyStats = dailySnapshot.exists() ? dailySnapshot.val() : {
                totalCheckins: 0,
                uniqueUsers: 0,
                totalDiamondsGiven: 0
            };

            dailyStats.totalCheckins += 1;
            dailyStats.totalDiamondsGiven += reward;
            // Note: uniqueUsers cần logic phức tạp hơn để đếm unique

            await set(dailyStatsRef, dailyStats);

            // Cập nhật thống kê tháng
            const monthlyStatsRef = ref(this.rtdb, `CheckinAnalytics/monthlyStats/${month}`);
            const monthlySnapshot = await get(monthlyStatsRef);
            const monthlyStats = monthlySnapshot.exists() ? monthlySnapshot.val() : {
                totalCheckins: 0,
                uniqueUsers: 0,
                totalDiamondsGiven: 0
            };

            monthlyStats.totalCheckins += 1;
            monthlyStats.totalDiamondsGiven += reward;

            await set(monthlyStatsRef, monthlyStats);

        } catch (error) {
            console.error('Error updating analytics:', error);
        }
    }

    // Lấy lịch sử điểm danh của user
    async getCheckinHistory(limit = 30) {
        try {
            const user = this.getCurrentUser();
            if (!user) return [];

            const { ref, get, query, orderByKey, limitToLast } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            
            const checkinHistoryRef = ref(this.rtdb, `CheckinHistory/${user.uid}/checkins`);
            const checkinQuery = query(checkinHistoryRef, orderByKey(), limitToLast(limit));
            const snapshot = await get(checkinQuery);
            
            if (!snapshot.exists()) return [];
            
            const checkins = snapshot.val();
            return Object.values(checkins).sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp));
        } catch (error) {
            console.error('Error getting checkin history:', error);
            return [];
        }
    }

    // Lấy thống kê điểm danh theo tháng
    async getMonthlyCheckins(year, month) {
        try {
            const user = this.getCurrentUser();
            if (!user) return [];

            const { ref, get, query, orderByChild, equalTo } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            
            const monthStr = `${year}-${String(month).padStart(2, '0')}`;
            const checkinHistoryRef = ref(this.rtdb, `CheckinHistory/${user.uid}/checkins`);
            const checkinQuery = query(checkinHistoryRef, orderByChild('date'), equalTo(monthStr));
            const snapshot = await get(checkinQuery);
            
            if (!snapshot.exists()) return [];
            
            return Object.values(snapshot.val());
        } catch (error) {
            console.error('Error getting monthly checkins:', error);
            return [];
        }
    }

    // Utility functions
    generateSessionId() {
        return 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    getDeviceType() {
        const userAgent = navigator.userAgent.toLowerCase();
        if (/mobile|android|iphone|ipad|tablet/.test(userAgent)) {
            return 'mobile';
        } else if (/tablet|ipad/.test(userAgent)) {
            return 'tablet';
        } else {
            return 'desktop';
        }
    }

    // Lấy thống kê admin (nếu cần)
    async getAdminAnalytics() {
        try {
            const { ref, get } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
            
            const analyticsRef = ref(this.rtdb, 'CheckinAnalytics');
            const snapshot = await get(analyticsRef);
            
            return snapshot.exists() ? snapshot.val() : null;
        } catch (error) {
            console.error('Error getting admin analytics:', error);
            return null;
        }
    }

    // Kiểm tra trạng thái Firebase
    checkFirebaseStatus() {
        const status = {
            rtdb: !!this.rtdb,
            auth: !!this.auth,
            windowFirebaseRTDB: !!window.firebaseRTDB,
            windowFirebaseAuth: !!window.firebaseAuth,
            user: !!this.getCurrentUser()
        };
        
        console.log('🔍 Firebase Status:', status);
        return status;
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
window.firebaseCheckinSystem = new FirebaseCheckinSystem();

// Export functions để sử dụng trong HTML
window.performCheckin = async function() {
    const result = await window.firebaseCheckinSystem.performCheckin();
    return result;
};

window.getUserCheckinData = async function() {
    return await window.firebaseCheckinSystem.getUserCheckinData();
};

window.getCheckinHistory = async function(limit = 30) {
    return await window.firebaseCheckinSystem.getCheckinHistory(limit);
};

window.hasCheckedInToday = async function() {
    return await window.firebaseCheckinSystem.hasCheckedInToday();
};

// Debug functions
window.checkFirebaseStatus = function() {
    return window.firebaseCheckinSystem.checkFirebaseStatus();
};

// Debug function to check user data
window.debugUserData = async function() {
    try {
        const user = window.firebaseCheckinSystem.getCurrentUser();
        if (!user) {
            console.log('❌ No user logged in');
            return;
        }

        const { ref, get } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');
        const userRef = ref(window.firebaseCheckinSystem.rtdb, `Users/${user.uid}`);
        const snapshot = await get(userRef);
        
        if (snapshot.exists()) {
            const rawData = snapshot.val();
            console.log('🔍 Raw user data:', rawData);
            console.log('🔍 Data type:', typeof rawData);
            console.log('🔍 Parsed data:', window.firebaseCheckinSystem.safeParseFirebaseData(rawData));
        } else {
            console.log('❌ No user data found');
        }
    } catch (error) {
        console.error('Error debugging user data:', error);
    }
};

console.log('🔥 Firebase Check-in System initialized!');
