using System.Net.Http.Json;
using System.Text.Json;

namespace MiniSupermarket.WinForms
{
    public partial class FormLogin : Form
    {
        // HttpClient dùng để kết nối đến Backend API
        private static readonly HttpClient _client = new HttpClient
        {
            // Backend của bạn đang chạy tại port 7250
            BaseAddress = new Uri("https://localhost:7250/api/")
        };

        public FormLogin()
        {
            InitializeComponent();
        }

        // Xử lý khi bấm nút Đăng nhập
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            // Lấy tài khoản và mật khẩu từ ô nhập
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();

            // Kiểm tra dữ liệu nhập
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show(
                    "Vui lòng nhập đầy đủ tài khoản và mật khẩu!",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            try
            {
                // Dữ liệu gửi đến API Login
                var loginData = new
                {
                    Username = username,
                    Password = password
                };

                // Gọi API POST /api/auth/login
                var response = await _client.PostAsJsonAsync(
                    "auth/login",
                    loginData
                );

                // Nếu đăng nhập thành công
                if (response.IsSuccessStatusCode)
                {
                    // Đọc dữ liệu JSON từ Backend
                    string jsonString =
                        await response.Content.ReadAsStringAsync();

                    using JsonDocument doc =
                        JsonDocument.Parse(jsonString);

                    // Lấy JWT Token
                    SessionManager.JwtToken =
                        doc.RootElement
                            .GetProperty("token")
                            .GetString()
                        ?? string.Empty;

                    // Lấy Role
                    SessionManager.CurrentRole =
                        doc.RootElement
                            .GetProperty("role")
                            .GetString()
                        ?? string.Empty;

                    

                    MessageBox.Show(
                        $"Đăng nhập thành công!\nQuyền: {SessionManager.CurrentRole}",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Mở Form quản lý chính
                    FormCategoryManagement mainForm =
                        new FormCategoryManagement();

                    // Ẩn Form Login
                    this.Hide();

                    // Hiển thị Form chính
                    mainForm.ShowDialog();

                    // Đóng Form Login
                    this.Close();
                }
                else
                {
                    // Đăng nhập thất bại
                    MessageBox.Show(
                        "Sai tài khoản hoặc mật khẩu!",
                        "Đăng nhập thất bại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi kết nối hoặc lỗi API
                MessageBox.Show(
                    "Lỗi kết nối đến Server:\n" + ex.Message,
                    "Lỗi hệ thống",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}