using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormLogin : Form
    {
        // HttpClient dùng riêng để gọi API đăng nhập
        private static readonly HttpClient _client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7250/api/")
        };

        public FormLogin()
        {
            InitializeComponent();
        }

        // Xử lý khi bấm nút Đăng nhập
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();

            // Kiểm tra dữ liệu nhập
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show(
                    "Vui lòng nhập đầy đủ tài khoản và mật khẩu!",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var loginData = new
                {
                    Username = username,
                    Password = password
                };

                // Gọi API POST /api/auth/login
                var response = await _client.PostAsJsonAsync("auth/login", loginData);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Sai tài khoản hoặc mật khẩu!",
                        "Đăng nhập thất bại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Đọc JSON trả về từ Backend
                string jsonString = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(jsonString);

                // Lưu thông tin phiên đăng nhập
                SessionManager.CurrentUsername = username;

                SessionManager.JwtToken =
                    doc.RootElement.GetProperty("token").GetString()
                    ?? string.Empty;

                SessionManager.CurrentRole =
                    doc.RootElement.GetProperty("role").GetString()
                    ?? string.Empty;

                // Gắn token vào HttpClient dùng chung để các Form con gọi API được
                ApiClientService.Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", SessionManager.JwtToken);

                // Báo cho Program.cs biết đăng nhập thành công rồi đóng form
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi kết nối đến Server:\n" + ex.Message,
                    "Lỗi hệ thống",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}