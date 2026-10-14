using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSupermarket.API.Services;

namespace MiniSupermarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        // GET /api/users
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = AccountStore.GetAll()
                .OrderBy(u => u.Id)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.FullName,
                    u.Role,
                    u.IsActive
                });

            return Ok(users);
        }

        // POST /api/users
        [HttpPost]
        public IActionResult Create([FromBody] CreateUserRequestDto? request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest("Vui lòng nhập đầy đủ tên đăng nhập, họ tên và mật khẩu.");
            }

            string username = request.Username.Trim();
            string fullName = request.FullName.Trim();
            string password = request.Password;
            string role = request.Role?.Trim() ?? string.Empty;

            if (username.Contains(' '))
                return BadRequest("Tên đăng nhập không được chứa khoảng trắng.");

            if (password.Length < 6)
                return BadRequest("Mật khẩu phải có ít nhất 6 ký tự.");

            string? matchedRole = AccountStore.ValidRoles.FirstOrDefault(r =>
                string.Equals(r, role, System.StringComparison.OrdinalIgnoreCase));

            if (matchedRole == null)
                return BadRequest("Vai trò không hợp lệ (Admin, Cashier hoặc Warehouse).");

            var created = AccountStore.TryAdd(username, password, fullName, matchedRole);

            if (created == null)
                return Conflict("Tên đăng nhập đã tồn tại.");

            return StatusCode(201, new
            {
                created.Id,
                created.Username,
                created.FullName,
                created.Role,
                created.IsActive
            });
        }
    }

    // Du lieu nhan tu form tao tai khoan
    public class CreateUserRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}