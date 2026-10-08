🛒 HỆ THỐNG QUẢN LÝ SIÊU THỊ MINI (MINISUPERMARKET SYSTEM)

> **Môn học:** Lập trình Ứng dụng .NET Core (Mã môn: 229162)
> **Buổi thực hành:** Buổi 3 - Tích hợp SQL Server và Entity Framework Core Code-First

---

## 🏗️ 1. Mục tiêu và Bối cảnh Thực tế

* **Mục tiêu:** Thay thế hoàn toàn cơ chế lưu tạm trên RAM (In-Memory) bằng cơ sở dữ liệu quan hệ Microsoft SQL Server, làm chủ kỹ thuật Entity Framework Core (EF Core) Code-First, nắm vững các lệnh Migrations và truy vấn dữ liệu bất đồng bộ bằng LINQ (`async/await`).


* **Bối cảnh thực tế đồ án:** Khi nhân viên thu ngân thêm hoặc sửa nhóm hàng trên phần mềm WinForms, dữ liệu phải được lưu trữ vĩnh viễn trong CSDL của SQL Server, đảm bảo tắt máy hoặc khởi động lại API thì dữ liệu vẫn còn nguyên vẹn.



---

## 🛠️ 2. Công nghệ Sử dụng

* **Ngôn ngữ:** C# (.NET 8.0)


* **Backend:** ASP.NET Core Web API, Entity Framework Core (`Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Design`), LINQ


* **Frontend:** Windows Forms (.NET 8.0), `System.Net.Http.Json`, `System.Text.Json`

* **Cơ sở dữ liệu:** Microsoft SQL Server


* **Công cụ kiểm thử:** Swagger UI, SQL Server Management Studio (SSMS)



---

## 📂 3. Cấu trúc Solution

```text
MiniSupermarketSystem/
│
├── MiniSupermarket.API/                # Dự án Web API (Backend)
│   ├── Data/                           # Chứa SupermarketDbContext.cs
│   ├── Controllers/                    # Chứa CategoriesController.cs (kết nối EF Core)
│   ├── Models/                         # Chứa Category.cs và Product.cs
│   └── Program.cs                      # Cấu hình chuỗi kết nối và đăng ký DbContext DI
│
└── MiniSupermarket.WinForms/           # Dự án Windows Forms (Frontend Client)
    ├── Program.cs                      # Cấu hình khởi chạy ứng dụng
    ├── FormLogin.cs                    # Giao diện và xử lý đăng nhập hệ thống
    ├── SessionManager.cs               # Lớp tĩnh lưu trữ JwtToken và CurrentRole
    └── FormCategoryManagement.cs       # Giao diện quản lý danh mục (giao tiếp API trực tiếp với SQL Server)

```

---

## 🚀 4. Hướng dẫn Chạy và Kiểm thử Dự án

### Bước 1: Cấu hình Backend và Cơ sở dữ liệu SQL Server

1. Cài đặt các gói NuGet cần thiết cho `MiniSupermarket.API`: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, và `Microsoft.EntityFrameworkCore.Design`.


2. Khai báo chuỗi kết nối (`ConnectionStrings:DefaultConnection`) trong tệp `appsettings.json` trỏ tới SQL Server của bạn.


3. Đăng ký `SupermarketDbContext` vào container Dependency Injection trong `Program.cs`.


4. Chạy lệnh Migration trong Package Manager Console để khởi tạo cơ sở dữ liệu và bảng:


```shell
Add-Migration InitialCreateDatabase
Update-Database

```



### Bước 2: Chạy phía Backend (Web API)

1. Mở Solution bằng Visual Studio 2022.


2. Nhấp chuột phải vào project `MiniSupermarket.API` chọn **Set as Startup Project**.


3. Nhấn **F5** để chạy. Trình duyệt sẽ tự động mở giao diện Swagger UI để kiểm tra các API danh mục (`CategoriesController`) thao tác trực tiếp trên SQL Server.



### Bước 3: Chạy phía Frontend (WinForms Client)

1. Đảm bảo cổng (Port) trong cấu hình kết nối API của WinForms khớp với cổng của Web API đang chạy.
2. Nhấp chuột phải vào project `MiniSupermarket.WinForms` chọn **Debug -> Start new instance**.


3. Thử nghiệm các chức năng thêm, sửa, xóa danh mục trên giao diện. Dữ liệu thay đổi sẽ được ghi nhận vĩnh viễn xuống đĩa cứng của SQL Server, không bị mất đi khi tắt ứng dụng.



---

## 👨‍💻 5. Tác giả

* **Họ tên sinh viên:** Ngô Minh Khôi
* **Mã sinh viên:** 2124110139
* **Lớp học phần:** CCQ2411D
