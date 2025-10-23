// ===================================
// Admin Panel JavaScript
// ===================================

let revenueChart = null;
let usersChart = null;

// Navigation
document.querySelectorAll('.menu-link').forEach(link => {
    link.addEventListener('click', (e) => {
        e.preventDefault();
        
        // Update active menu
        document.querySelectorAll('.menu-link').forEach(l => l.classList.remove('active'));
        link.classList.add('active');
        
        // Show section
        const sectionName = link.dataset.section;
        showSection(sectionName);
    });
});

function showSection(sectionName) {
    // Hide all sections
    document.querySelectorAll('.content-section').forEach(section => {
        section.classList.remove('active');
    });
    
    // Show target section
    const targetSection = document.getElementById(`section-${sectionName}`);
    if (targetSection) {
        targetSection.classList.add('active');
    }
    
    // Update header
    const titles = {
        'dashboard': ['Dashboard', 'Tổng quan hệ thống'],
        'users': ['Quản lý Users', 'Danh sách người dùng trong hệ thống'],
        'transactions': ['Giao dịch', 'Lịch sử giao dịch nạp tiền'],
        'promo-codes': ['Mã khuyến mãi', 'Quản lý mã khuyến mãi'],
        'settings': ['Cài đặt', 'Cài đặt hệ thống']
    };
    
    if (titles[sectionName]) {
        document.getElementById('page-title').textContent = titles[sectionName][0];
        document.getElementById('page-subtitle').textContent = titles[sectionName][1];
    }
    
    // Load data for section
    loadSectionData(sectionName);
}

function loadSectionData(sectionName) {
    switch(sectionName) {
        case 'users':
            loadUsers();
            break;
        case 'transactions':
            loadTransactions();
            break;
        case 'promo-codes':
            loadPromoCodes();
            break;
    }
}

// Load Dashboard Data
function loadDashboardData() {
    loadStats();
    loadRecentTransactions();
    initCharts();
}

// Load Stats
function loadStats() {
    if (!window.firebaseDB) return;
    
    const usersRef = window.firebaseRef(window.firebaseDB, 'Users');
    const transactionsRef = window.firebaseRef(window.firebaseDB, 'rechargeHistory');
    
    // Load users count
    window.firebaseGet(usersRef).then((snapshot) => {
        const users = snapshot.val();
        const totalUsers = users ? Object.keys(users).length : 0;
        document.getElementById('total-users').textContent = formatNumber(totalUsers);
        
        // Count active users (logged in last 7 days)
        const sevenDaysAgo = Date.now() - (7 * 24 * 60 * 60 * 1000);
        let activeCount = 0;
        if (users) {
            Object.values(users).forEach(user => {
                if (user.lastLogin && user.lastLogin > sevenDaysAgo) {
                    activeCount++;
                }
            });
        }
        document.getElementById('active-users').textContent = formatNumber(activeCount);
    });
    
    // Load transactions
    window.firebaseGet(transactionsRef).then((snapshot) => {
        const transactions = snapshot.val();
        if (!transactions) {
            document.getElementById('total-transactions').textContent = '0';
            document.getElementById('total-revenue').textContent = '0đ';
            return;
        }
        
        const transArray = Object.values(transactions);
        const totalTransactions = transArray.length;
        const totalRevenue = transArray.reduce((sum, t) => sum + (t.amount || 0), 0);
        
        document.getElementById('total-transactions').textContent = formatNumber(totalTransactions);
        document.getElementById('total-revenue').textContent = formatCurrency(totalRevenue);
    });
}

// Load Recent Transactions
function loadRecentTransactions() {
    const transactionsRef = window.firebaseRef(window.firebaseDB, 'rechargeHistory');
    
    window.firebaseGet(transactionsRef).then((snapshot) => {
        const transactions = snapshot.val();
        const container = document.getElementById('recent-transactions');
        
        if (!transactions) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-inbox"></i>
                    <h3>Chưa có giao dịch</h3>
                    <p>Chưa có giao dịch nào trong hệ thống</p>
                </div>
            `;
            return;
        }
        
        const transArray = Object.entries(transactions)
            .map(([id, data]) => ({ id, ...data }))
            .sort((a, b) => (b.timestamp || 0) - (a.timestamp || 0))
            .slice(0, 10);
        
        let html = `
            <table class="data-table">
                <thead>
                    <tr>
                        <th>User</th>
                        <th>Gói</th>
                        <th>Số tiền</th>
                        <th>Kim Cương</th>
                        <th>Phương thức</th>
                        <th>Trạng thái</th>
                        <th>Thời gian</th>
                    </tr>
                </thead>
                <tbody>
        `;
        
        transArray.forEach(trans => {
            html += `
                <tr>
                    <td>${trans.userEmail || 'N/A'}</td>
                    <td>Gói ${trans.packageId || 'N/A'}</td>
                    <td>${formatCurrency(trans.amount || 0)}</td>
                    <td><i class="fas fa-gem" style="color: #60a5fa;"></i> ${formatNumber(trans.diamonds || 0)}</td>
                    <td>${trans.paymentMethod || 'N/A'}</td>
                    <td><span class="badge badge-${trans.status === 'success' ? 'success' : 'warning'}">${trans.status || 'pending'}</span></td>
                    <td>${formatDateTime(trans.timestamp)}</td>
                </tr>
            `;
        });
        
        html += `
                </tbody>
            </table>
        `;
        
        container.innerHTML = html;
    });
}

// Load Users
function loadUsers() {
    const usersRef = window.firebaseRef(window.firebaseDB, 'Users');
    const container = document.getElementById('users-table');
    
    window.firebaseGet(usersRef).then((snapshot) => {
        const users = snapshot.val();
        
        if (!users) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-users"></i>
                    <h3>Chưa có người dùng</h3>
                    <p>Chưa có người dùng nào đăng ký</p>
                </div>
            `;
            return;
        }
        
        const usersArray = Object.entries(users).map(([id, data]) => ({ id, ...data }));
        
        let html = `
            <table class="data-table">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Email</th>
                        <th>Tên</th>
                        <th>Kim Cương</th>
                        <th>Vàng</th>
                        <th>Level</th>
                        <th>Check-in</th>
                        <th>Streak</th>
                        <th style="min-width: 180px;">Hành động</th>
                    </tr>
                </thead>
                <tbody>
        `;
        
        usersArray.forEach(user => {
            // Parse JSON data
            let expData = {};
            let checkinStats = {};
            let bagData = {};
            let questData = {};
            
            try {
                if (user.ExpData) {
                    expData = JSON.parse(user.ExpData);
                }
                if (user.CheckinStats) {
                    checkinStats = user.CheckinStats;
                }
                if (user.BagData) {
                    bagData = JSON.parse(user.BagData);
                }
                if (user.QuestData) {
                    questData = JSON.parse(user.QuestData);
                }
            } catch (e) {
                console.warn('Error parsing user data:', e);
            }
            
            html += `
                <tr>
                    <td><code>${user.id.substring(0, 8)}...</code></td>
                    <td>${user.email || user.Email || 'N/A'}</td>
                    <td>${user.Name || user.displayName || user.DisplayName || 'N/A'}</td>
                    <td><i class="fas fa-gem" style="color: #60a5fa;"></i> ${formatNumber(user.Diamond || 0)}</td>
                    <td><i class="fas fa-coins" style="color: #fbbf24;"></i> ${formatNumber(user.Gold || 0)}</td>
                    <td><span class="badge badge-info">Lv.${expData.currentLevel || 1}</span></td>
                    <td><i class="fas fa-calendar-check" style="color: #10b981;"></i> ${checkinStats.totalCheckins || 0}</td>
                    <td><span class="badge badge-success">${checkinStats.currentStreak || 0} ngày</span></td>
                    <td>
                        <div class="action-buttons">
                            <button class="btn btn-sm btn-info" onclick="viewUser('${user.id}')" title="Xem chi tiết">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-sm btn-success" onclick="addResources('${user.id}')" title="Thêm tài nguyên">
                                <i class="fas fa-plus-circle"></i>
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="deleteUser('${user.id}')" title="Xóa">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        });
        
        html += `
                </tbody>
            </table>
        `;
        
        container.innerHTML = html;
    });
}

// Load Transactions
function loadTransactions() {
    const transactionsRef = window.firebaseRef(window.firebaseDB, 'rechargeHistory');
    const container = document.getElementById('transactions-table');
    
    window.firebaseGet(transactionsRef).then((snapshot) => {
        const transactions = snapshot.val();
        
        if (!transactions) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-coins"></i>
                    <h3>Chưa có giao dịch</h3>
                    <p>Chưa có giao dịch nào trong hệ thống</p>
                </div>
            `;
            return;
        }
        
        const transArray = Object.entries(transactions)
            .map(([id, data]) => ({ id, ...data }))
            .sort((a, b) => (b.timestamp || 0) - (a.timestamp || 0));
        
        let html = `
            <table class="data-table">
                <thead>
                    <tr>
                        <th>Mã GD</th>
                        <th>User</th>
                        <th>Gói</th>
                        <th>Số tiền</th>
                        <th>Kim Cương</th>
                        <th>Phương thức</th>
                        <th>Trạng thái</th>
                        <th>Thời gian</th>
                        <th>Hành động</th>
                    </tr>
                </thead>
                <tbody>
        `;
        
        transArray.forEach(trans => {
            html += `
                <tr>
                    <td><code>${trans.txnRef || 'N/A'}</code></td>
                    <td>${trans.userEmail || 'N/A'}</td>
                    <td>Gói ${trans.packageId || 'N/A'}</td>
                    <td>${formatCurrency(trans.amount || 0)}</td>
                    <td><i class="fas fa-gem" style="color: #60a5fa;"></i> ${formatNumber(trans.diamonds || 0)}</td>
                    <td>${trans.paymentMethod || 'N/A'}</td>
                    <td><span class="badge badge-${trans.status === 'success' ? 'success' : 'warning'}">${trans.status || 'pending'}</span></td>
                    <td>${formatDateTime(trans.timestamp)}</td>
                    <td>
                        <div class="action-buttons">
                            <button class="btn btn-sm btn-info" onclick="viewTransaction('${trans.id}')">
                                <i class="fas fa-eye"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        });
        
        html += `
                </tbody>
            </table>
        `;
        
        container.innerHTML = html;
    });
}

// Load Promo Codes
function loadPromoCodes() {
    const promoRef = window.firebaseRef(window.firebaseDB, 'promoCodes');
    const container = document.getElementById('promo-codes-table');
    
    window.firebaseGet(promoRef).then((snapshot) => {
        const promoCodes = snapshot.val();
        
        if (!promoCodes) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-gift"></i>
                    <h3>Chưa có mã khuyến mãi</h3>
                    <p>Chưa có mã khuyến mãi nào trong hệ thống</p>
                    <button class="btn btn-primary" onclick="createPromoCode()">
                        <i class="fas fa-plus"></i>
                        Tạo mã đầu tiên
                    </button>
                </div>
            `;
            return;
        }
        
        const promoArray = Object.entries(promoCodes).map(([id, data]) => ({ id, ...data }));
        
        let html = `
            <table class="data-table">
                <thead>
                    <tr>
                        <th>Mã Code</th>
                        <th>Mô tả</th>
                        <th>Kim Cương</th>
                        <th>Số lượt dùng</th>
                        <th>Trạng thái</th>
                        <th>Hành động</th>
                    </tr>
                </thead>
                <tbody>
        `;
        
        promoArray.forEach(promo => {
            html += `
                <tr>
                    <td><code><strong>${promo.code || 'N/A'}</strong></code></td>
                    <td>${promo.description || 'Không có mô tả'}</td>
                    <td><i class="fas fa-gem" style="color: #60a5fa;"></i> ${formatNumber(promo.diamonds || 0)}</td>
                    <td>${promo.usedCount || 0} / ${promo.maxUses || '∞'}</td>
                    <td><span class="badge badge-${promo.active ? 'success' : 'danger'}">${promo.active ? 'Hoạt động' : 'Tắt'}</span></td>
                    <td>
                        <div class="action-buttons">
                            <button class="btn btn-sm btn-warning" onclick="editPromoCode('${promo.id}')">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="deletePromoCode('${promo.id}')">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        });
        
        html += `
                </tbody>
            </table>
        `;
        
        container.innerHTML = html;
    });
}

// Initialize Charts
function initCharts() {
    // Revenue Chart
    const revenueCtx = document.getElementById('revenueChart');
    if (revenueCtx) {
        revenueChart = new Chart(revenueCtx, {
            type: 'line',
            data: {
                labels: ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'],
                datasets: [{
                    label: 'Doanh thu (VNĐ)',
                    data: [120000, 190000, 300000, 250000, 420000, 350000, 500000],
                    borderColor: '#667eea',
                    backgroundColor: 'rgba(102, 126, 234, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }
    
    // Users Chart
    const usersCtx = document.getElementById('usersChart');
    if (usersCtx) {
        usersChart = new Chart(usersCtx, {
            type: 'bar',
            data: {
                labels: ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'],
                datasets: [{
                    label: 'Người dùng mới',
                    data: [12, 19, 15, 25, 22, 18, 30],
                    backgroundColor: '#10b981',
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }
}

// Create Promo Code
function createPromoCode() {
    Swal.fire({
        title: 'Tạo mã khuyến mãi mới',
        html: `
            <div style="text-align: left;">
                <div class="form-group">
                    <label class="form-label">Mã Code</label>
                    <input type="text" id="promo-code" class="form-control" placeholder="VD: LANGHOARUC888">
                </div>
                <div class="form-group">
                    <label class="form-label">Mô tả</label>
                    <input type="text" id="promo-desc" class="form-control" placeholder="Mô tả mã">
                </div>
                <div class="form-group">
                    <label class="form-label">Số Kim Cương</label>
                    <input type="number" id="promo-diamonds" class="form-control" placeholder="1000">
                </div>
                <div class="form-group">
                    <label class="form-label">Số lượt sử dụng tối đa (0 = không giới hạn)</label>
                    <input type="number" id="promo-max-uses" class="form-control" value="0">
                </div>
            </div>
        `,
        showCancelButton: true,
        confirmButtonText: 'Tạo mã',
        cancelButtonText: 'Hủy',
        confirmButtonColor: '#667eea',
        preConfirm: () => {
            const code = document.getElementById('promo-code').value.trim().toUpperCase();
            const description = document.getElementById('promo-desc').value.trim();
            const diamonds = parseInt(document.getElementById('promo-diamonds').value) || 0;
            const maxUses = parseInt(document.getElementById('promo-max-uses').value) || 0;
            
            if (!code) {
                Swal.showValidationMessage('Vui lòng nhập mã code');
                return false;
            }
            
            if (diamonds <= 0) {
                Swal.showValidationMessage('Số Kim Cương phải lớn hơn 0');
                return false;
            }
            
            return { code, description, diamonds, maxUses };
        }
    }).then((result) => {
        if (result.isConfirmed) {
            const { code, description, diamonds, maxUses } = result.value;
            const promoRef = window.firebaseRef(window.firebaseDB, 'promoCodes');
            const newPromoRef = window.firebasePush(promoRef);
            
            window.firebaseSet(newPromoRef, {
                code: code,
                description: description,
                diamonds: diamonds,
                maxUses: maxUses,
                usedCount: 0,
                active: true,
                createdAt: Date.now()
            }).then(() => {
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công!',
                    text: 'Đã tạo mã khuyến mãi mới',
                    confirmButtonColor: '#667eea'
                });
                loadPromoCodes();
            });
        }
    });
}

// View User
function viewUser(userId) {
    const userRef = window.firebaseRef(window.firebaseDB, 'Users/' + userId);
    window.firebaseGet(userRef).then((snapshot) => {
        const user = snapshot.val();
        if (!user) {
        Swal.fire({
                icon: 'error',
                title: 'Không tìm thấy',
                text: 'Người dùng không tồn tại',
                confirmButtonColor: '#667eea'
            });
            return;
        }
        
        // Parse game data
        let expData = null, bagData = null, bagUpgradeData = null, questData = null, checkinStats = null;
        try {
            expData = user.ExpData ? JSON.parse(user.ExpData) : null;
        } catch(e) {}
        try {
            bagData = user.BagData ? JSON.parse(user.BagData) : null;
        } catch(e) {}
        try {
            bagUpgradeData = user.BagUpgradeData ? JSON.parse(user.BagUpgradeData) : null;
        } catch(e) {}
        try {
            questData = user.QuestData ? JSON.parse(user.QuestData) : null;
        } catch(e) {}
        try {
            checkinStats = user.CheckinStats || null;
        } catch(e) {}
        
        // Build items HTML
        let itemsHTML = '';
        if (bagData && bagData.items && bagData.items.length > 0) {
            itemsHTML = bagData.items.map(item => `
                <div style="background: white; padding: 8px; border-radius: 5px; margin: 5px 0; border-left: 3px solid #10b981;">
                    <strong>${item.itemName}</strong>: ${item.quantity} cái 
                    <span style="color: #fbbf24;">(${item.sellPrice} vàng)</span>
                </div>
            `).join('');
        } else {
            itemsHTML = '<p style="color: #64748b; font-style: italic;">Túi trống</p>';
        }
        
        // Build quests HTML
        let questsHTML = '';
        if (questData && questData.questList && questData.questList.length > 0) {
            questsHTML = questData.questList.map((quest, index) => `
                <div style="background: white; padding: 10px; border-radius: 5px; margin: 5px 0; border-left: 3px solid ${quest.isCompleted ? '#10b981' : '#f59e0b'};">
                    <div style="display: flex; align-items: center; gap: 8px;">
                        <span style="font-size: 1.2em;">${quest.isCompleted ? '✅' : '⏳'}</span>
                        <div style="flex: 1;">
                            <strong>${quest.title}</strong>
                            <p style="font-size: 0.85em; color: #64748b; margin: 5px 0 0 0;">${quest.description}</p>
                        </div>
                    </div>
                </div>
            `).join('');
        } else {
            questsHTML = '<p style="color: #64748b; font-style: italic;">Chưa có nhiệm vụ</p>';
        }
        
        Swal.fire({
            title: '<i class="fas fa-user-circle"></i> Thông tin chi tiết',
            html: `
                <div style="text-align: left; max-height: 600px; overflow-y: auto;">
                    <!-- Thông tin cơ bản -->
                    <div style="background: #f8fafc; padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #667eea; margin-bottom: 10px;">
                            <i class="fas fa-id-card"></i> Thông tin cơ bản
                        </h4>
                        <p><strong><i class="fas fa-id-badge"></i> User ID:</strong> <code>${userId}</code></p>
                        <p><strong><i class="fas fa-envelope"></i> Email:</strong> ${user.email || user.Email || 'N/A'}</p>
                        <p><strong><i class="fas fa-user"></i> Tên hiển thị:</strong> ${user.displayName || user.DisplayName || user.Name || 'N/A'}</p>
                        <p><strong><i class="fas fa-gamepad"></i> Tên trong game:</strong> 
                            <span style="color: #667eea; font-weight: 600;">${user.Name || 'N/A'}</span>
                        </p>
                        ${(() => {
                            const createdAt = formatDateTime(user.createdAt || user.CreatedAt);
                            return createdAt ? `<p><strong><i class="fas fa-calendar-plus"></i> Ngày tạo:</strong> ${createdAt}</p>` : '';
                        })()}
                        ${(() => {
                            const lastLogin = formatDateTime(user.lastLogin || user.LastLogin);
                            return lastLogin ? `<p><strong><i class="fas fa-clock"></i> Đăng nhập cuối:</strong> ${lastLogin}</p>` : '';
                        })()}
                    </div>
                    
                    <!-- Tài nguyên -->
                    <div style="background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%); padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #10b981; margin-bottom: 10px;">
                            <i class="fas fa-wallet"></i> Tài nguyên
                        </h4>
                        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px;">
                            <div style="background: white; padding: 10px; border-radius: 8px; text-align: center;">
                                <i class="fas fa-gem" style="color: #60a5fa; font-size: 1.5em;"></i>
                                <div style="font-size: 1.5em; color: #60a5fa; font-weight: 700; margin: 5px 0;">
                                    ${formatNumber(user.Diamond || 0)}
                                </div>
                                <small style="color: #64748b;">Kim Cương</small>
                            </div>
                            <div style="background: white; padding: 10px; border-radius: 8px; text-align: center;">
                                <i class="fas fa-coins" style="color: #fbbf24; font-size: 1.5em;"></i>
                                <div style="font-size: 1.5em; color: #fbbf24; font-weight: 700; margin: 5px 0;">
                                    ${formatNumber(user.Gold || 0)}
                                </div>
                                <small style="color: #64748b;">Vàng</small>
                            </div>
                        </div>
                    </div>
                    
                    <!-- Check-in Stats -->
                    ${checkinStats ? `
                    <div style="background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%); padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #0ea5e9; margin-bottom: 10px;">
                            <i class="fas fa-calendar-check"></i> Thống kê Check-in
                        </h4>
                        <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 10px;">
                            <div style="background: white; padding: 10px; border-radius: 8px; text-align: center;">
                                <i class="fas fa-calendar-check" style="color: #0ea5e9; font-size: 1.5em;"></i>
                                <div style="font-size: 1.5em; color: #0ea5e9; font-weight: 700; margin: 5px 0;">
                                    ${checkinStats.totalCheckins || 0}
                                </div>
                                <small style="color: #64748b;">Tổng check-in</small>
                            </div>
                            <div style="background: white; padding: 10px; border-radius: 8px; text-align: center;">
                                <i class="fas fa-fire" style="color: #f59e0b; font-size: 1.5em;"></i>
                                <div style="font-size: 1.5em; color: #f59e0b; font-weight: 700; margin: 5px 0;">
                                    ${checkinStats.currentStreak || 0}
                                </div>
                                <small style="color: #64748b;">Streak hiện tại</small>
                            </div>
                            <div style="background: white; padding: 10px; border-radius: 8px; text-align: center;">
                                <i class="fas fa-trophy" style="color: #10b981; font-size: 1.5em;"></i>
                                <div style="font-size: 1.5em; color: #10b981; font-weight: 700; margin: 5px 0;">
                                    ${checkinStats.longestStreak || 0}
                                </div>
                                <small style="color: #64748b;">Streak dài nhất</small>
                            </div>
                            <div style="background: white; padding: 10px; border-radius: 8px; text-align: center;">
                                <i class="fas fa-gem" style="color: #60a5fa; font-size: 1.5em;"></i>
                                <div style="font-size: 1.5em; color: #60a5fa; font-weight: 700; margin: 5px 0;">
                                    ${formatNumber(checkinStats.totalDiamondsEarned || 0)}
                                </div>
                                <small style="color: #64748b;">Kim cương nhận</small>
                            </div>
                        </div>
                        <div style="margin-top: 10px; padding: 10px; background: white; border-radius: 8px;">
                            <p><strong><i class="fas fa-clock"></i> Check-in cuối:</strong> ${checkinStats.lastCheckinDate || 'Chưa có'}</p>
                            <p><strong><i class="fas fa-calendar-alt"></i> Ngày check-in:</strong> ${checkinStats.checkinDates ? checkinStats.checkinDates.join(', ') : 'Chưa có'}</p>
                        </div>
                    </div>
                    ` : ''}
                    
                    <!-- Kinh nghiệm & Level -->
                    ${expData ? `
                    <div style="background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%); padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #3b82f6; margin-bottom: 10px;">
                            <i class="fas fa-star"></i> Kinh nghiệm & Level
                        </h4>
                        <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 10px;">
                            <p><strong><i class="fas fa-level-up-alt"></i> Level:</strong> <span style="color: #3b82f6; font-size: 1.2em; font-weight: 700;">${expData.currentLevel || 1}</span></p>
                            <p><strong><i class="fas fa-chart-line"></i> EXP:</strong> ${expData.currentExp || 0}/${expData.expToNextLevel || 0}</p>
                            <p><strong><i class="fas fa-trophy"></i> Tổng EXP:</strong> ${formatNumber(expData.totalExpEarned || 0)}</p>
                            <p><strong><i class="fas fa-dice-d20"></i> Điểm kỹ năng:</strong> <span style="color: #10b981; font-weight: 600;">${expData.statPoints || 0}</span></p>
                        </div>
                        <div style="background: white; padding: 8px; border-radius: 5px; margin-top: 10px;">
                            <div style="background: #e0e7ff; height: 20px; border-radius: 10px; overflow: hidden;">
                                <div style="background: linear-gradient(90deg, #3b82f6, #8b5cf6); height: 100%; width: ${((expData.currentExp / expData.expToNextLevel) * 100).toFixed(1)}%; transition: width 0.3s;"></div>
                            </div>
                            <small style="color: #64748b; display: block; text-align: center; margin-top: 5px;">
                                ${((expData.currentExp / expData.expToNextLevel) * 100).toFixed(1)}% đến level tiếp theo
                            </small>
                        </div>
                    </div>
                    ` : ''}
                    
                    <!-- Chỉ số nhân vật -->
                    ${user.stats ? `
                    <div style="background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%); padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #f59e0b; margin-bottom: 10px;">
                            <i class="fas fa-fist-raised"></i> Chỉ số nhân vật
                        </h4>
                        <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 8px;">
                            <div style="background: white; padding: 8px; border-radius: 5px;">
                                <strong><i class="fas fa-bolt" style="color: #f59e0b;"></i> Agility:</strong> ${user.stats.agility || 0}
                            </div>
                            <div style="background: white; padding: 8px; border-radius: 5px;">
                                <strong><i class="fas fa-dumbbell" style="color: #ef4444;"></i> Strength:</strong> ${user.stats.strength || 0}
                            </div>
                            <div style="background: white; padding: 8px; border-radius: 5px;">
                                <strong><i class="fas fa-brain" style="color: #8b5cf6;"></i> Intelligence:</strong> ${user.stats.intelligence || 0}
                            </div>
                            <div style="background: white; padding: 8px; border-radius: 5px;">
                                <strong><i class="fas fa-heart" style="color: #ec4899;"></i> Vitality:</strong> ${user.stats.vitality || 0}
                            </div>
                            <div style="background: white; padding: 8px; border-radius: 5px;">
                                <strong><i class="fas fa-running" style="color: #10b981;"></i> Speed:</strong> x${(user.stats.speedMultiplier || 1).toFixed(2)}
                            </div>
                            <div style="background: white; padding: 8px; border-radius: 5px;">
                                <strong><i class="fas fa-fire" style="color: #f59e0b;"></i> Damage:</strong> x${(user.stats.damageMultiplier || 1).toFixed(2)}
                            </div>
                        </div>
                    </div>
                    ` : ''}
                    
                    <!-- Túi đồ -->
                    <div style="background: #fdf4ff; padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #a855f7; margin-bottom: 10px;">
                            <i class="fas fa-shopping-bag"></i> Túi đồ
                            ${bagUpgradeData ? `<span style="font-size: 0.8em; color: #64748b;">(Level ${bagUpgradeData.bagLevel || 1} - ${bagData?.items?.length || 0}/${bagUpgradeData.bagCapacity || 20})</span>` : ''}
                        </h4>
                        <div style="max-height: 200px; overflow-y: auto;">
                            ${itemsHTML}
                        </div>
                    </div>
                    
                    <!-- Nhiệm vụ -->
                    ${questData ? `
                    <div style="background: #ecfdf5; padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #10b981; margin-bottom: 10px;">
                            <i class="fas fa-tasks"></i> Nhiệm vụ
                            <span style="font-size: 0.8em; color: #64748b;">
                                (Đang làm: ${questData.currentQuestIndex + 1}/${questData.questList?.length || 0})
                            </span>
                        </h4>
                        <div style="max-height: 250px; overflow-y: auto;">
                            ${questsHTML}
                        </div>
                    </div>
                    ` : ''}
                    
                    <!-- User ID -->
                    <div style="background: #f1f5f9; padding: 10px; border-radius: 10px; text-align: center;">
                        <small style="color: #64748b;"><i class="fas fa-fingerprint"></i> User ID:</small>
                        <code style="display: block; background: white; padding: 8px; border-radius: 5px; margin-top: 5px; word-break: break-all; font-size: 0.85em;">${userId}</code>
                    </div>
                </div>
            `,
            width: '700px',
            confirmButtonColor: '#667eea',
            confirmButtonText: '<i class="fas fa-check"></i> Đóng'
        });
    });
}


// Delete Promo Code
function deletePromoCode(promoId) {
    Swal.fire({
        title: 'Xác nhận xóa?',
        text: 'Bạn có chắc muốn xóa mã khuyến mãi này?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Xóa',
        cancelButtonText: 'Hủy',
        confirmButtonColor: '#ef4444'
    }).then((result) => {
        if (result.isConfirmed) {
            const promoRef = window.firebaseRef(window.firebaseDB, 'promoCodes/' + promoId);
            window.firebaseRemove(promoRef).then(() => {
                Swal.fire({
                    icon: 'success',
                    title: 'Đã xóa!',
                    text: 'Mã khuyến mãi đã được xóa',
                    confirmButtonColor: '#667eea'
                });
                loadPromoCodes();
            });
        }
    });
}

// Utility Functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount) + 'đ';
}

function formatNumber(number) {
    return new Intl.NumberFormat('vi-VN').format(number);
}

function formatDateTime(timestamp) {
    if (!timestamp) return null;
    const date = new Date(timestamp);
    if (isNaN(date.getTime())) return null;
    return date.toLocaleString('vi-VN');
}

function refreshData() {
    Swal.fire({
        title: 'Đang làm mới...',
        didOpen: () => {
            Swal.showLoading();
        },
        timer: 1000
    });
    
    setTimeout(() => {
        loadDashboardData();
        Swal.close();
    }, 1000);
}

function logout() {
    Swal.fire({
        title: 'Đăng xuất?',
        text: 'Bạn có chắc muốn đăng xuất?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Đăng xuất',
        cancelButtonText: 'Hủy',
        confirmButtonColor: '#ef4444'
    }).then((result) => {
        if (result.isConfirmed) {
            window.firebaseAuth.signOut().then(() => {
                window.location.href = 'index.html';
            });
        }
    });
}

function exportUsers() {
    Swal.fire({
        icon: 'info',
        title: 'Chức năng đang phát triển',
        text: 'Tính năng xuất Excel đang được phát triển',
        confirmButtonColor: '#667eea'
    });
}

function exportTransactions() {
    Swal.fire({
        icon: 'info',
        title: 'Chức năng đang phát triển',
        text: 'Tính năng xuất Excel đang được phát triển',
        confirmButtonColor: '#667eea'
    });
}

function viewTransaction(transId) {
    const transRef = window.firebaseRef(window.firebaseDB, 'rechargeHistory/' + transId);
    window.firebaseGet(transRef).then((snapshot) => {
        const trans = snapshot.val();
        Swal.fire({
            title: 'Chi tiết giao dịch',
            html: `
                <div style="text-align: left;">
                    <p><strong>Mã GD:</strong> ${trans.txnRef || 'N/A'}</p>
                    <p><strong>User:</strong> ${trans.userEmail || 'N/A'}</p>
                    <p><strong>Gói:</strong> Gói ${trans.packageId || 'N/A'}</p>
                    <p><strong>Số tiền:</strong> ${formatCurrency(trans.amount || 0)}</p>
                    <p><strong>Kim Cương:</strong> ${formatNumber(trans.diamonds || 0)}</p>
                    <p><strong>Phương thức:</strong> ${trans.paymentMethod || 'N/A'}</p>
                    <p><strong>Trạng thái:</strong> ${trans.status || 'N/A'}</p>
                    <p><strong>Thời gian:</strong> ${formatDateTime(trans.timestamp)}</p>
                </div>
            `,
            confirmButtonColor: '#667eea'
        });
    });
}

function editPromoCode(promoId) {
    const promoRef = window.firebaseRef(window.firebaseDB, 'promoCodes/' + promoId);
    window.firebaseGet(promoRef).then((snapshot) => {
        const promo = snapshot.val();
        Swal.fire({
            title: 'Chỉnh sửa mã khuyến mãi',
            html: `
                <div style="text-align: left;">
                    <div class="form-group">
                        <label class="form-label">Trạng thái</label>
                        <select id="edit-promo-active" class="form-control">
                            <option value="true" ${promo.active ? 'selected' : ''}>Hoạt động</option>
                            <option value="false" ${!promo.active ? 'selected' : ''}>Tắt</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Số lượt sử dụng tối đa</label>
                        <input type="number" id="edit-promo-max-uses" class="form-control" value="${promo.maxUses || 0}">
                    </div>
                </div>
            `,
            showCancelButton: true,
            confirmButtonText: 'Cập nhật',
            cancelButtonText: 'Hủy',
            confirmButtonColor: '#667eea',
            preConfirm: () => {
                const active = document.getElementById('edit-promo-active').value === 'true';
                const maxUses = parseInt(document.getElementById('edit-promo-max-uses').value) || 0;
                return { active, maxUses };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                window.firebaseSet(promoRef, {
                    ...promo,
                    active: result.value.active,
                    maxUses: result.value.maxUses
                }).then(() => {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công!',
                        text: 'Đã cập nhật mã khuyến mãi',
                        confirmButtonColor: '#667eea'
                    });
                    loadPromoCodes();
                });
            }
        });
    });
}

// Add Resources to User
function addResources(userId) {
    const userRef = window.firebaseRef(window.firebaseDB, 'Users/' + userId);
    window.firebaseGet(userRef).then((snapshot) => {
        const user = snapshot.val();
        if (!user) {
            Swal.fire({
                icon: 'error',
                title: 'Không tìm thấy',
                text: 'Người dùng không tồn tại',
                confirmButtonColor: '#667eea'
            });
            return;
        }
        
        // Parse ExpData để hiển thị thông tin hiện tại
        let expData = {};
        let currentLevel = 1;
        let currentExp = 0;
        let expToNextLevel = 100;
        let statPoints = 0;
        
        try {
            if (user.ExpData) {
                expData = JSON.parse(user.ExpData);
                currentLevel = expData.currentLevel || 1;
                currentExp = expData.currentExp || 0;
                expToNextLevel = expData.expToNextLevel || 100;
                statPoints = expData.statPoints || 0;
            }
        } catch (e) {
            console.warn('Error parsing ExpData:', e);
        }

        Swal.fire({
            title: '<i class="fas fa-gift"></i> Thêm tài nguyên & Kinh nghiệm',
            html: `
                <div style="text-align: left;">
                    <div style="background: #f0fdf4; padding: 15px; border-radius: 10px; margin-bottom: 20px;">
                        <h4 style="color: #10b981; margin-bottom: 10px;">
                            <i class="fas fa-wallet"></i> Số dư hiện tại
                        </h4>
                        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px;">
                        <p><strong><i class="fas fa-gem" style="color: #60a5fa;"></i> Kim Cương:</strong> 
                            <span style="font-size: 1.2em; color: #60a5fa;">${formatNumber(user.Diamond || 0)}</span>
                        </p>
                        <p><strong><i class="fas fa-coins" style="color: #fbbf24;"></i> Vàng:</strong> 
                            <span style="font-size: 1.2em; color: #fbbf24;">${formatNumber(user.Gold || 0)}</span>
                        </p>
                            <p><strong><i class="fas fa-star" style="color: #3b82f6;"></i> Level:</strong> 
                                <span style="font-size: 1.2em; color: #3b82f6;">${currentLevel}</span>
                            </p>
                            <p><strong><i class="fas fa-dice-d20" style="color: #10b981;"></i> Điểm kỹ năng:</strong> 
                                <span style="font-size: 1.2em; color: #10b981;">${statPoints}</span>
                            </p>
                        </div>
                        <div style="background: white; padding: 8px; border-radius: 5px; margin-top: 10px;">
                            <p><strong><i class="fas fa-chart-line"></i> EXP:</strong> ${currentExp}/${expToNextLevel} (${((currentExp/expToNextLevel)*100).toFixed(1)}%)</p>
                        </div>
                    </div>
                    
                    <div style="background: #fef3c7; padding: 15px; border-radius: 10px; margin-bottom: 20px;">
                        <h4 style="color: #f59e0b; margin-bottom: 10px;">
                            <i class="fas fa-gift"></i> Tài nguyên
                        </h4>
                        <div class="form-group">
                            <label class="form-label"><i class="fas fa-gem" style="color: #60a5fa;"></i> Cộng thêm Kim Cương</label>
                            <input type="number" id="add-diamonds" class="form-control" placeholder="0" min="0" value="0">
                            <small style="color: #64748b;">Nhập số kim cương muốn cộng thêm</small>
                        </div>
                        
                        <div class="form-group">
                            <label class="form-label"><i class="fas fa-coins" style="color: #fbbf24;"></i> Cộng thêm Vàng</label>
                            <input type="number" id="add-gold" class="form-control" placeholder="0" min="0" value="0">
                            <small style="color: #64748b;">Nhập số vàng muốn cộng thêm</small>
                        </div>
                    </div>
                    
                    <div style="background: #eff6ff; padding: 15px; border-radius: 10px; margin-bottom: 20px;">
                        <h4 style="color: #3b82f6; margin-bottom: 10px;">
                            <i class="fas fa-star"></i> Kinh nghiệm & Kỹ năng
                        </h4>
                        <div class="form-group">
                            <label class="form-label"><i class="fas fa-chart-line" style="color: #3b82f6;"></i> Cộng thêm EXP</label>
                            <input type="number" id="add-exp" class="form-control" placeholder="0" min="0" value="0">
                            <small style="color: #64748b;">Nhập số EXP muốn cộng thêm</small>
                        </div>
                        
                        <div class="form-group">
                            <label class="form-label"><i class="fas fa-dice-d20" style="color: #10b981;"></i> Cộng thêm Điểm kỹ năng</label>
                            <input type="number" id="add-stat-points" class="form-control" placeholder="0" min="0" value="0">
                            <small style="color: #64748b;">Nhập số điểm kỹ năng muốn cộng thêm</small>
                        </div>
                    </div>
                    
                    <div class="form-group">
                        <label class="form-label"><i class="fas fa-comment"></i> Lý do (tùy chọn)</label>
                        <input type="text" id="add-reason" class="form-control" placeholder="VD: Quà tặng từ admin, Sự kiện đặc biệt">
                    </div>
                </div>
            `,
            width: '600px',
            showCancelButton: true,
            confirmButtonText: '<i class="fas fa-check"></i> Xác nhận cộng',
            cancelButtonText: '<i class="fas fa-times"></i> Hủy',
            confirmButtonColor: '#10b981',
            preConfirm: () => {
                const addDiamonds = parseInt(document.getElementById('add-diamonds').value) || 0;
                const addGold = parseInt(document.getElementById('add-gold').value) || 0;
                const addExp = parseInt(document.getElementById('add-exp').value) || 0;
                const addStatPoints = parseInt(document.getElementById('add-stat-points').value) || 0;
                const reason = document.getElementById('add-reason').value.trim();
                
                if (addDiamonds < 0 || addGold < 0 || addExp < 0 || addStatPoints < 0) {
                    Swal.showValidationMessage('Số lượng không được âm');
                    return false;
                }
                
                if (addDiamonds === 0 && addGold === 0 && addExp === 0 && addStatPoints === 0) {
                    Swal.showValidationMessage('Vui lòng nhập ít nhất một loại tài nguyên hoặc kinh nghiệm');
                    return false;
                }
                
                return { addDiamonds, addGold, addExp, addStatPoints, reason };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                const { addDiamonds, addGold, addExp, addStatPoints, reason } = result.value;
                const currentDiamonds = user.Diamond || 0;
                const currentGold = user.Gold || 0;
                const newDiamonds = currentDiamonds + addDiamonds;
                const newGold = currentGold + addGold;
                
                // Xử lý EXP và Level
                let newExpData = { ...expData };
                if (addExp > 0) {
                    newExpData.currentExp = (newExpData.currentExp || 0) + addExp;
                    newExpData.totalExpEarned = (newExpData.totalExpEarned || 0) + addExp;
                    
                    // Kiểm tra level up (logic đơn giản: mỗi 100 EXP = 1 level)
                    const newLevel = Math.floor(newExpData.currentExp / 100) + 1;
                    if (newLevel > (newExpData.currentLevel || 1)) {
                        newExpData.currentLevel = newLevel;
                        newExpData.expToNextLevel = newLevel * 100;
                        newExpData.currentExp = newExpData.currentExp % 100;
                    }
                }
                
                // Xử lý điểm kỹ năng
                if (addStatPoints > 0) {
                    newExpData.statPoints = (newExpData.statPoints || 0) + addStatPoints;
                }
                
                const updateData = {
                    ...user,
                    Diamond: newDiamonds,  // Chỉ lưu với chữ in hoa
                    Gold: newGold,         // Chỉ lưu với chữ in hoa
                    ExpData: JSON.stringify(newExpData), // Cập nhật ExpData
                    lastUpdated: Date.now()
                };
                
                // Lưu lịch sử tặng quà
                const historyRef = window.firebaseRef(window.firebaseDB, 'giftHistory');
                const newHistoryRef = window.firebasePush(historyRef);
                window.firebaseSet(newHistoryRef, {
                    userId: userId,
                    userEmail: user.email || user.Email || 'N/A',
                    diamonds: addDiamonds,
                    gold: addGold,
                    exp: addExp,
                    statPoints: addStatPoints,
                    reason: reason || 'Quà tặng từ admin',
                    timestamp: Date.now()
                });
                
                window.firebaseSet(userRef, updateData).then(() => {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công!',
                        html: `
                            <div style="text-align: left;">
                                <p><strong>Đã cộng thành công:</strong></p>
                                ${addDiamonds > 0 ? `<p><i class="fas fa-gem" style="color: #60a5fa;"></i> +${formatNumber(addDiamonds)} Kim Cương</p>` : ''}
                                ${addGold > 0 ? `<p><i class="fas fa-coins" style="color: #fbbf24;"></i> +${formatNumber(addGold)} Vàng</p>` : ''}
                                ${addExp > 0 ? `<p><i class="fas fa-chart-line" style="color: #3b82f6;"></i> +${formatNumber(addExp)} EXP</p>` : ''}
                                ${addStatPoints > 0 ? `<p><i class="fas fa-dice-d20" style="color: #10b981;"></i> +${formatNumber(addStatPoints)} Điểm kỹ năng</p>` : ''}
                                <hr>
                                <p><strong>Số dư mới:</strong></p>
                                <p><i class="fas fa-gem" style="color: #60a5fa;"></i> ${formatNumber(newDiamonds)} Kim Cương</p>
                                <p><i class="fas fa-coins" style="color: #fbbf24;"></i> ${formatNumber(newGold)} Vàng</p>
                                <p><i class="fas fa-star" style="color: #3b82f6;"></i> Level ${newExpData.currentLevel || currentLevel}</p>
                                <p><i class="fas fa-dice-d20" style="color: #10b981;"></i> ${newExpData.statPoints || statPoints} Điểm kỹ năng</p>
                                ${addExp > 0 ? `<p><i class="fas fa-chart-line" style="color: #3b82f6;"></i> EXP: ${newExpData.currentExp || 0}/${newExpData.expToNextLevel || 100}</p>` : ''}
                            </div>
                        `,
                        confirmButtonColor: '#667eea'
                    });
                    loadUsers();
                }).catch((error) => {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: 'Không thể cộng tài nguyên: ' + error.message,
                        confirmButtonColor: '#ef4444'
                    });
                });
            }
        });
    });
}

// Delete User
function deleteUser(userId) {
    const userRef = window.firebaseRef(window.firebaseDB, 'Users/' + userId);
    window.firebaseGet(userRef).then((snapshot) => {
        const user = snapshot.val();
        if (!user) {
            Swal.fire({
                icon: 'error',
                title: 'Không tìm thấy',
                text: 'Người dùng không tồn tại',
                confirmButtonColor: '#667eea'
            });
            return;
        }
        
        Swal.fire({
            title: '<i class="fas fa-exclamation-triangle"></i> Cảnh báo!',
            html: `
                <div style="text-align: left;">
                    <div style="background: #fef2f2; padding: 15px; border-radius: 10px; margin-bottom: 15px; border-left: 4px solid #ef4444;">
                        <p style="color: #ef4444; font-weight: 600; margin-bottom: 10px;">
                            <i class="fas fa-exclamation-circle"></i> Hành động này không thể hoàn tác!
                        </p>
                        <p style="color: #64748b; font-size: 0.9em;">
                            Tất cả dữ liệu game, lịch sử giao dịch và thông tin của người dùng sẽ bị xóa vĩnh viễn.
                        </p>
                    </div>
                    
                    <div style="background: #f8fafc; padding: 15px; border-radius: 10px;">
                        <h4 style="color: #1e293b; margin-bottom: 10px;">Thông tin người dùng sẽ xóa:</h4>
                        <p><strong><i class="fas fa-envelope"></i> Email:</strong> ${user.email || user.Email || 'N/A'}</p>
                        <p><strong><i class="fas fa-user"></i> Tên:</strong> ${user.displayName || user.DisplayName || user.Name || 'N/A'}</p>
                        <p><strong><i class="fas fa-gem" style="color: #60a5fa;"></i> Kim Cương:</strong> ${formatNumber(user.Diamond || user.diamonds || 0)}</p>
                        <p><strong><i class="fas fa-coins" style="color: #fbbf24;"></i> Vàng:</strong> ${formatNumber(user.Gold || user.gold || 0)}</p>
                    </div>
                    
                    <div class="form-group" style="margin-top: 20px;">
                        <label class="form-label" style="color: #ef4444;">
                            <i class="fas fa-keyboard"></i> Nhập "<strong>XOA</strong>" để xác nhận:
                        </label>
                        <input type="text" id="confirm-delete" class="form-control" placeholder="Nhập XOA" style="border: 2px solid #ef4444;">
                    </div>
                </div>
            `,
            width: '600px',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: '<i class="fas fa-trash"></i> Xóa vĩnh viễn',
            cancelButtonText: '<i class="fas fa-times"></i> Hủy',
            confirmButtonColor: '#ef4444',
            cancelButtonColor: '#64748b',
            preConfirm: () => {
                const confirmText = document.getElementById('confirm-delete').value.trim().toUpperCase();
                if (confirmText !== 'XOA') {
                    Swal.showValidationMessage('Vui lòng nhập "XOA" để xác nhận');
                    return false;
                }
                return true;
            }
        }).then((result) => {
            if (result.isConfirmed) {
                // Hiển thị loading
                Swal.fire({
                    title: 'Đang xóa...',
                    html: 'Vui lòng đợi',
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });
                
                // Xóa user từ database
                window.firebaseRemove(userRef).then(() => {
                    // Xóa dữ liệu liên quan nếu có
                    // TODO: Xóa thêm dữ liệu ở các collection khác nếu cần
                    
                    Swal.fire({
                        icon: 'success',
                        title: 'Đã xóa!',
                        html: `
                            <p>Người dùng <strong>${user.email}</strong> đã được xóa khỏi hệ thống.</p>
                            <p style="color: #64748b; font-size: 0.9em; margin-top: 10px;">
                                <i class="fas fa-info-circle"></i> Lưu ý: Người dùng vẫn có thể đăng ký lại bằng email này.
                            </p>
                        `,
                        confirmButtonColor: '#667eea',
                        timer: 3000
                    });
                    loadUsers();
                }).catch((error) => {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: 'Không thể xóa người dùng: ' + error.message,
                        confirmButtonColor: '#ef4444'
                    });
                });
            }
        });
    });
}

// Sync Auth Data for existing users
async function syncAuthData() {
    Swal.fire({
        title: 'Đồng bộ dữ liệu Auth',
        text: 'Chức năng này sẽ cập nhật thông tin authentication cho tất cả user hiện có.',
        icon: 'info',
        showCancelButton: true,
        confirmButtonText: 'Đồng bộ',
        cancelButtonText: 'Hủy',
        confirmButtonColor: '#667eea'
    }).then(async (result) => {
        if (result.isConfirmed) {
            try {
                // Get all users from RTDB
                const usersRef = window.firebaseRef(window.firebaseDB, 'Users');
                const snapshot = await window.firebaseGet(usersRef);
                const users = snapshot.val();
                
                if (!users) {
                    Swal.fire({
                        icon: 'info',
                        title: 'Không có dữ liệu',
                        text: 'Không tìm thấy user nào để đồng bộ.',
                        confirmButtonColor: '#667eea'
                    });
                    return;
                }
                
                let updatedCount = 0;
                const userIds = Object.keys(users);
                
                for (const userId of userIds) {
                    const user = users[userId];
                    
                    // Check if user already has auth data
                    if (!user.email && !user.displayName) {
                        // Skip users without auth data for now
                        // This would require additional Firebase Auth integration
                        console.log(`Skipping user ${userId} - no auth data available`);
                    } else {
                        // User already has auth data, no need to sync
                        console.log(`User ${userId} already has auth data`);
                    }
                }
                
                Swal.fire({
                    icon: 'success',
                    title: 'Đồng bộ hoàn tất!',
                    text: `Đã cập nhật thông tin authentication cho ${updatedCount} user.`,
                    confirmButtonColor: '#10b981'
                });
                
                // Reload users table
                loadUsers();
                
            } catch (error) {
                console.error('Sync error:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi đồng bộ!',
                    text: 'Không thể đồng bộ dữ liệu: ' + error.message,
                    confirmButtonColor: '#ef4444'
                });
            }
        }
    });
}

// Make sync function globally available
window.syncAuthData = syncAuthData;

console.log('✨ Admin panel loaded successfully!');


