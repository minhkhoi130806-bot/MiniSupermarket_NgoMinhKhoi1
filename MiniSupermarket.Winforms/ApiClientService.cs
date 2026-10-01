using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MiniSupermarket.WinForms
{
    public static class ApiClientService
    {
        // HttpClient dùng để gọi Backend API
        private static readonly HttpClient _client = new HttpClient
        {
            // Backend của bạn đang chạy tại port 7250
            BaseAddress = new Uri("https://localhost:7250/api/")
        };

        // Hàm đăng nhập và lấy Token
        public static async Task<bool> LoginAsync(string username, string password)
        {
            // Dữ liệu gửi lên API
            var loginObj = new
            {
                Username = username,
                Password = password
            };

            // Gọi POST /api/auth/login
            var response = await _client.PostAsJsonAsync(
                "auth/login",
                loginObj
            );

            if (response.IsSuccessStatusCode)
            {
                // Đọc dữ liệu JSON từ Server
                var jsonString =
                    await response.Content.ReadAsStringAsync();

                using var doc =
                    JsonDocument.Parse(jsonString);

                // Lấy Token
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

                return true;
            }

            return false;
        }

        // Hàm gọi API có gắn Bearer Token
        public static async Task<string> GetDataWithTokenAsync(string endpoint)
        {
            // Gắn JWT Token vào Header Authorization
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    SessionManager.JwtToken
                );

            // Gọi API
            var response = await _client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else if (response.StatusCode ==
                     System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception(
                    "Phiên làm việc hết hạn hoặc chưa đăng nhập!"
                );
            }

            throw new Exception(
                "Lỗi khi gọi dữ liệu từ Server."
            );
        }
    }
}