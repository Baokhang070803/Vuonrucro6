import sys, os, subprocess, requests, zipfile, shutil, tempfile, random, math, json
from PyQt5.QtWidgets import (QApplication, QWidget, QPushButton, QVBoxLayout, QHBoxLayout,
                             QLabel, QProgressBar, QMessageBox, QFrame, QScrollArea, QCheckBox)
from PyQt5.QtCore import Qt, QThread, pyqtSignal, QPropertyAnimation, QRect, QEasingCurve, QTimer, QPointF, QRectF, QPropertyAnimation, QEasingCurve
from PyQt5.QtGui import QFont, QPixmap, QPainter, QColor, QIcon, QPen, QLinearGradient, QRadialGradient, QPainterPath
from PyQt5.QtWidgets import QGraphicsDropShadowEffect, QGraphicsBlurEffect
import urllib3
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

# Import cho tạo shortcut trên Windows
try:
    import win32com.client
    WINDOWS_SHORTCUT_AVAILABLE = True
except ImportError:
    WINDOWS_SHORTCUT_AVAILABLE = False

# ============== VISUAL EFFECTS CLASSES ==============
class LoadingScreen(QWidget):
    """Màn hình loading đẹp khi khởi động launcher"""
    def __init__(self):
        super().__init__()
        self.setWindowFlags(Qt.FramelessWindowHint)
        self.setAttribute(Qt.WA_TranslucentBackground)
        self.setFixedSize(500, 300)
        
        # Layout
        layout = QVBoxLayout(self)
        layout.setAlignment(Qt.AlignCenter)
        layout.setSpacing(20)
        layout.setContentsMargins(40, 40, 40, 40)
        
        # Bỏ icon - chỉ hiển thị text
        
        # Title
        self.logo_label = QLabel("LÀNG HOA RỰC")
        self.logo_label.setStyleSheet("""
            color: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                                   stop:0 #ffffff,
                                   stop:0.3 #a8d8ff,
                                   stop:0.7 #ffa8e8,
                                   stop:1 #ffffff);
            font-size: 28px;
            font-weight: 900;
            letter-spacing: 2px;
        """)
        self.logo_label.setAlignment(Qt.AlignCenter)
        
        # Subtitle - màu sáng hơn
        self.subtitle_label = QLabel("Đang khởi động...")
        self.subtitle_label.setStyleSheet("""
            color: rgba(255,255,255,240);
            font-size: 16px;
            font-weight: 600;
        """)
        self.subtitle_label.setAlignment(Qt.AlignCenter)
        
        # Progress bar
        self.progress = QProgressBar()
        self.progress.setFixedHeight(8)
        self.progress.setFormat("")  # Bỏ % hiển thị
        self.progress.setStyleSheet("""
            QProgressBar {
                background: rgba(255,255,255,30);
                border: none;
                border-radius: 4px;
                text-align: center;
            }
            QProgressBar::chunk {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                                           stop:0 #60a0ff,
                                           stop:0.5 #80c0ff,
                                           stop:1 #a0e0ff);
                border-radius: 4px;
            }
        """)
        
        # Loading animation - 5 giây
        self.loading_animation = QPropertyAnimation(self.progress, b"value")
        self.loading_animation.setDuration(5000)
        self.loading_animation.setStartValue(0)
        self.loading_animation.setEndValue(100)
        self.loading_animation.setEasingCurve(QEasingCurve.OutCubic)
        
        # Timer cho text animation
        self.text_timer = QTimer()
        self.text_timer.timeout.connect(self.update_loading_text)
        self.text_timer.start(500)
        self.loading_steps = [
            "Đang khởi động...",
            "Tải giao diện...",
            "Khởi tạo hệ thống...",
            "Kết nối server...",
            "Tải dữ liệu...",
            "Kiểm tra phiên bản...",
            "Khởi tạo engine...",
            "Sẵn sàng!"
        ]
        self.current_step = 0
        
        layout.addWidget(self.logo_label)
        layout.addWidget(self.subtitle_label)
        layout.addWidget(self.progress)
        
        # Style cho loading screen - màu sáng hơn
        self.setStyleSheet("""
            QWidget {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                                           stop:0 rgba(40,60,100,220),
                                           stop:1 rgba(30,50,90,240));
                border-radius: 20px;
            }
        """)
        
        # Shadow effect
        shadow = QGraphicsDropShadowEffect(self)
        shadow.setBlurRadius(30)
        shadow.setOffset(0, 8)
        shadow.setColor(QColor(0,0,0,200))
        self.setGraphicsEffect(shadow)
        
        # Start animations
        self.loading_animation.start()
        self.text_timer.start()
        
        # Bỏ icon animation
        
        # Title animation (fade in/out)
        self.title_animation = QPropertyAnimation(self.logo_label, b"windowOpacity")
        self.title_animation.setDuration(2000)
        self.title_animation.setStartValue(0.8)
        self.title_animation.setEndValue(1.0)
        self.title_animation.setEasingCurve(QEasingCurve.InOutSine)
        self.title_animation.setLoopCount(-1)
        self.title_animation.start()
        
        # Bỏ progress glow timer để tránh warning
        self.glow_intensity = 0.0
    
    def update_loading_text(self):
        if self.current_step < len(self.loading_steps):
            self.subtitle_label.setText(self.loading_steps[self.current_step])
            self.current_step += 1
        else:
            self.text_timer.stop()
    
    def center_on_screen(self):
        screen = QApplication.primaryScreen().availableGeometry()
        geo = self.frameGeometry()
        geo.moveCenter(screen.center())
        self.move(geo.topLeft())

class ToastNotification(QWidget):
    """Custom Toast Notification với animation đẹp"""
    def __init__(self, parent=None, title="", message="", icon="✅", duration=3000):
        super().__init__(parent)
        self.setWindowFlags(Qt.FramelessWindowHint | Qt.WindowStaysOnTopHint | Qt.Tool)
        self.setAttribute(Qt.WA_TranslucentBackground)
        self.setFixedSize(400, 80)
        
        # Layout
        layout = QHBoxLayout(self)
        layout.setContentsMargins(15, 10, 15, 10)
        
        # Icon
        icon_label = QLabel(icon)
        icon_label.setStyleSheet("font-size: 24px; color: #4CAF50;")
        icon_label.setFixedWidth(30)
        
        # Content
        content_layout = QVBoxLayout()
        content_layout.setSpacing(2)
        
        title_label = QLabel(title)
        title_label.setStyleSheet("""
            color: white; 
            font-size: 16px; 
            font-weight: bold;
        """)
        
        message_label = QLabel(message)
        message_label.setStyleSheet("""
            color: rgba(255,255,255,200); 
            font-size: 13px;
        """)
        message_label.setWordWrap(True)
        
        content_layout.addWidget(title_label)
        content_layout.addWidget(message_label)
        
        layout.addWidget(icon_label)
        layout.addLayout(content_layout)
        
        # Style
        self.setStyleSheet("""
            QWidget {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                                           stop:0 rgba(30,40,65,220),
                                           stop:1 rgba(20,30,50,240));
                border: 2px solid rgba(100,150,255,100);
                border-radius: 12px;
            }
        """)
        
        # Shadow effect
        shadow = QGraphicsDropShadowEffect(self)
        shadow.setBlurRadius(20)
        shadow.setOffset(0, 4)
        shadow.setColor(QColor(0,0,0,150))
        self.setGraphicsEffect(shadow)
        
        # Animation
        self.animation = QPropertyAnimation(self, b"windowOpacity")
        self.animation.setDuration(300)
        self.animation.setEasingCurve(QEasingCurve.OutCubic)
        
        # Auto close timer
        self.timer = QTimer()
        self.timer.timeout.connect(self.fade_out)
        self.timer.setSingleShot(True)
        self.timer.start(duration)
        
        # Show animation
        self.show()
        self.fade_in()
    
    def fade_in(self):
        self.animation.setStartValue(0.0)
        self.animation.setEndValue(1.0)
        self.animation.start()
    
    def fade_out(self):
        self.animation.setStartValue(1.0)
        self.animation.setEndValue(0.0)
        self.animation.finished.connect(self.close)
        self.animation.start()
    
    def show_toast(self):
        # Position ở góc trên phải
        if self.parent():
            parent_rect = self.parent().geometry()
            x = parent_rect.x() + parent_rect.width() - self.width() - 20
            y = parent_rect.y() + 20
            self.move(x, y)
        self.show()

class LoadingSpinner(QWidget):
    """Vòng tròn loading xoay cho kiểm tra cập nhật"""
    def __init__(self, parent=None):
        super().__init__(parent)
        self.setFixedSize(40, 40)
        self.angle = 0
        self.timer = QTimer()
        self.timer.timeout.connect(self.update_rotation)
        self.timer.start(50)  # 20 FPS
        
    def update_rotation(self):
        self.angle += 10
        if self.angle >= 360:
            self.angle = 0
        self.update()
        
    def paintEvent(self, event):
        painter = QPainter(self)
        painter.setRenderHint(QPainter.Antialiasing)
        
        # Vẽ vòng tròn loading
        rect = self.rect().adjusted(5, 5, -5, -5)
        
        # Vòng tròn nền
        painter.setPen(QPen(QColor(100, 150, 255, 50), 3))
        painter.drawEllipse(rect)
        
        # Vòng tròn xoay
        painter.setPen(QPen(QColor(100, 150, 255, 200), 3))
        painter.setBrush(Qt.NoBrush)
        
        # Tạo arc xoay
        start_angle = self.angle * 16  # Qt sử dụng 1/16 độ
        span_angle = 270 * 16  # 270 độ
        
        painter.drawArc(rect, start_angle, span_angle)


# Thread tải file không chặn UI
class DownloadThread(QThread):
    """Thread tải file hỗ trợ cả Google Drive (kể cả file lớn cần confirm token)."""
    progress = pyqtSignal(int)          # % tiến độ (0-100)
    finished = pyqtSignal()             # Hoàn tất
    error = pyqtSignal(str)             # Lỗi
    status = pyqtSignal(str)            # Thông báo trạng thái từng bước
    file_name_detected = pyqtSignal(str)  # Tên file thực từ Google Drive
    paused = pyqtSignal()               # Download đã tạm dừng

    def __init__(self, source: str, save_path: str, is_gdrive=False):
        super().__init__()
        self.source = source            # URL hoặc file id
        self.save_path = save_path
        self.is_gdrive = is_gdrive
        self.detected_filename = None    # Lưu tên file thực
        self._is_paused = False          # Flag để tạm dừng
        self._is_cancelled = False       # Flag để hủy
    
    def pause(self):
        """Tạm dừng download"""
        self._is_paused = True
        print("[DOWNLOAD] Đã yêu cầu tạm dừng")
    
    def resume(self):
        """Tiếp tục download"""
        self._is_paused = False
        print("[DOWNLOAD] Đã yêu cầu tiếp tục")
    
    def cancel(self):
        """Hủy download"""
        self._is_cancelled = True
        print("[DOWNLOAD] Đã yêu cầu hủy")

    def _get_zip_file_from_folder(self, folder_id: str):
        """Lấy file zip DUY NHẤT từ thư mục Google Drive công khai"""
        import re
        
        self.status.emit("Đang tìm file game trong thư mục...")
        
        # Truy cập folder public và parse HTML
        folder_url = f"https://drive.google.com/drive/folders/{folder_id}"
        
        print(f"[DOWNLOAD] Đang truy cập folder: {folder_url}")
        
        try:
            # Tăng timeout cho kết nối chậm
            response = requests.get(folder_url, timeout=30)
            print(f"[DOWNLOAD] Folder response: {response.status_code}")
            
            if response.status_code != 200:
                raise Exception(f"Không thể truy cập thư mục (HTTP {response.status_code})")
            
            html_content = response.text
            print(f"[DOWNLOAD] HTML length: {len(html_content)}")
            
            # Tìm file .zip trong HTML
            # Google Drive nhúng data trong JavaScript, cần decode escape sequences
            
            import html as html_lib
            decoded_content = html_lib.unescape(html_content)
            
            # Tìm file ID: [null,"FILE_ID"]
            # Google Drive ID thường 28-44 ký tự
            file_id_pattern = r'\[null,"([a-zA-Z0-9_-]{28,44})"\]'
            file_ids = re.findall(file_id_pattern, decoded_content)
            print(f"[DOWNLOAD] Tìm thấy {len(file_ids)} file IDs")
            
            if file_ids:
                print(f"[DOWNLOAD] File ID đầu tiên: {file_ids[0]}")
            
            # Tìm tên file .zip - match toàn bộ tên file
            zip_name_pattern = r'\b([a-zA-Z][a-zA-Z0-9_\-\.]*\.zip)\b'
            all_names = re.findall(zip_name_pattern, decoded_content)
            
            # Lọc tên hợp lệ
            valid_names = []
            seen = set()
            for name in all_names:
                if (len(name) > 5  # Tên file phải dài hơn 5 ký tự
                    and len(name) < 100 
                    and name not in seen
                    and not name.startswith('.')
                    and name.count('.') <= 3
                    and not name.endswith('..zip')):
                    valid_names.append(name)
                    seen.add(name)
            
            print(f"[DOWNLOAD] Valid ZIP names: {valid_names[:5]}")
            
            if file_ids and valid_names:
                zip_files = [(file_ids[0], valid_names[0])]
            
            if not zip_files:
                import tempfile
                debug_file = os.path.join(tempfile.gettempdir(), 'gdrive_folder_download.html')
                with open(debug_file, 'w', encoding='utf-8') as f:
                    f.write(html_content)
                raise Exception(f"Không tìm thấy file ZIP. HTML: {debug_file}")
            
            # Lấy file đầu tiên
            file_id, file_name = zip_files[0]
            
            print(f"[DOWNLOAD] ✅ File: {file_name}")
            print(f"[DOWNLOAD] ✅ ID: {file_id}")
            
            # Lưu tên file thực và emit signal
            self.detected_filename = file_name
            self.file_name_detected.emit(file_name)
            
            self.status.emit(f"Tìm thấy: {file_name}")
            return file_id
                
        except requests.RequestException as e:
            print(f"[DOWNLOAD] Lỗi kết nối: {e}")
            raise Exception(f"Không thể kết nối Google Drive: {e}")
        
    def _get_current_installed_file(self):
        """Lấy tên file game hiện đã cài đặt"""
        try:
            version_file = os.path.join(self.install_dir, 'installed_file.txt')
            if os.path.exists(version_file):
                with open(version_file, 'r', encoding='utf-8') as f:
                    return f.read().strip()
        except Exception:
            pass
        return None

    def _download_gdrive(self, file_id: str):
        import re, time, tempfile, html
        session = requests.Session()
        base_url = "https://drive.google.com/uc?export=download"
        self.status.emit("Kết nối Google Drive...")
        response = session.get(base_url, params={"id": file_id}, stream=True, allow_redirects=True)
        if response.status_code != 200:
            raise Exception(f"HTTP {response.status_code} (bước đầu)")

        def _extract_token_and_link(resp_text: str):
            # Tìm link chứa confirm trực tiếp trong HTML
            # Ví dụ: href="/uc?export=download&confirm=XYZ&id=FILE"
            link_match = re.search(r'href="(/uc\?export=download&[^"]*confirm=[^"]+)"', resp_text)
            token = None
            if link_match:
                link_raw = html.unescape(link_match.group(1))
                m_tok = re.search(r'confirm=([0-9A-Za-z_]+)', link_raw)
                if m_tok:
                    token = m_tok.group(1)
                return token, "https://drive.google.com" + link_raw
            # Tìm input hidden confirm
            m2 = re.search(r'name="confirm"\s+value="([0-9A-Za-z_]+)"', resp_text)
            if m2:
                token = m2.group(1)
            return token, None

        def _cookie_token(resp):
            for k, v in resp.cookies.items():
                if k.startswith('download_warning'):
                    return v
            return None

        def _download_stream(resp):
            total_inner = int(resp.headers.get('content-length', 0))
            downloaded_inner = 0
            last_emit_inner = 0
            start_inner = time.time()
            
            print(f"[DOWNLOAD] Bắt đầu tải file, kích thước: {total_inner/1024/1024:.2f}MB")
            
            with open(self.save_path, 'wb') as f_out:
                for chunk in resp.iter_content(65536):  # 64KB chunks cho progress mượt hơn
                    # Kiểm tra pause
                    while self._is_paused and not self._is_cancelled:
                        self.status.emit("⏸️ Đã tạm dừng")
                        time.sleep(0.1)
                    
                    # Kiểm tra cancel
                    if self._is_cancelled:
                        self.status.emit("❌ Đã hủy tải")
                        print("[DOWNLOAD] Hủy bởi người dùng")
                        return
                    
                    if not chunk:
                        continue
                    f_out.write(chunk)
                    downloaded_inner += len(chunk)
                    if total_inner > 0:
                        pct = int(downloaded_inner / total_inner * 100)
                        self.progress.emit(pct)
                    now_inner = time.time()
                    if now_inner - last_emit_inner > 0.6:
                        speed = downloaded_inner / (1024*1024) / (now_inner - start_inner + 1e-6)
                        if total_inner > 0:
                            self.status.emit(f"Đang tải: {downloaded_inner/1024/1024:.2f}MB / {total_inner/1024/1024:.2f}MB ({pct}%) - {speed:.2f} MB/s")
                            print(f"[DOWNLOAD] Progress: {pct}% - {speed:.2f} MB/s")
                        else:
                            self.status.emit(f"Đang tải: {downloaded_inner/1024/1024:.2f}MB - {speed:.2f} MB/s")
                        last_emit_inner = now_inner
            
            print(f"[DOWNLOAD] Hoàn tất tải file: {downloaded_inner/1024/1024:.2f}MB")
            if total_inner == 0:
                self.progress.emit(100)

        def _looks_like_html(path):
            try:
                if os.path.getsize(path) > 65536:  # >64KB coi như không phải trang cảnh báo nhỏ
                    return False
                with open(path, 'rb') as tf:
                    head = tf.read(2048).lower()
                    return b'<html' in head
            except Exception:
                return False

        # Nếu có header Content-Disposition -> file thật
        if 'Content-Disposition' in response.headers:
            _download_stream(response)
        else:
            # Thử lấy token qua cookie trước
            token = _cookie_token(response)
            token2 = None
            alt_link = None
            if not token:
                token2, alt_link = _extract_token_and_link(response.text)
            token = token or token2
            if not token and not alt_link:
                # Thử domain usercontent luôn
                self.status.emit("Thử domain usercontent...")
                uc_url = f"https://drive.usercontent.google.com/download?id={file_id}&export=download"
                response = session.get(uc_url, stream=True)
                if 'Content-Disposition' not in response.headers:
                    # Không thành công
                    debug_path = os.path.join(tempfile.gettempdir(), 'drive_debug.html')
                    try:
                        with open(debug_path, 'wb') as dbg:
                            dbg.write(response.content[:200000])
                    except Exception:
                        pass
                    raise Exception(f"Không lấy được token tải (đã lưu HTML debug: {debug_path})")
                _download_stream(response)
            else:
                if alt_link:
                    self.status.emit("Dùng link tải trực tiếp trong HTML...")
                    response = session.get(alt_link, stream=True)
                else:
                    self.status.emit("Xác nhận tải file lớn...")
                    response = session.get(base_url, params={"id": file_id, "confirm": token}, stream=True)
                if response.status_code != 200:
                    raise Exception(f"HTTP {response.status_code} (sau confirm)")
                if 'Content-Disposition' not in response.headers:
                    # Thêm một thử usercontent
                    self.status.emit("Fallback usercontent...")
                    uc_url = f"https://drive.usercontent.google.com/download?id={file_id}&export=download&confirm={token or ''}"
                    response = session.get(uc_url, stream=True)
                    if 'Content-Disposition' not in response.headers:
                        debug_path = os.path.join(tempfile.gettempdir(), 'drive_debug.html')
                        try:
                            with open(debug_path, 'wb') as dbg:
                                dbg.write(response.content[:200000])
                        except Exception:
                            pass
                        raise Exception(f"Không xác nhận được tải (đã lưu HTML: {debug_path})")
                _download_stream(response)

        # Sau khi tải xong kiểm tra có phải HTML không
        if _looks_like_html(self.save_path):
            debug_path = os.path.join(tempfile.gettempdir(), 'drive_debug_saved.html')
            try:
                shutil.copy2(self.save_path, debug_path)
            except Exception:
                pass
            raise Exception(f"Google Drive trả về HTML (đã lưu {debug_path}).")

    def _download_http(self, url: str):
        self.status.emit("Kết nối máy chủ...")
        r = requests.get(url, stream=True, verify=False, timeout=30)
        if r.status_code != 200:
            raise Exception(f"HTTP {r.status_code}")
        total = int(r.headers.get("content-length", 0))
        downloaded = 0
        with open(self.save_path, 'wb') as f:
            for chunk in r.iter_content(32768):
                if not chunk:
                    continue
                f.write(chunk)
                downloaded += len(chunk)
                if total > 0:
                    self.progress.emit(int(downloaded / total * 100))
        if total == 0:
            self.progress.emit(100)
        # Phát hiện tải về trang HTML thay vì file thật (Google cảnh báo quét virus / chưa bật chia sẻ)
        try:
            if os.path.getsize(self.save_path) < 2048:
                with open(self.save_path, 'rb') as testf:
                    head = testf.read(500).lower()
                    if b'<html' in head and b'google' in head:
                        raise Exception("Tải thất bại: Google Drive trả về trang HTML (chưa bật chia sẻ công khai hoặc cần xác nhận).")
        except Exception:
            raise

    def run(self):
        try:
            print(f"[DOWNLOAD THREAD] Bắt đầu - source: {self.source}, is_gdrive: {self.is_gdrive}")
            self.status.emit("Bắt đầu tải...")
            if self.is_gdrive:
                # Nếu source là thư mục, tìm file zip trước
                if self.source.startswith("folder:"):
                    folder_id = self.source.replace("folder:", "")
                    print(f"[DOWNLOAD THREAD] Tìm file trong thư mục: {folder_id}")
                    file_id = self._get_zip_file_from_folder(folder_id)
                    if file_id is None:
                        # Không cần tải, đã có phiên bản mới nhất
                        print(f"[DOWNLOAD THREAD] Đã có phiên bản mới nhất, bỏ qua tải")
                        self.status.emit("Đã có phiên bản mới nhất!")
                        self.finished.emit()
                        return
                    print(f"[DOWNLOAD THREAD] Bắt đầu tải file ID: {file_id}")
                    self._download_gdrive(file_id)
                else:
                    print(f"[DOWNLOAD THREAD] Tải trực tiếp file ID: {self.source}")
                    self._download_gdrive(self.source)
            else:
                print(f"[DOWNLOAD THREAD] Tải HTTP: {self.source}")
                self._download_http(self.source)
            print(f"[DOWNLOAD THREAD] Hoàn tất tải file")
            self.status.emit("Hoàn tất tải. Đang chuẩn bị giải nén...")
            self.finished.emit()
        except Exception as e:
            print(f"[DOWNLOAD THREAD] LỖI: {e}")
            self.error.emit(str(e))
class UpdateCheckThread(QThread):
    """Thread kiểm tra cập nhật từ Google Drive"""
    update_available = pyqtSignal(str, str)  # new_file, current_file
    no_update = pyqtSignal(str)  # current_file
    error = pyqtSignal(str)
    
    def __init__(self, folder_id: str, install_dir: str):
        super().__init__()
        self.folder_id = folder_id
        self.install_dir = install_dir
    
    def run(self):
        try:
            import re
            
            # Truy cập folder public và parse HTML
            folder_url = f"https://drive.google.com/drive/folders/{self.folder_id}"
            
            print(f"[UPDATE CHECK] Đang truy cập folder...")
            response = requests.get(folder_url, timeout=30)
            print(f"[UPDATE CHECK] Folder response: {response.status_code}")
            
            if response.status_code != 200:
                raise Exception(f"Không thể truy cập thư mục (HTTP {response.status_code})")
            
            html_content = response.text
            
            # Debug: Lưu HTML để kiểm tra
            import tempfile
            debug_file = os.path.join(tempfile.gettempdir(), 'gdrive_folder.html')
            with open(debug_file, 'w', encoding='utf-8') as f:
                f.write(html_content)
            print(f"[UPDATE CHECK] HTML saved to: {debug_file}")
            
            # Tìm file .zip trong HTML  
            # Google Drive nhúng data trong JavaScript, cần decode escape sequences
            
            # Giải mã HTML escape sequences (\x22 -> ", etc.)
            import html as html_lib
            decoded_content = html_lib.unescape(html_content)
            
            # Tìm file ID: [null,"FILE_ID"]
            # Google Drive ID thường 28-44 ký tự
            file_id_pattern = r'\[null,"([a-zA-Z0-9_-]{28,44})"\]'
            file_ids = re.findall(file_id_pattern, decoded_content)
            print(f"[UPDATE CHECK] Tìm thấy {len(file_ids)} file IDs")
            
            if file_ids:
                print(f"[UPDATE CHECK] File ID đầu tiên: {file_ids[0]}")
            
            # Tìm tên file .zip - match toàn bộ tên file
            # Pattern: word boundary + filename + .zip
            zip_name_pattern = r'\b([a-zA-Z][a-zA-Z0-9_\-\.]*\.zip)\b'
            all_names = re.findall(zip_name_pattern, decoded_content)
            
            # Lọc tên hợp lệ (loại bỏ path, URL, etc)
            valid_names = []
            seen = set()
            for name in all_names:
                if (len(name) > 5  # Tên file phải dài hơn 5 ký tự
                    and len(name) < 100 
                    and name not in seen
                    and not name.startswith('.')
                    and name.count('.') <= 3  # Tối đa 3 dấu chấm (vd: file.v1.0.zip)
                    and not name.endswith('..zip')):  # Không có dấu chấm kép
                    valid_names.append(name)
                    seen.add(name)
            
            print(f"[UPDATE CHECK] Valid ZIP names: {valid_names[:5]}")
            
            zip_files = []
            if file_ids and valid_names:
                zip_files.append((file_ids[0], valid_names[0]))
            
            if not zip_files:
                raise Exception(f"Không tìm thấy file ZIP. HTML saved to: {debug_file}")
            
            # Lấy file đầu tiên
            latest_file_name = zip_files[0][1]
            print(f"[UPDATE CHECK] ✅ File mới nhất: {latest_file_name}")
            
            # Kiểm tra file hiện tại
            current_file = None
            installed_file_path = os.path.join(self.install_dir, 'installed_file.txt')
            if os.path.exists(installed_file_path):
                try:
                    with open(installed_file_path, 'r', encoding='utf-8') as f:
                        current_file = f.read().strip()
                    print(f"[UPDATE CHECK] File đã cài: {current_file}")
                except Exception:
                    pass
            else:
                print(f"[UPDATE CHECK] Chưa có file cài đặt")
            
            # So sánh
            if current_file and current_file == latest_file_name:
                print(f"[UPDATE CHECK] Không có cập nhật")
                self.no_update.emit(current_file)
            else:
                print(f"[UPDATE CHECK] Có cập nhật mới!")
                self.update_available.emit(latest_file_name, current_file or "Chưa cài đặt")
                
        except Exception as e:
            print(f"[UPDATE CHECK] Lỗi: {e}")
            self.error.emit(str(e))

class ExtractThread(QThread):
    progress = pyqtSignal(int)
    finished = pyqtSignal(str)  # path cài đặt
    error = pyqtSignal(str)

    def __init__(self, archive_path: str, target_dir: str):
        super().__init__()
        self.archive_path = archive_path
        self.target_dir = target_dir

    def run(self):
        try:
            os.makedirs(self.target_dir, exist_ok=True)
            # Nếu là zip thì giải nén, nếu không chỉ copy sang
            if zipfile.is_zipfile(self.archive_path):
                with zipfile.ZipFile(self.archive_path, 'r') as zf:
                    members = zf.infolist()
                    files = [m for m in members if not m.is_dir()]
                    total = len(files) or 1
                    done = 0
                    for m in files:
                        # Ngăn path traversal
                        out_path = os.path.normpath(os.path.join(self.target_dir, m.filename))
                        if not out_path.startswith(os.path.abspath(self.target_dir)):
                            continue
                        zf.extract(m, self.target_dir)
                        done += 1
                        self.progress.emit(int(done / total * 100))
            else:
                # Sao chép file đơn
                dst = os.path.join(self.target_dir, os.path.basename(self.archive_path))
                shutil.copy2(self.archive_path, dst)
                self.progress.emit(100)
            self.finished.emit(self.target_dir)
        except Exception as e:
            self.error.emit(str(e))


def get_config_path() -> str:
    """Lấy đường dẫn file config lưu trong AppData"""
    appdata = os.getenv('APPDATA', os.path.expanduser('~'))
    config_dir = os.path.join(appdata, 'LangHoaRucLauncher')
    os.makedirs(config_dir, exist_ok=True)
    return os.path.join(config_dir, 'config.json')

def load_config() -> dict:
    """Load config từ file"""
    config_path = get_config_path()
    if os.path.exists(config_path):
        try:
            with open(config_path, 'r', encoding='utf-8') as f:
                return json.load(f)
        except Exception as e:
            print(f"[CONFIG] Không thể load config: {e}")
    return {}

def save_config(config: dict):
    """Lưu config vào file"""
    config_path = get_config_path()
    try:
        with open(config_path, 'w', encoding='utf-8') as f:
            json.dump(config, f, indent=2, ensure_ascii=False)
        print(f"[CONFIG] Đã lưu config: {config_path}")
    except Exception as e:
        print(f"[CONFIG] Không thể lưu config: {e}")

def resource_path(relative_path: str) -> str:
    """Trả về đường dẫn thực tới resource khi chạy bình thường hoặc sau khi đóng gói PyInstaller.
    PyInstaller (6.x) one-folder đặt data có thể trong _internal\ hoặc ngay cạnh exe; one-file giải nén vào sys._MEIPASS.
    """
    base = getattr(sys, '_MEIPASS', os.path.abspath(os.path.dirname(__file__)))
    # Thử trực tiếp
    p1 = os.path.join(base, relative_path)
    if os.path.exists(p1):
        return p1
    # Nếu chạy dạng one-folder với _internal
    p2 = os.path.join(base, '_internal', relative_path)
    if os.path.exists(p2):
        return p2
    # Nếu script được chạy từ repo gốc nhưng relative_path đã đủ
    if os.path.exists(relative_path):
        return os.path.abspath(relative_path)
    return p1  # trả về p1 dù chưa tồn tại để caller tự kiểm tra

def find_background_image() -> str:
    """Tìm file nền ưu tiên tên br.png, background.png, bg.png, nếu không lấy file ảnh đầu tiên trong img."""
    candidates = ["br.png", "background.png", "bg.png"]
    for name in candidates:
        # Thử ở thư mục gốc trước
        rp_root = resource_path(name)
        if os.path.exists(rp_root):
            return rp_root
        # Thử trong img/
        rp_img = resource_path(os.path.join('img', name))
        if os.path.exists(rp_img):
            return rp_img
    img_dir = resource_path('img')
    try:
        if os.path.isdir(img_dir):
            for f in os.listdir(img_dir):
                if f.lower().endswith(('.png', '.jpg', '.jpeg')):
                    return os.path.join(img_dir, f)
    except Exception:
        pass
    return ''

class Launcher(QWidget):
    """Launcher với bố cục cân đối đơn giản kiểu Genshin: nền ảnh toàn màn, panel phải chứa thông tin."""
    # Thư mục Google Drive chứa file game
    GDRIVE_FOLDER_ID = "1JFZBCSk-XUDpxoj_mS5DIAEcNMYW0l9Y"

    def __init__(self):
        super().__init__()
        self.setWindowTitle("Làng Hoa Rực")
        # Kích thước lớn hơn
        self.resize(1400, 780)
        self.setWindowFlags(Qt.FramelessWindowHint | Qt.WindowStaysOnTopHint)
        self.setAttribute(Qt.WA_TranslucentBackground, False)  # Tắt để có nền
        
        # Thêm loading spinner cho kiểm tra cập nhật
        self.update_checking = False
        
        # Nền đơn giản: ưu tiên br.png nếu có
        bg_path = find_background_image()
        if bg_path:
            print(f"[BG] Dung anh nen: {bg_path}")
            self._bg_pix = QPixmap(bg_path)
        else:
            print("[BG] Khong tim thay anh nen (br.png). Hien thi mau toi.")
            self._bg_pix = None
        
        # Load config đã lưu (thư mục cài đặt trước đó)
        config = load_config()
        saved_install_dir = config.get('install_dir', None)
        
        # Thiết lập thư mục cài đặt
        if saved_install_dir and os.path.exists(saved_install_dir):
            # Dùng thư mục đã lưu nếu còn tồn tại
            self.install_dir = saved_install_dir
            self.custom_install = True
            print(f"[CONFIG] Đã load thư mục cài đặt: {self.install_dir}")
        else:
            # Mặc định (ổ hệ thống hoặc C:)
            system_drive = os.getenv("SystemDrive", "C:")
            if not system_drive.endswith(":"):
                system_drive = system_drive.rstrip("\\/")
            self.install_dir = os.path.join(f"{system_drive}", "MyGameClient")
            self.custom_install = False
            print(f"[CONFIG] Dùng thư mục mặc định: {self.install_dir}")
        
        # Tên file exe thực tế bên trong thư mục con sau khi giải nén
        self.game_executable_name = "Vườn Rực Rỡ.exe"
        # Panel phải
        self.build_layout()
        self.enhance_ui()
        self.center_on_screen()
        
        # Bỏ animation timer - thiết kế tĩnh
        
        # Kiểm tra game đã cài đặt chưa khi khởi động
        self.check_game_installed()
        
        # Kiểm tra cập nhật tự động khi khởi động
        QTimer.singleShot(1000, self.auto_check_updates)
        
        self.show()

    def center_on_screen(self):
        screen = QApplication.primaryScreen().availableGeometry()
        geo = self.frameGeometry()
        geo.moveCenter(screen.center())
        self.move(geo.topLeft())

    # --- Paint background đơn giản ---
    def paintEvent(self, event):
        painter = QPainter(self)
        painter.setRenderHint(QPainter.SmoothPixmapTransform)
        painter.setRenderHint(QPainter.Antialiasing)
        
        win_w, win_h = self.width(), self.height()
        
        # Vẽ background image nếu có
        if self._bg_pix and not self._bg_pix.isNull():
            # Cover logic
            pix_w, pix_h = self._bg_pix.width(), self._bg_pix.height()
            scale = max(win_w / pix_w, win_h / pix_h)
            new_w, new_h = int(pix_w * scale), int(pix_h * scale)
            x = (win_w - new_w) // 2
            y = (win_h - new_h) // 2
            painter.drawPixmap(x, y, new_w, new_h, self._bg_pix)
        else:
            # Gradient background tĩnh - phong cách Genshin
            gradient = QLinearGradient(0, 0, win_w, win_h)
            gradient.setColorAt(0.0, QColor(20, 30, 50))
            gradient.setColorAt(0.5, QColor(30, 40, 70))
            gradient.setColorAt(1.0, QColor(15, 25, 45))
            painter.fillRect(self.rect(), gradient)
        
        # Overlay mờ đơn giản
        overlay = QColor(0, 0, 0, 100)
        painter.fillRect(self.rect(), overlay)

    def build_layout(self):
        # Container tổng
        root = QHBoxLayout(self)
        root.setContentsMargins(30, 20, 30, 20)
        root.setSpacing(0)

        # Khu vực trống bên trái - thiết kế đơn giản
        self.left_placeholder = QFrame()
        self.left_placeholder.setObjectName("heroArea")
        self.left_placeholder.setSizePolicy(self.left_placeholder.sizePolicy().Expanding, self.left_placeholder.sizePolicy().Expanding)
        hero_layout = QVBoxLayout(self.left_placeholder)
        hero_layout.setContentsMargins(60, 80, 60, 80)
        hero_layout.setSpacing(20)
        
        # Logo/Title đơn giản
        self.hero_title = QLabel("LÀNG HOA RỰC")
        self.hero_title.setObjectName("HeroTitle")
        self.hero_title.setStyleSheet("""
            font-size: 48px; 
            font-weight: 700; 
            letter-spacing: 2px; 
            color: #ffffff;
            padding: 0px;
        """)
        
        # Subtitle đơn giản
        self.hero_sub = QLabel("Một hành trình sắc màu")
        self.hero_sub.setStyleSheet("""
            color: rgba(255,255,255,180); 
            font-size: 16px; 
            font-weight: 400;
            letter-spacing: 0.5px;
        """)
        self.hero_sub.setWordWrap(True)
        
        hero_layout.addWidget(self.hero_title)
        hero_layout.addWidget(self.hero_sub)
        hero_layout.addStretch(1)

        # Panel phải - thiết kế hiện đại glass morphism
        self.side_panel = QFrame()
        self.side_panel.setObjectName("SidePanel")
        self.side_panel.setFixedWidth(500)
        self.side_panel.setStyleSheet("""
            QFrame#SidePanel {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 rgba(30, 35, 50, 240),
                    stop:1 rgba(20, 25, 40, 250));
                border: 2px solid rgba(100, 150, 255, 100);
                border-radius: 16px;
            }
        """)

        side_layout = QVBoxLayout(self.side_panel)
        side_layout.setContentsMargins(24, 24, 24, 24)
        side_layout.setSpacing(18)

        # Header với title và window controls
        header = QHBoxLayout()
        self.title_label = QLabel("LAUNCHER")
        self.title_label.setStyleSheet("""
            color: #ffffff;
            font-size: 18px; 
            font-weight: 600; 
            letter-spacing: 1px;
        """)
        header.addWidget(self.title_label)
        header.addStretch(1)
        
        # Window controls
        controls_layout = QHBoxLayout()
        controls_layout.setSpacing(8)
        
        # Nút minimize
        minimize_btn = QPushButton("−")
        minimize_btn.clicked.connect(self.showMinimized)
        minimize_btn.setFixedSize(32, 32)
        minimize_btn.setCursor(Qt.PointingHandCursor)
        minimize_btn.setStyleSheet("""
            QPushButton {
                background: rgba(255, 255, 255, 10);
                color: #ffffff; 
                border: 1px solid rgba(255, 255, 255, 30); 
                border-radius: 6px;
                font-size: 16px;
                font-weight: normal;
            }
            QPushButton:hover {
                background: rgba(255, 255, 255, 20);
                border: 1px solid rgba(255, 255, 255, 50);
            }
            QPushButton:pressed {
                background: rgba(255, 255, 255, 5);
            }
        """)
        
        # Nút maximize/restore
        self.maximize_btn = QPushButton("□")
        self.maximize_btn.clicked.connect(self.toggle_maximize)
        self.maximize_btn.setFixedSize(32, 32)
        self.maximize_btn.setCursor(Qt.PointingHandCursor)
        self.maximize_btn.setStyleSheet("""
            QPushButton {
                background: rgba(255, 255, 255, 10);
                color: #ffffff; 
                border: 1px solid rgba(255, 255, 255, 30); 
                border-radius: 6px;
                font-size: 16px;
                font-weight: normal;
            }
            QPushButton:hover {
                background: rgba(255, 255, 255, 20);
                border: 1px solid rgba(255, 255, 255, 50);
            }
            QPushButton:pressed {
                background: rgba(255, 255, 255, 5);
            }
        """)
        
        # Nút close
        close_btn = QPushButton("×")
        close_btn.clicked.connect(self.close)
        close_btn.setFixedSize(32, 32)
        close_btn.setCursor(Qt.PointingHandCursor)
        close_btn.setStyleSheet("""
            QPushButton {
                background: rgba(255, 255, 255, 10);
                color: #ffffff; 
                border: 1px solid rgba(255, 255, 255, 30); 
                border-radius: 6px;
                font-size: 16px;
                font-weight: normal;
            }
            QPushButton:hover {
                background: rgba(255, 255, 255, 20);
                border: 1px solid rgba(255, 255, 255, 50);
            }
            QPushButton:pressed {
                background: rgba(255, 255, 255, 5);
            }
        """)
        
        controls_layout.addWidget(minimize_btn)
        controls_layout.addWidget(self.maximize_btn)
        controls_layout.addWidget(close_btn)
        header.addLayout(controls_layout)
        side_layout.addLayout(header)

        # Trạng thái đơn giản
        status_row = QHBoxLayout()
        self.status_label = QLabel("Sẵn sàng")
        self.status_label.setStyleSheet("""
            color: #b0c0d0; 
            font-size: 14px;
            font-weight: 400;
            padding: 0px;
        """)
        
        # Loading spinner (ẩn ban đầu)
        self.loading_spinner = LoadingSpinner()
        self.loading_spinner.hide()
        
        status_row.addWidget(self.status_label)
        status_row.addWidget(self.loading_spinner)
        side_layout.addLayout(status_row)

        # Thanh tiến trình hiện đại với gradient
        self.progress = QProgressBar()
        self.progress.setValue(0)
        self.progress.setAlignment(Qt.AlignCenter)
        self.progress.setFormat("%p%")
        self.progress.setFixedHeight(10)
        self.progress.setStyleSheet("""
            QProgressBar {
                background: rgba(255, 255, 255, 15); 
                border: 1px solid rgba(100, 150, 255, 50);
                border-radius: 5px; 
                color: white;
                text-align: center; 
                font-size: 11px;
                font-weight: 600;
            }
            QProgressBar::chunk {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #4a9eff,
                    stop:0.5 #6ab0ff,
                    stop:1 #8ac5ff); 
                border-radius: 4px;
            }
        """)
        side_layout.addWidget(self.progress)

        # Nhãn hiển thị thư mục cài đặt hiện tại
        self.install_path_label = QLabel()
        self.install_path_label.setStyleSheet("color:#9090a0; font-size:11px;")
        side_layout.addWidget(self.install_path_label)
        self.update_install_path()  # khởi tạo theo thư mục mặc định

        # Hàng chọn thư mục đơn giản
        from PyQt5.QtWidgets import QFileDialog
        path_row = QHBoxLayout()
        path_lbl = QLabel("Thư mục:")
        path_lbl.setStyleSheet("""
            color: #ffffff; 
            font-size: 13px; 
            font-weight: 500;
        """)
        self.path_value = QLabel(self.install_dir)
        self.path_value.setStyleSheet("""
            color: #b0c0d0; 
            font-size: 11px;
            background: rgba(255,255,255,5);
            padding: 8px 12px;
            border-radius: 6px;
            border: 1px solid rgba(255,255,255,10);
        """)
        self.path_value.setWordWrap(True)
        choose_btn = QPushButton("Chọn")
        choose_btn.setFixedWidth(80)
        choose_btn.setFixedHeight(32)
        choose_btn.setCursor(Qt.PointingHandCursor)
        choose_btn.setStyleSheet("""
            QPushButton {
                background: rgba(255, 255, 255, 10);
                color: #ffffff; 
                border: 1px solid rgba(255, 255, 255, 30); 
                border-radius: 6px; 
                font-size: 11px;
                font-weight: 500;
            }
            QPushButton:hover {
                background: rgba(255, 255, 255, 20);
                border: 1px solid rgba(255, 255, 255, 50);
            }
            QPushButton:pressed {
                background: rgba(255, 255, 255, 5);
            }
        """)
        def choose_dir():
            directory = QFileDialog.getExistingDirectory(self, "Chọn thư mục cài đặt", self.install_dir)
            if directory:
                self.install_dir = directory
                self.custom_install = True
                self.install_path_label.setText(f"📂 Thư mục cài đặt: {self.install_dir}")
                self.path_value.setText(self.install_dir)
                # Lưu config ngay khi chọn thư mục
                save_config({'install_dir': self.install_dir})
                # Kiểm tra lại game sau khi đổi thư mục
                self.check_game_installed()
        choose_btn.clicked.connect(choose_dir)
        path_row.addWidget(path_lbl)
        path_row.addWidget(self.path_value, 1)
        path_row.addWidget(choose_btn)
        side_layout.addLayout(path_row)

        # Bỏ checkbox tạo shortcut

        # Khu nút chính đơn giản
        btn_row = QHBoxLayout()
        btn_row.setSpacing(12)
        
        self.updateBtn = QPushButton("Cài đặt")
        self.updateBtn.setToolTip("Tải và cài đặt dữ liệu game")
        self.updateBtn.setCursor(Qt.PointingHandCursor)
        self.updateBtn.setFixedHeight(50)
        self.updateBtn.setStyleSheet("""
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #4a9eff,
                    stop:1 #6ab0ff); 
                color: white; 
                border: 2px solid rgba(100, 180, 255, 150);
                border-radius: 10px; 
                font-weight: 700; 
                font-size: 15px;
                letter-spacing: 0.5px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #5ba8ff,
                    stop:1 #7bc0ff);
                border: 2px solid rgba(120, 200, 255, 200);
            }
            QPushButton:pressed {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #3a8eef,
                    stop:1 #5aa0ff);
            }
            QPushButton:disabled {
                background: rgba(80, 90, 110, 150); 
                color: rgba(180, 190, 200, 180);
                border: 2px solid rgba(100, 110, 130, 100);
            }
        """)
        
        self.playBtn = QPushButton("Chơi")
        self.playBtn.setCursor(Qt.PointingHandCursor)
        self.playBtn.setFixedHeight(50)
        self.playBtn.setStyleSheet("""
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #00c851,
                    stop:1 #00e865); 
                color: white; 
                border: 2px solid rgba(0, 220, 100, 150);
                border-radius: 10px; 
                font-weight: 700; 
                font-size: 15px;
                letter-spacing: 0.5px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #00d861,
                    stop:1 #00ff75);
                border: 2px solid rgba(0, 255, 120, 200);
            }
            QPushButton:pressed {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #00b841,
                    stop:1 #00d855);
            }
            QPushButton:disabled {
                background: rgba(80, 90, 110, 150); 
                color: rgba(180, 190, 200, 180);
                border: 2px solid rgba(100, 110, 130, 100);
            }
        """)
        
        self.playBtn.setEnabled(False)
        self.updateBtn.clicked.connect(self.update_game)
        self.playBtn.clicked.connect(self.play_game)
        btn_row.addWidget(self.updateBtn)
        btn_row.addWidget(self.playBtn)
        side_layout.addLayout(btn_row)
        
        # Nút Pause/Resume và Cancel (ẩn ban đầu)
        control_row = QHBoxLayout()
        self.pauseBtn = QPushButton("⏸️ Tạm dừng")
        self.pauseBtn.setCursor(Qt.PointingHandCursor)
        self.pauseBtn.setFixedHeight(40)
        self.pauseBtn.setVisible(False)
        self.pauseBtn.setStyleSheet("""
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #ff9f00,
                    stop:1 #ffb030); 
                color: white; 
                border: 2px solid rgba(255, 180, 50, 150);
                border-radius: 8px; 
                font-weight: 600; 
                font-size: 13px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #ffaf10,
                    stop:1 #ffc040);
                border: 2px solid rgba(255, 200, 80, 200);
            }
        """)
        
        self.cancelBtn = QPushButton("❌ Hủy")
        self.cancelBtn.setCursor(Qt.PointingHandCursor)
        self.cancelBtn.setFixedHeight(40)
        self.cancelBtn.setVisible(False)
        self.cancelBtn.setStyleSheet("""
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #ff4444,
                    stop:1 #ff6666); 
                color: white; 
                border: 2px solid rgba(255, 100, 100, 150);
                border-radius: 8px; 
                font-weight: 600; 
                font-size: 13px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #ff5555,
                    stop:1 #ff7777);
                border: 2px solid rgba(255, 120, 120, 200);
            }
        """)
        
        self.pauseBtn.clicked.connect(self.toggle_pause)
        self.cancelBtn.clicked.connect(self.cancel_download)
        control_row.addWidget(self.pauseBtn)
        control_row.addWidget(self.cancelBtn)
        side_layout.addLayout(control_row)
        
        # Thêm nút "Kiểm tra cập nhật" với gradient đẹp
        check_update_row = QHBoxLayout()
        self.checkUpdateBtn = QPushButton("Kiểm tra cập nhật")
        self.checkUpdateBtn.setCursor(Qt.PointingHandCursor)
        self.checkUpdateBtn.setFixedHeight(36)
        self.checkUpdateBtn.setStyleSheet("""
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 rgba(100, 150, 255, 100),
                    stop:1 rgba(150, 180, 255, 120));
                color: white; 
                border: 1px solid rgba(120, 170, 255, 150); 
                border-radius: 8px; 
                font-weight: 600; 
                font-size: 13px;
                letter-spacing: 0.3px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 rgba(120, 170, 255, 150),
                    stop:1 rgba(170, 200, 255, 170));
                border: 1px solid rgba(140, 190, 255, 200);
            }
            QPushButton:pressed {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 rgba(80, 130, 235, 120),
                    stop:1 rgba(130, 160, 235, 140));
            }
            QPushButton:disabled {
                background: rgba(80, 90, 110, 100); 
                color: rgba(160, 170, 180, 150);
                border: 1px solid rgba(100, 110, 130, 100);
            }
        """)
        self.checkUpdateBtn.clicked.connect(self.manual_check_updates)
        check_update_row.addStretch()
        check_update_row.addWidget(self.checkUpdateBtn)
        side_layout.addLayout(check_update_row)

        # Phiên bản đơn giản
        self.version_label = QLabel("Phiên bản: 1.0")
        self.version_label.setStyleSheet("""
            color: #9090a0; 
            font-size: 12px; 
            font-weight: 400;
            padding: 0px;
        """)
        side_layout.addWidget(self.version_label)

        # Tin tức với gradient đẹp
        news_box = QFrame()
        news_box.setObjectName("NewsBox")
        news_box.setStyleSheet("""
            QFrame#NewsBox {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 rgba(50, 80, 150, 80),
                    stop:1 rgba(30, 50, 100, 100));
                border: 1px solid rgba(100, 150, 255, 80);
                border-radius: 10px;
            }
        """)
        nb_layout = QVBoxLayout(news_box)
        nb_layout.setContentsMargins(16, 12, 16, 12)
        nb_layout.setSpacing(8)
        news_title = QLabel("Thông báo")
        news_title.setStyleSheet("""
            color: #ffffff;
            font-size: 14px; 
            font-weight: 600; 
            letter-spacing: 0.5px;
        """)
        self.info_label = QLabel("⏳ Đang tải tin tức...")
        self.info_label.setStyleSheet("""
            color: #b0c0d0; 
            font-size: 12px; 
            line-height: 150%;
        """)
        self.info_label.setWordWrap(True)
        nb_layout.addWidget(news_title)
        nb_layout.addWidget(self.info_label)
        side_layout.addWidget(news_box)
        
        # Load tin tức từ Firebase
        self.load_news_from_firebase()

        side_layout.addStretch(1)

        # Footer đơn giản
        footer = QLabel("© 2025 Làng Hoa Rực")
        footer.setStyleSheet("""
            color: #707080; 
            font-size: 10px;
            font-weight: 400;
        """)
        footer.setAlignment(Qt.AlignCenter)
        side_layout.addWidget(footer)

        root.addWidget(self.left_placeholder, 1)
        root.addWidget(self.side_panel, 0)

    def enhance_ui(self):
        # Shadow đơn giản cho panel
        shadow = QGraphicsDropShadowEffect(self)
        shadow.setBlurRadius(20)
        shadow.setOffset(0, 4)
        shadow.setColor(QColor(0,0,0,100))
        self.side_panel.setGraphicsEffect(shadow)

    # Cho phép kéo cửa sổ
    def mousePressEvent(self, event):
        if event.button() == Qt.LeftButton:
            self._drag_pos = event.globalPos() - self.frameGeometry().topLeft()
            event.accept()

    def mouseMoveEvent(self, event):
        if event.buttons() & Qt.LeftButton:
            self.move(event.globalPos() - self._drag_pos)
            event.accept()
    
    def toggle_maximize(self):
        """Toggle maximize/restore window"""
        if self.isMaximized():
            self.showNormal()
            self.maximize_btn.setText("□")
        else:
            self.showMaximized()
            self.maximize_btn.setText("❐")

    def check_game_installed(self):
        """Kiểm tra xem game đã được cài đặt chưa. Nếu có thì bật nút Chơi."""
        game_path = self.find_game_executable()
        if game_path and os.path.exists(game_path):
            self.playBtn.setEnabled(True)
            self.status_label.setText("Game đã được cài đặt. Sẵn sàng chơi!")
        else:
            self.playBtn.setEnabled(False)
            self.status_label.setText("Chưa cài đặt game")

    def update_game(self):
        """Kiểm tra phiên bản mới và cài đặt dữ liệu từ Google Drive nếu cần."""
        self.status_label.setText("Đang kiểm tra phiên bản...")
        self.updateBtn.setEnabled(False)
        
        # Kiểm tra file đã cài đặt
        local_version_path = os.path.join(self.install_dir, "version.txt")
        local_file_path = os.path.join(self.install_dir, "installed_file.txt")
        local_version = "0"
        local_file = None
        
        if os.path.exists(local_version_path):
            try:
                with open(local_version_path, 'r', encoding='utf-8') as vf:
                    local_version = vf.read().strip()
            except Exception:
                pass
                
        if os.path.exists(local_file_path):
            try:
                with open(local_file_path, 'r', encoding='utf-8') as f:
                    local_file = f.read().strip()
            except Exception:
                pass

        # Chuẩn bị thư mục cài đặt
        try:
            os.makedirs(self.install_dir, exist_ok=True)
        except Exception as e:
            QMessageBox.critical(self, "Lỗi", f"Không tạo được thư mục cài đặt: {e}")
            self.updateBtn.setEnabled(True)
            return

        # Dùng thư mục Google Drive để tìm file zip
        gdrive_folder_id = self.GDRIVE_FOLDER_ID
        temp_dir = tempfile.gettempdir()
        save_path = os.path.join(temp_dir, f"game_update.zip")
        self.download_temp_path = save_path
        self.new_version = "latest"
        self.actual_filename = None  # Sẽ được cập nhật khi download thread phát hiện tên file
        self.thread = DownloadThread(f"folder:{gdrive_folder_id}", save_path, is_gdrive=True)
        self.thread.progress.connect(self.progress.setValue)
        self.thread.status.connect(lambda s: self.status_label.setText(s))
        self.thread.file_name_detected.connect(self.on_filename_detected)  # Nhận tên file thực
        self.thread.finished.connect(self.download_finished_version)
        self.thread.error.connect(self.download_error)
        
        # Hiển thị nút pause/cancel khi bắt đầu download
        self.pauseBtn.setVisible(True)
        self.cancelBtn.setVisible(True)
        self.is_downloading_paused = False
        
        self.thread.start()
    
    def on_filename_detected(self, filename):
        """Callback khi DownloadThread phát hiện tên file thực"""
        self.actual_filename = filename
        print(f"[LAUNCHER] Tên file thực: {filename}")
    
    def toggle_pause(self):
        """Toggle pause/resume download"""
        if not hasattr(self, 'thread') or not self.thread.isRunning():
            return
        
        if self.is_downloading_paused:
            # Resume
            self.thread.resume()
            self.pauseBtn.setText("⏸️ Tạm dừng")
            self.is_downloading_paused = False
            print("[LAUNCHER] Đã tiếp tục download")
        else:
            # Pause
            self.thread.pause()
            self.pauseBtn.setText("▶️ Tiếp tục")
            self.is_downloading_paused = True
            print("[LAUNCHER] Đã tạm dừng download")
    
    def cancel_download(self):
        """Hủy download"""
        if not hasattr(self, 'thread') or not self.thread.isRunning():
            return
        
        reply = QMessageBox.question(self, "Xác nhận", 
                                     "Bạn có chắc muốn hủy tải xuống?",
                                     QMessageBox.Yes | QMessageBox.No)
        
        if reply == QMessageBox.Yes:
            self.thread.cancel()
            self.pauseBtn.setVisible(False)
            self.cancelBtn.setVisible(False)
            self.status_label.setText("Đã hủy tải xuống.")
            self.updateBtn.setEnabled(True)
            print("[LAUNCHER] Đã hủy download")

    def download_finished_version(self):
        # Ẩn nút pause/cancel
        self.pauseBtn.setVisible(False)
        self.cancelBtn.setVisible(False)
        
        # Kiểm tra xem có cần tải không
        if not os.path.exists(self.download_temp_path):
            # Không cần tải, đã có phiên bản mới nhất
            self.status_label.setText("Đã có phiên bản mới nhất!")
            self.progress.setValue(100)
            self.updateBtn.setEnabled(True)
            self.playBtn.setEnabled(True)
            QMessageBox.information(self, "Thông báo", "Bạn đang ở phiên bản mới nhất!")
            return
            
        # Bắt đầu giải nén / cài đặt
        self.status_label.setText("Đang giải nén...")
        self.progress.setValue(0)
        self.extract_thread = ExtractThread(self.download_temp_path, self.install_dir)
        self.extract_thread.progress.connect(self.progress.setValue)
        self.extract_thread.finished.connect(self.extraction_finished)
        self.extract_thread.error.connect(self.download_error)
        self.extract_thread.start()

    def extraction_finished(self, installed_path: str):
        # Ghi version
        try:
            with open(os.path.join(installed_path, 'version.txt'), 'w', encoding='utf-8') as vf:
                vf.write(self.new_version)
        except Exception as e:
            QMessageBox.warning(self, "Cảnh báo", f"Không ghi được version: {e}")
        
        # Lưu tên file đã cài đặt (dùng tên file thực từ Google Drive)
        try:
            # Ưu tiên dùng tên file thực, nếu không có thì dùng tên file tạm
            installed_file_name = self.actual_filename or os.path.basename(self.download_temp_path)
            with open(os.path.join(installed_path, 'installed_file.txt'), 'w', encoding='utf-8') as f:
                f.write(installed_file_name)
            print(f"[LAUNCHER] Đã lưu tên file cài đặt: {installed_file_name}")
        except Exception as e:
            print(f"Không thể lưu tên file đã cài: {e}")
        
        # Lưu thư mục cài đặt vào config
        save_config({'install_dir': self.install_dir})
        print(f"[CONFIG] Đã lưu thư mục cài đặt: {self.install_dir}")
        
        self.status_label.setText("Cài đặt xong. Sẵn sàng chơi!")
        self.progress.setValue(100)
        self.updateBtn.setEnabled(True)
        self.playBtn.setEnabled(True)
        
        # Hiển thị thông báo cập nhật thành công
        display_name = self.actual_filename or os.path.basename(self.download_temp_path)
        QMessageBox.information(self, "Cập nhật thành công", 
                              f"Đã cài đặt phiên bản mới!\n"
                              f"File: {display_name}")
        
        # Xoá file tạm nếu tồn tại
        try:
            if os.path.exists(self.download_temp_path):
                os.remove(self.download_temp_path)
        except Exception:
            pass

    def download_error(self, error_msg):
        # Ẩn nút pause/cancel
        self.pauseBtn.setVisible(False)
        self.cancelBtn.setVisible(False)
        
        self.status_label.setText("Lỗi cài đặt. Có thể chơi phiên bản hiện tại.")
        self.updateBtn.setEnabled(True)
        self.playBtn.setEnabled(True)
        # Hiển thị lỗi chi tiết
        QMessageBox.warning(self, "Lỗi", f"Không thể tải xuống:\n{error_msg}")

    def play_game(self):
        game_path = self.find_game_executable()
        if game_path and os.path.exists(game_path):
            self.status_label.setText("Đang mở game...")
            try:
                subprocess.Popen([game_path])
                # Đóng launcher sau 1 giây
                QTimer.singleShot(1000, self.close)
            except Exception as e:
                self.status_label.setText("Chưa mở game.")
        else:
            self.status_label.setText("Chưa mở game.")

    def find_game_executable(self):
        """Tìm file exe trong cây thư mục cài đặt.
        - Ưu tiên tên đúng self.game_executable_name (không phân biệt hoa thường).
        - Nếu không có, lấy exe đầu tiên tìm thấy để vẫn cho phép chạy (fallback).
        """
        if not self.install_dir or not os.path.isdir(self.install_dir):
            return None
        preferred_lower = self.game_executable_name.lower()
        fallback = None
        for root, dirs, files in os.walk(self.install_dir):
            for f in files:
                if f.lower().endswith('.exe'):
                    full = os.path.join(root, f)
                    if f.lower() == preferred_lower:
                        return full
                    if fallback is None:
                        fallback = full
        return fallback

    # Bỏ function tạo shortcut

    def manual_check_updates(self):
        """Kiểm tra cập nhật thủ công khi người dùng bấm nút"""
        if self.update_checking:
            QMessageBox.information(self, "Thông báo", "Đang kiểm tra cập nhật, vui lòng đợi...")
            return
        
        self.checkUpdateBtn.setEnabled(False)
        self.auto_check_updates()
    
    def auto_check_updates(self):
        """Kiểm tra cập nhật tự động khi khởi động launcher"""
        if self.update_checking:
            return
            
        self.update_checking = True
        self.status_label.setText("Đang kiểm tra cập nhật...")
        self.loading_spinner.show()
        
        # Tạo thread kiểm tra cập nhật
        self.check_thread = UpdateCheckThread(self.GDRIVE_FOLDER_ID, self.install_dir)
        self.check_thread.update_available.connect(self.on_update_available)
        self.check_thread.no_update.connect(self.on_no_update)
        self.check_thread.error.connect(self.on_check_error)
        self.check_thread.start()
    
    def on_update_available(self, new_file_name, current_file_name):
        """Có phiên bản mới"""
        self.loading_spinner.hide()
        self.status_label.setText(f"Có phiên bản mới: {new_file_name}")
        self.update_checking = False
        self.checkUpdateBtn.setEnabled(True)
        
        # Hiển thị thông báo cập nhật - chỉ hiển thị phiên bản mới
        reply = QMessageBox.question(self, "Cập nhật có sẵn", 
                                   f"Tìm thấy phiên bản mới: {new_file_name}\n\n"
                                   f"Bạn có muốn cập nhật ngay bây giờ không?",
                                   QMessageBox.Yes | QMessageBox.No)
        
        if reply == QMessageBox.Yes:
            self.update_game()
        else:
            self.status_label.setText("Sẵn sàng.")
    
    def on_no_update(self, current_file_name):
        """Không có cập nhật"""
        self.loading_spinner.hide()
        self.status_label.setText("Đã có phiên bản mới nhất.")
        self.update_checking = False
        self.checkUpdateBtn.setEnabled(True)
        
        # Hiển thị thông báo
        QMessageBox.information(self, "Thông báo", 
                              f"Bạn đang sử dụng phiên bản mới nhất!\n"
                              f"File: {current_file_name}")
        
        # Tự động ẩn thông báo sau 2 giây
        QTimer.singleShot(2000, lambda: self.status_label.setText("Sẵn sàng."))
    
    def on_check_error(self, error_msg):
        """Lỗi khi kiểm tra cập nhật"""
        self.loading_spinner.hide()
        self.status_label.setText("Không thể kiểm tra cập nhật.")
        self.update_checking = False
        self.checkUpdateBtn.setEnabled(True)
        
        # Hiển thị lỗi chi tiết
        QMessageBox.warning(self, "Lỗi kiểm tra cập nhật", 
                          f"Không thể kiểm tra cập nhật:\n{error_msg}\n\n"
                          f"Vui lòng kiểm tra kết nối internet hoặc thử lại sau.")
        
        # Tự động ẩn thông báo sau 3 giây
        QTimer.singleShot(3000, lambda: self.status_label.setText("Sẵn sàng."))

    def update_install_path(self):
        # Không ghi đè nếu người dùng đã chọn thủ công
        if self.custom_install:
            return
        # Sử dụng thư mục mặc định
        self.install_path_label.setText(f"📂 Thư mục cài đặt: {self.install_dir}")
        if hasattr(self, 'path_value'):
            self.path_value.setText(self.install_dir)
    
    def load_news_from_firebase(self):
        """Load tin tức từ Firebase Realtime Database"""
        try:
            # Firebase Database URL
            firebase_url = "https://trangtrai-2769b-default-rtdb.firebaseio.com/News.json"
            
            print("[NEWS] Đang tải tin tức từ Firebase...")
            
            # Gửi request đến Firebase
            response = requests.get(firebase_url, timeout=10)
            
            if response.status_code == 200:
                news_data = response.json()
                
                if news_data:
                    # Chuyển dict thành list và sắp xếp theo priority
                    news_list = []
                    for key, news in news_data.items():
                        if news.get('isActive', False):
                            news_list.append(news)
                    
                    # Sắp xếp theo priority (thấp nhất = quan trọng nhất)
                    news_list.sort(key=lambda x: x.get('priority', 999))
                    
                    # Lấy 3 tin đầu tiên
                    top_news = news_list[:3]
                    
                    # Format tin tức
                    news_text = ""
                    for news in top_news:
                        title = news.get('title', '')
                        news_text += f"{title}\n"
                    
                    # Bỏ newline cuối cùng
                    news_text = news_text.rstrip('\n')
                    
                    # Cập nhật UI
                    self.info_label.setText(news_text)
                    print(f"[NEWS] ✅ Đã tải {len(top_news)} tin tức")
                else:
                    self.info_label.setText("📢 Chưa có tin tức mới.")
                    print("[NEWS] Không có tin tức trong database")
            else:
                raise Exception(f"HTTP {response.status_code}")
                
        except Exception as e:
            print(f"[NEWS] ❌ Lỗi: {e}")
            # Fallback: Hiển thị tin tức mặc định
            self.info_label.setText(
                "🎉 Chào mừng đến Làng Hoa Rực!\n"
                "🎁 Sự kiện tuần: Đăng nhập nhận quà!\n"
                "💎 Mẹo: Hoàn thành nhiệm vụ ngày!"
            )


if __name__ == "__main__":
    print("🚀 Đang khởi động launcher...")
    app = QApplication(sys.argv)
    
    # Set font để hỗ trợ tiếng Việt tốt hơn
    font = QFont("Arial", 10)
    app.setFont(font)
    
    print("✅ QApplication đã được tạo")
    
    # Hiển thị loading screen trước
    loading = LoadingScreen()
    loading.center_on_screen()
    loading.show()
    
    # Process events để hiển thị loading
    app.processEvents()
    
    # Tạo launcher (ẩn)
    launcher = Launcher()
    launcher.hide()  # Ẩn launcher ban đầu
    
    # Timer để chuyển từ loading sang launcher
    def show_launcher():
        loading.close()
        launcher.show()
        print("✅ Cửa sổ launcher đã được tạo")
        print("🎮 Launcher đã sẵn sàng!")
        print("📐 Vị trí cửa sổ:", launcher.geometry())
    
    # Chờ 5 giây rồi hiển thị launcher
    QTimer.singleShot(5000, show_launcher)
    
    sys.exit(app.exec_())
