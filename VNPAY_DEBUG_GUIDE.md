# 🔧 Hướng dẫn Debug VNPay - Lỗi "Sai chữ ký"

## 🚨 Vấn đề hiện tại
- **Lỗi**: "Sai chữ ký" 
- **Mã tra cứu**: NgkZIcYgG7
- **Thời gian**: 22/10/2025 10:15:31 SA

## 🔍 Nguyên nhân thường gặp

### 1. **Sai thuật toán mã hóa**
- ❌ Sử dụng SHA256 thay vì SHA512
- ❌ Không sử dụng HMAC
- ✅ **Giải pháp**: Sử dụng HMAC-SHA512

### 2. **Sai thứ tự tham số**
- ❌ Không sắp xếp tham số theo alphabet
- ❌ Bao gồm tham số rỗng trong hash
- ✅ **Giải pháp**: Sắp xếp theo alphabet, loại bỏ tham số rỗng

### 3. **Sai encoding**
- ❌ Không encode URL đúng cách
- ❌ Khoảng trắng được encode sai
- ✅ **Giải pháp**: Sử dụng encodeURIComponent()

### 4. **Sai secret key**
- ❌ Secret key không đúng
- ❌ Secret key bị cắt ngắn
- ✅ **Giải pháp**: Kiểm tra secret key từ VNPay

## 🛠️ Cách debug

### Bước 1: Kiểm tra Console
Mở Developer Tools (F12) và kiểm tra console logs:

```javascript
// Chạy test function
testVNPaySignature();
```

### Bước 2: Kiểm tra tham số
```javascript
// Kiểm tra tham số được gửi
console.log('vnp_Params:', vnp_Params);
console.log('Sign data:', signData);
console.log('Secret key length:', VNPAY_CONFIG.hashSecret.length);
```

### Bước 3: So sánh với tài liệu VNPay
- Kiểm tra format tham số
- Kiểm tra thứ tự sắp xếp
- Kiểm tra encoding

## 🔧 Code đã sửa

### Function createSecureHash (Đã sửa)
```javascript
function createSecureHash(params) {
    // Lọc bỏ các tham số rỗng và null
    const filteredParams = {};
    Object.keys(params).forEach(key => {
        const value = params[key];
        if (value !== '' && value !== null && value !== undefined) {
            filteredParams[key] = value;
        }
    });

    // Sắp xếp tham số theo thứ tự bảng chữ cái
    const sortedKeys = Object.keys(filteredParams).sort();
    
    // Tạo chuỗi ký tự cần mã hóa (theo chuẩn VNPay)
    const signData = sortedKeys
        .map(key => `${key}=${filteredParams[key]}`)
        .join('&');

    console.log('=== VNPAY HASH DEBUG ===');
    console.log('Filtered params:', filteredParams);
    console.log('Sorted keys:', sortedKeys);
    console.log('Sign data:', signData);
    console.log('Secret key length:', VNPAY_CONFIG.hashSecret.length);
    console.log('========================');
    
    // Tạo chữ ký theo chuẩn VNPay với HMAC-SHA512
    if (typeof CryptoJS !== 'undefined') {
        try {
            const hmac = CryptoJS.HmacSHA512(signData, VNPAY_CONFIG.hashSecret);
            const secureHash = hmac.toString(CryptoJS.enc.Hex).toUpperCase();
            console.log('Generated hash:', secureHash);
            console.log('Hash length:', secureHash.length);
            return secureHash;
        } catch (error) {
            console.error('Error creating HMAC:', error);
            throw new Error('Failed to create secure hash');
        }
    } else {
        throw new Error('CryptoJS not available');
    }
}
```

## 🧪 Test Functions

### 1. Test với dữ liệu mẫu
```javascript
// Chạy trong console
testVNPaySignature();
```

### 2. Test với tham số tối thiểu
```javascript
// Chạy trong console
testVNPayWithMinimalParams();
```

### 3. Test chữ ký thủ công
```javascript
// Test với dữ liệu cụ thể
const testParams = {
    vnp_Version: '2.1.0',
    vnp_Command: 'pay',
    vnp_TmnCode: 'G0DXNG46',
    vnp_Amount: '100000',
    vnp_CreateDate: '20250122120000',
    vnp_CurrCode: 'VND',
    vnp_IpAddr: '127.0.0.1',
    vnp_Locale: 'vn',
    vnp_OrderInfo: 'Nap tien tai khoan',
    vnp_OrderType: 'other',
    vnp_ReturnUrl: window.location.origin + window.location.pathname,
    vnp_TxnRef: 'TEST123456'
};

const signature = createSecureHash(testParams);
console.log('Test signature:', signature);
```

## 📋 Checklist Debug

- [ ] Kiểm tra secret key có đúng không
- [ ] Kiểm tra tham số có đầy đủ không
- [ ] Kiểm tra thứ tự sắp xếp alphabet
- [ ] Kiểm tra encoding URL
- [ ] Kiểm tra format chữ ký (128 ký tự hex)
- [ ] Kiểm tra CryptoJS có load không
- [ ] Kiểm tra console logs

## 🚀 Cách test

1. **Mở trang recharge.html**
2. **Mở Developer Tools (F12)**
3. **Chạy test functions trong console**
4. **Kiểm tra logs để tìm lỗi**
5. **So sánh với tài liệu VNPay**

## 📞 Liên hệ hỗ trợ

Nếu vẫn gặp lỗi, cung cấp thông tin:
- Console logs
- Tham số được gửi
- Chữ ký được tạo
- Secret key (chỉ 4 ký tự đầu và cuối)
