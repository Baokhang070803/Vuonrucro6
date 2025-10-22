// Firebase Configuration
const firebaseConfig = {
    apiKey: "AIzaSyBXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    authDomain: "trangtrai-2769b.firebaseapp.com",
    databaseURL: "https://trangtrai-2769b-default-rtdb.firebaseio.com",
    projectId: "trangtrai-2769b",
    storageBucket: "trangtrai-2769b.appspot.com",
    messagingSenderId: "123456789012",
    appId: "1:123456789012:web:abcdefghijklmnopqrstuvwxyz"
};

// Initialize Firebase with dynamic imports
async function initializeFirebase() {
    try {
        const { initializeApp } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-app.js');
        const { getAuth } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-auth.js');
        const { getDatabase } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js');

        const app = initializeApp(firebaseConfig);
        const auth = getAuth(app);
        const database = getDatabase(app);

        // Make Firebase available globally
        window.firebaseAuth = auth;
        window.firebaseRTDB = database;
        window.firebaseApp = app;

        console.log('🔥 Firebase initialized successfully!');
    } catch (error) {
        console.error('❌ Firebase initialization failed:', error);
    }
}

// Initialize Firebase
initializeFirebase();
