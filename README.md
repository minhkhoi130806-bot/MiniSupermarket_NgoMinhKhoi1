# 🛒 HỆ THỐNG QUẢN LÝ SIÊU THỊ MINI (MINISUPERMARKET SYSTEM)

> **Môn học:** Lập trình Ứng dụng .NET Core (Mã môn: 229162)  
> **Buổi thực hành:** Buổi 4 - Thiết kế kiến trúc giao diện WinForms Shell và điều hướng phân quyền người dùng

---

## 🏗️ 1. Mục tiêu và Bối cảnh Thực tế

- **Mục tiêu:**
  - Làm chủ kỹ thuật thiết kế giao diện Windows Forms theo mô hình Single-Form Architecture, sử dụng thanh điều hướng Sidebar bên trái và vùng nội dung chính Main Content Panel.
  - Nắm vững kỹ thuật nhúng động Form con vào vùng chứa bằng `TopLevel = false` và `Dock = Fill`, giúp các màn hình nghiệp vụ hiển thị trong cùng một cửa sổ.
  - Xây dựng cơ chế phân quyền giao diện theo vai trò người dùng (Role-Based Access Control - RBAC) với ba vai trò: Admin, Cashier và Warehouse.
  - Hiện thực hóa việc điều hướng đến màn hình phù hợp sau khi đăng nhập thành công.
  - Tạo trải nghiệm giao diện thống nhất thông qua việc thay đổi màu nút đang chọn và cập nhật tiêu đề màn hình.

- **Bối cảnh thực tế đồ án:**

  Trong hệ thống quản lý siêu thị mini, mỗi nhân viên được cấp quyền sử dụng những chức năng phù hợp với công việc.

  - **Admin:** Quản lý toàn bộ hệ thống, bao gồm danh mục, sản phẩm, khách hàng, báo cáo doanh thu và tài khoản.
  - **Cashier (Thu ngân):** Sử dụng màn hình bán hàng POS và quản lý khách hàng thân thiết.
  - **Warehouse (Thủ kho):** Quản lý danh mục nhóm hàng, sản phẩm và hoạt động kho.

  Yêu cầu đặt ra là xây dựng màn hình điều khiển trung tâm `FormMainShell`, tự động hiển thị các chức năng phù hợp với vai trò sau khi người dùng đăng nhập.

---

## 🛠️ 2. Công nghệ sử dụng

- **Ngôn ngữ:** C#.
- **Nền tảng:** .NET 8.0.
- **Backend:** ASP.NET Core Web API.
- **Frontend:** Windows Forms (.NET 8.0).
- **Giao tiếp API:** `System.Net.Http.Json`, `System.Text.Json`.
- **Giao diện:** Windows Forms Controls như `Panel`, `Button`, `Label`, `DataGridView`, `TextBox` và `ComboBox`.
- **Kỹ thuật chính:** Single-Form Architecture, Dynamic Child Forms, Role-Based Access Control (RBAC), bất đồng bộ `async/await`.
- **Công cụ phát triển:** Visual Studio 2022.

---

## 📂 3. Cấu trúc Solution

```text
MiniSupermarket_NgoMinhKhoi1/
│
├── MiniSupermarket.API/                 # Backend Web API
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Migrations/
│   ├── Program.cs
│   └── appsettings.json
│
└── MiniSupermarket.WinForms/            # Frontend Windows Forms
    ├── Program.cs                       # Điểm khởi chạy ứng dụng
    ├── FormLogin.cs                     # Giao diện đăng nhập
    ├── SessionManager.cs                # Lưu thông tin phiên đăng nhập
    ├── ApiClientService.cs               # Cấu hình HttpClient gọi API
    ├── FormMainShell.cs                  # Giao diện điều hướng trung tâm
    ├── FormCategoryManagement.cs         # Quản lý danh mục
    ├── FormProductManagement.cs          # Quản lý sản phẩm và kho
    ├── FormCustomerManagement.cs         # Quản lý khách hàng
    ├── FormPOS.cs                        # Màn hình bán hàng
    ├── FormQuickReport.cs                # Báo cáo doanh thu
    └── FormUserManagement.cs             # Quản lý tài khoản
```

*Lưu ý: Cấu trúc trên mô tả các thành phần theo nội dung thực hành Buổi 4. Tên thư mục hoặc Form thực tế có thể khác tùy cấu trúc Solution hiện tại.*

---

## 🚀 4. Nội dung thực hành và hướng dẫn triển khai

### Bước 1: Thiết kế giao diện FormMainShell

Tạo Form `FormMainShell.cs` làm màn hình điều khiển chính sau khi đăng nhập.

Cấu hình Form:

- `Size`: 1280 × 720.
- `StartPosition`: CenterScreen.
- `Text`: Hệ thống Quản lý Bán lẻ & Tồn kho Siêu thị Mini.

Bố trí giao diện gồm ba khu vực chính:

**1. panelSidebar – Thanh điều hướng bên trái**

- Chiều rộng: 230.
- Màu nền: Xanh đen đậm.
- Chứa logo MiniMart và các nút chức năng:
  - `btnPOS`: Bán hàng (POS).
  - `btnCategory`: Quản lý Danh mục.
  - `btnProduct`: Quản lý Sản phẩm.
  - `btnCustomer`: Quản lý Khách hàng.
  - `btnReports`: Báo cáo Doanh thu.
  - `btnUserManage`: Quản trị Tài khoản.
  - `btnLogout`: Đăng xuất.

**2. panelTopHeader – Thanh tiêu đề**

- Chiều cao: 60.
- Chứa `lblTitle` để hiển thị tên màn hình hiện tại.
- Chứa `lblUserInfo` để hiển thị tên đăng nhập và vai trò người dùng.

**3. panelMainContent – Vùng nội dung chính**

- Sử dụng `Dock = Fill`.
- Là nơi nhúng các Form con nghiệp vụ.
- Không mở thêm cửa sổ riêng cho từng chức năng khi chuyển màn hình.

### Bước 2: Xây dựng cơ chế nhúng Form con

Sử dụng phương thức `OpenChildForm()` trong `FormMainShell.cs` để mở Form nghiệp vụ bên trong `panelMainContent`.

Các thao tác chính:

1. Đóng Form con đang hiển thị nếu có.
2. Ghi nhận Form con mới được chọn.
3. Thiết lập `TopLevel = false`.
4. Thiết lập `FormBorderStyle = None`.
5. Thiết lập `Dock = Fill`.
6. Thêm Form con vào `panelMainContent`.
7. Cập nhật `lblTitle`.
8. Hiển thị Form con và làm nổi bật nút chức năng đang chọn.

Kết quả mong muốn: Người dùng có thể chuyển giữa các màn hình nghiệp vụ ngay trong cửa sổ chính, không tạo ra nhiều cửa sổ rời rạc.

### Bước 3: Xây dựng cơ chế phân quyền RBAC

Sử dụng phương thức `ApplyRolePermissions()` để điều chỉnh khả năng hiển thị của các nút theo vai trò được lưu trong `SessionManager.CurrentRole`.

| Chức năng | Admin | Cashier | Warehouse |
|---|:---:|:---:|:---:|
| Bán hàng POS | Có | Có | Không |
| Quản lý danh mục | Có | Không | Có |
| Quản lý sản phẩm | Có | Không | Có |
| Quản lý khách hàng | Có | Có | Không |
| Báo cáo doanh thu | Có | Không | Không |
| Quản trị tài khoản | Có | Không | Không |

Nếu vai trò không hợp lệ, ứng dụng hiển thị thông báo cảnh báo và đóng màn hình điều khiển.

Ngoài việc ẩn các nút trên giao diện, các chức năng quan trọng cần được kiểm tra quyền trước khi thực thi. Nếu API có xác thực và phân quyền, backend cũng phải kiểm tra quyền truy cập để tránh trường hợp người dùng bỏ qua giao diện client.

### Bước 4: Điều hướng đến màn hình mặc định

Sau khi đăng nhập thành công, `FormMainShell` hiển thị thông tin người dùng và mở màn hình mặc định theo vai trò.

- **Admin:** Mở màn hình quản lý danh mục.
- **Cashier:** Mở màn hình bán hàng POS.
- **Warehouse:** Mở màn hình quản lý sản phẩm và kho.

Màn hình mặc định có thể được thay đổi theo yêu cầu triển khai thực tế.

### Bước 5: Xây dựng các Form nghiệp vụ động

Các Form con được nhúng vào `panelMainContent` bao gồm:

- **FormPOS:** Quản lý giỏ hàng, quét mã vạch, tính tổng tiền, nhận tiền khách và xử lý thanh toán.
- **FormCategoryManagement:** Thêm, sửa, xóa và hiển thị danh mục nhóm hàng.
- **FormProductManagement:** Quản lý thông tin sản phẩm, giá bán, số lượng tồn kho và danh mục.
- **FormCustomerManagement:** Quản lý khách hàng và thông tin tích điểm.
- **FormQuickReport:** Hiển thị báo cáo doanh thu và hiệu suất bán hàng.
- **FormUserManagement:** Quản lý tài khoản nhân viên, vai trò và trạng thái hoạt động.

Các Form nghiệp vụ sử dụng `ApiClientService` để giao tiếp với Web API khi cần đọc hoặc cập nhật dữ liệu.

### Bước 6: Xây dựng chức năng đăng xuất

Khi người dùng chọn `btnLogout`, ứng dụng hiển thị hộp thoại xác nhận.

Nếu người dùng đồng ý:

1. Xóa thông tin phiên đăng nhập.
2. Xóa JWT token và thông tin vai trò nếu đang sử dụng.
3. Đóng hoặc ẩn màn hình điều khiển.
4. Quay lại màn hình đăng nhập `FormLogin`.

---

## 🧪 5. Dữ liệu tài khoản mẫu kiểm thử phân quyền

Theo tài liệu thực hành Buổi 4, hệ thống có 15 tài khoản mẫu với mật khẩu ban đầu là `123456`.

| STT | Tên đăng nhập | Vai trò |
|---:|---|---|
| 1 | admin01 | Admin |
| 2 | admin02 | Admin |
| 3 | cashier01 | Cashier |
| 4 | cashier02 | Cashier |
| 5 | cashier03 | Cashier |
| 6 | cashier04 | Cashier |
| 7 | cashier05 | Cashier |
| 8 | ware01 | Warehouse |
| 9 | ware02 | Warehouse |
| 10 | ware03 | Warehouse |
| 11 | ware04 | Warehouse |
| 12 | admin_backup | Admin |
| 13 | cashier06 | Cashier |
| 14 | ware05 | Warehouse |
| 15 | supervisor | Admin |

*Lưu ý: Đây là bộ tài khoản mẫu phục vụ kiểm thử theo tài liệu. Cần bảo đảm các tài khoản thực sự tồn tại trong hệ thống đăng nhập trước khi sử dụng.*

---

## ✅ 6. Đánh giá kết quả đạt được

- [ ] Giao diện `FormMainShell` được chia thành Sidebar, Header và Main Content Panel.
- [ ] Các Form con được nhúng động trong vùng nội dung chính.
- [ ] Nút chức năng đang chọn được đổi màu nổi bật.
- [ ] Tiêu đề màn hình được cập nhật đúng khi chuyển chức năng.
- [ ] Tài khoản Admin hiển thị đầy đủ chức năng.
- [ ] Tài khoản Cashier chỉ truy cập POS và khách hàng.
- [ ] Tài khoản Warehouse chỉ truy cập danh mục và sản phẩm.
- [ ] Tài khoản có vai trò không hợp lệ không thể truy cập màn hình chính.
- [ ] Chức năng đăng xuất xóa thông tin phiên và quay lại màn hình đăng nhập.
- [ ] Các thao tác gọi API được xử lý phù hợp và hiển thị thông báo khi có lỗi.

---

## 📚 7. Bài tập thực hành mở rộng

**Đề bài:** Xây dựng màn hình Báo cáo Doanh thu Nhanh (`FormQuickReport`) và kiểm soát quyền truy cập.

Yêu cầu:

1. Tạo Form `FormQuickReport`.
2. Bổ sung bộ chọn ngày xem báo cáo.
3. Hiển thị tổng số hóa đơn, tổng doanh thu và mặt hàng bán chạy nhất.
4. Chỉ cho phép tài khoản Admin sử dụng chức năng xem báo cáo doanh thu.
5. Kiểm tra quyền truy cập cả ở giao diện và tại API nếu backend hỗ trợ xác thực, phân quyền.
6. Kiểm thử bằng tài khoản Admin, Cashier và Warehouse.

---

## 👨‍💻 8. Thông tin sinh viên

- **Họ tên sinh viên:** Ngô Minh Khôi
- **Mã sinh viên:** 2124110139
- **Lớp học phần:** CCQ2411D
- **Môn học:** Lập trình Ứng dụng .NET Core
- **Nội dung thực hành:** Buổi 4 – Thiết kế giao diện WinForms Shell và điều hướng phân quyền người dùng
