namespace MiniSupermarket.API.Models
{
    // Lớp biểu diễn thực thể Vai trò nhân viên trong siêu thị mini
    public class Role
    {
        // Mã định danh vai trò (Khóa chính)
        public int Id { get; set; }

        // Tên vai trò (Ví dụ: Admin, Cashier, Warehouse)
        public string RoleName { get; set; } = string.Empty;

        // Mô tả chi tiết chức năng của vai trò
        public string? Description { get; set; }
    }
}

