using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private static readonly string[] ValidRoles = { "Admin", "Cashier", "Warehouse" };
        private readonly SupermarketDbContext _context;

        public UsersController(SupermarketDbContext context) => _context = context;

        private string? CurrentUsername =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name;

        private static string? MatchRole(string? role) =>
            ValidRoles.FirstOrDefault(r =>
                string.Equals(r, role?.Trim(), StringComparison.OrdinalIgnoreCase));

        private async Task<bool> HasOtherActiveAdmin(int excludeId) =>
            await _context.Users.AnyAsync(u =>
                u.Id != excludeId && u.Role == "Admin" && u.IsActive);

        // GET /api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Users.AsNoTracking()
                .OrderBy(u => u.Id)
                .Select(u => new { u.Id, u.Username, u.FullName, u.Role, u.IsActive })
                .ToListAsync();
            return Ok(list);
        }

        // POST /api/users
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequestDto? request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName))
                return BadRequest("Vui lòng nhập đầy đủ tên đăng nhập, họ tên và mật khẩu.");

            string username = request.Username.Trim();
            if (username.Contains(' '))
                return BadRequest("Tên đăng nhập không được chứa khoảng trắng.");
            if (request.Password.Length < 6)
                return BadRequest("Mật khẩu phải có ít nhất 6 ký tự.");

            string? role = MatchRole(request.Role);
            if (role == null)
                return BadRequest("Vai trò không hợp lệ (Admin, Cashier hoặc Warehouse).");

            if (await _context.Users.AnyAsync(u => u.Username == username))
                return Conflict("Tên đăng nhập đã tồn tại.");

            var user = new User
            {
                Username = username,
                Password = request.Password.Trim(),
                FullName = request.FullName.Trim(),
                Role = role,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { user.Id, user.Username, user.FullName, user.Role, user.IsActive });
        }

        // PUT /api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequestDto? request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ.");

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy tài khoản.");

            if (string.IsNullOrWhiteSpace(request.FullName))
                return BadRequest("Họ tên không được để trống.");

            string? role = MatchRole(request.Role);
            if (role == null) return BadRequest("Vai trò không hợp lệ.");

            if (!string.IsNullOrEmpty(request.Password) && request.Password.Length < 6)
                return BadRequest("Mật khẩu mới phải có ít nhất 6 ký tự.");

            if (user.Role == "Admin" && role != "Admin" && !await HasOtherActiveAdmin(id))
                return BadRequest("Phải còn ít nhất một Admin đang hoạt động.");

            user.FullName = request.FullName.Trim();
            user.Role = role;
            if (!string.IsNullOrEmpty(request.Password))
                user.Password = request.Password.Trim();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT /api/users/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] UserStatusRequestDto? request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ.");

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy tài khoản.");

            if (!request.IsActive)
            {
                if (string.Equals(user.Username, CurrentUsername, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Không thể tự khóa tài khoản đang đăng nhập.");

                if (user.Role == "Admin" && !await HasOtherActiveAdmin(id))
                    return BadRequest("Phải còn ít nhất một Admin đang hoạt động.");
            }

            user.IsActive = request.IsActive;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class CreateUserRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateUserRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Password { get; set; }
    }

    public class UserStatusRequestDto
    {
        public bool IsActive { get; set; }
    }
}