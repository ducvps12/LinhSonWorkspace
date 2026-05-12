# Báo cáo Sử dụng Generative AI (GenAI) trong Đồ án PRN212

**Tên dự án:** Linh Son Workspace Booking Management System
**Môn học:** PRN212

---

## 1. Mục tiêu ứng dụng GenAI
Trong quá trình phát triển ứng dụng quản lý không gian làm việc Linh Son Workspace, Generative AI (ở đây là các LLM Agent/Assistant) đã được sử dụng như một người hướng dẫn (mentor) và người lập trình cặp (pair-programmer) nhằm:
- Rút ngắn thời gian thiết kế giao diện UI/UX với WPF.
- Tối ưu hóa các truy vấn LINQ phức tạp.
- Đảm bảo thiết kế Database tuân thủ chuẩn Entity Framework Core Code-First.
- Nâng cao chất lượng code bằng cách áp dụng các Design Pattern (MVVM).

---

## 2. Các giai đoạn áp dụng GenAI

### 2.1. Phân tích yêu cầu & Thiết kế Database (EF Core)
Thay vì tự tạo từng bảng SQL thủ công, AI được sử dụng để chuyển đổi các yêu cầu nghiệp vụ thành các lớp Models trong C# và thiết lập quan hệ (Relationships) cho EF Core.

**Prompt mẫu đã sử dụng:**
> *"Tôi đang làm đồ án PRN212 quản lý Booking Workspace bằng WPF và EF Core. Hãy giúp tôi thiết kế các Model C# với Data Annotations và Fluent API cho AppDbContext. Hệ thống cần các bảng: Role, User, Customer, Workspace, WorkspaceType, Booking, và ActivityLog. Quan hệ giữa Booking và Workspace là 1-nhiều."*

**Đóng góp của AI:**
AI đã tạo ra file `AppDbContext.cs` với cấu hình `OnModelCreating` chuẩn xác (xử lý DeleteBehavior.Restrict để tránh lỗi Multiple Cascade Paths trong SQL Server).

### 2.2. Xây dựng giao diện (WPF XAML & UI/UX)
Việc viết XAML thủ công cho một giao diện hiện đại (Modern Dark Theme) tốn rất nhiều thời gian. AI đã giúp tạo ra các `ResourceDictionary` chứa các Style dùng chung.

**Prompt mẫu đã sử dụng:**
> *"Viết cho tôi mã XAML để tạo một giao diện Dashboard đẹp mắt, có Dark mode (#16213e), bo góc (CornerRadius) và đổ bóng (DropShadowEffect) cho các Card. Sử dụng DataGrid để hiển thị danh sách Booking."*

**Đóng góp của AI:**
AI đã định nghĩa các Global Styles (như `PrimaryButton`, `ModernTextBox`, `ContentCard`) trong `App.xaml`, giúp toàn bộ ứng dụng có giao diện nhất quán, đẹp mắt mà không cần code lặp lại.

### 2.3. Xử lý Logic Nghiệp vụ & LINQ (Conflict Detection)
Phần khó nhất của dự án là thuật toán kiểm tra xem khách hàng đặt phòng có bị trùng lịch với người khác không. 

**Prompt mẫu đã sử dụng:**
> *"Làm sao để viết câu lệnh LINQ kiểm tra xem khoảng thời gian từ StartTime đến EndTime mà khách vừa chọn có bị trùng với bất kỳ Booking nào đã tồn tại của cùng một WorkspaceId hay không? Lưu ý bỏ qua các booking có status là Cancelled."*

**Đóng góp của AI:**
AI đã cung cấp giải pháp xử lý khoảng thời gian (Time Overlap Logic) cực kỳ tối ưu:
```csharp
bool isConflict = await _context.Bookings.AnyAsync(b => 
    b.WorkspaceId == workspaceId && 
    b.Status != "Cancelled" && 
    startTime < b.EndTime && 
    endTime > b.StartTime);
```

### 2.4. Xử lý Concurrency & Stream I/O
Theo yêu cầu PRN212, ứng dụng cần xử lý đa luồng và đọc/ghi file.

**Đóng góp của AI:**
- AI hướng dẫn sử dụng `async/await` cho toàn bộ thao tác với DB để UI không bị đơ (frozen).
- Hướng dẫn dùng `StreamWriter` để viết hàm `ExportBookingsToCsvAsync` xuất dữ liệu ra file Excel và hàm `LogActivityAsync` để ghi log hệ thống ra file `.txt`.

---

## 3. Đánh giá & Bài học rút ra khi dùng GenAI

**Ưu điểm:**
- Nâng cao năng suất: GenAI gõ boilerplate code (như các class ViewModel với INotifyPropertyChanged) rất nhanh.
- Học hỏi Best Practices: GenAI hướng dẫn cách tổ chức thư mục chuẩn MVVM (Models, Views, ViewModels, Services).

**Hạn chế & Cách khắc phục:**
- Thỉnh thoảng GenAI đề xuất dùng các thư viện ngoài (Third-party packages) không phù hợp với yêu cầu cơ bản của môn học. **Cách khắc phục:** Cần xem xét kỹ code AI đưa ra, yêu cầu AI chỉ sử dụng các thư viện chuẩn của .NET (như System.IO, System.Linq).
- AI không thể kiểm thử ứng dụng hộ mình. Lập trình viên vẫn phải tự chạy debug (Visual Studio) và kiểm tra lại luồng hoạt động (flow).

**Kết luận:**
GenAI là một công cụ hỗ trợ tuyệt vời, giúp sinh viên vượt qua các rào cản kỹ thuật khó (như cấu hình SQL Server, viết giao diện XAML phức tạp). Tuy nhiên, tư duy thiết kế luồng chạy và quyết định nghiệp vụ (Business logic) vẫn phụ thuộc hoàn toàn vào kiến thức lập trình nền tảng của bản thân sinh viên (OOP, C#).
