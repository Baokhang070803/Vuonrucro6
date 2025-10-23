// Admin Configuration
// This file contains configuration settings for the admin panel

const adminConfig = {
    // Firebase configuration
    firebase: {
        apiKey: "AIzaSyDA2nXjMUZu7IcTmQIg3uNgJdb8PBWy618",
        authDomain: "trangtrai-2769b.firebaseapp.com",
        databaseURL: "https://trangtrai-2769b-default-rtdb.firebaseio.com",
        projectId: "trangtrai-2769b",
        storageBucket: "trangtrai-2769b.firebasestorage.app",
        messagingSenderId: "455581137012",
        appId: "1:455581137012:web:4f3c2a5c8fe7d1a4b3b921",
        measurementId: "G-3RWSR0ZC1G"
    },
    
    // Admin settings
    settings: {
        maxUsersPerPage: 20,
        maxItemsPerPage: 12,
        maxCodesPerPage: 9,
        autoRefreshInterval: 30000, // 30 seconds
        sessionTimeout: 3600000, // 1 hour
        enableNotifications: true,
        enableAutoSave: true
    },
    
    // UI settings
    ui: {
        theme: 'dark',
        language: 'vi',
        animations: true,
        soundEffects: false
    },
    
    // Security settings
    security: {
        requireStrongPasswords: true,
        enableTwoFactor: false,
        maxLoginAttempts: 5,
        lockoutDuration: 900000 // 15 minutes
    }
};

// Admin email checker function
export function isAdminEmail(email) {
    const adminEmails = [
        'admin@langhoaruc.com',
        'nguyentienchuc2023ct@gmail.com',
        // Add more admin emails as needed
    ];
    return adminEmails.includes(email?.toLowerCase());
}

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = adminConfig;
    module.exports.isAdminEmail = isAdminEmail;
} else {
    window.adminConfig = adminConfig;
    window.isAdminEmail = isAdminEmail;
}


