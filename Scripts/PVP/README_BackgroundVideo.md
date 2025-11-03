# 🎬 BACKGROUND VIDEO CHO PVP SCENE

## 📋 **TỔNG QUAN:**
Hệ thống background video cho PVP 3v3 battle với các tính năng:
- ✅ Phát video nền liên tục
- ✅ Tự động pause khi có skill animation
- ✅ Resume sau khi skill animation kết thúc
- ✅ Tích hợp với game speed (X1/X2/X4)
- ✅ Auto setup từ code

---

## 🚀 **CÁCH SỬ DỤNG:**

### **Method 1: Auto Setup (Khuyến nghị)**

1. **Thêm script vào scene:**
   ```csharp
   // Kéo PVPBackgroundVideoAutoSetup vào GameObject bất kỳ trong scene
   // Hoặc attach vào Turn3v3Manager
   ```

2. **Setup video clip:**
   ```csharp
   // Trong Inspector của PVPBackgroundVideoAutoSetup:
   // - Kéo video clip vào "Background Video Clip"
   // - Bật "Auto Setup On Start"
   ```

3. **Chạy scene:**
   ```csharp
   // Script sẽ tự động tạo:
   // - BackgroundVideoController
   // - VideoPlayer
   // - BackgroundVideoCanvas
   // - BackgroundVideoImage
   // - RenderTexture
   ```

### **Method 2: Manual Setup**

1. **Tạo GameObject:**
   ```
   BackgroundVideo (GameObject)
   ├── BackgroundVideoController (Component)
   └── VideoPlayer (Component)
   ```

2. **Tạo Canvas:**
   ```
   BackgroundVideoCanvas (GameObject)
   ├── Canvas (Component)
   ├── CanvasScaler (Component)
   ├── GraphicRaycaster (Component)
   └── BackgroundVideoImage (RawImage)
   ```

3. **Setup trong Inspector:**
   ```csharp
   BackgroundVideoController:
   - Background Video Player: VideoPlayer
   - Background Video Clip: VideoClip
   - Background Video Canvas: Canvas GameObject
   - Background Video Image: RawImage
   ```

---

## ⚙️ **SETTINGS:**

### **BackgroundVideoController:**
```csharp
[Header("Background Video Settings")]
- Background Video Player: VideoPlayer component
- Background Video Clip: Video clip nền
- Background Video Canvas: Canvas chứa video
- Background Video Image: RawImage hiển thị video

[Header("Video Behavior")]
- Auto Play On Start: Tự động phát khi Start
- Loop Video: Lặp lại video
- Pause On Skill Animation: Pause khi có skill animation
- Resume After Skill Animation: Resume sau skill animation

[Header("Video Quality")]
- Render Texture: RenderTexture cho video
- Video Resolution: Độ phân giải video (1920x1080)
```

### **Integration:**
```csharp
[Header("Integration")]
- Skill Video Player: VideoSkillPlayer (để pause/resume)
- Skill Animation Controller: SkillAnimationController (để pause/resume)
```

---

## 🎮 **TÍCH HỢP VỚI PVP SYSTEM:**

### **1. Turn3v3Manager:**
```csharp
// Đã được tích hợp sẵn:
[SerializeField] private BackgroundVideoController backgroundVideoController;

// Trong Start():
if (backgroundVideoController == null)
    backgroundVideoController = GetComponent<BackgroundVideoController>();
```

### **2. Auto Pause/Resume:**
```csharp
// Khi skill animation bắt đầu:
OnSkillVideoStarted() → PauseBackgroundVideo()

// Khi skill animation kết thúc:
OnSkillVideoFinished() → ResumeBackgroundVideo()
```

### **3. Game Speed Sync:**
```csharp
// Background video sẽ tự động sync với Time.timeScale
// Khi GameSpeedToggle thay đổi X1/X2/X4
```

---

## 🔧 **API METHODS:**

### **BackgroundVideoController:**
```csharp
// Control methods
PlayBackgroundVideo()           // Phát video nền
StopBackgroundVideo()          // Dừng video nền
PauseBackgroundVideo()         // Tạm dừng video nền
ResumeBackgroundVideo()        // Tiếp tục video nền

// Setup methods
SetupBackgroundVideo(player, clip, canvas, image)
SetBackgroundVideoClip(newClip)

// Properties
IsPlaying                      // Video đang phát
IsPaused                       // Video đang pause
```

### **Events:**
```csharp
OnBackgroundVideoStarted       // Khi video bắt đầu
OnBackgroundVideoStopped       // Khi video dừng
OnBackgroundVideoPaused        // Khi video pause
OnBackgroundVideoResumed       // Khi video resume
```

---

## 🎯 **WORKFLOW:**

```
1. Scene Start
   ↓
2. PVPBackgroundVideoAutoSetup.AutoSetupBackgroundVideoSystem()
   ↓
3. Tạo BackgroundVideoController + VideoPlayer + Canvas + RawImage
   ↓
4. BackgroundVideoController.PlayBackgroundVideo()
   ↓
5. Video phát nền liên tục
   ↓
6. Khi có skill animation:
   - PauseBackgroundVideo()
   - Skill animation chạy
   - ResumeBackgroundVideo()
   ↓
7. Lặp lại cho đến hết trận đấu
```

---

## 🐛 **TROUBLESHOOTING:**

### **Video không phát:**
```csharp
// Check:
1. Video clip có được assign không?
2. VideoPlayer có được setup không?
3. RenderTexture có được tạo không?
4. Canvas có được active không?
```

### **Video không pause/resume:**
```csharp
// Check:
1. SkillVideoPlayer có được assign không?
2. SkillAnimationController có được assign không?
3. Events có được subscribe không?
```

### **Performance issues:**
```csharp
// Solutions:
1. Giảm video resolution (1280x720 thay vì 1920x1080)
2. Dùng compressed video format
3. Disable video khi không cần thiết
```

---

## 📝 **NOTES:**

1. **Video Format:** Khuyến nghị dùng MP4 với H.264 codec
2. **Resolution:** 1920x1080 hoặc 1280x720
3. **File Size:** Nên < 50MB để load nhanh
4. **Loop:** Video sẽ tự động loop nếu bật Loop Video
5. **Performance:** Video nền có thể ảnh hưởng performance trên mobile

---

## 🎬 **EXAMPLE USAGE:**

```csharp
// Trong Turn3v3Manager.Start():
if (backgroundVideoController != null)
{
    // Video sẽ tự động phát
    Debug.Log("🎬 Background video ready!");
}

// Manual control:
backgroundVideoController.PauseBackgroundVideo();   // Pause
backgroundVideoController.ResumeBackgroundVideo(); // Resume
backgroundVideoController.StopBackgroundVideo();    // Stop
```

**Background video system đã sẵn sàng!** 🚀
