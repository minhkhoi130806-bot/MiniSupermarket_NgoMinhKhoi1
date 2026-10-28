# 🛒 HỆ THỐNG QUẢN LÝ SIÊU THỊ MINI (MINISUPERMARKET SYSTEM)

> **Môn học:** Lập trình Ứng dụng .NET Core (Mã môn: 229162)  
> **Đồ án cuối khóa:** Xây dựng Hệ thống Bán lẻ & Quản lý Kho Siêu thị Mini (Chuẩn 50 Tiêu chí Đánh giá)

---

## 🏗️ 1. Mô hình Kiến trúc Hệ thống & Kỹ thuật Nền tảng (Client - Server)

Dự án được thiết kế và triển khai chuẩn mực theo mô hình kiến trúc phân tầng chuyên nghiệp, kết nối phi trạng thái (Stateless) giữa ứng dụng máy trạm và máy chủ:

* **`MiniSupermarket.API` (Backend Server):**
  - Dự án **ASP.NET Core Web API** xây dựng chuẩn kiến trúc RESTful.
  - Tích hợp bộ điều khiển xác thực `AuthController` cấp phát mã thông hành bảo mật **JWT (JSON Web Token)**.
  - Phân quyền người dùng nghiêm ngặt dựa trên vai trò (**Role-Based Access Control - RBAC**) với thuộc tính `[Authorize(Roles = "...")]`.
  - Quản trị dữ liệu bằng **Entity Framework Core (Code-First)** kết hợp tối ưu hóa truy vấn hiệu năng cao bằng **ADO.NET thuần**, **LINQ Server-side Paging**, **Stored Procedure** và **Transaction Scope**.

* **`MiniSupermarket.WinForms` (Frontend Desktop Client):**
  - Ứng dụng máy trạm Windows Forms được thiết kế theo mô hình **Single-Form Shell Architecture (`FormMainShell`)**.
  - Nhúng động các màn hình nghiệp vụ con (`TopLevel = false`, `Dock = Fill`) giúp ứng dụng chuyển đổi mượt mà, không giật lag.
  - Giao tiếp máy chủ tập trung thông qua `ApiClientService` (`HttpClient`), tự động đính kèm `Bearer Token` vào Header của mọi request.
  - Giao diện người dùng hiện đại: Điều hướng phân quyền động, phân trang lưới dữ liệu, quầy tính tiền POS thông minh, cảnh báo tồn kho đỏ, biểu đồ cột thống kê doanh thu và xuất báo cáo đa định dạng (Excel .xlsx & CSV .csv).

* **Hệ quản trị CSDL Microsoft SQL Server:**
  - Cơ sở dữ liệu quan hệ được sinh tự động thông qua **EF Core Migrations**.
  - Tự động nạp sẵn dữ liệu khởi tạo (**Seed Data**) bao gồm các vai trò, tài khoản nhân viên, nhóm hàng, sản phẩm và khách hàng thân thiết.

---

## 🛠️ 2. Công nghệ & Kỹ thuật Sử dụng

* **Ngôn ngữ lập trình:** C# (.NET 8.0 / .NET 9.0)
* **Backend Framework:** ASP.NET Core Web API
* **Bảo mật & Phân quyền:** JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`), Role-Based Authorization
* **Công nghệ Cơ sở dữ liệu:**
  - Entity Framework Core (`Microsoft.EntityFrameworkCore.SqlServer`, Migrations, Code-First)
  - ADO.NET thuần (`Microsoft.Data.SqlClient`: `SqlConnection`, `SqlCommand`, `SqlDataReader`, `SqlParameter`)
  - Thủ tục lưu trữ CSDL (**Stored Procedure**: `sp_GetDailyRevenue`)
  - Giao dịch an toàn (**Transaction Scope**: Cam kết trừ kho và điểm thưởng đồng thời, tự động Rollback khi lỗi)
* **Frontend Framework:** Windows Forms (.NET Desktop Application)
* **Giao tiếp API:** `System.Net.Http`, `System.Net.Http.Json`, `System.Text.Json`
* **Xử lý giao diện & Báo cáo:** DataGridView Auto-Formatting, Phân trang LINQ `.Skip().Take()`, Biểu đồ cột `RevenueBarChart`, Xuất báo cáo đa định dạng Excel `.xlsx` (OpenXML) & CSV `.csv` (UTF-8 BOM) tự động tạo kiểu đẹp mắt.
* **Công cụ phát triển & Kiểm thử:** Visual Studio 2022, Swagger UI (tích hợp Authorization Bearer), Git & GitHub

---

## 📂 3. Cấu trúc Solution Đồ án

```text
MiniSupermarket_NgoMinhKhoi1/
│
├── MiniSupermarket.API/                 # Dự án Web API (Backend Server)
│   ├── Controllers/                     # Các Controller chuyên biệt chuẩn RESTful
│   │   ├── AuthController.cs            # Đăng nhập, xác thực và cấp mã JWT Token
│   │   ├── CategoriesController.cs      # CRUD Nhóm hàng, phân trang & phân quyền Admin
│   │   ├── ProductsController.cs        # CRUD Sản phẩm, ADO.NET Barcode & phân trang
│   │   ├── CustomersController.cs       # Quản lý khách hàng, điểm thưởng & phân trang
│   │   ├── OrdersController.cs          # Tạo hóa đơn bán hàng bọc trong Transaction
│   │   ├── ReportsController.cs         # Dashboard KPI, Top 5 Best-Sellers & Stored Procedure
│   │   ├── RolesController.cs           # Danh mục vai trò hệ thống
│   │   └── UsersController.cs           # Quản trị tài khoản nhân viên
│   ├── Data/
│   │   └── SupermarketDbContext.cs      # Cấu hình DbContext, Fluent API & Seed Data
│   ├── Models/                          # Các lớp thực thể (Entities) & DTOs truyền nhận
│   ├── Migrations/                      # Lịch sử đồng bộ CSDL Code-First
│   ├── Program.cs                       # Cấu hình CORS, JWT Authentication, Swagger Security
│   └── appsettings.json                 # Chuỗi kết nối CSDL SQL Server
│
└── MiniSupermarket.WinForms/            # Dự án Windows Forms (Frontend Client)
    ├── Program.cs                       # Điểm khởi chạy ứng dụng (Mở FormLogin)
    ├── FormLogin.cs                     # Màn hình đăng nhập xác thực
    ├── SessionManager.cs                # Quản lý phiên làm việc, lưu JWT Token & Role
    ├── ApiClientService.cs              # Cấu hình HttpClient tập trung
    ├── FormMainShell.cs                 # Giao diện Shell điều hướng trung tâm & RBAC
    ├── FormCategoryManagement.cs        # Quản lý Nhóm hàng (Phân trang 8 dòng/trang)
    ├── FormProductManagement.cs         # Quản lý Sản phẩm (Phân trang, Cảnh báo tồn kho đỏ, Xuất CSV)
    ├── FormCustomerManagement.cs        # Quản lý Khách hàng (Phân trang, Điểm tích lũy)
    ├── FormPOS.cs                       # Quầy bán hàng tính tiền (Quét mã, Đổi điểm F11, In bill)
    ├── FormQuickReport.cs               # Báo cáo doanh thu (KPI Cards, Biểu đồ cột, Top 5 bán chạy)
    └── FormUserManagement.cs            # Quản trị người dùng & phân quyền tài khoản
```

---

## 🏆 4. Bảng Tổng hợp Tính năng & 50 Tiêu chí Đã Hoàn Thành

Hệ thống đã triển khai hoàn tất **100% các tiêu chí kỹ thuật** trong bảng đánh giá đồ án môn học:

### 4.1. Kiến trúc phân tầng & Kết nối API chuẩn RESTful (STT 11.0 ➔ 20.0)
- [x] **Cấu trúc Solution 2 Project:** Tách bạch hoàn toàn giữa Backend API và Frontend Desktop.
- [x] **Cấu hình Program.cs:** Đăng ký dịch vụ DI, kích hoạt chính sách CORS `AllowAll` cho phép gọi API đa nền tảng.
- [x] **Định tuyến chuẩn RESTful:** Sử dụng đúng các Attribute `[Route]`, phương thức HTTP chuẩn (`GET`, `POST`, `PUT`, `DELETE`) và mã trạng thái (`200 OK`, `201 Created`, `204 NoContent`, `400 BadRequest`, `401 Unauthorized`, `403 Forbidden`, `404 NotFound`).
- [x] **Truyền nhận dữ liệu qua DTO:** Ứng dụng mô hình DTO độc lập, bóc tách dữ liệu JSON không để lộ cấu trúc bảng CSDL.
- [x] **Lập trình bất đồng bộ (`async / await`):** 100% các tác vụ I/O, truy vấn CSDL và gọi mạng đều sử dụng Non-blocking `async/await`.
- [x] **Kiểm thử trực quan với Swagger UI:** Tích hợp bộ công cụ Swagger hỗ trợ đính kèm mã xác thực `Bearer {token}` ngay trên giao diện web.
- [x] **Thực thể & Cấu hình CSDL:** Tạo đầy đủ các Model Entity, ràng buộc khóa chính, khóa ngoại, kiểm tra tính hợp lệ qua Data Annotations và Fluent API.

### 4.2. Cơ sở dữ liệu & Tối ưu hóa truy vấn nâng cao (STT 21.0 ➔ 35.0)
- [x] **EF Core Migrations & Seed Data:** Khởi tạo CSDL tự động kèm bộ dữ liệu mẫu đầy đủ cho tất cả các bảng.
- [x] **CRUD & LINQ Queries:** Thực hiện trọn vẹn các thao tác dữ liệu, tối ưu hóa đọc dữ liệu bằng `.AsNoTracking()` và nạp liên kết `.Include()`.
- [x] **Phân trang dữ liệu máy chủ (Server-side Paging):** Áp dụng LINQ `.Skip()` và `.Take()`, giảm tải đường truyền mạng và bộ nhớ máy trạm.
- [x] **Bảo mật JWT & Phân quyền RBAC:** Cấp phát Token mã hóa đối xứng, kiểm soát chặt chẽ quyền Admin thông qua `[Authorize(Roles = "Admin")]`.
- [x] **Tra cứu mã vạch bằng ADO.NET thuần:** Xây dựng endpoint `api/products/adonet-barcode/{barcode}` sử dụng `SqlConnection`, `SqlCommand`, `SqlDataReader` và tham số hóa `@Barcode` ngăn chặn triệt để lỗ hổng tấn công **SQL Injection**.
- [x] **Báo cáo bằng Stored Procedure:** Tự động tạo và thực thi thủ tục `sp_GetDailyRevenue` thông qua ADO.NET để tổng hợp doanh thu theo khoảng ngày.
- [x] **Giao dịch thanh toán an toàn (Transaction Scope):** Bọc toàn bộ quy trình tạo đơn hàng, trừ tồn kho và trừ điểm tích lũy khách hàng trong cùng một giao dịch CSDL (tự động Rollback hoàn tác nếu xảy ra sự cố).

### 4.3. Ứng dụng Desktop WinForms & Trải nghiệm Người dùng (STT 36.0 ➔ 48.0)
- [x] **Kiến trúc Shell không giật lag (`FormMainShell`):** Điều hướng trung tâm với thanh Sidebar cố định, nhúng động Form con tức thời.
- [x] **Phân quyền giao diện động theo 3 Role:** 
  - *Admin:* Toàn quyền quản trị tất cả các chức năng.
  - *Cashier (Thu ngân):* Chỉ hiển thị màn hình Bán hàng (POS) và Khách hàng.
  - *Warehouse (Thủ kho):* Chỉ hiển thị màn hình Danh mục và Quản lý Sản phẩm.
- [x] **Kết nối API tập trung:** Đóng gói toàn bộ cấu hình `HttpClient` và xử lý lỗi mạng thân thiện qua `ApiClientService`.
- [x] **Phân trang giao diện 8 dòng/trang đồng bộ:** Thiết kế thanh Footer chuẩn giao diện hiện đại với các nút `[⏮ Đầu] [◀ Trước] [Trang x/y] [Sau ▶] [Cuối ⏭]` cho cả 3 màn hình Sản phẩm, Nhóm hàng và Khách hàng.
- [x] **Cảnh báo hàng sắp hết kho (Low-Stock Alert):** Bắt sự kiện `CellFormatting` của DataGridView, tự động đổi màu nền ô tồn kho thành màu đỏ cam (`LightCoral`) cho các sản phẩm có số lượng tồn `<= 10`.
- [x] **Quầy thu ngân POS thông minh:** Hỗ trợ nhập mã vạch, tự động tính tổng tiền, tính tiền thừa và in phiếu thanh toán chi tiết.
- [x] **Đổi điểm thưởng trừ tiền hóa đơn (Loyalty Points):** Khách hàng thân thiết có thể sử dụng điểm tích lũy (phím tắt `F11` hoặc bấm vào tên khách) để trừ trực tiếp vào hóa đơn và cập nhật ngay vào CSDL.
- [x] **Dashboard KPI & Biểu đồ cột Doanh thu:** Hiển thị 4 thẻ chỉ số KPI tổng quan, biểu đồ cột tăng trưởng doanh thu (`RevenueBarChart`) và thống kê Top 5 sản phẩm bán chạy nhất (`Best-Sellers`).
- [x] **Xuất báo cáo đa định dạng Excel (.xlsx) & CSV (.csv) tự động trình bày đẹp:**
  - *Báo cáo Sản phẩm & Tồn kho (`FormProductManagement`):* Xuất toàn bộ danh sách kèm số lượng tồn, giá bán; tự động định dạng bảng tiêu đề xanh lá đậm, dòng xen kẽ (zebra), viền bảng, định dạng tiền tệ `#,##0 VNĐ`, bôi màu đỏ cảnh báo các mặt hàng sắp hết/hết hàng (`<= 10`), dòng tổng kết cộng dồn số lượng và giá trị kho.
  - *Báo cáo Doanh thu & Đơn hàng (`FormQuickReport`):* Xuất theo khoảng thời gian tùy chọn với 4 thẻ tóm tắt KPI màu sắc nét, bảng chi tiết từng đơn hàng, tự động định dạng số tiền, ngày giờ và hỏi mở file ngay sau khi xuất. Hỗ trợ cả `.xlsx` (OpenXML chuẩn) và `.csv` (UTF-8 có BOM chống lỗi font tiếng Việt trong Excel).

---

## 🚀 5. Hướng dẫn Cài đặt & Khởi chạy Dự án

### Bước 1: Khởi chạy phía Backend (Web API)
1. Mở Solution `MiniSupermarket.sln` bằng **Visual Studio 2022**.
2. Kiểm tra chuỗi kết nối trong `MiniSupermarket.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MiniSupermarketDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```
3. Nhấp chuột phải vào project `MiniSupermarket.API` ➔ Chọn **Set as Startup Project**.
4. Nhấn **F5** để khởi chạy. Trình duyệt sẽ tự động mở giao diện tài liệu **Swagger UI**.

### Bước 2: Khởi chạy phía Frontend (WinForms Client)
1. Nhấp chuột phải vào project `MiniSupermarket.Winforms` ➔ Chọn **Debug -> Start New Instance** (hoặc đặt làm Startup Project và chạy).
2. Giao diện đăng nhập `FormLogin` sẽ xuất hiện đầu tiên.
3. Sử dụng các tài khoản kiểm thử phân quyền mẫu dưới đây để đăng nhập và trải nghiệm:

| Tên tài khoản | Mật khẩu | Vai trò (Role) | Chức năng hiển thị |
| :--- | :--- | :--- | :--- |
| **admin** | `admin123` | **Admin** | Đầy đủ quyền: POS, Danh mục, Sản phẩm, Khách hàng, Báo cáo & Tài khoản |
| **cashier01** | `cashier123` | **Cashier** | Màn hình Bán hàng (POS) và Tra cứu/Đăng ký Khách hàng |
| **ware01** | `ware123` | **Warehouse** | Màn hình Quản lý Danh mục (Nhóm hàng) và Quản lý Sản phẩm / Kho |

---

## 👨‍💻 6. Tác giả

* **Họ tên sinh viên:** Ngô Minh Khôi
* **Mã sinh viên:** 2124110139
* **Lớp học phần:** CCQ2411D
* **Môn học:** Lập trình Ứng dụng .NET Core
