// ===================================
// Recharge Page JavaScript
// ===================================

let selectedPackage = null;
let selectedPayment = null;

// Load user balance from Firebase or localStorage
function loadUserBalance() {
    // Wait for Firebase Auth to initialize
    if (!window.firebaseAuth) {
        console.log('Firebase Auth chưa sẵn sàng, đợi...');
        setTimeout(loadUserBalance, 100);
        return;
    }

    // Import onAuthStateChanged
    import('https://www.gstatic.com/firebasejs/10.7.1/firebase-auth.js').then(({ onAuthStateChanged }) => {
        onAuthStateChanged(window.firebaseAuth, (user) => {
            if (user) {
                // User đã đăng nhập - Load balance từ Firebase
                console.log('User logged in:', user.email);
                loadUserBalanceFromFirebase(user.uid);
            } else {
                // User chưa đăng nhập
                console.log('User not logged in');
                document.getElementById('diamondBalance').textContent = '0';
                document.getElementById('goldBalance').textContent = '0';
            }
        });
    });
}

// Load balance từ Firebase
function loadUserBalanceFromFirebase(userId) {
    import('https://www.gstatic.com/firebasejs/10.7.1/firebase-database.js').then(({ ref, get }) => {
        const userRef = ref(window.firebaseRTDB, 'users/' + userId);
        get(userRef).then((snapshot) => {
            if (snapshot.exists()) {
                const userData = snapshot.val();
                document.getElementById('diamondBalance').textContent = formatNumber(userData.diamonds || 0);
                document.getElementById('goldBalance').textContent = formatNumber(userData.gold || 0);
            } else {
                document.getElementById('diamondBalance').textContent = '0';
                document.getElementById('goldBalance').textContent = '0';
            }
        }).catch((error) => {
            console.error('Error loading balance:', error);
            document.getElementById('diamondBalance').textContent = '0';
            document.getElementById('goldBalance').textContent = '0';
        });
    });
}

// Select package function
function selectPackage(packageId, price, diamonds) {
    // Remove previous selection
    document.querySelectorAll('.package-card').forEach(card => {
        card.classList.remove('selected');
    });

    // Add selection to current package
    const selectedCard = document.querySelector(`[data-package="${packageId}"]`);
    if (selectedCard) {
        selectedCard.classList.add('selected');
    }

    // Store package info
    selectedPackage = {
        id: packageId,
        price: price,
        diamonds: diamonds
    };

    // Update order summary
    updateOrderSummary();

    // Scroll to payment methods
    document.getElementById('paymentMethods').scrollIntoView({ 
        behavior: 'smooth', 
        block: 'center' 
    });
}

// Select payment method function
function selectPaymentMethod(method) {
    // Remove previous selection
    document.querySelectorAll('.payment-card').forEach(card => {
        card.classList.remove('selected');
    });

    // Add selection to current method
    event.currentTarget.classList.add('selected');

    // Store payment method
    selectedPayment = method;

    // Update order summary
    updateOrderSummary();
}

// Update order summary
function updateOrderSummary() {
    const packageNameEl = document.getElementById('selectedPackageName');
    const paymentMethodEl = document.getElementById('selectedPaymentMethod');
    const totalAmountEl = document.getElementById('totalAmount');
    const totalDiamondsEl = document.getElementById('totalDiamonds');
    const confirmBtn = document.getElementById('btnConfirmPayment');

    // Update package name
    if (selectedPackage) {
        const packageNames = {
            1: 'Gói Khởi Đầu',
            2: 'Gói Tiết Kiệm',
            3: 'Gói Nâng Cấp',
            4: 'Gói Cao Cấp',
            5: 'Gói Đại Gia'
        };
        packageNameEl.textContent = packageNames[selectedPackage.id];
        totalAmountEl.textContent = formatCurrency(selectedPackage.price) + ' VNĐ';
        totalDiamondsEl.textContent = formatNumber(selectedPackage.diamonds);
    }

    // Update payment method
    if (selectedPayment) {
        const methodNames = {
            'momo': 'Ví MoMo',
            'banking': 'Chuyển khoản ngân hàng',
            'card': 'Thẻ cào điện thoại'
        };
        paymentMethodEl.textContent = methodNames[selectedPayment];
    }

    // Enable/disable confirm button
    if (selectedPackage && selectedPayment) {
        confirmBtn.disabled = false;
    } else {
        confirmBtn.disabled = true;
    }
}

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount);
}

// Format number
function formatNumber(number) {
    return new Intl.NumberFormat('vi-VN').format(number);
}

// Confirm payment
async function confirmPayment() {
    if (!selectedPackage || !selectedPayment) {
        Swal.fire({
            icon: 'warning',
            title: 'Thiếu thông tin',
            text: 'Vui lòng chọn gói nạp và phương thức thanh toán!',
            confirmButtonColor: '#667eea'
        });
        return;
    }

    // Check if user is logged in
    if (!window.firebaseAuth || !window.firebaseAuth.currentUser) {
        const result = await Swal.fire({
            icon: 'warning',
            title: 'Chưa đăng nhập',
            text: 'Bạn cần đăng nhập để thực hiện nạp tiền!',
            showCancelButton: true,
            confirmButtonText: 'Đăng nhập ngay',
            cancelButtonText: 'Hủy',
            confirmButtonColor: '#667eea',
            cancelButtonColor: '#6c757d'
        });

        if (result.isConfirmed) {
            window.location.href = 'index.html';
        }
        return;
    }
    
    console.log('User confirmed payment:', window.firebaseAuth.currentUser.email);

    // Process payment based on method - Tất cả đều qua VNPay
    switch (selectedPayment) {
        case 'momo':
        case 'banking':
            processVNPayPayment();
            break;
        case 'card':
            showCardPayment();
            break;
    }
}

// Process VNPay Payment
function processVNPayPayment() {
    // Thông tin cấu hình VNPay
    const vnp_TmnCode = "9SMU243L";
    const vnp_HashSecret = "54NPAQSBPH9OZN9SEAP4JJ6MUSEY6C2G";
    let vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    const vnp_ReturnUrl = window.location.origin + "/vnpay_recharge_return.html";

    // Tạo mã giao dịch unique
    const vnp_TxnRef = 'NAPTIEN_' + Date.now();
    const vnp_OrderInfo = `Nap tien game Vườn Rực Rỡ - Goi ${selectedPackage.id}`;
    const vnp_OrderType = "billpayment";
    const vnp_Amount = selectedPackage.price * 100; // VNPay yêu cầu nhân 100
    const vnp_Locale = "vn";
    const vnp_IpAddr = "127.0.0.1";
    const vnp_CreateDate = moment().format('YYYYMMDDHHmmss');
    
    // Tạo params
    let vnp_Params = {
        'vnp_Version': '2.1.0',
        'vnp_Command': 'pay',
        'vnp_TmnCode': vnp_TmnCode,
        'vnp_Locale': vnp_Locale,
        'vnp_CurrCode': 'VND',
        'vnp_TxnRef': vnp_TxnRef,
        'vnp_OrderInfo': vnp_OrderInfo,
        'vnp_OrderType': vnp_OrderType,
        'vnp_Amount': vnp_Amount,
        'vnp_ReturnUrl': vnp_ReturnUrl,
        'vnp_IpAddr': vnp_IpAddr,
        'vnp_CreateDate': vnp_CreateDate
    };

    // Sắp xếp params theo thứ tự alphabet
    vnp_Params = Object.keys(vnp_Params).sort().reduce(
        (obj, key) => { 
            obj[key] = vnp_Params[key]; 
            return obj;
        }, 
        {}
    );

    // Tạo query string
    let querystring = new URLSearchParams(vnp_Params).toString();
    
    // Tạo secure hash
    let hash = CryptoJS.HmacSHA512(querystring, vnp_HashSecret);
    let vnp_SecureHash = CryptoJS.enc.Hex.stringify(hash);

    // Tạo URL đầy đủ
    vnp_Url += '?' + querystring + '&vnp_SecureHash=' + vnp_SecureHash;
    
    // Lưu thông tin gói nạp vào localStorage để xử lý sau
    const rechargeData = {
        txnRef: vnp_TxnRef,
        packageId: selectedPackage.id,
        price: selectedPackage.price,
        diamonds: selectedPackage.diamonds,
        paymentMethod: selectedPayment,
        timestamp: Date.now()
    };
    
    localStorage.setItem('pendingRecharge_' + vnp_TxnRef, JSON.stringify(rechargeData));
    
    // Chuyển hướng đến VNPay
    window.location.href = vnp_Url;
}

// Show MoMo payment info
function showMomoPayment() {
    Swal.fire({
        title: '<div style="font-size: 1.8rem; font-weight: 700; color: #a61e69;">Thanh Toán Qua MoMo</div>',
        html: `
            <div style="text-align: center; padding: 20px;">
                <div style="background: linear-gradient(135deg, #a61e69 0%, #d91f7a 100%); 
                            padding: 20px; 
                            border-radius: 15px; 
                            margin-bottom: 20px;">
                    <img src="https://upload.wikimedia.org/wikipedia/vi/f/fe/MoMo_Logo.png" 
                         alt="MoMo" 
                         style="width: 120px; 
                                height: 120px; 
                                object-fit: contain; 
                                background: white; 
                                padding: 10px; 
                                border-radius: 10px;">
                </div>
                
                <div style="background: #f8f9fa; 
                            padding: 25px; 
                            border-radius: 12px; 
                            margin: 20px 0; 
                            text-align: left;">
                    <h3 style="color: #2c3e50; margin-bottom: 15px;">
                        <i class="fas fa-info-circle" style="color: #a61e69;"></i>
                        Thông Tin Thanh Toán
                    </h3>
                    <p style="margin: 10px 0; color: #5a6c7d;">
                        <strong>Số điện thoại:</strong> <span style="color: #a61e69; font-weight: 600;">0123 456 789</span>
                    </p>
                    <p style="margin: 10px 0; color: #5a6c7d;">
                        <strong>Tên:</strong> <span style="color: #2c3e50; font-weight: 600;">LÀNG HOA RỰC</span>
                    </p>
                    <p style="margin: 10px 0; color: #5a6c7d;">
                        <strong>Số tiền:</strong> <span style="color: #e74c3c; font-weight: 700; font-size: 1.3rem;">${formatCurrency(selectedPackage.price)} VNĐ</span>
                    </p>
                    <p style="margin: 10px 0; color: #5a6c7d;">
                        <strong>Nội dung:</strong> <span style="color: #667eea; font-weight: 600;">NAPTIEN ${selectedPackage.id}</span>
                    </p>
                </div>

                <div style="background: #fff3cd; 
                            padding: 15px; 
                            border-radius: 10px; 
                            border-left: 4px solid #f59e0b;">
                    <p style="margin: 0; color: #856404; font-size: 0.95rem;">
                        <i class="fas fa-exclamation-triangle" style="color: #f59e0b;"></i>
                        <strong>Lưu ý:</strong> Vui lòng chuyển đúng số tiền và nội dung để được cộng Kim Cương tự động
                    </p>
                </div>
            </div>
        `,
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-check"></i> Đã Chuyển Khoản',
        cancelButtonText: 'Hủy',
        confirmButtonColor: '#a61e69',
        cancelButtonColor: '#6c757d',
        width: '600px',
        customClass: {
            popup: 'payment-popup'
        }
    }).then((result) => {
        if (result.isConfirmed) {
            processPaymentConfirmation();
        }
    });
}

// Show Banking payment info
function showBankingPayment() {
    Swal.fire({
        title: '<div style="font-size: 1.8rem; font-weight: 700; color: #3b82f6;">Chuyển Khoản Ngân Hàng</div>',
        html: `
            <div style="text-align: center; padding: 20px;">
                <div style="background: linear-gradient(135deg, #3b82f6 0%, #60a5fa 100%); 
                            padding: 20px; 
                            border-radius: 15px; 
                            margin-bottom: 20px;">
                    <i class="fas fa-university" style="font-size: 80px; color: white;"></i>
                </div>
                
                <div style="background: #f8f9fa; 
                            padding: 25px; 
                            border-radius: 12px; 
                            margin: 20px 0; 
                            text-align: left;">
                    <h3 style="color: #2c3e50; margin-bottom: 15px;">
                        <i class="fas fa-info-circle" style="color: #3b82f6;"></i>
                        Thông Tin Tài Khoản
                    </h3>
                    <p style="margin: 10px 0; color: #5a6c7d;">
                        <strong>Ngân hàng:</strong> <span style="color: #3b82f6; font-weight: 600;">Vietcombank</span>
                    </p>
                    <p style="margin: 10px 0; color: #5a6c7d;">
                        <strong>Số tài khoản:</strong> <span style="color: #3b82f6; font-weight: 600;">1234567890</span>
                    </p>
                    <p style="margin: 10px 0; color: #5a6c7d;">
                        <strong>Chủ tài khoản:</strong> <span style="color: #2c3e50; font-weight: 600;">NGUYEN VAN A</span>
                    </p>
                    <p style="margin: 10px 0; color: #5a6c7d;">
                        <strong>Số tiền:</strong> <span style="color: #e74c3c; font-weight: 700; font-size: 1.3rem;">${formatCurrency(selectedPackage.price)} VNĐ</span>
                    </p>
                    <p style="margin: 10px 0; color: #5a6c7d;">
                        <strong>Nội dung:</strong> <span style="color: #667eea; font-weight: 600;">NAPTIEN ${selectedPackage.id}</span>
                    </p>
                </div>

                <div style="background: #fff3cd; 
                            padding: 15px; 
                            border-radius: 10px; 
                            border-left: 4px solid #f59e0b;">
                    <p style="margin: 0; color: #856404; font-size: 0.95rem;">
                        <i class="fas fa-exclamation-triangle" style="color: #f59e0b;"></i>
                        <strong>Lưu ý:</strong> Kim Cương sẽ được cộng sau 5-15 phút kể từ khi chuyển khoản thành công
                    </p>
                </div>
            </div>
        `,
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-check"></i> Đã Chuyển Khoản',
        cancelButtonText: 'Hủy',
        confirmButtonColor: '#3b82f6',
        cancelButtonColor: '#6c757d',
        width: '600px',
        customClass: {
            popup: 'payment-popup'
        }
    }).then((result) => {
        if (result.isConfirmed) {
            processPaymentConfirmation();
        }
    });
}

// Show Card payment info
function showCardPayment() {
    Swal.fire({
        title: '<div style="font-size: 1.8rem; font-weight: 700; color: #10b981;">Nạp Thẻ Cào</div>',
        html: `
            <div style="padding: 20px;">
                <div style="background: linear-gradient(135deg, #10b981 0%, #34d399 100%); 
                            padding: 20px; 
                            border-radius: 15px; 
                            margin-bottom: 20px;">
                    <i class="fas fa-sim-card" style="font-size: 80px; color: white;"></i>
                </div>
                
                <div style="text-align: left; margin: 20px 0;">
                    <div style="margin-bottom: 15px;">
                        <label style="display: block; margin-bottom: 5px; color: #2c3e50; font-weight: 600;">
                            Loại thẻ:
                        </label>
                        <select id="cardType" style="width: 100%; 
                                                      padding: 12px; 
                                                      border: 2px solid #e1e8ed; 
                                                      border-radius: 8px; 
                                                      font-size: 1rem;">
                            <option value="viettel">Viettel</option>
                            <option value="mobifone">Mobifone</option>
                            <option value="vinaphone">Vinaphone</option>
                        </select>
                    </div>
                    
                    <div style="margin-bottom: 15px;">
                        <label style="display: block; margin-bottom: 5px; color: #2c3e50; font-weight: 600;">
                            Mệnh giá:
                        </label>
                        <select id="cardValue" style="width: 100%; 
                                                       padding: 12px; 
                                                       border: 2px solid #e1e8ed; 
                                                       border-radius: 8px; 
                                                       font-size: 1rem;">
                            <option value="10000">10,000 VNĐ</option>
                            <option value="20000">20,000 VNĐ</option>
                            <option value="50000">50,000 VNĐ</option>
                            <option value="100000">100,000 VNĐ</option>
                            <option value="200000">200,000 VNĐ</option>
                            <option value="500000">500,000 VNĐ</option>
                        </select>
                    </div>
                    
                    <div style="margin-bottom: 15px;">
                        <label style="display: block; margin-bottom: 5px; color: #2c3e50; font-weight: 600;">
                            Mã thẻ:
                        </label>
                        <input type="text" 
                               id="cardCode" 
                               placeholder="Nhập mã thẻ" 
                               style="width: 100%; 
                                      padding: 12px; 
                                      border: 2px solid #e1e8ed; 
                                      border-radius: 8px; 
                                      font-size: 1rem;">
                    </div>
                    
                    <div style="margin-bottom: 15px;">
                        <label style="display: block; margin-bottom: 5px; color: #2c3e50; font-weight: 600;">
                            Số seri:
                        </label>
                        <input type="text" 
                               id="cardSerial" 
                               placeholder="Nhập số seri" 
                               style="width: 100%; 
                                      padding: 12px; 
                                      border: 2px solid #e1e8ed; 
                                      border-radius: 8px; 
                                      font-size: 1rem;">
                    </div>
                </div>

                <div style="background: #fff3cd; 
                            padding: 15px; 
                            border-radius: 10px; 
                            border-left: 4px solid #f59e0b;">
                    <p style="margin: 0; color: #856404; font-size: 0.9rem;">
                        <i class="fas fa-exclamation-triangle" style="color: #f59e0b;"></i>
                        <strong>Lưu ý:</strong> Thẻ cào chỉ nhận 80-90% giá trị. Vui lòng kiểm tra kỹ thông tin trước khi gửi
                    </p>
                </div>
            </div>
        `,
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-paper-plane"></i> Gửi Thẻ',
        cancelButtonText: 'Hủy',
        confirmButtonColor: '#10b981',
        cancelButtonColor: '#6c757d',
        width: '600px',
        customClass: {
            popup: 'payment-popup'
        },
        preConfirm: () => {
            const cardType = document.getElementById('cardType').value;
            const cardValue = document.getElementById('cardValue').value;
            const cardCode = document.getElementById('cardCode').value.trim();
            const cardSerial = document.getElementById('cardSerial').value.trim();

            if (!cardCode || !cardSerial) {
                Swal.showValidationMessage('Vui lòng nhập đầy đủ mã thẻ và số seri');
                return false;
            }

            return { cardType, cardValue, cardCode, cardSerial };
        }
    }).then((result) => {
        if (result.isConfirmed) {
            processCardPayment(result.value);
        }
    });
}

// Process card payment
function processCardPayment(cardData) {
    Swal.fire({
        title: 'Đang xử lý...',
        html: '<div style="color: #667eea; font-size: 2.5rem;">⏳</div>',
        showConfirmButton: false,
        allowOutsideClick: false,
        timer: 2000,
        didOpen: () => {
            Swal.showLoading();
        }
    }).then(() => {
        // TODO: Send card data to server for verification
        Swal.fire({
            icon: 'success',
            title: 'Gửi Thẻ Thành Công!',
            html: `
                <p style="font-size: 1.1rem; margin: 15px 0;">
                    Thẻ cào của bạn đang được xác thực.
                </p>
                <p style="color: #5a6c7d;">
                    Kim Cương sẽ được cộng sau khi xác thực thành công (5-30 phút).
                </p>
            `,
            confirmButtonText: 'Đã hiểu',
            confirmButtonColor: '#667eea'
        });
    });
}

// Process payment confirmation
function processPaymentConfirmation() {
    Swal.fire({
        title: 'Đang xử lý...',
        html: '<div style="color: #667eea; font-size: 2.5rem;">⏳</div>',
        showConfirmButton: false,
        allowOutsideClick: false,
        timer: 2000,
        didOpen: () => {
            Swal.showLoading();
        }
    }).then(() => {
        // TODO: Verify payment with server
        Swal.fire({
            icon: 'success',
            title: 'Đã Ghi Nhận!',
            html: `
                <p style="font-size: 1.1rem; margin: 15px 0;">
                    Chúng tôi đã ghi nhận giao dịch của bạn.
                </p>
                <p style="color: #5a6c7d;">
                    Kim Cương sẽ được cộng tự động sau khi xác nhận thanh toán (5-15 phút).
                </p>
                <p style="color: #10b981; font-weight: 600; margin-top: 15px;">
                    <i class="fas fa-gem"></i>
                    Bạn sẽ nhận được: ${formatNumber(selectedPackage.diamonds)} Kim Cương
                </p>
            `,
            confirmButtonText: 'Tuyệt vời!',
            confirmButtonColor: '#667eea'
        }).then(() => {
            // Reset selections
            selectedPackage = null;
            selectedPayment = null;
            
            // Remove selections from UI
            document.querySelectorAll('.package-card').forEach(card => {
                card.classList.remove('selected');
            });
            document.querySelectorAll('.payment-card').forEach(card => {
                card.classList.remove('selected');
            });
            
            // Reset order summary
            document.getElementById('selectedPackageName').textContent = 'Chưa chọn';
            document.getElementById('selectedPaymentMethod').textContent = 'Chưa chọn';
            document.getElementById('totalAmount').textContent = '0 VNĐ';
            document.getElementById('totalDiamonds').textContent = '0';
            document.getElementById('btnConfirmPayment').disabled = true;
        });
    });
}

// Initialize page
document.addEventListener('DOMContentLoaded', () => {
    loadUserBalance();
    
    console.log('💰 Recharge page loaded successfully!');
});

// Add custom styles for Swal popup
const style = document.createElement('style');
style.innerHTML = `
    .payment-popup {
        border-radius: 20px !important;
    }
`;
document.head.appendChild(style);

