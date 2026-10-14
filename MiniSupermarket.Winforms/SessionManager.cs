namespace MiniSupermarket.WinForms
{
    public static class SessionManager
    {
        // Lưu JWT Token sau khi đăng nhập
        public static string JwtToken { get; set; } = string.Empty;

        // Lưu tên tài khoản (Username) hiện tại
        public static string CurrentUsername { get; set; } = string.Empty;

        // Lưu quyền của tài khoản: Admin, Cashier, hoặc Warehouse
        public static string CurrentRole { get; set; } = string.Empty;
    }
}