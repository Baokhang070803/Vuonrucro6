# Làng Hoa Rực Launcher

Launcher game viết bằng Python + PyQt5. Hỗ trợ:
- Tải gói game từ Google Drive (file lớn có confirm token)
- Giải nén tự động
- Chọn ổ hoặc thư mục cài đặt tùy chỉnh
- Giao diện theo phong cách game lớn (panel phải, nền artwork)
- Tìm file thực thi game sâu trong thư mục cài đặt

## 1. Yêu cầu môi trường
- Windows 10/11 64-bit
- Python 3.11+ (chỉ cần cho bước build; người dùng cuối có thể dùng bản EXE đóng gói)

## 2. Cài đặt để chạy dạng mã nguồn
```powershell
python -m venv .venv
.venv\Scripts\Activate.ps1
pip install -r requirements.txt
python launcher.py
```

## 3. Đóng gói thành file EXE chia sẻ
Chạy script build:
```powershell
powershell -ExecutionPolicy Bypass -File .\build_windows.ps1
```
Tùy chọn one-file (gộp 1 exe lớn):
```powershell
powershell -ExecutionPolicy Bypass -File .\build_windows.ps1 -OneFile
```
Kết quả nằm trong thư mục `dist/`.

Ưu và nhược điểm:
- `--onefile`: tiện chia sẻ, chạy chậm hơn lần đầu vì giải nén tạm.
- Không dùng `--onefile`: thư mục đầy đủ, khởi động nhanh hơn.

## 4. Phân phối cho người dùng cuối
Phát hành thư mục trong `dist/LangHoaRucLauncher/` (hoặc file EXE nếu onefile) kèm thư mục `img`. Đóng gói thành ZIP, người dùng chỉ cần giải nén và chạy `LangHoaRucLauncher.exe`.

Có thể đặt thêm `icon.ico` để build có icon tùy chỉnh.

## 5. Tùy chỉnh
- Thay nền: thay file trong `img/` và cập nhật hằng `BACKGROUND_IMAGE` trong `launcher.py`.
- Đổi ID Google Drive: sửa `GDRIVE_FILE_ID`.
- Đổi tên file game chính nếu khác: sửa `self.game_executable_name`.

## 6. Lỗi thường gặp
| Hiện tượng | Nguyên nhân | Cách xử lý |
|-----------|-------------|------------|
| Báo HTML / không tải được | File Drive không public hoặc giới hạn băng thông | Đảm bảo share "Anyone with link" hoặc tạo bản copy mới |
| Không tìm thấy exe | Cấu trúc thư mục khác | Cập nhật `game_executable_name` hoặc kiểm tra giải nén |
| Mất nền | Sai đường dẫn ảnh | Giữ cấu trúc `img/` đúng hoặc cập nhật hằng |

## 7. Gợi ý nâng cấp
- Thêm hệ thống patch (so sánh manifest SHA256)
- Nút hủy tải
- Auto cập nhật version từ JSON online
- Hiển thị tốc độ và thời gian ước tính hoàn thành rõ hơn
- Logging ra file `logs/launcher.log`

---
Chúc bạn phát hành thành công!
