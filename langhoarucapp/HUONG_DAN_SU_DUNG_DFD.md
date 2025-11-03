# HƯỚNG DẪN SỬ DỤNG FILE DFD

## 1. CÁC FILE ĐÃ TẠO

1. **DFD_LangHoaRucLauncher.drawio**: File Draw.io chứa tất cả các DFD
2. **PHAN_TICH_LUONG_DU_LIEU.md**: Tài liệu phân tích chi tiết luồng dữ liệu

## 2. CÁCH MỞ FILE DFD

### Cách 1: Mở trên Draw.io Online
1. Truy cập: https://app.diagrams.net/ hoặc https://draw.io
2. Chọn **File → Open from → Device**
3. Chọn file `DFD_LangHoaRucLauncher.drawio`
4. File sẽ được tải và hiển thị các diagram

### Cách 2: Mở trên Draw.io Desktop
1. Tải Draw.io Desktop từ: https://github.com/jgraph/drawio-desktop/releases
2. Cài đặt và mở ứng dụng
3. Chọn **File → Open**
4. Chọn file `DFD_LangHoaRucLauncher.drawio`

### Cách 3: Mở trên VS Code
1. Cài đặt extension "Draw.io Integration" trong VS Code
2. Click chuột phải vào file `.drawio`
3. Chọn "Open with Draw.io"

## 3. CẤU TRÚC FILE DFD

File Draw.io chứa **5 diagram**:

### 3.1. DFD Mức 0 - Context Diagram
- **Tên diagram**: `DFD_Muc0`
- **Mô tả**: Sơ đồ tổng quan hệ thống với external entities
- **Thành phần**:
  - External Entities: Người dùng, Google Drive, Firebase Database
  - Process: Hệ thống Launcher Game
  - Data Flows: Tất cả luồng dữ liệu vào/ra

### 3.2. DFD Mức 1 - Top Level Decomposition
- **Tên diagram**: `DFD_Muc1`
- **Mô tả**: Phân rã hệ thống thành 7 process chính
- **Processes**:
  - 1.0: Quản lý giao diện người dùng
  - 2.0: Quản lý cấu hình
  - 3.0: Kiểm tra cập nhật
  - 4.0: Tải file game
  - 5.0: Giải nén và cài đặt
  - 6.0: Chạy game
  - 7.0: Tải tin tức
- **Data Stores**:
  - D1: Cấu hình (config.json)
  - D2: Thư mục cài đặt game
  - D3: File tạm (temp)

### 3.3. DFD Mức 2 - Process 3.0 Kiểm tra cập nhật
- **Tên diagram**: `DFD_Muc2_3`
- **Mô tả**: Chi tiết quá trình kiểm tra cập nhật
- **Sub-processes**:
  - 3.1: Lấy thông tin file từ Drive
  - 3.2: Đọc file đã cài
  - 3.3: So sánh phiên bản

### 3.4. DFD Mức 2 - Process 4.0 Tải file game
- **Tên diagram**: `DFD_Muc2_4`
- **Mô tả**: Chi tiết quá trình tải file
- **Sub-processes**:
  - 4.1: Tìm file ZIP trong folder
  - 4.2: Xác thực token download
  - 4.3: Tải file streaming
  - 4.4: Kiểm tra file hợp lệ

### 3.5. DFD Mức 2 - Process 5.0 Giải nén và cài đặt
- **Tên diagram**: `DFD_Muc2_5`
- **Mô tả**: Chi tiết quá trình giải nén
- **Sub-processes**:
  - 5.1: Mở file ZIP kiểm tra
  - 5.2: Giải nén từng file
  - 5.3: Ghi metadata

## 4. KÝ HIỆU SỬ DỤNG

### External Entities (Thực thể ngoài)
- **Màu**: Xanh lá (#d5e8d4)
- **Ví dụ**: Người dùng, Google Drive, Firebase Database

### Processes (Quá trình xử lý)
- **Màu**: Vàng (#fff2cc)
- **Định dạng**: Số.0 Tên (ví dụ: 3.0 Kiểm tra cập nhật)
- **Sub-process**: Số.Chữ Tên (ví dụ: 3.1 Lấy thông tin file)

### Data Stores (Kho dữ liệu)
- **Hình dạng**: Hình trụ (cylinder)
- **Màu**: Đỏ nhạt (#f8cecc)
- **Ví dụ**: D1, D2, D3

### Data Flows (Luồng dữ liệu)
- **Mũi tên**: Chỉ hướng luồng dữ liệu
- **Nhãn**: Tên dữ liệu được truyền

## 5. CÁCH XEM TỪNG DIAGRAM

Trong Draw.io:
1. Mở file
2. Ở panel bên trái, tìm tab **Layers** hoặc **Pages**
3. Click vào tên diagram muốn xem:
   - `DFD Mức 0 - Context Diagram`
   - `DFD Mức 1 - Top Level Decomposition`
   - `DFD Mức 2 - Process 3.0 Kiểm tra cập nhật`
   - `DFD Mức 2 - Process 4.0 Tải file game`
   - `DFD Mức 2 - Process 5.0 Giải nén và cài đặt`

## 6. CHỈNH SỬA DIAGRAM

Bạn có thể:
- Di chuyển các thành phần
- Thay đổi kích thước
- Thêm/sửa/xóa các luồng dữ liệu
- Thay đổi màu sắc, font chữ
- Thêm annotation, notes

**Lưu ý**: 
- Lưu file sau khi chỉnh sửa: **File → Save** (Ctrl+S)
- Export sang PDF/PNG: **File → Export as → PDF** hoặc **PNG**

## 7. LIÊN KẾT VỚI TÀI LIỆU PHÂN TÍCH

Xem chi tiết từng process trong file **PHAN_TICH_LUONG_DU_LIEU.md**:
- Mục 3: Chi tiết các process
- Mục 4: Luồng dữ liệu theo kịch bản
- Mục 7: Cấu trúc dữ liệu

## 8. GỢI Ý SỬ DỤNG

1. **Trình bày**: Export các diagram sang PNG hoặc PDF để chèn vào báo cáo
2. **Phân tích**: Kết hợp với file phân tích để hiểu rõ luồng dữ liệu
3. **Phát triển**: Sử dụng làm tài liệu thiết kế khi phát triển tính năng mới
4. **Bảo trì**: Cập nhật DFD khi có thay đổi trong hệ thống

---

**Chúc bạn sử dụng hiệu quả!**

