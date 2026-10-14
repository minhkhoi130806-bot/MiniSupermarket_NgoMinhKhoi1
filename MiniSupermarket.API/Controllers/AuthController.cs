using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MiniSupermarket.API.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiniSupermarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto? request)
        {
            // Kiem tra du lieu dang nhap
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Vui long nhap tai khoan va mat khau!"
                });
            }

            string username = request.Username.Trim();
            string password = request.Password.Trim();

            // Tim tai khoan trong kho dung chung va kiem tra mat khau
            var account = AccountStore.Find(username);

            if (account == null || account.Password != password)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Sai tai khoan hoac mat khau!"
                });
            }

            if (!account.IsActive)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Tai khoan da bi khoa!"
                });
            }

            // Tao JWT theo tai khoan va vai tro
            string token = GenerateJwtToken(account.Username, account.Role);

            return Ok(new
            {
                success = true,
                token = token,
                username = account.Username,
                role = account.Role
            });
        }

        // Tao JWT Token
        private string GenerateJwtToken(string username, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            string secret =
                _configuration["JwtSettings:Secret"]
                ?? "SupermarketSecretKeyDoAnMonHoc2026SecureString!!";

            byte[] key = Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                }),

                Expires = DateTime.UtcNow.AddHours(2),

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

    // Du lieu nhan tu man hinh dang nhap
    public class LoginRequestDto
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}