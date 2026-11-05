# 🛒 HỆ THỐNG QUẢN LÝ BÁN LẺ & TỒN KHO SIÊU THỊ MINI (MINIMART POS)

> **Môn học:** Lập trình Ứng dụng .NET Core (Mã môn: 229162)  
> **Sinh viên thực hiện:** Ngô Minh Khôi — **MSSV:** 2124110139 — **Lớp:** CCQ2411C / CCQ2411D  
> **Đồ án cuối khóa:** Xây dựng Hệ thống Bán lẻ & Quản lý Kho Siêu thị Mini (Chuẩn 50 Tiêu chí Đánh giá của Giảng viên)  
> **Repository GitHub:** [https://github.com/minhkhoi130806-bot/MiniSupermarket_NgoMinhKhoi1](https://github.com/minhkhoi130806-bot/MiniSupermarket_NgoMinhKhoi1)

---

## 📑 MỤC LỤC
1. [Mô hình Kiến trúc Hệ thống](#1-mô-hình-kiến-trúc-hệ-thống)
2. [Công nghệ & Kỹ thuật Nền tảng](#2-công-nghệ--kỹ-thuật-nền-tảng)
3. [Cấu trúc Solution Đồ án](#3-cấu-trúc-solution-đồ-án)
4. [Hướng dẫn Cài đặt & Khởi chạy Chi tiết (Tiêu chí 10)](#4-hướng-dẫn-cài-đặt--khởi-chạy-chi-tiết-tiêu-chí-10)
5. [Tác giả](#5-tác-giả)
6. [Tài khoản Đăng nhập & Ma trận Phân quyền RBAC](#6-tài-khoản-đăng-nhập--ma-trận-phân-quyền-rbac)
7. [Bảng Đặc tả Danh mục Web API Endpoints (Tiêu chí 3 & 5)](#7-bảng-đặc-tả-danh-mục-web-api-endpoints-tiêu-chí-3--5)
8. [Bảng Đối chiếu 50 Tiêu chí Đánh giá Đồ án](#8-bảng-đối-chiếu-50-tiêu-chí-đánh-giá-đồ-án)
9. [Tiến độ Git Branches theo các Buổi thực hành (Tiêu chí 9)](#9-tiến-độ-git-branches-theo-các-buổi-thực-hành-tiêu-chí-9)

---

## 🏗️ 1. Mô hình Kiến trúc Hệ thống

Dự án được thiết kế chuẩn mực theo mô hình kiến trúc Client - Server phân tầng độc lập (Multi-Tier Architecture):

```text
┌────────────────────────────────────────────────────────┐
│     WINDOWS FORMS DESKTOP CLIENT (MiniSupermarket.WinForms)     │
│  - Single-Form Shell (FormMainShell: TopLevel=false, Dock=Fill) │
│  - Quầy POS Quét mã vạch + Đổi điểm thưởng (FormPOS)           │
│  - Quản lý & Tra cứu Đơn hàng phân trang (FormOrderManagement)  │
│  - Báo cáo Doanh thu: Biểu đồ Cột + Biểu đồ Tròn Donut          │
│  - Gọi API tập trung đính kèm JWT Bearer (ApiClientService)    │
└───────────────────────────┬────────────────────────────┘
                            │  HTTP / HTTPS (JSON RESTful)
                            ▼
┌────────────────────────────────────────────────────────┐
│         BACKEND WEB API (MiniSupermarket.API)          │
│  - ASP.NET Core 8.0 Web API, Non-blocking async/await  │
│  - Xác thực bảo mật JWT Token + Phân quyền [Authorize] │
│  - Entity Framework Core Code-First + Migration + Seed │
│  - ADO.NET thuần: SqlDataReader, SqlParameter chống SQLi│
│  - Stored Procedure gom nhóm doanh thu theo ngày       │
│  - Transaction Scope bọc thanh toán, bảo đảm ACID      │
└───────────────────────────┬────────────────────────────┘
                            │  TCP / IP
                            ▼
┌────────────────────────────────────────────────────────┐
│         DATABASE SERVER (Microsoft SQL Server)         │
│  - CSDL Quan hệ: Users, Roles, Categories, Products,   │
│                  Customers, Orders, OrderItems         │
└────────────────────────────────────────────────────────┘
```

---

## 🛠️ 2. Công nghệ & Kỹ thuật Nền tảng

* **Ngôn ngữ lập trình:** C# (.NET 8.0 / .NET 9.0)
* **Backend:** ASP.NET Core Web API, RESTful JSON API
* **Bảo mật:** JWT Bearer Token (`Microsoft.AspNetCore.Authentication.JwtBearer`), Role-Based Authorization
* **Cơ sở dữ liệu & Tối ưu hóa:**
  - **Entity Framework Core:** Code-First, Migrations, Fluent API, Server-side Pagination LINQ `.Skip().Take()`.
  - **ADO.NET thuần:** `SqlConnection`, `SqlCommand`, `SqlDataReader`, `SqlParameter` ngăn ngừa triệt để lỗ hổng **SQL Injection**.
  - **Stored Procedure:** `sp_GetDailyRevenueReport` tính doanh thu theo ngày.
  - **SqlTransaction:** Bọc trọn vẹn quy trình lưu hóa đơn, trừ kho và trừ điểm tích lũy trong 1 giao dịch, tự động `Rollback` khi gặp sự cố.
* **Frontend:** Windows Forms (.NET Desktop Application), kiến trúc Single-Form Shell.
* **Đồ họa & Trực quan hóa:** Vẽ biểu đồ cột doanh thu ngày (Tiêu chí 45) và Biểu đồ tròn Donut cơ cấu nhóm hàng (vẽ bằng GDI+, chia 50/50, canh lề phải chống đè chữ tuyệt đối).
* **Xuất dữ liệu:** Xuất Excel/CSV đa định dạng cho Sản phẩm, Đơn hàng và Báo cáo doanh thu (UTF-8 có BOM chống lỗi font tiếng Việt).

---

## 📂 3. Cấu trúc Solution Đồ án

```text
MiniSupermarket_NgoMinhKhoi1/
│
├── MiniSupermarket.API/                 # Backend Web API
│   ├── Controllers/                     # Controllers chuyên biệt chuẩn RESTful
│   │   ├── AuthController.cs            # Đăng nhập & cấp JWT Token
│   │   ├── CategoriesController.cs      # CRUD Nhóm hàng, phân trang máy chủ & tìm kiếm
│   │   ├── CustomersController.cs       # Quản lý khách hàng, điểm tích lũy & phân trang
│   │   ├── OrdersController.cs          # Tạo hóa đơn (Transaction) & phân trang lịch sử
│   │   ├── ProductsController.cs        # CRUD Sản phẩm, ADO.NET Barcode & phân trang
│   │   ├── ReportsController.cs         # Dashboard KPI (TC 44), Top 5 Best-Sellers (TC 46)
│   │   ├── RolesController.cs           # Danh mục vai trò hệ thống
│   │   └── UsersController.cs           # Quản trị tài khoản nhân viên
│   ├── Data/
│   │   └── SupermarketDbContext.cs      # DbContext, cấu hình Fluent API & Seed Data mẫu
│   ├── Migrations/                      # Lịch sử Migration CSDL
│   ├── Models/                          # Entities CSDL & DTOs truyền nhận dữ liệu
│   ├── Program.cs                       # Cấu hình CORS, JWT Authentication & Swagger
│   └── appsettings.json                 # Chuỗi kết nối SQL Server
│
├── MiniSupermarket.WinForms/            # Frontend Desktop Client
│   ├── Program.cs                       # Khởi chạy FormLogin
│   ├── FormLogin.cs                     # Đăng nhập xác thực, lưu Token (Tiêu chí 38)
│   ├── SessionManager.cs                # Biến tĩnh lưu JWT Token, Role và Username
│   ├── ApiClientService.cs              # HttpClient tập trung đính kèm Token (Tiêu chí 40)
│   ├── FormMainShell.cs                 # Giao diện Shell điều hướng trung tâm (Tiêu chí 36, 37, 39)
│   ├── FormPOS.cs                       # Quầy bán hàng POS, quét mã vạch, đổi điểm (TC 42, 47)
│   ├── FormOrderManagement.cs           # Quản lý & Tra cứu Đơn hàng phân trang, lọc 4 nút
│   ├── FormProductManagement.cs         # Quản lý Sản phẩm, cảnh báo tồn kho đỏ, phân trang CRUD
│   ├── FormCategoryManagement.cs        # Quản lý Nhóm hàng, phân trang máy chủ
│   ├── FormCustomerManagement.cs        # Quản lý Khách hàng, phân trang máy chủ
│   ├── FormQuickReport.cs               # Báo cáo doanh thu: 4 Thẻ KPI (TC 44), Biểu đồ Cột + Donut
│   └── FormUserManagement.cs            # Quản trị người dùng hệ thống
│
└── .gitignore                           # Loại bỏ hoàn toàn bin/, obj/, .vs/ khỏi Git
```

---

## 🚀 4. Hướng dẫn Cài đặt & Khởi chạy Chi tiết (Tiêu chí 10)

### 4.1. Yêu cầu môi trường
* Hệ điều hành: Windows 10 / 11
* .NET SDK: Phiên bản **8.0** hoặc **9.0**
* CSDL: **Microsoft SQL Server** (hỗ trợ SQL Server LocalDB, Express, hoặc Standard)
* Công cụ khuyến nghị: **Visual Studio 2022** (với Workload *.NET Desktop Development* & *ASP.NET and web development*) hoặc VS Code.

---

### 4.2. Cấu hình Chuỗi kết nối Cơ sở dữ liệu (Connection String)
Mở tệp `MiniSupermarket.API/appsettings.json`, kiểm tra hoặc cập nhật chuỗi kết nối phù hợp với máy của bạn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MiniSupermarketDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "SuperSecretKeyForMiniSupermarketJWTToken2026!#@$",
    "Issuer": "MiniSupermarketAPI",
    "Audience": "MiniSupermarketWinforms"
  }
}
```

> **Ghi chú**: CSDL sử dụng Entity Framework Core Migration, sẽ tự động sinh các bảng và nạp sẵn toàn bộ dữ liệu mẫu (Seed Data) ngay trong lần đầu chạy API.

---

### 4.3. Cách 1: Khởi chạy bằng Visual Studio 2022 (Khuyên dùng)
1. Mở tệp `MiniSupermarket_NgoMinhKhoi.sln` bằng **Visual Studio 2022**.
2. **Khởi chạy Backend (API):**
   - Nhấp chuột phải vào project `MiniSupermarket.API` ➔ Chọn **Set as Startup Project**.
   - Nhấn **Ctrl + F5** (Start Without Debugging).
   - Trình duyệt sẽ mở trang tài liệu kiểm thử **Swagger UI** tại: `http://localhost:5000/swagger` (hoặc cổng HTTPS tương ứng).
3. **Khởi chạy Frontend (WinForms Client):**
   - Nhấp chuột phải vào project `MiniSupermarket.Winforms` ➔ Chọn **Debug** ➔ **Start New Instance**.
   - Màn hình đăng nhập `FormLogin` sẽ xuất hiện ngay lập tức.

---

### 4.4. Cách 2: Khởi chạy bằng Dòng lệnh (.NET CLI)

1. **Bước 1: Khởi động Backend API** (Mở Terminal 1):
   ```powershell
   cd D:\MiniSupermarket_NgoMinhKhoi1
   dotnet run --project MiniSupermarket.API
   ```
   *Máy chủ API sẽ lắng nghe tại cổng `http://localhost:5000`*.

2. **Bước 2: Khởi động WinForms Desktop** (Mở Terminal 2):
   ```powershell
   cd D:\MiniSupermarket_NgoMinhKhoi1
   dotnet run --project MiniSupermarket.Winforms
   ```

---

## 🧑‍💻 5. Tác giả

* **Họ tên sinh viên:** Ngô Minh Khôi
* **Mã sinh viên:** 2124110139
* **Lớp học phần:** CCQ2411D

---

## 🔑 6. Tài khoản Đăng nhập & Ma trận Phân quyền RBAC

Hệ thống được nạp sẵn các tài khoản kiểm thử tương ứng với từng vai trò người dùng trong CSDL:

| Tên đăng nhập | Mật khẩu | Vai trò (Role) | Chức năng được phép truy cập |
| :--- | :--- | :--- | :--- |
| **`admin01`** | `123456` | **Admin** | **Toàn quyền**: Bán hàng POS, Quản lý đơn hàng, Danh mục, Sản phẩm, Khách hàng, Báo cáo doanh thu & Quản trị tài khoản. |
| **`admin02`** | `123456` | **Admin** | Tài khoản quản trị viên dự phòng. |
| **`cashier01`** | `123456` | **Cashier** | **Thu ngân**: Quầy bán hàng POS, Quản lý & tra cứu đơn hàng, Xem danh sách sản phẩm, **Thêm & Cập nhật khách hàng thành viên**. *(Ẩn Báo cáo doanh thu và Quản trị tài khoản)* |
| **`ware01`** | `123456` | **Warehouse** | **Thủ kho**: Quản lý Nhóm hàng và Quản lý Sản phẩm / Tồn kho. |

---

## 🌐 7. Bảng Đặc tả Danh mục Web API Endpoints (Tiêu chí 3 & 5)

Toàn bộ các API đều được xây dựng theo chuẩn RESTful, hỗ trợ bảo mật JWT Bearer Token:

| Module | Phương thức | Đường dẫn API | Quyền hạn | Mô tả chức năng |
| :--- | :---: | :--- | :---: | :--- |
| **Auth** | `POST` | `/api/auth/login` | Public | Đăng nhập hệ thống, trả về JWT Bearer Token và Role. |
| **Orders** | `GET` | `/api/orders` | Authenticated | Lấy danh sách đơn hàng có **phân trang máy chủ** (hỗ trợ lọc ngày `from`/`to`, từ khóa `search`, và `exportAll=true`). |
| **Orders** | `GET` | `/api/orders/{id}` | Authenticated | Lấy chi tiết từng mặt hàng và thông tin thanh toán của 1 hóa đơn. |
| **Orders** | `POST` | `/api/orders` | Cashier/Admin | Tạo đơn hàng mới, bọc trong **SqlTransaction** (trừ kho và trừ điểm tích lũy an toàn). |
| **Products** | `GET` | `/api/products/paged` | Authenticated | Lấy danh sách sản phẩm **phân trang máy chủ** (hỗ trợ lọc từ khóa `keyword`). |
| **Products** | `GET` | `/api/products/{id}` | Authenticated | Lấy chi tiết 1 sản phẩm theo ID. |
| **Products** | `GET` | `/api/products/barcode/{code}`| Authenticated | Tra cứu sản phẩm bằng Barcode để POS đưa vào giỏ hàng. |
| **Products** | `GET` | `/api/products/adonet-barcode/{code}` | Authenticated | **Tiêu chí 32**: Tra cứu Barcode bằng **ADO.NET thuần** (`SqlDataReader`, `SqlParameter`). |
| **Products** | `POST` | `/api/products` | Admin | Thêm sản phẩm mới vào CSDL. |
| **Products** | `PUT` | `/api/products/{id}` | Admin | Cập nhật thông tin sản phẩm. |
| **Products** | `DELETE`| `/api/products/{id}` | Admin | Xóa sản phẩm khỏi CSDL. |
| **Categories**| `GET` | `/api/categories/paged`| Authenticated | Lấy danh mục nhóm hàng phân trang máy chủ. |
| **Categories**| `GET` | `/api/categories` | Authenticated | Lấy toàn bộ nhóm hàng (đổ vào ComboBox). |
| **Categories**| `POST` | `/api/categories` | Admin | Thêm nhóm hàng mới. |
| **Categories**| `PUT` | `/api/categories/{id}` | Admin | Cập nhật nhóm hàng. |
| **Categories**| `DELETE`| `/api/categories/{id}` | Admin | Xóa nhóm hàng. |
| **Customers** | `GET` | `/api/customers/paged` | Authenticated | Lấy danh sách khách hàng phân trang máy chủ. |
| **Customers** | `GET` | `/api/customers/phone/{phone}`| Authenticated| Tra cứu khách hàng theo SĐT để tích điểm / trừ điểm tại POS. |
| **Customers** | `POST` | `/api/customers` | Admin, Cashier | Thêm khách hàng thành viên mới vào hệ thống. |
| **Customers** | `PUT` | `/api/customers/{id}` | Admin, Cashier | Cập nhật thông tin khách hàng. |
| **Customers** | `DELETE`| `/api/customers/{id}` | Admin | Xóa khách hàng khỏi hệ thống (Chỉ Admin). |
| **Reports** | `GET` | `/api/reports/dashboard-summary`| Admin | **Tiêu chí 44**: Trả về 4 chỉ số KPI (Doanh thu hôm nay, Tổng đơn, Khách hàng mới, Hàng sắp hết). |
| **Reports** | `GET` | `/api/reports/revenue` | Admin | **Tiêu chí 45**: Báo cáo doanh thu theo ngày & cơ cấu nhóm hàng (gọi Stored Procedure). |
| **Reports** | `GET` | `/api/reports/top-selling` | Admin | **Tiêu chí 46**: Top 5 sản phẩm bán chạy nhất bằng LINQ GroupBy. |
| **Users** | `GET` | `/api/users` | Admin | Quản trị danh sách tài khoản nhân viên. |

---

## 🏆 8. Bảng Đối chiếu 50 Tiêu chí Đánh giá Đồ án

| Nhóm | STT | Tiêu chí đánh giá | Vị trí File mã nguồn | Mô tả chi tiết thực hiện |
| :---: | :---: | :--- | :--- | :--- |
| **Báo cáo** | 1.0 - 8.0 | Quy chuẩn văn bản, thiết kế hệ thống, phân tích CSDL, kịch bản test | `BaoCao_DoAn.docx` | Trình bày chuẩn mực Word, sơ đồ DFD, ERD, kiến trúc 3 lớp. |
| **Git** | 9.0 | Lịch sử Git tối thiểu 5 commit tương ứng các buổi học | Git Repository | Có đầy đủ các nhánh `Buoi-2`, `Buoi-3`, `buoi-4`, `buoi-5`, `buoi-6` và `main`. |
| **Git** | 10.0 | File README.md hướng dẫn chi tiết & .gitignore sạch | `README.md`, `.gitignore` | Hướng dẫn chạy 2 project, loại bỏ hoàn toàn `bin/`, `obj/`, `node_modules/`. |
| **API** | 11.0 | Solution 2 Project độc lập & Cấu hình Program.cs | `Program.cs` | Tách biệt `MiniSupermarket.API` và `WinForms`, bật CORS `AllowAll`. |
| **API** | 12.0 | Chuỗi kết nối `appsettings.json` | `appsettings.json` | Lưu chuỗi kết nối tập trung, không hardcode trong mã C#. |
| **API** | 13.0 | Controller chuyên biệt & Route chuẩn RESTful | `Controllers/*.cs` | Tách các file riêng: `AuthController`, `ProductsController`... |
| **API** | 14.0 | Chuẩn HTTP Methods & Status Codes | `Controllers/*.cs` | Dùng đúng `GET`, `POST`, `PUT`, `DELETE` và mã `200`, `201`, `400`, `404`. |
| **API** | 15.0 | Sử dụng DTOs truyền nhận dữ liệu | `Models/*Dto.cs` | Tách lớp DTO nhận/trả dữ liệu, không trả trực tiếp thực thể Entity. |
| **API** | 16.0 | Lập trình bất đồng bộ (`async / await`) | Toàn bộ Controllers | 100% tác vụ I/O, truy vấn CSDL đều dùng Non-blocking async. |
| **API** | 17.0 | Kiểm thử trực quan với Swagger UI | `Program.cs` | Tích hợp Swagger kích hoạt Bearer Token Header kiểm thử ngay trên web. |
| **API** | 18.0 - 20.0 | Models, Khóa chính, Khóa ngoại, DataAnnotations | `Models/*.cs` | Đầy đủ thuộc tính `[Key]`, `[Required]`, `[MaxLength]`, quan hệ 1-N. |
| **DB** | 21.0 - 24.0 | EF Core Code-First, Migration, Seed Data, CRUD | `SupermarketDbContext.cs` | Tự động sinh bảng và nạp sẵn 15 Users, 20 Products, 20 Orders mẫu. |
| **DB** | 25.0 | **Phân trang dữ liệu máy chủ (Server-side Paging)** | `OrdersController.cs`, `ProductsController.cs` | Dùng LINQ `.Skip((page-1)*pageSize).Take(pageSize)` phía Server API. |
| **DB** | 26.0 - 30.0 | JWT Authentication & RBAC | `AuthController.cs`, `Program.cs` | Cấp phát JWT Token, phân quyền chặt chẽ `[Authorize(Roles = "...")]`. |
| **DB** | 31.0 - 33.0 | **ADO.NET thuần & SqlParameter chống SQL Injection** | `ProductsController.cs` (L175-L225) | Tra cứu Barcode bằng `SqlConnection`, `SqlCommand`, `SqlDataReader`, `@Barcode`. |
| **DB** | 34.0 | **Stored Procedure báo cáo doanh thu** | `ReportsController.cs` | Tạo và gọi thủ tục `sp_GetDailyRevenueReport` bằng `CommandType.StoredProcedure`. |
| **DB** | 35.0 | **Giao dịch bọc trong SqlTransaction (ACID)** | `OrdersController.cs` | Lưu đơn hàng, trừ tồn kho và trừ điểm thưởng trong 1 Transaction, tự Rollback khi lỗi. |
| **WinForms**| 36.0 | Giao diện Shell phân vùng cố định | `FormMainShell.cs` | Menu bên trái (230px), Tiêu đề bên trên (56px), Vùng làm việc trung tâm. |
| **WinForms**| 37.0 | Đổi màn hình mượt mà không giật lag | `FormMainShell.cs` (L261) | Nhúng Form con vào panel trung tâm (`TopLevel = false`, `Dock = Fill`). |
| **WinForms**| 38.0 | Màn hình Đăng nhập lưu JWT Token | `FormLogin.cs`, `SessionManager.cs` | Đăng nhập API, lưu Token vào biến tĩnh `SessionManager.JwtToken`. |
| **WinForms**| 39.0 | Ẩn/Hiện nút bấm theo quyền vai trò | `FormMainShell.cs` (L320) | Thu ngân chỉ thấy POS, Đơn hàng, Xem SP; Quản trị viên thấy toàn bộ menu. |
| **WinForms**| 40.0 | Gọi API tập trung qua `HttpClient` | `ApiClientService.cs` | Lớp tĩnh dùng chung `HttpClient`, tự động gắn Bearer Token vào Request. |
| **WinForms**| 41.0 | DataGridView tải dữ liệu từ Web API | `FormProductManagement.cs` | Nạp danh sách qua API, các nút Thêm/Sửa/Xóa phân trang trơn tru. |
| **WinForms**| 42.0 | Quầy bán hàng POS & Thông báo lỗi thân thiện | `FormPOS.cs` | Quét mã tự cộng dồn, tự tính tiền thừa, thông báo tiếng Việt bọc `try-catch`. |
| **WinForms**| 43.0 | **Cảnh báo hàng sắp hết kho (Low-Stock Alert <= 10)**| `FormProductManagement.cs` (L432) | Bắt sự kiện `CellFormatting`, đổi màu nền đỏ cam ô tồn kho `<= 10`. |
| **WinForms**| 44.0 | **Dashboard 4 Thẻ chỉ số tổng quan (KPI Cards)** | `FormQuickReport.cs`, `ReportsController.cs` | Hiển thị to, nổi bật: Doanh thu hôm nay, Tổng đơn, Khách mới, Hàng sắp hết. |
| **WinForms**| 45.0 | **Biểu đồ Cột doanh thu theo ngày (Bar Chart)** | `FormQuickReport.cs` | Tự vẽ biểu đồ cột GDI+ trực quan, kèm Biểu đồ tròn Donut cơ cấu nhóm hàng. |
| **WinForms**| 46.0 | **Thống kê Top 5 sản phẩm bán chạy nhất** | `ReportsController.cs`, `FormQuickReport.cs` | LINQ `GroupBy(ProductId) -> OrderByDescending(Sum Qty) -> Take(5)`. |
| **WinForms**| 47.0 | **Đổi điểm thưởng trừ tiền hóa đơn (Loyalty Points)**| `FormPOS.cs`, `OrdersController.cs` | Khách thân thiết đổi điểm (F11) trừ tiền mặt ngay trên bill và cập nhật CSDL. |
| **WinForms**| 48.0 | **Xuất báo cáo Danh mục & Đơn hàng ra Excel/CSV** | `FormOrderManagement.cs`, `FormProductManagement.cs` | Xuất file `.xlsx` & `.csv` chuẩn UTF-8 BOM, tự động định dạng bảng đẹp mắt. |
| **Bảo vệ** | 49.0 - 50.0 | Demo bán hàng trơn tru & Vấn đáp chuyên nghiệp | Cả hệ thống | Thao tác tự tin, hiểu rõ luồng dữ liệu Client - API - SQL Server. |

---

## 🌿 9. Tiến độ Git Branches theo các Buổi thực hành (Tiêu chí 9)

Kho lưu trữ GitHub đã được phân nhánh và đẩy đầy đủ các mốc bài làm:

* **`Buoi-2`**: Khởi tạo Solution, cấu hình 2 Project Web API & WinForms, Entity Models cơ bản.
* **`Buoi-3`**: Cấu hình EF Core Code-First, DbContext, Migration, Seed Data và các API CRUD cơ bản.
* **`buoi-4`**: Tích hợp bảo mật JWT Bearer Token, phân quyền RBAC và xây dựng giao diện Shell WinForms.
* **`buoi-5`**: Hoàn thiện Quầy bán hàng POS, quét mã vạch Barcode, trừ kho Transaction và tích điểm thành viên.
* **`buoi-6`**: Hoàn thiện Màn hình Quản lý Đơn hàng, Biểu đồ tròn Donut, 4 Thẻ KPI Dashboard Tiêu chí 44, sửa lỗi phân trang CRUD và tối ưu toàn bộ giao diện.
* **`main`**: Nhánh chính thức hoàn thiện 100% chuẩn 50 tiêu chí đồ án môn học.

---

> **© 2026 - Ngô Minh Khôi (MSSV: 2124110139) - Đồ án Lập trình Ứng dụng .NET Core.**
