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
    'reports': ['Quản lý phản hồi', 'Danh sách phản hồi từ người dùng'],
    'news': ['Quản lý tin tức', 'Quản lý tin tức và thông báo']
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
        case 'reports':
            loadReports();
            break;
        case 'news':
            loadNews();
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
    const transactionsRef = window.firebaseRef(window.firebaseDB, 'vnpay_transactions');
    
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
        // amount trong vnpay_transactions có đơn vị là đồng (VNĐ) nhưng lưu dưới dạng string và nhân 100
        const totalRevenue = transArray.reduce((sum, t) => {
            const amount = parseInt(t.amount) || 0;
            // Chia 100 vì VNPay lưu số tiền * 100 (VD: 200000đ lưu là "20000000")
            return sum + (amount / 100);
        }, 0);
        
        document.getElementById('total-transactions').textContent = formatNumber(totalTransactions);
        document.getElementById('total-revenue').textContent = formatCurrency(totalRevenue);
    });
}

// Load Recent Transactions
function loadRecentTransactions() {
    const transactionsRef = window.firebaseRef(window.firebaseDB, 'vnpay_transactions');
    
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
                        <th>Mã GD</th>
                        <th>Gói</th>
                        <th>Số tiền</th>
                        <th>Loại</th>
                        <th>Ngân hàng</th>
                        <th>Trạng thái</th>
                        <th>Thời gian</th>
                    </tr>
                </thead>
                <tbody>
        `;
        
        transArray.forEach(trans => {
            const amount = parseInt(trans.amount) / 100 || 0; // Chia 100 vì VNPay lưu * 100
            const packageName = trans.packageData?.name || 'N/A';
            const packageType = trans.packageData?.type || 'N/A';
            const isCompleted = trans.status === 'completed' || trans.responseCode === '00';
            
            html += `
                <tr>
                    <td><code style="font-size: 0.85em;">${trans.transactionNo || trans.id.substring(0, 15)}</code></td>
                    <td>${packageName}</td>
                    <td><strong style="color: #10b981;">${formatCurrency(amount)}</strong></td>
                    <td>${packageType === 'diamond' ? '<i class="fas fa-gem" style="color: #60a5fa;"></i> Kim Cương' : '<i class="fas fa-coins" style="color: #fbbf24;"></i> Vàng'}</td>
                    <td><span class="badge badge-info">${trans.bankCode || 'N/A'}</span></td>
                    <td><span class="badge badge-${isCompleted ? 'success' : 'warning'}">${isCompleted ? 'Hoàn thành' : 'Pending'}</span></td>
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
                        <th style="min-width: 180px; text-align: center;">Hành động</th>
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
                    <td style="text-align: center;">
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
    const transactionsRef = window.firebaseRef(window.firebaseDB, 'vnpay_transactions');
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
                        <th>Mã GD VNPay</th>
                        <th>Mã đơn hàng</th>
                        <th>Gói nạp</th>
                        <th>Số tiền</th>
                        <th>Loại</th>
                        <th>Ngân hàng</th>
                        <th>Mã giao dịch NH</th>
                        <th>Trạng thái</th>
                        <th>Thời gian</th>
                        <th style="text-align: center;">Hành động</th>
                    </tr>
                </thead>
                <tbody>
        `;
        
        transArray.forEach(trans => {
            const amount = parseInt(trans.amount) / 100 || 0; // Chia 100 vì VNPay lưu * 100
            const packageName = trans.packageData?.name || 'N/A';
            const packageType = trans.packageData?.type || 'N/A';
            const isCompleted = trans.status === 'completed' || trans.responseCode === '00';
            
            html += `
                <tr>
                    <td><code style="font-size: 0.85em;">${trans.transactionNo || 'N/A'}</code></td>
                    <td><code style="font-size: 0.75em; color: #64748b;">${trans.id.substring(0, 20)}...</code></td>
                    <td><strong>${packageName}</strong></td>
                    <td><strong style="color: #10b981;">${formatCurrency(amount)}</strong></td>
                    <td>${packageType === 'diamond' ? '<i class="fas fa-gem" style="color: #60a5fa;"></i> Kim Cương' : '<i class="fas fa-coins" style="color: #fbbf24;"></i> Vàng'}</td>
                    <td><span class="badge badge-info">${trans.bankCode || 'N/A'}</span></td>
                    <td><code style="font-size: 0.85em;">${trans.bankTranNo || 'N/A'}</code></td>
                    <td><span class="badge badge-${isCompleted ? 'success' : 'warning'}">${isCompleted ? 'Hoàn thành' : 'Pending'}</span></td>
                    <td>${formatDateTime(trans.timestamp)}</td>
                    <td style="text-align: center;">
                        <div class="action-buttons">
                            <button class="btn btn-sm btn-info" onclick="viewTransaction('${trans.id}')" title="Xem chi tiết">
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
    const promoRef = window.firebaseRef(window.firebaseDB, 'PromoCodes');
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
                        <th>Loại thưởng</th>
                        <th>Giá trị</th>
                        <th>Level tối thiểu</th>
                        <th>Số lượt dùng</th>
                        <th>Ngày hết hạn</th>
                        <th>Trạng thái</th>
                        <th style="text-align: center;">Hành động</th>
                    </tr>
                </thead>
                <tbody>
        `;
        
        promoArray.forEach(promo => {
            const isExpired = promo.expiryDate && new Date(promo.expiryDate) < new Date();
            const rewardIcon = promo.rewardType === 'diamond' 
                ? '<i class="fas fa-gem" style="color: #60a5fa;"></i> Kim Cương' 
                : '<i class="fas fa-coins" style="color: #fbbf24;"></i> Vàng';
            const isActive = promo.isActive && !isExpired;
            
            html += `
                <tr>
                    <td><code><strong style="color: #667eea;">${promo.code || 'N/A'}</strong></code></td>
                    <td>${promo.description || 'Không có mô tả'}</td>
                    <td>${rewardIcon}</td>
                    <td><strong style="color: #10b981;">${formatNumber(promo.rewardValue || 0)}</strong></td>
                    <td><span class="badge badge-info">Lv.${promo.minLevel || 1}</span></td>
                    <td><span class="badge badge-${promo.usedCount >= promo.usageLimit ? 'danger' : 'success'}">${promo.usedCount || 0} / ${promo.usageLimit || '∞'}</span></td>
                    <td><small>${promo.expiryDate || 'Không giới hạn'}</small></td>
                    <td><span class="badge badge-${isActive ? 'success' : 'danger'}">${isActive ? (isExpired ? 'Hết hạn' : 'Hoạt động') : 'Tắt'}</span></td>
                    <td style="text-align: center;">
                        <div class="action-buttons">
                            <button class="btn btn-sm btn-info" onclick="viewPromoCode('${promo.id}')" title="Xem chi tiết">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-sm btn-warning" onclick="editPromoCode('${promo.id}')" title="Chỉnh sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="deletePromoCode('${promo.id}')" title="Xóa">
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
    // Load real data for charts
    loadRevenueChart();
    loadUsersChart();
}

// Load Revenue Chart with real data
function loadRevenueChart() {
    const transactionsRef = window.firebaseRef(window.firebaseDB, 'vnpay_transactions');
    
    window.firebaseGet(transactionsRef).then((snapshot) => {
        const transactions = snapshot.val();
    const revenueCtx = document.getElementById('revenueChart');
        
        if (!revenueCtx) return;
        
        // Get last 7 days data
        const today = new Date();
        const last7Days = [];
        const labels = [];
        const revenueData = new Array(7).fill(0);
        
        // Create array of last 7 days
        for (let i = 6; i >= 0; i--) {
            const date = new Date(today);
            date.setDate(date.getDate() - i);
            last7Days.push(date.toISOString().split('T')[0]); // Format: YYYY-MM-DD
            
            // Vietnamese day labels
            const dayNames = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];
            labels.push(dayNames[date.getDay()]);
        }
        
        // Calculate revenue for each day
        if (transactions) {
            Object.values(transactions).forEach(trans => {
                if (trans.timestamp && trans.status === 'completed') {
                    const transDate = new Date(trans.timestamp).toISOString().split('T')[0];
                    const dayIndex = last7Days.indexOf(transDate);
                    
                    if (dayIndex !== -1) {
                        const amount = parseInt(trans.amount) / 100 || 0;
                        revenueData[dayIndex] += amount;
                    }
                }
            });
        }
        
        // Destroy old chart if exists
        if (revenueChart) {
            revenueChart.destroy();
        }
        
        revenueChart = new Chart(revenueCtx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Doanh thu (VNĐ)',
                    data: revenueData,
                    borderColor: '#667eea',
                    backgroundColor: 'rgba(102, 126, 234, 0.1)',
                    tension: 0.4,
                    fill: true,
                    borderWidth: 3,
                    pointBackgroundColor: '#667eea',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointRadius: 5,
                    pointHoverRadius: 7
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return 'Doanh thu: ' + formatCurrency(context.parsed.y);
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function(value) {
                                return formatCurrency(value);
                            }
                        }
                    }
                }
            }
        });
    });
}

// Load Users Chart with real data
function loadUsersChart() {
    const usersRef = window.firebaseRef(window.firebaseDB, 'Users');
    
    window.firebaseGet(usersRef).then((snapshot) => {
        const users = snapshot.val();
    const usersCtx = document.getElementById('usersChart');
        
        if (!usersCtx) return;
        
        // Get last 7 days data
        const today = new Date();
        const last7Days = [];
        const labels = [];
        const usersData = new Array(7).fill(0);
        
        // Create array of last 7 days
        for (let i = 6; i >= 0; i--) {
            const date = new Date(today);
            date.setDate(date.getDate() - i);
            last7Days.push(date.toISOString().split('T')[0]);
            
            const dayNames = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];
            labels.push(dayNames[date.getDay()]);
        }
        
        // Count new users for each day
        if (users) {
            Object.values(users).forEach(user => {
                if (user.createdAt || user.CreatedAt) {
                    const createdTimestamp = user.createdAt || user.CreatedAt;
                    const userDate = new Date(createdTimestamp).toISOString().split('T')[0];
                    const dayIndex = last7Days.indexOf(userDate);
                    
                    if (dayIndex !== -1) {
                        usersData[dayIndex]++;
                    }
                }
            });
        }
        
        // Destroy old chart if exists
        if (usersChart) {
            usersChart.destroy();
        }
        
        usersChart = new Chart(usersCtx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Người dùng mới',
                    data: usersData,
                    backgroundColor: '#10b981',
                    borderRadius: 8,
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return 'Users mới: ' + context.parsed.y;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1,
                            precision: 0
                        }
                    }
                }
            }
        });
    });
}

// Create Promo Code
function createPromoCode() {
    Swal.fire({
        title: '<i class="fas fa-plus-circle"></i> Tạo mã khuyến mãi mới',
        html: `
            <div style="text-align: left;">
                <div class="form-group">
                    <label class="form-label"><i class="fas fa-code"></i> Mã Code</label>
                    <input type="text" id="promo-code" class="form-control" placeholder="VD: LANGHOARUC2025" style="text-transform: uppercase; font-weight: bold;">
                    <small style="color: #64748b;">Sẽ tự động chuyển thành chữ in hoa</small>
                </div>
                
                <div class="form-group">
                    <label class="form-label"><i class="fas fa-align-left"></i> Mô tả</label>
                    <textarea id="promo-desc" class="form-control" rows="2" placeholder="Mô tả về mã khuyến mãi này..."></textarea>
                </div>
                
                <div class="form-group">
                    <label class="form-label"><i class="fas fa-gift"></i> Loại thưởng</label>
                    <select id="promo-reward-type" class="form-control">
                        <option value="diamond">💎 Kim Cương</option>
                        <option value="gold">🪙 Vàng</option>
                    </select>
                </div>
                
                <div class="form-group">
                    <label class="form-label"><i class="fas fa-coins"></i> Giá trị thưởng</label>
                    <input type="number" id="promo-reward-value" class="form-control" placeholder="1000" min="1">
                </div>
                
                <div class="form-group">
                    <label class="form-label"><i class="fas fa-level-up-alt"></i> Level tối thiểu</label>
                    <input type="number" id="promo-min-level" class="form-control" value="1" min="1">
                    <small style="color: #64748b;">Player phải đạt level này mới dùng được</small>
                </div>
                
                <div class="form-group">
                    <label class="form-label"><i class="fas fa-layer-group"></i> Số lượt sử dụng tối đa</label>
                    <input type="number" id="promo-usage-limit" class="form-control" value="100" min="0">
                    <small style="color: #64748b;">0 = không giới hạn</small>
                </div>
                
                <div class="form-group">
                    <label class="form-label"><i class="fas fa-calendar-alt"></i> Ngày bắt đầu</label>
                    <input type="date" id="promo-start-date" class="form-control" value="${new Date().toISOString().split('T')[0]}">
                </div>
                
                <div class="form-group">
                    <label class="form-label"><i class="fas fa-calendar-times"></i> Ngày hết hạn</label>
                    <input type="date" id="promo-expiry-date" class="form-control">
                    <small style="color: #64748b;">Để trống = không giới hạn</small>
                </div>
            </div>
        `,
        width: '600px',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-check"></i> Tạo mã',
        cancelButtonText: '<i class="fas fa-times"></i> Hủy',
        confirmButtonColor: '#667eea',
        preConfirm: () => {
            const code = document.getElementById('promo-code').value.trim().toUpperCase();
            const description = document.getElementById('promo-desc').value.trim();
            const rewardType = document.getElementById('promo-reward-type').value;
            const rewardValue = parseInt(document.getElementById('promo-reward-value').value) || 0;
            const minLevel = parseInt(document.getElementById('promo-min-level').value) || 1;
            const usageLimit = parseInt(document.getElementById('promo-usage-limit').value) || 0;
            const startDate = document.getElementById('promo-start-date').value;
            const expiryDate = document.getElementById('promo-expiry-date').value;
            
            if (!code) {
                Swal.showValidationMessage('Vui lòng nhập mã code');
                return false;
            }
            
            if (code.length < 3) {
                Swal.showValidationMessage('Mã code phải có ít nhất 3 ký tự');
                return false;
            }
            
            if (rewardValue <= 0) {
                Swal.showValidationMessage('Giá trị thưởng phải lớn hơn 0');
                return false;
            }
            
            if (!startDate) {
                Swal.showValidationMessage('Vui lòng chọn ngày bắt đầu');
                return false;
            }
            
            return { code, description, rewardType, rewardValue, minLevel, usageLimit, startDate, expiryDate };
        }
    }).then((result) => {
        if (result.isConfirmed) {
            const { code, description, rewardType, rewardValue, minLevel, usageLimit, startDate, expiryDate } = result.value;
            
            // Kiểm tra xem mã đã tồn tại chưa
            const promoListRef = window.firebaseRef(window.firebaseDB, 'PromoCodes');
            window.firebaseGet(promoListRef).then((snapshot) => {
                const existingCodes = snapshot.val();
                if (existingCodes && existingCodes[code]) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Mã đã tồn tại!',
                        text: `Mã "${code}" đã được tạo trước đó. Vui lòng chọn mã khác.`,
                        confirmButtonColor: '#ef4444'
                    });
                    return;
                }
                
                // Tạo mã mới với code làm key
                const promoRef = window.firebaseRef(window.firebaseDB, 'PromoCodes/' + code);
                
                const newPromoData = {
                code: code,
                description: description,
                    rewardType: rewardType,
                    rewardValue: rewardValue,
                    minLevel: minLevel,
                    usageLimit: usageLimit,
                usedCount: 0,
                    isActive: true,
                    startDate: startDate,
                createdAt: Date.now()
                };
                
                if (expiryDate) {
                    newPromoData.expiryDate = expiryDate;
                }
                
                window.firebaseSet(promoRef, newPromoData).then(() => {
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công!',
                        html: `
                            <div style="text-align: left;">
                                <p>Đã tạo mã khuyến mãi mới:</p>
                                <div style="background: #667eea; color: white; padding: 15px; border-radius: 10px; text-align: center; margin: 15px 0;">
                                    <h2 style="margin: 0; letter-spacing: 2px;">${code}</h2>
                                </div>
                                <p><strong>Giá trị:</strong> ${formatNumber(rewardValue)} ${rewardType === 'diamond' ? 'Kim Cương' : 'Vàng'}</p>
                                <p><strong>Giới hạn:</strong> ${usageLimit || 'Không giới hạn'}</p>
                            </div>
                        `,
                    confirmButtonColor: '#667eea'
                });
                loadPromoCodes();
                });
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


// View Promo Code
function viewPromoCode(promoId) {
    const promoRef = window.firebaseRef(window.firebaseDB, 'PromoCodes/' + promoId);
    window.firebaseGet(promoRef).then((snapshot) => {
        const promo = snapshot.val();
        if (!promo) {
            Swal.fire({
                icon: 'error',
                title: 'Không tìm thấy',
                text: 'Mã khuyến mãi không tồn tại',
                confirmButtonColor: '#667eea'
            });
            return;
        }
        
        const isExpired = promo.expiryDate && new Date(promo.expiryDate) < new Date();
        const rewardIcon = promo.rewardType === 'diamond' 
            ? '<i class="fas fa-gem" style="color: #60a5fa;"></i> Kim Cương' 
            : '<i class="fas fa-coins" style="color: #fbbf24;"></i> Vàng';
        const isActive = promo.isActive && !isExpired;
        
        Swal.fire({
            title: '<i class="fas fa-gift"></i> Chi tiết mã khuyến mãi',
            html: `
                <div style="text-align: left; max-height: 600px; overflow-y: auto;">
                    <!-- Mã Code -->
                    <div style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 20px; border-radius: 15px; margin-bottom: 20px; text-align: center;">
                        <div style="font-size: 2em; color: white; font-weight: 800; letter-spacing: 2px; margin-bottom: 10px;">
                            ${promo.code}
                        </div>
                        <span class="badge badge-${isActive ? 'success' : 'danger'}" style="font-size: 1.1em; padding: 8px 16px;">
                            ${isActive ? '✅ Hoạt động' : (isExpired ? '❌ Hết hạn' : '🔒 Đã tắt')}
                        </span>
                    </div>
                    
                    <!-- Thông tin cơ bản -->
                    <div style="background: #f8fafc; padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #667eea; margin-bottom: 10px;">
                            <i class="fas fa-info-circle"></i> Thông tin cơ bản
                        </h4>
                        <p><strong><i class="fas fa-align-left"></i> Mô tả:</strong><br>
                            <span style="color: #64748b;">${promo.description || 'Không có mô tả'}</span>
                        </p>
                    </div>
                    
                    <!-- Phần thưởng -->
                    <div style="background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%); padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #10b981; margin-bottom: 10px;">
                            <i class="fas fa-trophy"></i> Phần thưởng
                        </h4>
                        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px;">
                            <div style="background: white; padding: 15px; border-radius: 8px; text-align: center;">
                                <div style="font-size: 2em; margin-bottom: 5px;">${rewardIcon}</div>
                                <div style="font-size: 1.8em; color: #10b981; font-weight: 700;">
                                    ${formatNumber(promo.rewardValue || 0)}
                                </div>
                                <small style="color: #64748b;">Giá trị thưởng</small>
                            </div>
                            <div style="background: white; padding: 15px; border-radius: 8px; text-align: center;">
                                <div style="font-size: 2em; margin-bottom: 5px;">
                                    <i class="fas fa-level-up-alt" style="color: #3b82f6;"></i>
                                </div>
                                <div style="font-size: 1.8em; color: #3b82f6; font-weight: 700;">
                                    ${promo.minLevel || 1}
                                </div>
                                <small style="color: #64748b;">Level tối thiểu</small>
                            </div>
                        </div>
                    </div>
                    
                    <!-- Thống kê sử dụng -->
                    <div style="background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%); padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #f59e0b; margin-bottom: 10px;">
                            <i class="fas fa-chart-bar"></i> Thống kê sử dụng
                        </h4>
                        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px;">
                            <div style="background: white; padding: 12px; border-radius: 8px;">
                                <strong><i class="fas fa-users"></i> Đã sử dụng:</strong>
                                <div style="font-size: 1.5em; color: #f59e0b; font-weight: 700; margin-top: 5px;">
                                    ${promo.usedCount || 0}
                                </div>
                            </div>
                            <div style="background: white; padding: 12px; border-radius: 8px;">
                                <strong><i class="fas fa-layer-group"></i> Giới hạn:</strong>
                                <div style="font-size: 1.5em; color: #3b82f6; font-weight: 700; margin-top: 5px;">
                                    ${promo.usageLimit || '∞'}
                                </div>
                            </div>
                        </div>
                        ${promo.usageLimit ? `
                        <div style="background: white; padding: 8px; border-radius: 5px; margin-top: 10px;">
                            <div style="background: #e0e7ff; height: 20px; border-radius: 10px; overflow: hidden;">
                                <div style="background: linear-gradient(90deg, #f59e0b, #dc2626); height: 100%; width: ${((promo.usedCount / promo.usageLimit) * 100).toFixed(1)}%; transition: width 0.3s;"></div>
                            </div>
                            <small style="color: #64748b; display: block; text-align: center; margin-top: 5px;">
                                Đã dùng ${((promo.usedCount / promo.usageLimit) * 100).toFixed(1)}%
                            </small>
                        </div>
                        ` : ''}
                    </div>
                    
                    <!-- Thời gian -->
                    <div style="background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%); padding: 15px; border-radius: 10px;">
                        <h4 style="color: #3b82f6; margin-bottom: 10px;">
                            <i class="fas fa-calendar-alt"></i> Thời gian hiệu lực
                        </h4>
                        <p><strong><i class="fas fa-play-circle"></i> Ngày bắt đầu:</strong> 
                            <span style="color: #10b981; font-weight: 600;">${promo.startDate || 'N/A'}</span>
                        </p>
                        <p><strong><i class="fas fa-stop-circle"></i> Ngày hết hạn:</strong> 
                            <span style="color: ${isExpired ? '#ef4444' : '#f59e0b'}; font-weight: 600;">
                                ${promo.expiryDate || 'Không giới hạn'}
                            </span>
                        </p>
                    </div>
                </div>
            `,
            width: '650px',
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
            const promoRef = window.firebaseRef(window.firebaseDB, 'PromoCodes/' + promoId);
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
    // Show loading
    Swal.fire({
        title: 'Đang xuất dữ liệu...',
        html: 'Vui lòng đợi trong giây lát',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
    
    const usersRef = window.firebaseRef(window.firebaseDB, 'Users');
    
    window.firebaseGet(usersRef).then((snapshot) => {
        const users = snapshot.val();
        
        if (!users) {
            Swal.fire({
                icon: 'warning',
                title: 'Không có dữ liệu',
                text: 'Không có người dùng nào để xuất',
        confirmButtonColor: '#667eea'
            });
            return;
        }
        
        // Prepare data for Excel
        const excelData = [];
        
        Object.entries(users).forEach(([id, user]) => {
            // Parse JSON data
            let expData = {};
            let checkinStats = {};
            let bagData = {};
            
            try {
                if (user.ExpData) expData = JSON.parse(user.ExpData);
                if (user.CheckinStats) checkinStats = user.CheckinStats;
                if (user.BagData) bagData = JSON.parse(user.BagData);
            } catch (e) {
                console.warn('Error parsing user data:', e);
            }
            
            excelData.push({
                'User ID': id,
                'Email': user.email || user.Email || 'N/A',
                'Tên hiển thị': user.Name || user.displayName || user.DisplayName || 'N/A',
                'Kim Cương': user.Diamond || 0,
                'Vàng': user.Gold || 0,
                'Level': expData.currentLevel || 1,
                'EXP': expData.currentExp || 0,
                'EXP cần': expData.expToNextLevel || 0,
                'Điểm kỹ năng': expData.statPoints || 0,
                'Tổng check-in': checkinStats.totalCheckins || 0,
                'Streak hiện tại': checkinStats.currentStreak || 0,
                'Streak dài nhất': checkinStats.longestStreak || 0,
                'KC từ check-in': checkinStats.totalDiamondsEarned || 0,
                'Số item trong túi': bagData.items ? bagData.items.length : 0,
                'Ngày tạo': user.createdAt ? new Date(user.createdAt).toLocaleString('vi-VN') : 'N/A',
                'Đăng nhập cuối': user.lastLogin ? new Date(user.lastLogin).toLocaleString('vi-VN') : 'N/A'
            });
        });
        
        // Create workbook
        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.json_to_sheet(excelData);
        
        // Auto-size columns
        const colWidths = [
            { wch: 35 }, // User ID
            { wch: 30 }, // Email
            { wch: 20 }, // Tên
            { wch: 12 }, // Kim Cương
            { wch: 12 }, // Vàng
            { wch: 8 },  // Level
            { wch: 10 }, // EXP
            { wch: 10 }, // EXP cần
            { wch: 12 }, // Điểm kỹ năng
            { wch: 12 }, // Tổng check-in
            { wch: 14 }, // Streak hiện tại
            { wch: 14 }, // Streak dài nhất
            { wch: 15 }, // KC từ check-in
            { wch: 14 }, // Số item
            { wch: 20 }, // Ngày tạo
            { wch: 20 }  // Đăng nhập cuối
        ];
        ws['!cols'] = colWidths;
        
        XLSX.utils.book_append_sheet(wb, ws, 'Users');
        
        // Generate filename with timestamp
        const timestamp = new Date().toISOString().split('T')[0];
        const filename = `LangHoaRuc_Users_${timestamp}.xlsx`;
        
        // Download file
        XLSX.writeFile(wb, filename);
        
        Swal.fire({
            icon: 'success',
            title: 'Xuất Excel thành công!',
            html: `
                <p>Đã xuất <strong>${excelData.length}</strong> người dùng</p>
                <p>File: <code>${filename}</code></p>
            `,
            confirmButtonColor: '#667eea'
        });
    }).catch((error) => {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Không thể xuất dữ liệu: ' + error.message,
            confirmButtonColor: '#ef4444'
        });
    });
}

function exportTransactions() {
    // Show loading
    Swal.fire({
        title: 'Đang xuất dữ liệu...',
        html: 'Vui lòng đợi trong giây lát',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
    
    const transactionsRef = window.firebaseRef(window.firebaseDB, 'vnpay_transactions');
    
    window.firebaseGet(transactionsRef).then((snapshot) => {
        const transactions = snapshot.val();
        
        if (!transactions) {
            Swal.fire({
                icon: 'warning',
                title: 'Không có dữ liệu',
                text: 'Không có giao dịch nào để xuất',
        confirmButtonColor: '#667eea'
            });
            return;
        }
        
        // Prepare data for Excel
        const excelData = [];
        
        Object.entries(transactions).forEach(([id, trans]) => {
            const amount = parseInt(trans.amount) / 100 || 0;
            const packageName = trans.packageData?.name || 'N/A';
            const packageType = trans.packageData?.type || 'N/A';
            
            excelData.push({
                'Mã GD VNPay': trans.transactionNo || 'N/A',
                'Mã đơn hàng': id,
                'Gói nạp': packageName,
                'Loại': packageType === 'diamond' ? 'Kim Cương' : 'Vàng',
                'Số tiền (VNĐ)': amount,
                'Ngân hàng': trans.bankCode || 'N/A',
                'Mã GD NH': trans.bankTranNo || 'N/A',
                'Loại thẻ': trans.cardType || 'N/A',
                'Trạng thái': trans.status === 'completed' || trans.responseCode === '00' ? 'Hoàn thành' : 'Pending',
                'Response Code': trans.responseCode || 'N/A',
                'Transaction Status': trans.transactionStatus || 'N/A',
                'Order Info': trans.orderInfo || 'N/A',
                'Ngày thanh toán': trans.payDate || 'N/A',
                'Hoàn thành lúc': trans.completedAt ? new Date(trans.completedAt).toLocaleString('vi-VN') : 'N/A',
                'Timestamp': trans.timestamp ? new Date(trans.timestamp).toLocaleString('vi-VN') : 'N/A'
            });
        });
        
        // Sort by timestamp descending
        excelData.sort((a, b) => {
            const dateA = new Date(a['Timestamp']);
            const dateB = new Date(b['Timestamp']);
            return dateB - dateA;
        });
        
        // Create workbook
        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.json_to_sheet(excelData);
        
        // Auto-size columns
        const colWidths = [
            { wch: 15 }, // Mã GD VNPay
            { wch: 35 }, // Mã đơn hàng
            { wch: 25 }, // Gói nạp
            { wch: 12 }, // Loại
            { wch: 15 }, // Số tiền
            { wch: 12 }, // Ngân hàng
            { wch: 18 }, // Mã GD NH
            { wch: 12 }, // Loại thẻ
            { wch: 12 }, // Trạng thái
            { wch: 12 }, // Response Code
            { wch: 15 }, // Transaction Status
            { wch: 30 }, // Order Info
            { wch: 18 }, // Ngày thanh toán
            { wch: 20 }, // Hoàn thành lúc
            { wch: 20 }  // Timestamp
        ];
        ws['!cols'] = colWidths;
        
        XLSX.utils.book_append_sheet(wb, ws, 'Transactions');
        
        // Generate filename with timestamp
        const timestamp = new Date().toISOString().split('T')[0];
        const filename = `LangHoaRuc_Transactions_${timestamp}.xlsx`;
        
        // Download file
        XLSX.writeFile(wb, filename);
        
        Swal.fire({
            icon: 'success',
            title: 'Xuất Excel thành công!',
            html: `
                <p>Đã xuất <strong>${excelData.length}</strong> giao dịch</p>
                <p>File: <code>${filename}</code></p>
            `,
            confirmButtonColor: '#667eea'
        });
    }).catch((error) => {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Không thể xuất dữ liệu: ' + error.message,
            confirmButtonColor: '#ef4444'
        });
    });
}

function viewTransaction(transId) {
    const transRef = window.firebaseRef(window.firebaseDB, 'vnpay_transactions/' + transId);
    window.firebaseGet(transRef).then((snapshot) => {
        const trans = snapshot.val();
        if (!trans) {
        Swal.fire({
                icon: 'error',
                title: 'Không tìm thấy',
                text: 'Giao dịch không tồn tại',
                confirmButtonColor: '#667eea'
            });
            return;
        }
        
        const amount = parseInt(trans.amount) / 100 || 0;
        const packageName = trans.packageData?.name || 'N/A';
        const packageDesc = trans.packageData?.description || 'N/A';
        const packageType = trans.packageData?.type || 'N/A';
        const isCompleted = trans.status === 'completed' || trans.responseCode === '00';
        
        Swal.fire({
            title: '<i class="fas fa-receipt"></i> Chi tiết giao dịch VNPay',
            html: `
                <div style="text-align: left; max-height: 600px; overflow-y: auto;">
                    <!-- Thông tin giao dịch -->
                    <div style="background: #f8fafc; padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #667eea; margin-bottom: 10px;">
                            <i class="fas fa-info-circle"></i> Thông tin giao dịch
                        </h4>
                        <p><strong><i class="fas fa-hashtag"></i> Mã GD VNPay:</strong> <code>${trans.transactionNo || 'N/A'}</code></p>
                        <p><strong><i class="fas fa-barcode"></i> Mã đơn hàng:</strong> <code style="font-size: 0.85em;">${transId}</code></p>
                        <p><strong><i class="fas fa-university"></i> Mã GD Ngân hàng:</strong> <code>${trans.bankTranNo || 'N/A'}</code></p>
                        <p><strong><i class="fas fa-money-check-alt"></i> Trạng thái:</strong> 
                            <span class="badge badge-${isCompleted ? 'success' : 'warning'}">${isCompleted ? 'Hoàn thành' : 'Pending'}</span>
                        </p>
                        <p><strong><i class="fas fa-code"></i> Response Code:</strong> <code>${trans.responseCode || 'N/A'}</code></p>
                    </div>
                    
                    <!-- Thông tin gói nạp -->
                    <div style="background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%); padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #10b981; margin-bottom: 10px;">
                            <i class="fas fa-box"></i> Thông tin gói nạp
                        </h4>
                        <p><strong><i class="fas fa-gift"></i> Tên gói:</strong> ${packageName}</p>
                        <p><strong><i class="fas fa-align-left"></i> Mô tả:</strong> ${packageDesc}</p>
                        <p><strong><i class="fas fa-tag"></i> Loại:</strong> 
                            ${packageType === 'diamond' ? '<i class="fas fa-gem" style="color: #60a5fa;"></i> Kim Cương' : '<i class="fas fa-coins" style="color: #fbbf24;"></i> Vàng'}
                        </p>
                        <p><strong><i class="fas fa-dollar-sign"></i> Số tiền:</strong> 
                            <span style="font-size: 1.3em; color: #10b981; font-weight: 700;">${formatCurrency(amount)}</span>
                        </p>
                        <p><strong><i class="fas fa-sticky-note"></i> Order Info:</strong> ${trans.orderInfo || 'N/A'}</p>
                    </div>
                    
                    <!-- Thông tin thanh toán -->
                    <div style="background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%); padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #3b82f6; margin-bottom: 10px;">
                            <i class="fas fa-credit-card"></i> Thông tin thanh toán
                        </h4>
                        <p><strong><i class="fas fa-university"></i> Ngân hàng:</strong> 
                            <span class="badge badge-info">${trans.bankCode || 'N/A'}</span>
                        </p>
                        <p><strong><i class="fas fa-credit-card"></i> Loại thẻ:</strong> ${trans.cardType || 'N/A'}</p>
                        <p><strong><i class="fas fa-calendar-alt"></i> Ngày thanh toán:</strong> ${trans.payDate || 'N/A'}</p>
                        <p><strong><i class="fas fa-check-circle"></i> Hoàn thành lúc:</strong> ${trans.completedAt ? new Date(trans.completedAt).toLocaleString('vi-VN') : 'N/A'}</p>
                    </div>
                    
                    <!-- Thông tin kỹ thuật -->
                    <div style="background: #fef3c7; padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #f59e0b; margin-bottom: 10px;">
                            <i class="fas fa-cog"></i> Thông tin kỹ thuật
                        </h4>
                        <p><strong><i class="fas fa-clock"></i> Timestamp:</strong> ${formatDateTime(trans.timestamp)}</p>
                        <p><strong><i class="fas fa-desktop"></i> User Agent:</strong></p>
                        <p style="font-size: 0.85em; color: #64748b; word-break: break-word; background: white; padding: 8px; border-radius: 5px;">${trans.userAgent || 'N/A'}</p>
                    </div>
                </div>
            `,
            width: '700px',
            confirmButtonColor: '#667eea',
            confirmButtonText: '<i class="fas fa-check"></i> Đóng'
        });
    });
}

function editPromoCode(promoId) {
    const promoRef = window.firebaseRef(window.firebaseDB, 'PromoCodes/' + promoId);
    window.firebaseGet(promoRef).then((snapshot) => {
        const promo = snapshot.val();
        if (!promo) {
        Swal.fire({
                icon: 'error',
                title: 'Không tìm thấy',
                text: 'Mã khuyến mãi không tồn tại',
                confirmButtonColor: '#667eea'
            });
            return;
        }
        
        Swal.fire({
            title: '<i class="fas fa-edit"></i> Chỉnh sửa mã khuyến mãi',
            html: `
                <div style="text-align: left;">
                    <div style="background: #667eea; color: white; padding: 15px; border-radius: 10px; text-align: center; margin-bottom: 20px;">
                        <h3 style="margin: 0; letter-spacing: 2px;">${promo.code}</h3>
                    </div>
                    
                    <div class="form-group">
                        <label class="form-label"><i class="fas fa-power-off"></i> Trạng thái</label>
                        <select id="edit-promo-active" class="form-control">
                            <option value="true" ${promo.isActive ? 'selected' : ''}>✅ Hoạt động</option>
                            <option value="false" ${!promo.isActive ? 'selected' : ''}>🔒 Tắt</option>
                        </select>
                    </div>
                    
                    <div class="form-group">
                        <label class="form-label"><i class="fas fa-layer-group"></i> Số lượt sử dụng tối đa</label>
                        <input type="number" id="edit-promo-max-uses" class="form-control" value="${promo.usageLimit || 0}" min="0">
                        <small style="color: #64748b;">0 = không giới hạn</small>
                    </div>
                    
                    <div class="form-group">
                        <label class="form-label"><i class="fas fa-calendar-alt"></i> Ngày hết hạn</label>
                        <input type="date" id="edit-promo-expiry" class="form-control" value="${promo.expiryDate || ''}">
                        <small style="color: #64748b;">Để trống = không giới hạn</small>
                    </div>
                    
                    <div style="background: #f0f9ff; padding: 12px; border-radius: 8px; margin-top: 15px;">
                        <p style="margin: 0; color: #64748b; font-size: 0.9em;">
                            <i class="fas fa-info-circle"></i> 
                            Hiện đã có <strong>${promo.usedCount || 0}</strong> lượt sử dụng
                        </p>
                    </div>
                </div>
            `,
            width: '550px',
            showCancelButton: true,
            confirmButtonText: '<i class="fas fa-save"></i> Cập nhật',
            cancelButtonText: '<i class="fas fa-times"></i> Hủy',
            confirmButtonColor: '#667eea',
            preConfirm: () => {
                const isActive = document.getElementById('edit-promo-active').value === 'true';
                const usageLimit = parseInt(document.getElementById('edit-promo-max-uses').value) || 0;
                const expiryDate = document.getElementById('edit-promo-expiry').value;
                return { isActive, usageLimit, expiryDate };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                const updateData = {
                    ...promo,
                    isActive: result.value.isActive,
                    usageLimit: result.value.usageLimit
                };
                
                if (result.value.expiryDate) {
                    updateData.expiryDate = result.value.expiryDate;
                }
                
                window.firebaseSet(promoRef, updateData).then(() => {
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
        
        // Parse QuestData để hiển thị nhiệm vụ
        let questData = null;
        let questListHTML = '';
        try {
            if (user.QuestData) {
                questData = JSON.parse(user.QuestData);
                if (questData.questList && Array.isArray(questData.questList)) {
                    const currentQuestIndex = questData.currentQuestIndex || 0;
                    questListHTML = questData.questList.map((quest, index) => {
                        const isCompleted = quest.isCompleted || false;
                        const isCurrent = index === currentQuestIndex;
                        const statusIcon = isCompleted ? '✅' : (isCurrent ? '⏳' : '🔒');
                        const statusText = isCompleted ? 'Hoàn thành' : (isCurrent ? 'Đang làm' : 'Chưa mở');
                        const canSkip = !isCompleted; // Chỉ có thể bỏ qua nhiệm vụ chưa hoàn thành
                        
                        return `
                            <div style="background: white; padding: 10px; border-radius: 5px; margin: 5px 0; border-left: 3px solid ${isCompleted ? '#10b981' : (isCurrent ? '#f59e0b' : '#64748b')};">
                                <label style="display: flex; align-items: flex-start; cursor: ${canSkip ? 'pointer' : 'not-allowed'}; opacity: ${canSkip ? '1' : '0.5'};">
                                    <input type="checkbox" 
                                           class="quest-checkbox" 
                                           data-quest-index="${index}"
                                           style="width: 18px; height: 18px; margin-right: 10px; margin-top: 2px; cursor: ${canSkip ? 'pointer' : 'not-allowed'};"
                                           ${!canSkip ? 'disabled' : ''}>
                                    <div style="flex: 1;">
                                        <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 5px;">
                                            <span style="font-size: 1.2em;">${statusIcon}</span>
                                            <strong>${quest.title}</strong>
                                            <span style="font-size: 0.85em; color: #64748b;">(${statusText})</span>
                                        </div>
                                        <p style="font-size: 0.85em; color: #64748b; margin: 0;">${quest.description}</p>
                                    </div>
                                </label>
                            </div>
                        `;
                    }).join('');
                }
            }
        } catch (e) {
            console.warn('Error parsing QuestData:', e);
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
                    
                    ${questListHTML ? `
                    <div style="background: #fef2f2; padding: 15px; border-radius: 10px; margin-bottom: 20px;">
                        <h4 style="color: #ef4444; margin-bottom: 10px;">
                            <i class="fas fa-tasks"></i> Bỏ qua nhiệm vụ
                        </h4>
                        <small style="color: #64748b; display: block; margin-bottom: 10px;">
                            <i class="fas fa-info-circle"></i> Chọn nhiệm vụ muốn bỏ qua (đánh dấu là hoàn thành). Có thể chọn nhiều nhiệm vụ.
                        </small>
                        <div style="max-height: 300px; overflow-y: auto;">
                            ${questListHTML}
                        </div>
                    </div>
                    ` : ''}
                    
                    <div class="form-group">
                        <label class="form-label"><i class="fas fa-comment"></i> Lý do (tùy chọn)</label>
                        <input type="text" id="add-reason" class="form-control" placeholder="VD: Quà tặng từ admin, Sự kiện đặc biệt">
                    </div>
                </div>
            `,
            width: '650px',
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
                
                // Lấy danh sách các nhiệm vụ được chọn để bỏ qua
                const selectedQuests = [];
                const checkboxes = document.querySelectorAll('.quest-checkbox:checked');
                checkboxes.forEach(cb => {
                    const questIndex = parseInt(cb.getAttribute('data-quest-index'));
                    selectedQuests.push(questIndex);
                });
                
                if (addDiamonds < 0 || addGold < 0 || addExp < 0 || addStatPoints < 0) {
                    Swal.showValidationMessage('Số lượng không được âm');
                    return false;
                }
                
                if (addDiamonds === 0 && addGold === 0 && addExp === 0 && addStatPoints === 0 && selectedQuests.length === 0) {
                    Swal.showValidationMessage('Vui lòng nhập ít nhất một loại tài nguyên/kinh nghiệm hoặc chọn nhiệm vụ bỏ qua');
                    return false;
                }
                
                return { addDiamonds, addGold, addExp, addStatPoints, selectedQuests, reason };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                const { addDiamonds, addGold, addExp, addStatPoints, selectedQuests, reason } = result.value;
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
                
                // Xử lý bỏ qua nhiệm vụ
                let newQuestData = null;
                let questsSkipped = [];
                if (selectedQuests.length > 0) {
                    try {
                        if (user.QuestData) {
                            newQuestData = JSON.parse(user.QuestData);
                            
                            // Kiểm tra nếu có questList
                            if (newQuestData.questList && Array.isArray(newQuestData.questList)) {
                                // Đánh dấu các nhiệm vụ được chọn là hoàn thành
                                selectedQuests.forEach(questIndex => {
                                    if (questIndex < newQuestData.questList.length) {
                                        const quest = newQuestData.questList[questIndex];
                                        if (!quest.isCompleted) {
                                            quest.isCompleted = true;
                                            questsSkipped.push(quest.title);
                                        }
                                    }
                                });
                                
                                // Tìm nhiệm vụ tiếp theo chưa hoàn thành
                                let nextQuestIndex = 0;
                                for (let i = 0; i < newQuestData.questList.length; i++) {
                                    if (!newQuestData.questList[i].isCompleted) {
                                        nextQuestIndex = i;
                                        break;
                                    }
                                }
                                
                                // Nếu tất cả nhiệm vụ đã hoàn thành, giữ ở nhiệm vụ cuối
                                if (newQuestData.questList.every(q => q.isCompleted)) {
                                    nextQuestIndex = newQuestData.questList.length - 1;
                                }
                                
                                newQuestData.currentQuestIndex = nextQuestIndex;
                            }
                        }
                    } catch (e) {
                        console.warn('Error parsing QuestData:', e);
                    }
                }
                
                const updateData = {
                    ...user,
                    Diamond: newDiamonds,  // Chỉ lưu với chữ in hoa
                    Gold: newGold,         // Chỉ lưu với chữ in hoa
                    ExpData: JSON.stringify(newExpData), // Cập nhật ExpData
                    lastUpdated: Date.now()
                };
                
                // Nếu đã bỏ qua nhiệm vụ, cập nhật QuestData
                if (questsSkipped.length > 0 && newQuestData) {
                    updateData.QuestData = JSON.stringify(newQuestData);
                }
                
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
                    questsSkipped: questsSkipped,
                    questsSkippedCount: questsSkipped.length,
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
                                ${questsSkipped.length > 0 ? `
                                <div style="background: #fef2f2; padding: 10px; border-radius: 8px; margin: 10px 0;">
                                    <p style="color: #ef4444; font-weight: 600; margin-bottom: 5px;">
                                        <i class="fas fa-forward"></i> Đã bỏ qua ${questsSkipped.length} nhiệm vụ:
                                    </p>
                                    ${questsSkipped.map(title => `<p style="color: #64748b; font-size: 0.9em; margin: 2px 0;">• ${title}</p>`).join('')}
                                </div>
                                ` : ''}
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

// Load Reports
function loadReports() {
    if (!window.firebaseDB) return;
    
    const reportsRef = window.firebaseRef(window.firebaseDB, 'Reports');
    
    window.firebaseGet(reportsRef).then((snapshot) => {
        const reports = snapshot.val();
        const reportsTable = document.getElementById('reports-table');
        
        if (!reports || Object.keys(reports).length === 0) {
            reportsTable.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-flag" style="font-size: 3rem; color: #64748b; margin-bottom: 20px;"></i>
                    <h3>Chưa có phản hồi nào</h3>
                    <p>Chưa có phản hồi nào từ người dùng</p>
                </div>
            `;
            return;
        }
        
        const reportsArray = Object.entries(reports).map(([id, report]) => ({
            id,
            ...report
        }));
        
        // Sort by timestamp (newest first)
        reportsArray.sort((a, b) => {
            const timeA = new Date(a.timestamp).getTime();
            const timeB = new Date(b.timestamp).getTime();
            return timeB - timeA;
        });
        
        let html = `
            <div class="data-table-container">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th style="min-width: 200px;">Nội dung</th>
                            <th style="min-width: 150px;">Người gửi</th>
                            <th style="min-width: 100px;">Trạng thái</th>
                            <th style="min-width: 150px;">Thời gian</th>
                            <th style="min-width: 180px; text-align: center;">Hành động</th>
                        </tr>
                    </thead>
                    <tbody>
        `;
        
        reportsArray.forEach(report => {
            const statusBadge = report.status === 'pending' 
                ? '<span class="badge badge-warning">Chờ xử lý</span>'
                : '<span class="badge badge-success">Đã xử lý</span>';
            
            const contentPreview = report.content.length > 50 
                ? report.content.substring(0, 50) + '...'
                : report.content;
            
            html += `
                <tr>
                    <td>
                        <div style="max-width: 200px;">
                            <p style="margin: 0; font-weight: 500; color: #1f2937;">${contentPreview}</p>
                        </div>
                    </td>
                    <td>
                        <div>
                            <p style="margin: 0; font-weight: 500;">${report.userEmail || 'N/A'}</p>
                            <small style="color: #64748b;">ID: ${report.userId ? report.userId.substring(0, 15) + '...' : 'N/A'}</small>
                        </div>
                    </td>
                    <td>${statusBadge}</td>
                    <td>
                        <small style="color: #64748b;">${formatDateTime(report.timestamp)}</small>
                    </td>
                    <td style="text-align: center;">
                        <div class="action-buttons">
                            <button class="btn btn-sm btn-info" onclick="viewReport('${report.id}')" title="Xem chi tiết">
                                <i class="fas fa-eye"></i>
                            </button>
                            ${report.status === 'pending' ? `
                            <button class="btn btn-sm btn-success" onclick="markReportResolved('${report.id}')" title="Đánh dấu đã xử lý">
                                <i class="fas fa-check"></i>
                            </button>
                            ` : ''}
                            <button class="btn btn-sm btn-danger" onclick="deleteReport('${report.id}')" title="Xóa">
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
            </div>
        `;
        
        reportsTable.innerHTML = html;
    }).catch((error) => {
        console.error('Error loading reports:', error);
        document.getElementById('reports-table').innerHTML = `
            <div class="error-state">
                <i class="fas fa-exclamation-triangle" style="font-size: 3rem; color: #ef4444; margin-bottom: 20px;"></i>
                <h3>Lỗi tải dữ liệu</h3>
                <p>Không thể tải danh sách phản hồi: ${error.message}</p>
            </div>
        `;
    });
}

// View Report Details
function viewReport(reportId) {
    const reportRef = window.firebaseRef(window.firebaseDB, 'Reports/' + reportId);
    
    window.firebaseGet(reportRef).then((snapshot) => {
        const report = snapshot.val();
        if (!report) {
            Swal.fire({
                icon: 'error',
                title: 'Không tìm thấy',
                text: 'Phản hồi không tồn tại',
                confirmButtonColor: '#667eea'
            });
            return;
        }
        
        const statusBadge = report.status === 'pending' 
            ? '<span class="badge badge-warning">Chờ xử lý</span>'
            : '<span class="badge badge-success">Đã xử lý</span>';
        
        Swal.fire({
            title: '<i class="fas fa-flag"></i> Chi tiết phản hồi',
            html: `
                <div style="text-align: left; max-height: 600px; overflow-y: auto;">
                    <!-- Thông tin cơ bản -->
                    <div style="background: #f8fafc; padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #667eea; margin-bottom: 10px;">
                            <i class="fas fa-info-circle"></i> Thông tin cơ bản
                        </h4>
                        <p><strong><i class="fas fa-envelope"></i> Email người gửi:</strong> ${report.userEmail || 'N/A'}</p>
                        <p><strong><i class="fas fa-id-badge"></i> User ID:</strong> <code>${report.userId || 'N/A'}</code></p>
                        <p><strong><i class="fas fa-flag"></i> Trạng thái:</strong> ${statusBadge}</p>
                        <p><strong><i class="fas fa-clock"></i> Thời gian:</strong> ${formatDateTime(report.timestamp)}</p>
                    </div>
                    
                    <!-- Nội dung phản hồi -->
                    <div style="background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%); padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #10b981; margin-bottom: 10px;">
                            <i class="fas fa-comment-dots"></i> Nội dung phản hồi
                        </h4>
                        <div style="background: white; padding: 15px; border-radius: 8px; border-left: 4px solid #10b981;">
                            <p style="margin: 0; line-height: 1.6; white-space: pre-wrap;">${report.content || 'Không có nội dung'}</p>
                        </div>
                    </div>
                </div>
            `,
            width: '700px',
            confirmButtonColor: '#667eea',
            confirmButtonText: '<i class="fas fa-check"></i> Đóng'
        });
    }).catch((error) => {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Không thể tải chi tiết phản hồi: ' + error.message,
            confirmButtonColor: '#ef4444'
        });
    });
}

// Mark Report as Resolved
function markReportResolved(reportId) {
    Swal.fire({
        title: '<i class="fas fa-check-circle"></i> Xác nhận xử lý',
        html: `
            <div style="text-align: left;">
                <p>Bạn có chắc chắn muốn đánh dấu phản hồi này là <strong>đã xử lý</strong>?</p>
                <div style="background: #f0f9ff; padding: 10px; border-radius: 8px; margin: 10px 0;">
                    <p style="margin: 0; color: #64748b; font-size: 0.9em;">
                        <i class="fas fa-info-circle"></i> 
                        Phản hồi sẽ được đánh dấu là "Đã xử lý" và không thể hoàn tác.
                    </p>
                </div>
            </div>
        `,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-check"></i> Xác nhận',
        cancelButtonText: '<i class="fas fa-times"></i> Hủy',
        confirmButtonColor: '#10b981',
        cancelButtonColor: '#6b7280'
    }).then((result) => {
        if (result.isConfirmed) {
            const reportRef = window.firebaseRef(window.firebaseDB, 'Reports/' + reportId);
            
            window.firebaseGet(reportRef).then((snapshot) => {
                const report = snapshot.val();
                if (!report) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: 'Phản hồi không tồn tại',
                        confirmButtonColor: '#ef4444'
                    });
                    return;
                }
                
                const updateData = {
                    ...report,
                    status: 'resolved',
                    resolvedAt: Date.now(),
                    resolvedBy: 'admin'
                };
                
                window.firebaseSet(reportRef, updateData).then(() => {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công!',
                        text: 'Phản hồi đã được đánh dấu là đã xử lý',
                        confirmButtonColor: '#10b981'
                    });
                    loadReports();
                }).catch((error) => {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: 'Không thể cập nhật trạng thái: ' + error.message,
                        confirmButtonColor: '#ef4444'
                    });
                });
            });
        }
    });
}

// Delete Report
function deleteReport(reportId) {
    Swal.fire({
        title: '<i class="fas fa-trash"></i> Xác nhận xóa',
        html: `
            <div style="text-align: left;">
                <p>Bạn có chắc chắn muốn <strong style="color: #ef4444;">xóa vĩnh viễn</strong> phản hồi này?</p>
                <div style="background: #fef2f2; padding: 10px; border-radius: 8px; margin: 10px 0; border-left: 4px solid #ef4444;">
                    <p style="margin: 0; color: #dc2626; font-size: 0.9em;">
                        <i class="fas fa-exclamation-triangle"></i> 
                        <strong>Cảnh báo:</strong> Hành động này không thể hoàn tác!
                    </p>
                </div>
            </div>
        `,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-trash"></i> Xóa vĩnh viễn',
        cancelButtonText: '<i class="fas fa-times"></i> Hủy',
        confirmButtonColor: '#ef4444',
        cancelButtonColor: '#6b7280'
    }).then((result) => {
        if (result.isConfirmed) {
            const reportRef = window.firebaseRef(window.firebaseDB, 'Reports/' + reportId);
            
            window.firebaseRemove(reportRef).then(() => {
                Swal.fire({
                    icon: 'success',
                    title: 'Đã xóa!',
                    text: 'Phản hồi đã được xóa vĩnh viễn',
                    confirmButtonColor: '#10b981'
                });
                loadReports();
            }).catch((error) => {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi!',
                    text: 'Không thể xóa phản hồi: ' + error.message,
                    confirmButtonColor: '#ef4444'
                });
            });
        }
    });
}

// Export Reports to Excel
function exportReports() {
    Swal.fire({
        title: '<i class="fas fa-download"></i> Xuất Excel',
        html: '<div class="spinner"></div><p>Đang chuẩn bị dữ liệu...</p>',
        allowOutsideClick: false,
        showConfirmButton: false
    });
    
    const reportsRef = window.firebaseRef(window.firebaseDB, 'Reports');
    
    window.firebaseGet(reportsRef).then((snapshot) => {
        const reports = snapshot.val();
        
        if (!reports || Object.keys(reports).length === 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Không có dữ liệu',
                text: 'Không có phản hồi nào để xuất',
                confirmButtonColor: '#667eea'
            });
            return;
        }
        
        const reportsArray = Object.entries(reports).map(([id, report]) => ({
            'ID': id,
            'Nội dung': report.content || '',
            'Email người gửi': report.userEmail || '',
            'User ID': report.userId || '',
            'Trạng thái': report.status === 'pending' ? 'Chờ xử lý' : 'Đã xử lý',
            'Thời gian tạo': formatDateTime(report.timestamp),
            'Thời gian xử lý': report.resolvedAt ? formatDateTime(report.resolvedAt) : '',
            'Người xử lý': report.resolvedBy || ''
        }));
        
        // Sort by timestamp (newest first)
        reportsArray.sort((a, b) => {
            const timeA = new Date(reports[Object.keys(reports).find(key => reports[key] === a)]?.timestamp).getTime();
            const timeB = new Date(reports[Object.keys(reports).find(key => reports[key] === b)]?.timestamp).getTime();
            return timeB - timeA;
        });
        
        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.json_to_sheet(reportsArray);
        
        // Set column widths
        ws['!cols'] = [
            { width: 20 }, // ID
            { width: 50 }, // Nội dung
            { width: 25 }, // Email
            { width: 20 }, // User ID
            { width: 15 }, // Trạng thái
            { width: 20 }, // Thời gian tạo
            { width: 20 }, // Thời gian xử lý
            { width: 15 }  // Người xử lý
        ];
        
        XLSX.utils.book_append_sheet(wb, ws, 'Reports');
        
        const timestamp = new Date().toISOString().split('T')[0];
        const filename = `LangHoaRuc_Reports_${timestamp}.xlsx`;
        
        XLSX.writeFile(wb, filename);
        
        Swal.fire({
            icon: 'success',
            title: 'Xuất thành công!',
            html: `
                <div style="text-align: left;">
                    <p><strong>File đã được tải xuống:</strong></p>
                    <p><code>${filename}</code></p>
                    <hr>
                    <p><strong>Thống kê:</strong></p>
                    <p>• Tổng số phản hồi: <strong>${reportsArray.length}</strong></p>
                    <p>• Chờ xử lý: <strong>${reportsArray.filter(r => r.Trạng_thái === 'Chờ xử lý').length}</strong></p>
                    <p>• Đã xử lý: <strong>${reportsArray.filter(r => r.Trạng_thái === 'Đã xử lý').length}</strong></p>
                </div>
            `,
            confirmButtonColor: '#10b981'
        });
    }).catch((error) => {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi xuất file!',
            text: 'Không thể xuất dữ liệu: ' + error.message,
            confirmButtonColor: '#ef4444'
        });
    });
}

// Load News
function loadNews() {
    if (!window.firebaseDB) return;
    
    const newsRef = window.firebaseRef(window.firebaseDB, 'News');
    
    window.firebaseGet(newsRef).then((snapshot) => {
        const news = snapshot.val();
        const newsTable = document.getElementById('news-table');
        
        if (!news || Object.keys(news).length === 0) {
            newsTable.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-newspaper" style="font-size: 3rem; color: #64748b; margin-bottom: 20px;"></i>
                    <h3>Chưa có tin tức nào</h3>
                    <p>Chưa có tin tức nào được tạo</p>
                </div>
            `;
            return;
        }
        
        const newsArray = Object.entries(news).map(([id, newsItem]) => ({
            id,
            ...newsItem
        }));
        
        // Sort by priority (ascending) then by date (descending)
        newsArray.sort((a, b) => {
            if (a.priority !== b.priority) {
                return a.priority - b.priority;
            }
            return new Date(b.date) - new Date(a.date);
        });
        
        let html = `
            <div class="data-table-container">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th style="min-width: 200px;">Tiêu đề</th>
                            <th style="min-width: 300px;">Nội dung</th>
                            <th style="min-width: 100px;">Ưu tiên</th>
                            <th style="min-width: 100px;">Trạng thái</th>
                            <th style="min-width: 120px;">Ngày tạo</th>
                            <th style="min-width: 180px; text-align: center;">Hành động</th>
                        </tr>
                    </thead>
                    <tbody>
        `;
        
        newsArray.forEach(newsItem => {
            const statusBadge = newsItem.isActive 
                ? '<span class="badge badge-success">Hoạt động</span>'
                : '<span class="badge badge-danger">Tắt</span>';
            
            const priorityBadge = newsItem.priority === 1 
                ? '<span class="badge badge-danger">Cao</span>'
                : newsItem.priority === 2 
                ? '<span class="badge badge-warning">Trung bình</span>'
                : '<span class="badge badge-info">Thấp</span>';
            
            const contentPreview = newsItem.content.length > 100 
                ? newsItem.content.substring(0, 100) + '...'
                : newsItem.content;
            
            html += `
                <tr>
                    <td>
                        <div style="max-width: 200px;">
                            <p style="margin: 0; font-weight: 600; color: #1f2937;">${newsItem.title || 'N/A'}</p>
                            <small style="color: #64748b;">ID: ${newsItem.id}</small>
                        </div>
                    </td>
                    <td>
                        <div style="max-width: 300px;">
                            <p style="margin: 0; color: #374151; line-height: 1.4;">${contentPreview}</p>
                        </div>
                    </td>
                    <td>${priorityBadge}</td>
                    <td>${statusBadge}</td>
                    <td>
                        <small style="color: #64748b;">${newsItem.date || 'N/A'}</small>
                    </td>
                    <td style="text-align: center;">
                        <div class="action-buttons">
                            <button class="btn btn-sm btn-info" onclick="viewNews('${newsItem.id}')" title="Xem chi tiết">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-sm btn-warning" onclick="editNews('${newsItem.id}')" title="Chỉnh sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="deleteNews('${newsItem.id}')" title="Xóa">
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
            </div>
        `;
        
        newsTable.innerHTML = html;
    }).catch((error) => {
        console.error('Error loading news:', error);
        document.getElementById('news-table').innerHTML = `
            <div class="error-state">
                <i class="fas fa-exclamation-triangle" style="font-size: 3rem; color: #ef4444; margin-bottom: 20px;"></i>
                <h3>Lỗi tải dữ liệu</h3>
                <p>Không thể tải danh sách tin tức: ${error.message}</p>
            </div>
        `;
    });
}

// Create News
function createNews() {
    Swal.fire({
        title: '<i class="fas fa-plus-circle"></i> Tạo tin tức mới',
        html: `
            <div style="text-align: left;">
                <div class="form-group">
                    <label class="form-label"><i class="fas fa-heading"></i> Tiêu đề</label>
                    <input type="text" id="news-title" class="form-control" placeholder="VD: 🎉 Sự kiện đặc biệt tuần này!" maxlength="100">
                    <small style="color: #64748b;">Tối đa 100 ký tự</small>
                </div>
                
                <div class="form-group">
                    <label class="form-label"><i class="fas fa-align-left"></i> Nội dung</label>
                    <textarea id="news-content" class="form-control" rows="4" placeholder="Nhập nội dung tin tức..." maxlength="500"></textarea>
                    <small style="color: #64748b;">Tối đa 500 ký tự</small>
                </div>
                
                <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px;">
                    <div class="form-group">
                        <label class="form-label"><i class="fas fa-sort-numeric-up"></i> Độ ưu tiên</label>
                        <select id="news-priority" class="form-control">
                            <option value="1">🔴 Cao (1)</option>
                            <option value="2" selected>🟡 Trung bình (2)</option>
                            <option value="3">🟢 Thấp (3)</option>
                        </select>
                    </div>
                    
                    <div class="form-group">
                        <label class="form-label"><i class="fas fa-calendar-alt"></i> Ngày tạo</label>
                        <input type="date" id="news-date" class="form-control" value="${new Date().toISOString().split('T')[0]}">
                    </div>
                </div>
                
                <div class="form-group">
                    <label style="display: flex; align-items: center; cursor: pointer;">
                        <input type="checkbox" id="news-active" style="width: 20px; height: 20px; margin-right: 10px;" checked>
                        <span style="font-weight: 600; color: #10b981;">
                            <i class="fas fa-power-off"></i> Kích hoạt tin tức
                        </span>
                    </label>
                    <small style="color: #64748b; display: block; margin-top: 5px; margin-left: 30px;">
                        Tin tức sẽ hiển thị cho người dùng khi được kích hoạt
                    </small>
                </div>
            </div>
        `,
        width: '600px',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-check"></i> Tạo tin tức',
        cancelButtonText: '<i class="fas fa-times"></i> Hủy',
        confirmButtonColor: '#667eea',
        preConfirm: () => {
            const title = document.getElementById('news-title').value.trim();
            const content = document.getElementById('news-content').value.trim();
            const priority = parseInt(document.getElementById('news-priority').value);
            const date = document.getElementById('news-date').value;
            const isActive = document.getElementById('news-active').checked;
            
            if (!title) {
                Swal.showValidationMessage('Tiêu đề không được để trống');
                return false;
            }
            
            if (!content) {
                Swal.showValidationMessage('Nội dung không được để trống');
                return false;
            }
            
            if (title.length > 100) {
                Swal.showValidationMessage('Tiêu đề không được quá 100 ký tự');
                return false;
            }
            
            if (content.length > 500) {
                Swal.showValidationMessage('Nội dung không được quá 500 ký tự');
                return false;
            }
            
            if (!date) {
                Swal.showValidationMessage('Ngày tạo không được để trống');
                return false;
            }
            
            return { title, content, priority, date, isActive };
        }
    }).then((result) => {
        if (result.isConfirmed) {
            const { title, content, priority, date, isActive } = result.value;
            
            // Generate unique ID
            const newsId = 'news_' + Date.now();
            
            const newsData = {
                id: newsId,
                title: title,
                content: content,
                priority: priority,
                date: date,
                isActive: isActive,
                createdAt: Date.now()
            };
            
            const newsRef = window.firebaseRef(window.firebaseDB, 'News/' + newsId);
            
            window.firebaseSet(newsRef, newsData).then(() => {
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công!',
                    html: `
                        <div style="text-align: left;">
                            <p><strong>Đã tạo tin tức mới:</strong></p>
                            <div style="background: #f0f9ff; padding: 10px; border-radius: 8px; margin: 10px 0;">
                                <p><strong>Tiêu đề:</strong> ${title}</p>
                                <p><strong>Độ ưu tiên:</strong> ${priority === 1 ? '🔴 Cao' : priority === 2 ? '🟡 Trung bình' : '🟢 Thấp'}</p>
                                <p><strong>Ngày tạo:</strong> ${date}</p>
                                <p><strong>Trạng thái:</strong> ${isActive ? '✅ Hoạt động' : '❌ Tắt'}</p>
                            </div>
                        </div>
                    `,
                    confirmButtonColor: '#667eea'
                });
                loadNews();
            }).catch((error) => {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi!',
                    text: 'Không thể tạo tin tức: ' + error.message,
                    confirmButtonColor: '#ef4444'
                });
            });
        }
    });
}

// View News Details
function viewNews(newsId) {
    const newsRef = window.firebaseRef(window.firebaseDB, 'News/' + newsId);
    
    window.firebaseGet(newsRef).then((snapshot) => {
        const news = snapshot.val();
        if (!news) {
            Swal.fire({
                icon: 'error',
                title: 'Không tìm thấy',
                text: 'Tin tức không tồn tại',
                confirmButtonColor: '#667eea'
            });
            return;
        }
        
        const statusBadge = news.isActive 
            ? '<span class="badge badge-success">Hoạt động</span>'
            : '<span class="badge badge-danger">Tắt</span>';
        
        const priorityBadge = news.priority === 1 
            ? '<span class="badge badge-danger">🔴 Cao</span>'
            : news.priority === 2 
            ? '<span class="badge badge-warning">🟡 Trung bình</span>'
            : '<span class="badge badge-info">🟢 Thấp</span>';
        
        Swal.fire({
            title: '<i class="fas fa-newspaper"></i> Chi tiết tin tức',
            html: `
                <div style="text-align: left; max-height: 600px; overflow-y: auto;">
                    <!-- Header -->
                    <div style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 20px; border-radius: 15px; margin-bottom: 20px; text-align: center;">
                        <h3 style="color: white; margin: 0; font-size: 1.5em;">${news.title}</h3>
                        <div style="margin-top: 10px;">
                            ${statusBadge}
                            ${priorityBadge}
                        </div>
                    </div>
                    
                    <!-- Thông tin cơ bản -->
                    <div style="background: #f8fafc; padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #667eea; margin-bottom: 10px;">
                            <i class="fas fa-info-circle"></i> Thông tin cơ bản
                        </h4>
                        <p><strong><i class="fas fa-hashtag"></i> ID:</strong> <code>${news.id}</code></p>
                        <p><strong><i class="fas fa-calendar-alt"></i> Ngày tạo:</strong> ${news.date}</p>
                        <p><strong><i class="fas fa-clock"></i> Thời gian tạo:</strong> ${formatDateTime(news.createdAt)}</p>
                        <p><strong><i class="fas fa-sort-numeric-up"></i> Độ ưu tiên:</strong> ${news.priority}</p>
                        <p><strong><i class="fas fa-power-off"></i> Trạng thái:</strong> ${news.isActive ? 'Hoạt động' : 'Tắt'}</p>
                    </div>
                    
                    <!-- Nội dung -->
                    <div style="background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%); padding: 15px; border-radius: 10px; margin-bottom: 15px;">
                        <h4 style="color: #10b981; margin-bottom: 10px;">
                            <i class="fas fa-align-left"></i> Nội dung tin tức
                        </h4>
                        <div style="background: white; padding: 15px; border-radius: 8px; border-left: 4px solid #10b981;">
                            <p style="margin: 0; line-height: 1.6; white-space: pre-wrap;">${news.content}</p>
                        </div>
                    </div>
                </div>
            `,
            width: '700px',
            confirmButtonColor: '#667eea',
            confirmButtonText: '<i class="fas fa-check"></i> Đóng'
        });
    }).catch((error) => {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Không thể tải chi tiết tin tức: ' + error.message,
            confirmButtonColor: '#ef4444'
        });
    });
}

// Edit News
function editNews(newsId) {
    const newsRef = window.firebaseRef(window.firebaseDB, 'News/' + newsId);
    
    window.firebaseGet(newsRef).then((snapshot) => {
        const news = snapshot.val();
        if (!news) {
            Swal.fire({
                icon: 'error',
                title: 'Không tìm thấy',
                text: 'Tin tức không tồn tại',
                confirmButtonColor: '#667eea'
            });
            return;
        }
        
        Swal.fire({
            title: '<i class="fas fa-edit"></i> Chỉnh sửa tin tức',
            html: `
                <div style="text-align: left;">
                    <div style="background: #667eea; color: white; padding: 15px; border-radius: 10px; text-align: center; margin-bottom: 20px;">
                        <h3 style="margin: 0;">${news.title}</h3>
                        <small>ID: ${news.id}</small>
                    </div>
                    
                    <div class="form-group">
                        <label class="form-label"><i class="fas fa-heading"></i> Tiêu đề</label>
                        <input type="text" id="edit-news-title" class="form-control" value="${news.title}" maxlength="100">
                        <small style="color: #64748b;">Tối đa 100 ký tự</small>
                    </div>
                    
                    <div class="form-group">
                        <label class="form-label"><i class="fas fa-align-left"></i> Nội dung</label>
                        <textarea id="edit-news-content" class="form-control" rows="4" maxlength="500">${news.content}</textarea>
                        <small style="color: #64748b;">Tối đa 500 ký tự</small>
                    </div>
                    
                    <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px;">
                        <div class="form-group">
                            <label class="form-label"><i class="fas fa-sort-numeric-up"></i> Độ ưu tiên</label>
                            <select id="edit-news-priority" class="form-control">
                                <option value="1" ${news.priority === 1 ? 'selected' : ''}>🔴 Cao (1)</option>
                                <option value="2" ${news.priority === 2 ? 'selected' : ''}>🟡 Trung bình (2)</option>
                                <option value="3" ${news.priority === 3 ? 'selected' : ''}>🟢 Thấp (3)</option>
                            </select>
                        </div>
                        
                        <div class="form-group">
                            <label class="form-label"><i class="fas fa-calendar-alt"></i> Ngày tạo</label>
                            <input type="date" id="edit-news-date" class="form-control" value="${news.date}">
                        </div>
                    </div>
                    
                    <div class="form-group">
                        <label style="display: flex; align-items: center; cursor: pointer;">
                            <input type="checkbox" id="edit-news-active" style="width: 20px; height: 20px; margin-right: 10px;" ${news.isActive ? 'checked' : ''}>
                            <span style="font-weight: 600; color: #10b981;">
                                <i class="fas fa-power-off"></i> Kích hoạt tin tức
                            </span>
                        </label>
                    </div>
                </div>
            `,
            width: '600px',
            showCancelButton: true,
            confirmButtonText: '<i class="fas fa-save"></i> Cập nhật',
            cancelButtonText: '<i class="fas fa-times"></i> Hủy',
            confirmButtonColor: '#f59e0b',
            preConfirm: () => {
                const title = document.getElementById('edit-news-title').value.trim();
                const content = document.getElementById('edit-news-content').value.trim();
                const priority = parseInt(document.getElementById('edit-news-priority').value);
                const date = document.getElementById('edit-news-date').value;
                const isActive = document.getElementById('edit-news-active').checked;
                
                if (!title) {
                    Swal.showValidationMessage('Tiêu đề không được để trống');
                    return false;
                }
                
                if (!content) {
                    Swal.showValidationMessage('Nội dung không được để trống');
                    return false;
                }
                
                if (title.length > 100) {
                    Swal.showValidationMessage('Tiêu đề không được quá 100 ký tự');
                    return false;
                }
                
                if (content.length > 500) {
                    Swal.showValidationMessage('Nội dung không được quá 500 ký tự');
                    return false;
                }
                
                if (!date) {
                    Swal.showValidationMessage('Ngày tạo không được để trống');
                    return false;
                }
                
                return { title, content, priority, date, isActive };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                const { title, content, priority, date, isActive } = result.value;
                
                const updateData = {
                    ...news,
                    title: title,
                    content: content,
                    priority: priority,
                    date: date,
                    isActive: isActive,
                    updatedAt: Date.now()
                };
                
                window.firebaseSet(newsRef, updateData).then(() => {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công!',
                        html: `
                            <div style="text-align: left;">
                                <p><strong>Đã cập nhật tin tức:</strong></p>
                                <div style="background: #f0f9ff; padding: 10px; border-radius: 8px; margin: 10px 0;">
                                    <p><strong>Tiêu đề:</strong> ${title}</p>
                                    <p><strong>Độ ưu tiên:</strong> ${priority === 1 ? '🔴 Cao' : priority === 2 ? '🟡 Trung bình' : '🟢 Thấp'}</p>
                                    <p><strong>Ngày tạo:</strong> ${date}</p>
                                    <p><strong>Trạng thái:</strong> ${isActive ? '✅ Hoạt động' : '❌ Tắt'}</p>
                                </div>
                            </div>
                        `,
                        confirmButtonColor: '#f59e0b'
                    });
                    loadNews();
                }).catch((error) => {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: 'Không thể cập nhật tin tức: ' + error.message,
                        confirmButtonColor: '#ef4444'
                    });
                });
            }
        });
    });
}

// Delete News
function deleteNews(newsId) {
    Swal.fire({
        title: '<i class="fas fa-trash"></i> Xác nhận xóa',
        html: `
            <div style="text-align: left;">
                <p>Bạn có chắc chắn muốn <strong style="color: #ef4444;">xóa vĩnh viễn</strong> tin tức này?</p>
                <div style="background: #fef2f2; padding: 10px; border-radius: 8px; margin: 10px 0; border-left: 4px solid #ef4444;">
                    <p style="margin: 0; color: #dc2626; font-size: 0.9em;">
                        <i class="fas fa-exclamation-triangle"></i> 
                        <strong>Cảnh báo:</strong> Hành động này không thể hoàn tác!
                    </p>
                </div>
            </div>
        `,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-trash"></i> Xóa vĩnh viễn',
        cancelButtonText: '<i class="fas fa-times"></i> Hủy',
        confirmButtonColor: '#ef4444',
        cancelButtonColor: '#6b7280'
    }).then((result) => {
        if (result.isConfirmed) {
            const newsRef = window.firebaseRef(window.firebaseDB, 'News/' + newsId);
            
            window.firebaseRemove(newsRef).then(() => {
                Swal.fire({
                    icon: 'success',
                    title: 'Đã xóa!',
                    text: 'Tin tức đã được xóa vĩnh viễn',
                    confirmButtonColor: '#10b981'
                });
                loadNews();
            }).catch((error) => {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi!',
                    text: 'Không thể xóa tin tức: ' + error.message,
                    confirmButtonColor: '#ef4444'
                });
            });
        }
    });
}

// Export News to Excel
function exportNews() {
    Swal.fire({
        title: '<i class="fas fa-download"></i> Xuất Excel',
        html: '<div class="spinner"></div><p>Đang chuẩn bị dữ liệu...</p>',
        allowOutsideClick: false,
        showConfirmButton: false
    });
    
    const newsRef = window.firebaseRef(window.firebaseDB, 'News');
    
    window.firebaseGet(newsRef).then((snapshot) => {
        const news = snapshot.val();
        
        if (!news || Object.keys(news).length === 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Không có dữ liệu',
                text: 'Không có tin tức nào để xuất',
                confirmButtonColor: '#667eea'
            });
            return;
        }
        
        const newsArray = Object.entries(news).map(([id, newsItem]) => ({
            'ID': newsItem.id || id,
            'Tiêu đề': newsItem.title || '',
            'Nội dung': newsItem.content || '',
            'Độ ưu tiên': newsItem.priority === 1 ? 'Cao' : newsItem.priority === 2 ? 'Trung bình' : 'Thấp',
            'Trạng thái': newsItem.isActive ? 'Hoạt động' : 'Tắt',
            'Ngày tạo': newsItem.date || '',
            'Thời gian tạo': newsItem.createdAt ? formatDateTime(newsItem.createdAt) : '',
            'Thời gian cập nhật': newsItem.updatedAt ? formatDateTime(newsItem.updatedAt) : ''
        }));
        
        // Sort by priority then by date
        newsArray.sort((a, b) => {
            const priorityOrder = { 'Cao': 1, 'Trung bình': 2, 'Thấp': 3 };
            if (priorityOrder[a['Độ ưu tiên']] !== priorityOrder[b['Độ ưu tiên']]) {
                return priorityOrder[a['Độ ưu tiên']] - priorityOrder[b['Độ ưu tiên']];
            }
            return new Date(b['Ngày tạo']) - new Date(a['Ngày tạo']);
        });
        
        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.json_to_sheet(newsArray);
        
        // Set column widths
        ws['!cols'] = [
            { width: 15 }, // ID
            { width: 30 }, // Tiêu đề
            { width: 50 }, // Nội dung
            { width: 15 }, // Độ ưu tiên
            { width: 15 }, // Trạng thái
            { width: 15 }, // Ngày tạo
            { width: 20 }, // Thời gian tạo
            { width: 20 }  // Thời gian cập nhật
        ];
        
        XLSX.utils.book_append_sheet(wb, ws, 'News');
        
        const timestamp = new Date().toISOString().split('T')[0];
        const filename = `LangHoaRuc_News_${timestamp}.xlsx`;
        
        XLSX.writeFile(wb, filename);
        
        Swal.fire({
            icon: 'success',
            title: 'Xuất thành công!',
            html: `
                <div style="text-align: left;">
                    <p><strong>File đã được tải xuống:</strong></p>
                    <p><code>${filename}</code></p>
                    <hr>
                    <p><strong>Thống kê:</strong></p>
                    <p>• Tổng số tin tức: <strong>${newsArray.length}</strong></p>
                    <p>• Đang hoạt động: <strong>${newsArray.filter(n => n.Trạng_thái === 'Hoạt động').length}</strong></p>
                    <p>• Đã tắt: <strong>${newsArray.filter(n => n.Trạng_thái === 'Tắt').length}</strong></p>
                    <p>• Độ ưu tiên cao: <strong>${newsArray.filter(n => n['Độ ưu tiên'] === 'Cao').length}</strong></p>
                </div>
            `,
            confirmButtonColor: '#10b981'
        });
    }).catch((error) => {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi xuất file!',
            text: 'Không thể xuất dữ liệu: ' + error.message,
            confirmButtonColor: '#ef4444'
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


