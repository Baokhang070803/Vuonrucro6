# PHÂN TÍCH LUỒNG DỮ LIỆU CHI TIẾT - LÀNG HOA RỰC LAUNCHER

## 1. TỔNG QUAN HỆ THỐNG

Hệ thống Launcher Game "Làng Hoa Rực" là ứng dụng desktop quản lý việc tải, cài đặt và chạy game. Hệ thống tương tác với các thành phần bên ngoài và lưu trữ dữ liệu cục bộ.

---

## 2. CÁC THÀNH PHẦN CHÍNH

### 2.1. External Entities (Thực thể ngoài)

1. **Người dùng**: Người sử dụng launcher
   - Gửi: Yêu cầu khởi động, yêu cầu cài đặt, yêu cầu chạy game, chọn thư mục
   - Nhận: Giao diện, thông báo trạng thái, tiến độ tải

2. **Google Drive**: Nguồn lưu trữ file game
   - Gửi: HTML folder, file ZIP, token xác thực
   - Nhận: Yêu cầu folder, yêu cầu tải file

3. **Firebase Database**: Cơ sở dữ liệu tin tức
   - Gửi: Dữ liệu tin tức (JSON)
   - Nhận: Yêu cầu tin tức

### 2.2. Data Stores (Kho dữ liệu)

1. **Cấu hình (config.json)**
   - Vị trí: `%APPDATA%\LangHoaRucLauncher\config.json`
   - Lưu: Thư mục cài đặt, thiết lập người dùng
   - Đọc/Ghi: Bởi Process 2.0

2. **Thư mục cài đặt game**
   - Vị trí: Mặc định `C:\MyGameClient` hoặc do người dùng chọn
   - Chứa: File game đã giải nén, `version.txt`, `installed_file.txt`
   - Đọc/Ghi: Bởi Process 3.0, 5.0, 6.0

3. **File tạm (temp)**
   - Vị trí: `%TEMP%\game_update.zip`
   - Chứa: File ZIP tải về từ Google Drive
   - Đọc/Ghi: Bởi Process 4.0, 5.0

---

## 3. CHI TIẾT CÁC PROCESS

### 3.0. PROCESS 1.0: QUẢN LÝ GIAO DIỆN NGƯỜI DÙNG

**Mô tả**: Quản lý tất cả tương tác với người dùng, hiển thị UI, xử lý sự kiện.

**Luồng dữ liệu vào**:
- Yêu cầu khởi động (từ Người dùng)
- Yêu cầu cài đặt (từ Người dùng)
- Yêu cầu chạy game (từ Người dùng)
- Chọn thư mục (từ Người dùng)
- Kết quả kiểm tra (từ Process 3.0)
- Tiến độ tải (từ Process 4.0)
- Tiến độ giải nén (từ Process 5.0)
- Dữ liệu tin tức (từ Process 7.0)
- Cấu hình đã load (từ Process 2.0)

**Luồng dữ liệu ra**:
- Hiển thị UI (đến Người dùng)
- Thông báo trạng thái (đến Người dùng)
- Yêu cầu load config (đến Process 2.0)
- Yêu cầu lưu config (đến Process 2.0)
- Yêu cầu kiểm tra (đến Process 3.0)
- Yêu cầu tải (đến Process 4.0)
- Yêu cầu chạy game (đến Process 6.0)
- Yêu cầu tin tức (đến Process 7.0)

**Xử lý**:
- Khởi tạo giao diện PyQt5
- Hiển thị loading screen (5 giây)
- Render background image
- Xử lý sự kiện click, drag window
- Cập nhật progress bar, status label
- Hiển thị toast notifications

---

### 3.1. PROCESS 2.0: QUẢN LÝ CẤU HÌNH

**Mô tả**: Quản lý việc đọc và ghi cấu hình người dùng.

**Luồng dữ liệu vào**:
- Yêu cầu load config (từ Process 1.0)
- Yêu cầu lưu config (từ Process 1.0)
- Dữ liệu config từ Data Store D1

**Luồng dữ liệu ra**:
- Cấu hình đã load (đến Process 1.0)
- Dữ liệu config ghi vào Data Store D1

**Chi tiết**:
- **Load config**: Đọc `config.json` từ AppData, parse JSON
- **Save config**: Ghi JSON vào `config.json`, tạo thư mục nếu chưa có
- **Format**: 
  ```json
  {
    "install_dir": "C:\\MyGameClient"
  }
  ```

---

### 3.2. PROCESS 3.0: KIỂM TRA CẬP NHẬT

**Mô tả**: Kiểm tra xem có phiên bản mới của game trên Google Drive hay không.

**Luồng dữ liệu vào**:
- Yêu cầu kiểm tra (từ Process 1.0)
- HTML folder (từ Google Drive)
- Thông tin file đã cài (từ Data Store D2)

**Luồng dữ liệu ra**:
- Kết quả kiểm tra (đến Process 1.0)
- Request folder HTML (đến Google Drive)
- Đọc installed_file.txt (từ Data Store D2)

**Chi tiết xử lý (DFD Mức 2)**:

#### 3.2.1. Process 3.1: Lấy thông tin file từ Drive
- Gửi HTTP GET đến `https://drive.google.com/drive/folders/{FOLDER_ID}`
- Parse HTML để tìm file ID (regex: `\[null,"([a-zA-Z0-9_-]{28,44})"\]`)
- Parse tên file ZIP (regex: `\b([a-zA-Z][a-zA-Z0-9_\-\.]*\.zip)\b`)
- Trả về: File ID, tên file mới nhất

#### 3.2.2. Process 3.2: Đọc file đã cài
- Đọc file `installed_file.txt` từ thư mục cài đặt
- Trả về: Tên file đã cài đặt (nếu có)

#### 3.2.3. Process 3.3: So sánh phiên bản
- So sánh tên file mới nhất với tên file đã cài
- Kết quả:
  - CÓ CẬP NHẬT: Tên file khác nhau → emit `update_available`
  - KHÔNG CẬP NHẬT: Tên file giống nhau → emit `no_update`
  - CHƯA CÀI: Không có installed_file.txt → emit `update_available`

---

### 3.3. PROCESS 4.0: TẢI FILE GAME

**Mô tả**: Tải file game từ Google Drive về thư mục tạm.

**Luồng dữ liệu vào**:
- Yêu cầu tải (từ Process 1.0)
- HTML folder (từ Google Drive)
- Response headers với token (từ Google Drive)
- File stream (chunks) từ Google Drive

**Luồng dữ liệu ra**:
- File đã tải xong (đến Process 1.0)
- Tiến độ tải, trạng thái (đến Process 1.0)
- Request folder HTML (đến Google Drive)
- Request file với token (đến Google Drive)
- Request download stream (đến Google Drive)
- Ghi chunks (đến Data Store D3)
- Yêu cầu giải nén (đến Process 5.0)

**Chi tiết xử lý (DFD Mức 2)**:

#### 3.3.1. Process 4.1: Tìm file ZIP trong folder
- Giống Process 3.1
- Trả về: File ID của file ZIP

#### 3.3.2. Process 4.2: Xác thực token download
- Gửi request đến `https://drive.google.com/uc?export=download&id={FILE_ID}`
- Kiểm tra response headers:
  - Nếu có `Content-Disposition`: File nhỏ, tải trực tiếp
  - Nếu không: File lớn, cần token
- Lấy token từ:
  - Cookie: `download_warning`
  - HTML: `name="confirm" value="..."`
  - Link trong HTML: `href="/uc?export=download&confirm=..."`
- Nếu không có token, thử `drive.usercontent.google.com`

#### 3.3.3. Process 4.3: Tải file streaming
- Gửi request với token: `?id={FILE_ID}&confirm={TOKEN}`
- Đọc stream chunks (64KB mỗi chunk)
- Xử lý pause/resume:
  - Nếu `_is_paused = True`: Dừng vòng lặp, chờ resume
  - Nếu `_is_cancelled = True`: Dừng, xóa file, thoát
- Tính toán tiến độ:
  - `progress = (downloaded / total) * 100`
  - Tốc độ: `speed = downloaded / time`
- Ghi chunks vào file tạm (`%TEMP%\game_update.zip`)
- Emit signal: `progress(int)`, `status(str)` mỗi 0.6 giây

#### 3.3.4. Process 4.4: Kiểm tra file hợp lệ
- Kiểm tra kích thước file:
  - Nếu < 64KB: Có thể là HTML error page
- Đọc 2048 bytes đầu:
  - Nếu chứa `<html` và `google`: Lỗi (trả về HTML)
  - Nếu không: File hợp lệ
- Nếu lỗi: Lưu debug HTML vào temp, raise exception
- Nếu hợp lệ: Emit `finished` signal

---

### 3.4. PROCESS 5.0: GIẢI NÉN VÀ CÀI ĐẶT

**Mô tả**: Giải nén file ZIP và cài đặt vào thư mục đích, tạo metadata.

**Luồng dữ liệu vào**:
- Yêu cầu giải nén (từ Process 4.0)
- File ZIP (từ Data Store D3)

**Luồng dữ liệu ra**:
- Hoàn tất cài đặt (đến Process 1.0)
- Tiến độ giải nén (đến Process 1.0)
- File game đã giải nén (đến Data Store D2)
- Metadata files (đến Data Store D2)

**Chi tiết xử lý (DFD Mức 2)**:

#### 3.4.1. Process 5.1: Mở file ZIP kiểm tra
- Mở file ZIP bằng `zipfile.ZipFile`
- Lấy danh sách files: `zf.infolist()`
- Lọc chỉ lấy files (không phải thư mục): `[m for m in members if not m.is_dir()]`
- Trả về: Danh sách files cần giải nén

#### 3.4.2. Process 5.2: Giải nén từng file
- Duyệt từng file trong danh sách:
  - Kiểm tra path traversal: Đảm bảo `out_path` nằm trong `target_dir`
  - Giải nén: `zf.extract(m, target_dir)`
  - Tính tiến độ: `progress = (done / total) * 100`
  - Emit signal mỗi file
- Trả về: Tên file ZIP đã giải nén (để lưu metadata)

#### 3.4.3. Process 5.3: Ghi metadata
- Ghi `version.txt`: Chứa version mới nhất ("latest")
- Ghi `installed_file.txt`: Chứa tên file đã cài (từ Process 4.0)
- Lưu config: Cập nhật `install_dir` vào config.json
- Emit `finished` signal với đường dẫn thư mục cài đặt

---

### 3.5. PROCESS 6.0: CHẠY GAME

**Mô tả**: Tìm file executable và khởi chạy game.

**Luồng dữ liệu vào**:
- Yêu cầu chạy game (từ Process 1.0)
- Thông tin game (từ Data Store D2)

**Luồng dữ liệu ra**:
- Không có (chạy process con)

**Chi tiết xử lý**:
1. Tìm file executable:
   - Ưu tiên: Tìm file có tên `Vườn Rực Rỡ.exe` (không phân biệt hoa thường)
   - Fallback: Tìm file `.exe` đầu tiên trong cây thư mục
   - Duyệt: `os.walk(install_dir)`
2. Khởi chạy:
   - Sử dụng `subprocess.Popen([game_path])`
   - Đóng launcher sau 1 giây: `QTimer.singleShot(1000, self.close)`

---

### 3.6. PROCESS 7.0: TẢI TIN TỨC

**Mô tả**: Tải tin tức từ Firebase Realtime Database.

**Luồng dữ liệu vào**:
- Yêu cầu tin tức (từ Process 1.0)
- News JSON data (từ Firebase Database)

**Luồng dữ liệu ra**:
- Dữ liệu tin tức (đến Process 1.0)
- Request News JSON (đến Firebase Database)

**Chi tiết xử lý**:
1. Gửi HTTP GET đến:
   ```
   https://trangtrai-2769b-default-rtdb.firebaseio.com/News.json
   ```
2. Parse JSON response:
   - Lọc chỉ tin có `isActive = true`
   - Sắp xếp theo `priority` (thấp nhất = quan trọng nhất)
   - Lấy 3 tin đầu tiên
3. Format hiển thị:
   - Mỗi tin: Chỉ hiển thị `title`
   - Ngăn cách bằng newline
4. Fallback:
   - Nếu lỗi: Hiển thị tin tức mặc định hardcoded
5. Cập nhật UI:
   - Hiển thị trong `info_label` widget

---

## 4. LUỒNG DỮ LIỆU CHI TIẾT THEO KỊCH BẢN

### 4.1. KỊCH BẢN: Khởi động launcher

```
1. Người dùng → Process 1.0: Yêu cầu khởi động
2. Process 1.0 → Process 2.0: Yêu cầu load config
3. Process 2.0 → Data Store D1: Đọc config
4. Data Store D1 → Process 2.0: Dữ liệu config
5. Process 2.0 → Process 1.0: Cấu hình đã load
6. Process 1.0 → Process 7.0: Yêu cầu tin tức
7. Process 7.0 → Firebase: Request News JSON
8. Firebase → Process 7.0: News JSON data
9. Process 7.0 → Process 1.0: Dữ liệu tin tức
10. Process 1.0 → Process 3.0: Yêu cầu kiểm tra (tự động sau 1s)
11. Process 3.0 → Google Drive: Request folder HTML
12. Google Drive → Process 3.0: HTML folder
13. Process 3.0 → Data Store D2: Đọc installed_file.txt
14. Data Store D2 → Process 3.0: Thông tin file đã cài
15. Process 3.0 → Process 1.0: Kết quả kiểm tra
16. Process 1.0 → Người dùng: Hiển thị UI, trạng thái
```

### 4.2. KỊCH BẢN: Cài đặt game

```
1. Người dùng → Process 1.0: Yêu cầu cài đặt
2. Process 1.0 → Process 4.0: Yêu cầu tải
3. Process 4.0 → Process 4.1: Tìm file ZIP
   a. Process 4.1 → Google Drive: Request folder HTML
   b. Google Drive → Process 4.1: HTML folder
   c. Process 4.1 → Process 4.2: File ID
4. Process 4.2 → Google Drive: Request file với token
5. Google Drive → Process 4.2: Response headers (token)
6. Process 4.2 → Process 4.3: File ID, token
7. Process 4.3 → Google Drive: Request download stream
8. Google Drive → Process 4.3: File stream (chunks)
9. Process 4.3 → Data Store D3: Ghi chunks
10. Process 4.3 → Process 1.0: Tiến độ tải (mỗi 0.6s)
11. Process 4.3 → Process 4.4: File path tạm
12. Process 4.4 → Data Store D3: Đọc để kiểm tra
13. Process 4.4 → Process 5.0: Yêu cầu giải nén
14. Process 5.0 → Process 5.1: Mở file ZIP
   a. Process 5.1 → Data Store D3: Đọc file ZIP
   b. Process 5.1 → Process 5.2: Danh sách files
15. Process 5.2 → Data Store D2: Ghi file giải nén
16. Process 5.2 → Process 1.0: Tiến độ giải nén
17. Process 5.2 → Process 5.3: Tên file đã cài
18. Process 5.3 → Data Store D2: Ghi version.txt, installed_file.txt
19. Process 5.3 → Process 1.0: Hoàn tất cài đặt
20. Process 1.0 → Người dùng: Thông báo thành công
```

### 4.3. KỊCH BẢN: Chạy game

```
1. Người dùng → Process 1.0: Yêu cầu chạy
2. Process 1.0 → Process 6.0: Yêu cầu chạy game
3. Process 6.0 → Data Store D2: Đọc file exe
4. Data Store D2 → Process 6.0: Đường dẫn file exe
5. Process 6.0: Khởi chạy subprocess
6. Process 1.0: Đóng launcher sau 1 giây
```

---

## 5. XỬ LÝ LỖI VÀ NGOẠI LỆ

### 5.1. Lỗi kết nối Google Drive
- **Phát hiện**: HTTP status code != 200, timeout
- **Xử lý**: 
  - Lưu debug HTML vào temp
  - Emit error signal
  - Hiển thị thông báo lỗi cho người dùng

### 5.2. File tải về là HTML (không phải ZIP)
- **Phát hiện**: Kích thước < 64KB, chứa `<html`
- **Xử lý**: 
  - Copy file vào debug location
  - Raise exception với thông báo rõ ràng

### 5.3. Không tìm thấy file ZIP trong folder
- **Phát hiện**: Không parse được file ID từ HTML
- **Xử lý**: 
  - Lưu HTML vào temp để debug
  - Raise exception

### 5.4. Lỗi giải nén
- **Phát hiện**: Exception từ zipfile module
- **Xử lý**: 
  - Emit error signal
  - Hiển thị thông báo lỗi

### 5.5. Lỗi Firebase
- **Phát hiện**: HTTP status code != 200, timeout
- **Xử lý**: 
  - Fallback: Hiển thị tin tức mặc định
  - Không ảnh hưởng đến chức năng chính

---

## 6. TỐI ƯU HÓA VÀ CẢI THIỆN

### 6.1. Threading
- Tất cả I/O operations chạy trong threads riêng:
  - `DownloadThread`: Tải file
  - `UpdateCheckThread`: Kiểm tra cập nhật
  - `ExtractThread`: Giải nén
- UI thread không bị block

### 6.2. Streaming Download
- Tải file theo chunks (64KB)
- Ghi trực tiếp vào disk
- Không lưu toàn bộ trong memory

### 6.3. Progress Updates
- Emit progress mỗi 0.6 giây (không quá nhiều)
- Hiển thị: %, dung lượng, tốc độ

### 6.4. Pause/Resume/Cancel
- Hỗ trợ tạm dừng tải
- Có thể hủy tải giữa chừng
- Xử lý cleanup file tạm

---

## 7. CẤU TRÚC DỮ LIỆU

### 7.1. Config JSON
```json
{
  "install_dir": "C:\\MyGameClient"
}
```

### 7.2. installed_file.txt
```
Game_v1.2.3.zip
```

### 7.3. version.txt
```
latest
```

### 7.4. News JSON (Firebase)
```json
{
  "news_001": {
    "id": "news_001",
    "title": "🎉 Chào mừng đến Làng Hoa Rực!",
    "content": "...",
    "date": "2025-10-28",
    "priority": 1,
    "isActive": true
  }
}
```

---

## 8. KẾT LUẬN

Hệ thống Launcher Game sử dụng kiến trúc client-server với:
- **Client**: Ứng dụng PyQt5 desktop
- **Server**: Google Drive (lưu trữ file), Firebase (tin tức)
- **Local Storage**: Config, thư mục cài đặt, file tạm

Luồng dữ liệu được tổ chức rõ ràng với các process độc lập, giao tiếp qua signals/slots (PyQt5) và data stores. Hệ thống xử lý tốt các trường hợp lỗi và cung cấp feedback rõ ràng cho người dùng.

