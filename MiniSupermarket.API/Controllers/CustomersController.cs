using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MiniSupermarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly SupermarketDbContext _context;

        // Tiêm DbContext thông qua Constructor Injection
        public CustomersController(SupermarketDbContext context)
        {
            _context = context;
        }

        // 1. READ: Lấy toàn bộ danh sách khách hàng từ SQL Server
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _context.Customers.AsNoTracking().ToListAsync();
            return Ok(customers);
        }

        // 2. READ: Lấy chi tiết khách hàng theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng trong hệ thống!" });
            }
            return Ok(customer);
        }

        // 3. SEARCH: Tìm kiếm khách hàng theo tên hoặc số điện thoại
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Vui lòng nhập từ khóa tìm kiếm (tên hoặc số điện thoại)!" });
            }

            var result = await _context.Customers
                .Where(c => c.CustomerName.Contains(keyword) || c.PhoneNumber.Contains(keyword))
                .ToListAsync();

            return Ok(result);
        }
        // Tra khách theo SĐT chính xác (dùng cho POS)
        [HttpGet("phone/{phone}")]
        public async Task<IActionResult> GetByPhone(string phone)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.PhoneNumber == phone);

            if (customer == null)
                return NotFound(new { message = "Không tìm thấy khách hàng!" });

            return Ok(customer);
        }

        // Thu ngân đăng ký nhanh khách mới (không cần quyền Admin)
        [Authorize]
        [HttpPost("quick-register")]
        public async Task<IActionResult> QuickRegister([FromBody] QuickRegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.CustomerName) || string.IsNullOrWhiteSpace(req.PhoneNumber))
                return BadRequest(new { message = "Tên và số điện thoại không được để trống!" });

            string phone = req.PhoneNumber.Trim();

            if (await _context.Customers.AnyAsync(c => c.PhoneNumber == phone))
                return Conflict(new { message = "Số điện thoại này đã tồn tại!" });

            var customer = new Customer
            {
                CustomerName = req.CustomerName.Trim(),
                PhoneNumber = phone
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(customer);
        }


        // CREATE: Chi Admin moi duoc them khach hang
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Customer newCustomer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = newCustomer.CustomerId },
                newCustomer
            );
        }

        // UPDATE: Chi Admin moi duoc cap nhat khach hang
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Customer updateData)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound(new { message = "Không tìm thấy khách hàng cần cập nhật!" });

            customer.CustomerName = updateData.CustomerName;
            customer.PhoneNumber = updateData.PhoneNumber;
            customer.Address = updateData.Address;
            customer.RewardPoints = updateData.RewardPoints;
            customer.MembershipRank = updateData.MembershipRank;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: Chi Admin moi duoc xoa khach hang
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound(new { message = "Không tìm thấy khách hàng cần xóa!" });

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/customers/paged?page=1&pageSize=8
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 8)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 8;

            var query = _context.Customers.AsNoTracking();

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var items = await query
                .OrderBy(c => c.CustomerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                currentPage = page,
                pageSize = pageSize,
                totalRecords = totalRecords,
                totalPages = totalPages,
                data = items
            });
        }
    }


    public class QuickRegisterRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}