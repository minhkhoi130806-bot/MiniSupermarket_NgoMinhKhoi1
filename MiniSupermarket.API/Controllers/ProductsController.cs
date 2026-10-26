using Microsoft.AspNetCore.Authorization;
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MiniSupermarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly SupermarketDbContext _context;

        public ProductsController(SupermarketDbContext context)
        {
            _context = context;
        }

        // 1. GET: Lấy danh sách toàn bộ sản phẩm kèm theo thông tin danh mục
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .Include(p => p.Category) // Lấy kèm thông tin Category
                .AsNoTracking()
                .ToListAsync();
            return Ok(products);
        }

        // 2. GET: Lấy chi tiết sản phẩm theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm trong hệ thống!" });
            }
            return Ok(product);
        }

        // 3. GET: Tìm kiếm sản phẩm theo tên
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Vui lòng nhập từ khóa tên sản phẩm cần tìm!" });
            }

            var result = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.ProductName.Contains(keyword))
                .ToListAsync();

            return Ok(result);
        }

        // 4. POST: Thêm sản phẩm mới (Chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product newProduct)
        {
            if (!User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Chỉ tài khoản Admin mới được quyền thêm mới sản phẩm!" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newProduct.ProductId }, newProduct);
        }

        // 5. PUT: Cập nhật sản phẩm (Chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product updateData)
        {
            if (!User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Chỉ tài khoản Admin mới được quyền cập nhật sản phẩm!" });
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm cần cập nhật!" });
            }

            product.ProductName = updateData.ProductName;
            product.Price = updateData.Price;
            product.StockQuantity = updateData.StockQuantity;
            product.CategoryId = updateData.CategoryId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 6. DELETE: Xóa sản phẩm (Chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Thu ngân không được quyền xóa sản phẩm!" });
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm cần xóa!" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        
// GET: api/products/barcode/{barcode}
// Tim san pham theo ma vach de POS them vao gio hang
[HttpGet("barcode/{barcode}")]
public async Task<IActionResult> GetByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return BadRequest(new
                {
                    message = "Vui long nhap ma vach san pham!"
                });
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Barcode == barcode);

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Khong tim thay san pham co ma vach nay!"
                });
            }

            return Ok(new
            {
                ProductId = product.ProductId,
                Barcode = product.Barcode,
                ProductName = product.ProductName,
                Price = product.Price,
                StockQuantity = product.StockQuantity
            });
        }


        // GET: api/products/adonet-barcode/{barcode}
        // Tra cứu Barcode bằng ADO.NET thuần: SqlConnection, SqlCommand, SqlDataReader, SqlParameter
        [HttpGet("adonet-barcode/{barcode}")]
        public async Task<IActionResult> GetBarcodeByAdoNet(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return BadRequest(new { message = "Vui lòng nhập mã vạch cần tra cứu!" });
            }

            var connectionString = _context.Database.GetConnectionString();

            // 1. Dùng using bọc SqlConnection đảm bảo đóng kết nối an toàn
            await using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                // 2. Câu lệnh SQL có tham số hóa chống SQL Injection
                string sql = @"SELECT TOP 1 p.ProductId, p.Barcode, p.ProductName, p.Price, p.StockQuantity, c.CategoryName 
                       FROM Products p
                       LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                       WHERE p.Barcode = @Barcode";

                await using (var cmd = new SqlCommand(sql, conn))
                {
                    // 3. Sử dụng SqlParameter chống tấn công SQL Injection
                    cmd.Parameters.Add(new SqlParameter("@Barcode", SqlDbType.NVarChar, 50) { Value = barcode.Trim() });

                    // 4. Sử dụng SqlDataReader duyệt một chiều siêu tốc
                    await using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow))
                    {
                        if (await reader.ReadAsync())
                        {
                            var product = new
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Barcode = reader.GetString(reader.GetOrdinal("Barcode")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                StockQuantity = reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                                CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? "" : reader.GetString(reader.GetOrdinal("CategoryName"))
                            };

                            return Ok(product);
                        }
                    }
                }
            }

            return NotFound(new { message = "Không tìm thấy sản phẩm có mã vạch này trong CSDL!" });
        }


        // GET: api/products/paged?page=1&pageSize=10
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var query = _context.Products.Include(p => p.Category).AsNoTracking();

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var items = await query
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.ProductId,
                    p.Barcode,
                    p.ProductName,
                    p.Price,
                    p.StockQuantity,
                    p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.CategoryName : "",
                    Category = p.Category
                })
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
}