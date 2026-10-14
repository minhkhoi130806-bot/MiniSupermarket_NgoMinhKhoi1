
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MiniSupermarket.WinForms
{
    public static class ApiClientService
    {
        // Cau hinh ket noi den Backend API
        public static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7250/api/")
        };

        // Dang nhap va luu Token, ten tai khoan, vai tro
        public static async Task<bool> LoginAsync(
            string username,
            string password)
        {
            try
            {
                var loginObj = new
                {
                    Username = username,
                    Password = password
                };

                var response = await Client.PostAsJsonAsync(
                    "auth/login",
                    loginObj
                );

                if (!response.IsSuccessStatusCode)
                {
                    SessionManager.JwtToken = string.Empty;
                    SessionManager.CurrentUsername = string.Empty;
                    SessionManager.CurrentRole = string.Empty;

                    return false;
                }

                var jsonString =
                    await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                // Doc Token tu ket qua dang nhap
                SessionManager.JwtToken =
                    root.TryGetProperty("token", out var token)
                        ? token.GetString() ?? string.Empty
                        : string.Empty;

                // Doc vai tro tai khoan
                SessionManager.CurrentRole =
                    root.TryGetProperty("role", out var role)
                        ? role.GetString() ?? string.Empty
                        : string.Empty;

                // Luu ten tai khoan dang nhap
                SessionManager.CurrentUsername = username;

                // Khong chap nhan ket qua dang nhap thieu Token
                if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
                {
                    SessionManager.CurrentUsername = string.Empty;
                    SessionManager.CurrentRole = string.Empty;
                    return false;
                }

                // Cap nhat Token cho cac request tiep theo
                Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        SessionManager.JwtToken
                    );

                return true;
            }
            catch (HttpRequestException)
            {
                throw new Exception(
                    "Khong the ket noi den Server. Hay kiem tra Backend."
                );
            }
            catch (JsonException)
            {
                throw new Exception(
                    "Du lieu dang nhap tra ve tu Server khong hop le."
                );
            }
        }

        // Goi API co kem Bearer Token
        public static async Task<string> GetDataWithTokenAsync(
            string endpoint)
        {
            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
            {
                throw new Exception(
                    "Ban chua dang nhap hoac phien lam viec da het han."
                );
            }

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    SessionManager.JwtToken
                );

            var response = await Client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new Exception(
                    "Phien lam viec het han hoac ban chua dang nhap."
                );
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception(
                    "Tai khoan khong co quyen truy cap chuc nang nay."
                );
            }

            var errorContent =
                await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Loi API ({(int)response.StatusCode}): " +
                $"{response.ReasonPhrase}\n{errorContent}"
            );
        }

        // Xoa thong tin dang nhap khi dang xuat
        public static void Logout()
        {
            SessionManager.JwtToken = string.Empty;
            SessionManager.CurrentUsername = string.Empty;
            SessionManager.CurrentRole = string.Empty;

            Client.DefaultRequestHeaders.Authorization = null;
        }
    }
}
