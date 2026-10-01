🛒 HỆ THỐNG QUẢN LÝ SIÊU THỊ MINI (MINISUPERMARKET SYSTEM)

> **Môn học:** Lập trình Ứng dụng .NET Core (Mã môn: 229162)
> **Buổi thực hành:** Buổi 2 - Bảo mật & Phân quyền JWT cho Web API

---

## 🏗️ 1. Mô hình Kiến trúc Hệ thống & Bảo mật (Client - Server)

Dự án được nâng cấp lên mô hình bảo mật phân tầng hiện đại, ứng dụng xác thực không trạng thái:

* **`MiniSupermarket.API` (Backend):** Dự án ASP.NET Core Web API tích hợp bộ điều khiển cấp phát mã thông hành `AuthController`, cấu hình bảo mật `JwtBearer` và bảo vệ tài nguyên bằng thuộc tính `[Authorize]`.


* **`MiniSupermarket.WinForms` (Frontend Client):** Ứng dụng Windows Forms bổ sung màn hình đăng nhập (`FormLogin`), quản lý phiên làm việc tập trung qua `SessionManager` và tự động đính kèm `Bearer Token` vào Header của mỗi gói tin HTTP gọi đến API.



---

## 🛠️ 2. Công nghệ Sử dụng

* **Ngôn ngữ:** C# (.NET 8.0)
* **Backend:** ASP.NET Core Web API, JWT (`System.IdentityModel.Tokens.Jwt`, `Microsoft.AspNetCore.Authentication.JwtBearer`), Controllers, LINQ


* **Frontend:** Windows Forms (.NET 8.0), `System.Net.Http.Json`, `System.Text.Json`

* **Công cụ kiểm thử:** Swagger UI (hỗ trợ xác thực khóa định danh qua `Authorize` / `Bearer` token)



---

## 📂 3. Cấu trúc Solution

```text
MiniSupermarketSystem/
│
├── MiniSupermarket.API/              # Dự án Web API (Backend)
│   ├── Controllers/                  # Chứa AuthController và CategoriesController ([Authorize])
│   ├── Models/                       # Chứa lớp thực thể Category.cs và LoginRequestDto
│   └── Program.cs                    # Cấu hình dịch vụ JwtBearer, UseAuthentication và UseAuthorization
│
└── MiniSupermarket.WinForms/         # Dự án Windows Forms (Frontend Client)
    ├── Program.cs                    # Cấu hình chạy khởi đầu với FormLogin
    ├── FormLogin.cs                  # Giao diện và xử lý đăng nhập hệ thống
    ├── SessionManager.cs             # Lớp tĩnh lưu trữ JwtToken và CurrentRole
    └── FormCategoryManagement.cs     # Giao diện quản lý danh mục (đính kèm Bearer Token qua HttpClient)

```

---

## 🚀 4. Hướng dẫn Chạy và Kiểm thử Dự án

### Bước 1: Chạy phía Backend (Web API)

1. Mở Solution bằng Visual Studio 2022.
2. Nhấp chuột phải vào project `MiniSupermarket.API` chọn **Set as Startup Project**.
3. Nhấn **F5** để chạy. Trình duyệt sẽ tự động mở giao diện Swagger UI. Tại đây bạn có thể dùng `POST /api/auth/login` với tài khoản mẫu (`admin` / `123456` hoặc `cashier` / `123456`) để lấy token kiểm chứng phân quyền.



### Bước 2: Chạy phía Frontend (WinForms Client)

1. Đảm bảo cổng (Port) trong `ApiClientService` hoặc `HttpClient` của WinForms khớp với cổng `https://localhost:XXXXX` của Web API đang chạy.


2. Nhấp chuột phải vào project `MiniSupermarket.WinForms` chọn **Debug -> Start new instance** (hoặc đặt `MiniSupermarket.WinForms` làm Startup Project).


3. Thử nghiệm các chức năng: Màn hình đăng nhập `FormLogin` sẽ hiện lên đầu tiên. Sau khi nhập đúng tài khoản mật khẩu, hệ thống sẽ lưu token vào `SessionManager`, tự động chuyển sang giao diện quản lý danh mục và đính kèm `Bearer Token` vào mọi yêu cầu gọi dữ liệu từ API.



---

## 👨‍💻 5. Tác giả

* **Họ tên sinh viên:** Ngô Minh Khôi


* **Mã sinh viên:** 2124110139


* **Lớp học phần:** CCQ2411D
