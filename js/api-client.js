/**
 * API Client for Vườn Rực Rỡ Game
 * Handles communication with Flask backend
 */

class GameAPI {
    constructor() {
        this.baseURL = window.location.origin + '/api';
        this.token = localStorage.getItem('access_token');
    }

    // Set authentication token
    setToken(token) {
        this.token = token;
        localStorage.setItem('access_token', token);
    }

    // Clear authentication token
    clearToken() {
        this.token = null;
        localStorage.removeItem('access_token');
    }

    // Make API request
    async request(endpoint, options = {}) {
        const url = `${this.baseURL}${endpoint}`;
        const config = {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        };

        // Add authorization header if token exists
        if (this.token) {
            config.headers['Authorization'] = `Bearer ${this.token}`;
        }

        try {
            const response = await fetch(url, config);
            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.error || 'API request failed');
            }

            return data;
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }

    // Authentication methods
    async register(username, email, password) {
        return await this.request('/register', {
            method: 'POST',
            body: JSON.stringify({ username, email, password })
        });
    }

    async login(username, password) {
        const response = await this.request('/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });
        
        if (response.access_token) {
            this.setToken(response.access_token);
        }
        
        return response;
    }

    async logout() {
        this.clearToken();
        // Redirect to home page
        window.location.href = '/';
    }

    // User profile methods
    async getProfile() {
        return await this.request('/user/profile');
    }

    async updateProfile(data) {
        return await this.request('/user/profile', {
            method: 'PUT',
            body: JSON.stringify(data)
        });
    }

    // Game methods
    async saveGame(gameData) {
        return await this.request('/game/save', {
            method: 'POST',
            body: JSON.stringify(gameData)
        });
    }

    async loadGame() {
        return await this.request('/game/load');
    }

    // Analytics methods
    async trackEvent(eventType, eventData = {}) {
        return await this.request('/analytics', {
            method: 'POST',
            body: JSON.stringify({
                event_type: eventType,
                event_data: eventData,
                user_id: this.getCurrentUserId()
            })
        });
    }

    // Leaderboard methods
    async getLeaderboard() {
        return await this.request('/leaderboard');
    }

    // Story methods
    async getStory() {
        return await this.request('/story');
    }

    // Utility methods
    getCurrentUserId() {
        const token = this.token;
        if (!token) return null;
        
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload.sub;
        } catch (error) {
            return null;
        }
    }

    isAuthenticated() {
        return !!this.token;
    }
}

// Create global API instance
window.gameAPI = new GameAPI();

// Enhanced tracking function that integrates with backend
window.trackEvent = async function(eventType, eventData = {}) {
    try {
        // Track with backend API
        await window.gameAPI.trackEvent(eventType, eventData);
    } catch (error) {
        console.error('Failed to track event:', error);
    }
    
    // Also track with Google Analytics if available
    if (typeof gtag !== 'undefined') {
        gtag('event', eventType, eventData);
    }
};

// Auto-save game progress
window.autoSaveGame = async function(gameData) {
    if (window.gameAPI.isAuthenticated()) {
        try {
            await window.gameAPI.saveGame(gameData);
            console.log('Game progress saved successfully');
        } catch (error) {
            console.error('Failed to save game:', error);
        }
    }
};

// Load game progress on page load
window.loadGameProgress = async function() {
    if (window.gameAPI.isAuthenticated()) {
        try {
            const gameData = await window.gameAPI.loadGame();
            return gameData;
        } catch (error) {
            console.error('Failed to load game:', error);
            return null;
        }
    }
    return null;
};
