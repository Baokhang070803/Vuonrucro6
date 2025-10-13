# ERD (Entity Relationship Diagram) - Dự án "Làng Hoa Rực"

## 1. THỰC THỂ CHÍNH

### 1.1. Users (Người dùng)
```sql
Users {
    uid: string (Primary Key)           -- Firebase User ID
    email: string (Unique)              -- Email đăng nhập
    displayName: string                 -- Tên hiển thị
    username: string (Unique)           -- Tên đăng nhập
    photoURL: string                    -- URL ảnh đại diện
    createdAt: timestamp                -- Ngày tạo tài khoản
    lastLogin: timestamp                -- Lần đăng nhập cuối
    provider: string                    -- Phương thức đăng nhập (email/google)
    isActive: boolean                   -- Trạng thái tài khoản
}
```

### 1.2. UserGameData (Dữ liệu game người dùng)
```sql
UserGameData {
    uid: string (Foreign Key)           -- Tham chiếu Users.uid
    Name: string                        -- Tên trong game
    Gold: integer                       -- Số vàng
    Diamond: integer                    -- Số kim cương
    MapInGame: json                     -- Dữ liệu bản đồ game
    lstTilemapDetail: array            -- Chi tiết tilemap
    lastPlayed: timestamp               -- Lần chơi cuối
    totalPlayTime: integer              -- Tổng thời gian chơi (phút)
}
```

### 1.3. CheckinDataLocal (Dữ liệu điểm danh - LocalStorage)
```sql
CheckinDataLocal {
    uid: string (Foreign Key)           -- Tham chiếu Users.uid
    totalDiamonds: integer              -- Tổng kim cương nhận từ điểm danh
    currentStreak: integer              -- Chuỗi điểm danh hiện tại
    totalCheckins: integer              -- Tổng số lần điểm danh
    checkinDates: array                 -- Danh sách ngày đã điểm danh
    lastCheckinDate: string             -- Ngày điểm danh cuối (YYYY-MM-DD)
    monthlyCheckins: json               -- Điểm danh theo tháng
    storageKey: string                  -- Key trong localStorage
}
```

### 1.4. DailyRewards (Phần thưởng hàng ngày)
```sql
DailyRewards {
    day: integer (Primary Key)           -- Ngày trong tháng (1-30)
    diamondReward: integer              -- Số kim cương thưởng
    isSpecialDay: boolean               -- Ngày đặc biệt (7, 14, 21, 30)
    bonusReward: json                   -- Phần thưởng bonus
    description: string                  -- Mô tả phần thưởng
}
```

### 1.5. StoryChapters (Chương truyện)
```sql
StoryChapters {
    chapterId: integer (Primary Key)    -- ID chương
    title: string                       -- Tiêu đề chương
    content: text                       -- Nội dung chương
    order: integer                       -- Thứ tự chương
    isUnlocked: boolean                 -- Trạng thái mở khóa
    unlockRequirements: json           -- Điều kiện mở khóa
    createdAt: timestamp                -- Ngày tạo
    updatedAt: timestamp                -- Ngày cập nhật
}
```

### 1.6. GameCharacters (Nhân vật game)
```sql
GameCharacters {
    characterId: string (Primary Key)   -- ID nhân vật
    name: string                        -- Tên nhân vật
    description: text                   -- Mô tả nhân vật
    imageUrl: string                    -- URL ảnh nhân vật
    role: string                        -- Vai trò trong game
    isUnlocked: boolean                 -- Trạng thái mở khóa
    unlockDate: timestamp               -- Ngày mở khóa
    createdAt: timestamp               -- Ngày tạo
}
```

## 2. THỰC THỂ BỔ SUNG

### 2.1. UserEvents (Sự kiện người dùng)
```sql
UserEvents {
    eventId: string (Primary Key)       -- ID sự kiện
    uid: string (Foreign Key)           -- Tham chiếu Users.uid
    eventType: string                   -- Loại sự kiện
    eventData: json                     -- Dữ liệu sự kiện
    timestamp: timestamp                -- Thời gian sự kiện
    sessionId: string                  -- ID phiên
    ipAddress: string                   -- Địa chỉ IP
    userAgent: string                   -- User agent
}
```

### 2.2. DownloadEvents (Sự kiện tải xuống)
```sql
DownloadEvents {
    eventId: string (Primary Key)       -- ID sự kiện
    uid: string (Foreign Key)           -- Tham chiếu Users.uid
    fileName: string                    -- Tên file
    fileSize: integer                   -- Kích thước file (bytes)
    downloadTime: timestamp             -- Thời gian tải xuống
    success: boolean                    -- Trạng thái thành công
    errorMessage: string                -- Thông báo lỗi (nếu có)
    ipAddress: string                   -- Địa chỉ IP
}
```

### 2.3. PageViews (Lượt xem trang)
```sql
PageViews {
    viewId: string (Primary Key)        -- ID lượt xem
    uid: string (Foreign Key)            -- Tham chiếu Users.uid
    pagePath: string                    -- Đường dẫn trang
    timestamp: timestamp                -- Thời gian xem
    sessionId: string                   -- ID phiên
    duration: integer                   -- Thời gian xem (giây)
    referrer: string                    -- Trang nguồn
    deviceType: string                  -- Loại thiết bị
}
```

### 2.4. UserSessions (Phiên người dùng)
```sql
UserSessions {
    sessionId: string (Primary Key)     -- ID phiên
    uid: string (Foreign Key)           -- Tham chiếu Users.uid
    startTime: timestamp                -- Thời gian bắt đầu
    endTime: timestamp                  -- Thời gian kết thúc
    deviceInfo: json                    -- Thông tin thiết bị
    ipAddress: string                   -- Địa chỉ IP
    userAgent: string                   -- User agent
    isActive: boolean                   -- Trạng thái hoạt động
    lastActivity: timestamp             -- Hoạt động cuối
}
```

### 2.5. UserPreferences (Tùy chọn người dùng)
```sql
UserPreferences {
    uid: string (Primary Key)           -- Tham chiếu Users.uid
    musicEnabled: boolean                -- Bật/tắt nhạc nền
    theme: string                        -- Chủ đề giao diện
    language: string                     -- Ngôn ngữ
    notifications: json                  -- Cài đặt thông báo
    privacy: json                        -- Cài đặt riêng tư
    updatedAt: timestamp                 -- Ngày cập nhật
}
```

### 2.6. PromoCodeData (Dữ liệu mã khuyến mãi)
```sql
PromoCodeData {
    uid: string (Foreign Key)           -- Tham chiếu Users.uid
    code: string                         -- Mã khuyến mãi
    usedAt: timestamp                    -- Thời gian sử dụng
    reward: json                         -- Phần thưởng nhận được
    isValid: boolean                     -- Trạng thái hợp lệ
    expiresAt: timestamp                 -- Thời gian hết hạn
}
```

### 2.7. GameProgress (Tiến độ game)
```sql
GameProgress {
    uid: string (Primary Key)           -- Tham chiếu Users.uid
    currentChapter: integer              -- Chương hiện tại
    unlockedChapters: array              -- Danh sách chương đã mở khóa
    storyProgress: json                  -- Tiến độ cốt truyện
    lastPlayed: timestamp                -- Lần chơi cuối
    totalPlayTime: integer               -- Tổng thời gian chơi
    achievements: json                   -- Thành tựu đạt được
    savePoints: json                     -- Điểm lưu game
}
```

### 2.8. CharacterGallery (Thư viện nhân vật)
```sql
CharacterGallery {
    characterId: string (Primary Key)   -- ID nhân vật
    name: string                        -- Tên nhân vật
    description: text                   -- Mô tả
    imageUrl: string                    -- URL ảnh
    unlockedBy: array                   -- Điều kiện mở khóa
    unlockDate: timestamp               -- Ngày mở khóa
    isFavorite: boolean                 -- Yêu thích
    viewCount: integer                  -- Số lần xem
}
```

### 2.9. MediaFiles (File media)
```sql
MediaFiles {
    fileId: string (Primary Key)        -- ID file
    fileName: string                    -- Tên file
    filePath: string                    -- Đường dẫn file
    fileType: string                    -- Loại file
    fileSize: integer                   -- Kích thước file
    uploadDate: timestamp               -- Ngày upload
    uploadedBy: string                  -- Người upload
    isPublic: boolean                   -- Trạng thái công khai
    downloadCount: integer              -- Số lần tải xuống
}
```

### 2.10. GameAssets (Tài nguyên game)
```sql
GameAssets {
    assetId: string (Primary Key)       -- ID tài nguyên
    assetName: string                   -- Tên tài nguyên
    assetType: string                   -- Loại tài nguyên
    assetPath: string                   -- Đường dẫn tài nguyên
    version: string                     -- Phiên bản
    size: integer                       -- Kích thước
    checksum: string                    -- Checksum
    isRequired: boolean                 -- Bắt buộc
    dependencies: array                 -- Phụ thuộc
}
```

### 2.11. ModalStates (Trạng thái modal)
```sql
ModalStates {
    modalId: string (Primary Key)       -- ID modal
    isOpen: boolean                     -- Trạng thái mở
    lastOpened: timestamp               -- Lần mở cuối
    openCount: integer                  -- Số lần mở
    userInteractions: json              -- Tương tác người dùng
    sessionId: string                   -- ID phiên
}
```

### 2.12. CarouselData (Dữ liệu carousel)
```sql
CarouselData {
    carouselId: string (Primary Key)    -- ID carousel
    currentIndex: integer               -- Index hiện tại
    autoPlay: boolean                   -- Tự động phát
    speed: integer                      -- Tốc độ
    totalItems: integer                 -- Tổng số item
    userInteractions: json              -- Tương tác người dùng
    lastUpdated: timestamp              -- Cập nhật cuối
}
```

### 2.13. SystemSettings (Cài đặt hệ thống)
```sql
SystemSettings {
    settingKey: string (Primary Key)   -- Key cài đặt
    settingValue: json                  -- Giá trị cài đặt
    description: text                   -- Mô tả
    lastUpdated: timestamp              -- Cập nhật cuối
    updatedBy: string                   -- Người cập nhật
    isPublic: boolean                   -- Công khai
    category: string                    -- Danh mục
}
```

### 2.14. ErrorLogs (Log lỗi)
```sql
ErrorLogs {
    logId: string (Primary Key)        -- ID log
    uid: string (Foreign Key)           -- Tham chiếu Users.uid
    errorType: string                   -- Loại lỗi
    errorMessage: text                  -- Thông báo lỗi
    timestamp: timestamp                -- Thời gian lỗi
    stackTrace: text                    -- Stack trace
    severity: string                    -- Mức độ nghiêm trọng
    resolved: boolean                   -- Đã xử lý
    resolvedAt: timestamp               -- Thời gian xử lý
}
```

### 2.15. CacheData (Dữ liệu cache)
```sql
CacheData {
    cacheKey: string (Primary Key)     -- Key cache
    cacheValue: json                    -- Giá trị cache
    expiresAt: timestamp                -- Thời gian hết hạn
    lastAccessed: timestamp             -- Truy cập cuối
    accessCount: integer                -- Số lần truy cập
    size: integer                       -- Kích thước
    category: string                    -- Danh mục
}
```

## 3. QUAN HỆ GIỮA CÁC THỰC THỂ

### 3.1. Quan hệ chính
```
Users (1) ──── (1) UserGameData
Users (1) ──── (1) CheckinDataLocal
Users (1) ──── (1) UserPreferences
Users (1) ──── (1) GameProgress
Users (1) ──── (M) UserEvents
Users (1) ──── (M) UserSessions
Users (1) ──── (M) DownloadEvents
Users (1) ──── (M) PageViews
Users (1) ──── (M) PromoCodeData
Users (1) ──── (M) ErrorLogs
```

### 3.2. Quan hệ phụ
```
DailyRewards (M) ──── (M) CheckinDataLocal
CharacterGallery (M) ──── (M) GameProgress
StoryChapters (M) ──── (M) GameProgress
MediaFiles (M) ──── (M) GameAssets
UserSessions (1) ──── (M) UserEvents
UserSessions (1) ──── (M) PageViews
```

## 4. CẤU TRÚC FIREBASE

### 4.1. Firebase Realtime Database
```
Users/
├── {uid}/
│   ├── Name: string
│   ├── Gold: integer
│   ├── Diamond: integer
│   └── MapInGame: json
DailyRewards/
├── {day}/
│   ├── diamondReward: integer
│   ├── isSpecialDay: boolean
│   └── bonusReward: json
SystemSettings/
├── {settingKey}/
│   ├── settingValue: json
│   ├── description: string
│   └── lastUpdated: timestamp
```

### 4.2. Firebase Firestore Collections
```
users/
├── {uid}/
│   ├── email: string
│   ├── displayName: string
│   ├── username: string
│   ├── createdAt: timestamp
│   └── lastLogin: timestamp

story_chapters/
├── {chapterId}/
│   ├── title: string
│   ├── content: string
│   ├── order: integer
│   └── isUnlocked: boolean

game_characters/
├── {characterId}/
│   ├── name: string
│   ├── description: string
│   ├── imageUrl: string
│   └── role: string

user_events/
├── {eventId}/
│   ├── uid: string
│   ├── eventType: string
│   ├── eventData: json
│   └── timestamp: timestamp

error_logs/
├── {logId}/
│   ├── uid: string
│   ├── errorType: string
│   ├── errorMessage: string
│   └── timestamp: timestamp
```

### 4.3. LocalStorage Structure
```javascript
// Check-in data
langhoaruc_checkin_data_{uid}: {
    totalDiamonds: integer,
    currentStreak: integer,
    totalCheckins: integer,
    checkinDates: array,
    lastCheckinDate: string,
    monthlyCheckins: object
}

// User preferences
langhoaruc_preferences_{uid}: {
    musicEnabled: boolean,
    theme: string,
    language: string,
    notifications: object,
    privacy: object
}

// Promo codes
langhoaruc_promo_{uid}: {
    code: string,
    usedAt: timestamp,
    reward: object,
    isValid: boolean
}

// Cache data
langhoaruc_cache_{key}: {
    value: object,
    expiresAt: timestamp,
    lastAccessed: timestamp
}
```

## 5. TÍNH NĂNG CHÍNH

### 5.1. Authentication System
- Firebase Auth (Email/Password, Google)
- User registration và login
- Password reset
- Session management

### 5.2. Daily Check-in System
- Điểm danh hàng ngày
- Phần thưởng kim cương
- Lịch điểm danh
- Đồng bộ Firebase

### 5.3. Game Progress System
- Lưu tiến độ game
- Mở khóa chương truyện
- Thành tựu và điểm lưu
- Đồng bộ đám mây

### 5.4. Story System
- 6 chương truyện
- Navigation giữa các chương
- Progress tracking
- Unlock requirements

### 5.5. Character Gallery
- Carousel nhân vật
- Thông tin chi tiết
- Unlock system
- Favorite system

### 5.6. Media Management
- Quản lý file media
- Upload và download
- Asset versioning
- Cache system

### 5.7. Analytics & Tracking
- User behavior tracking
- Download analytics
- Page view analytics
- Error logging

### 5.8. UI/UX Management
- Modal state management
- Carousel control
- User preferences
- Theme system

## 6. CÔNG NGHỆ SỬ DỤNG

### 6.1. Frontend
- HTML5, CSS3, JavaScript ES6+
- Responsive design
- Progressive Web App (PWA)
- Service Workers

### 6.2. Backend Services
- Firebase Authentication
- Firebase Firestore
- Firebase Realtime Database
- Firebase Storage
- Firebase Analytics

### 6.3. Third-party Libraries
- SweetAlert2 (Notifications)
- AOS (Animations)
- Font Awesome (Icons)
- Google Fonts (Typography)

### 6.4. Development Tools
- Google Analytics
- Firebase Console
- Chrome DevTools
- Git Version Control

## 7. BẢO MẬT VÀ PRIVACY

### 7.1. Data Protection
- Firebase Security Rules
- HTTPS encryption
- User data anonymization
- GDPR compliance

### 7.2. Authentication Security
- Firebase Auth security
- Password hashing
- Session management
- CSRF protection

### 7.3. Privacy Controls
- User consent management
- Data retention policies
- Right to deletion
- Data portability

## 8. PERFORMANCE OPTIMIZATION

### 8.1. Caching Strategy
- LocalStorage caching
- Service Worker caching
- CDN optimization
- Image optimization

### 8.2. Database Optimization
- Firebase indexing
- Query optimization
- Data pagination
- Real-time updates

### 8.3. Frontend Optimization
- Code splitting
- Lazy loading
- Image compression
- CSS optimization

## 9. MONITORING VÀ MAINTENANCE

### 9.1. Error Monitoring
- Firebase Crashlytics
- Error logging
- Performance monitoring
- User feedback

### 9.2. Analytics Monitoring
- User behavior analytics
- Performance metrics
- Business metrics
- A/B testing

### 9.3. Maintenance Tasks
- Database cleanup
- Cache management
- Security updates
- Performance optimization

---

**Tổng kết**: ERD này bao gồm đầy đủ các thực thể, quan hệ và cấu trúc dữ liệu cho dự án "Làng Hoa Rực" - một web game nông trại với hệ thống điểm danh, cốt truyện và quản lý người dùng phức tạp.
