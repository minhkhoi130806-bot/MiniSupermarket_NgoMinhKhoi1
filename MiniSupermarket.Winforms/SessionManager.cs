namespace MiniSupermarket.WinForms
{
    public static class SessionManager
    {
        // Lưu JWT Token sau khi đăng nhập
        public static string JwtToken { get; set; } = string.Empty;

        // Lưu quyền của tài khoản: Admin hoặc Cashier
        public static string CurrentRole { get; set; } = string.Empty;
    }
}