using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MiniSupermarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly SupermarketDbContext _context;

        public ReportsController(SupermarketDbContext context) => _context = context;

        // =========================================================================
        // GET /api/reports/revenue?from=2026-10-01&to=2026-10-22
        // Báo cáo doanh thu toàn diện: tóm tắt KPI, biểu đồ cột theo ngày,
        // biểu đồ tròn theo danh mục (yêu cầu của Thầy), top sản phẩm bán chạy.
        // =========================================================================
        [HttpGet("revenue")]
        public async Task<IActionResult> Revenue([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (to.Date < from.Date)
                return BadRequest("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

            DateTime start = from.Date;
            DateTime end = to.Date.AddDays(1);   // bao gồm trọn ngày kết thúc

            // Đơn hàng trong khoảng (bỏ qua đơn đã hủy)
            var orders = await _context.Orders.AsNoTracking()
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end && o.Status != "Cancelled")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderIds = orders.Select(o => o.OrderId).ToList();

            // Tên khách hàng
            var customerIds = orders.Where(o => o.CustomerId != null)
                .Select(o => o.CustomerId!.Value).Distinct().ToList();
            var customerNames = await _context.Customers.AsNoTracking()
                .Where(c => customerIds.Contains(c.CustomerId))
                .ToDictionaryAsync(c => c.CustomerId, c => c.CustomerName);

            // Toàn bộ chi tiết đơn
            var lines = await _context.OrderItems.AsNoTracking()
                .Where(i => orderIds.Contains(i.OrderId))
                .Select(i => new { i.OrderId, i.ProductId, i.Quantity, i.LineTotal, i.UnitPrice })
                .ToListAsync();

            var lineSums = lines
                .GroupBy(l => l.OrderId)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.LineTotal > 0 ? l.LineTotal : l.UnitPrice * l.Quantity));

            // Thành tiền thực của đơn: ưu tiên FinalAmount, thiếu thì lấy TotalAmount - giảm giá
            decimal FinalOf(Order o)
            {
                if (o.FinalAmount > 0) return o.FinalAmount;
                if (o.TotalAmount > 0) return Math.Max(0, o.TotalAmount - o.DiscountAmount);
                return lineSums.TryGetValue(o.OrderId, out var s) ? Math.Max(0, s - o.DiscountAmount) : 0;
            }

            // Lấy thông tin sản phẩm kèm danh mục (Category) để vẽ Biểu đồ tròn
            var productIds = lines.Select(l => l.ProductId).Distinct().ToList();
            var productsInfo = await _context.Products.AsNoTracking()
                .Include(p => p.Category)
                .Where(p => productIds.Contains(p.ProductId))
                .Select(p => new 
                { 
                    p.ProductId, 
                    p.ProductName, 
                    CategoryName = p.Category != null ? p.Category.CategoryName : "Khác" 
                })
                .ToListAsync();

            var productNames = productsInfo.ToDictionary(p => p.ProductId, p => p.ProductName);
            var productCategoryMap = productsInfo.ToDictionary(p => p.ProductId, p => p.CategoryName);

            bool IsCash(string? m) => string.Equals(m, "CASH", StringComparison.OrdinalIgnoreCase);

            decimal totalRevenue = orders.Sum(FinalOf);
            int totalOrders = orders.Count;

            // =====================================================================
            // TIÊU CHÍ 44: Thống kê tức thì số lượng khách hàng & tồn kho thấp
            // =====================================================================
            int totalCustomers = await _context.Customers.AsNoTracking().CountAsync();
            int lowStockCount = await _context.Products.AsNoTracking().CountAsync(p => p.StockQuantity <= 10);

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            decimal todayRevenue = await _context.Orders.AsNoTracking()
                .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow && o.Status != "Cancelled")
                .SumAsync(o => (decimal?)o.FinalAmount) ?? 0m;
            int todayOrders = await _context.Orders.AsNoTracking()
                .CountAsync(o => o.CreatedAt >= today && o.CreatedAt < tomorrow && o.Status != "Cancelled");

            var summary = new
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalItems = lines.Sum(l => l.Quantity),
                AverageOrderValue = totalOrders == 0 ? 0 : Math.Round(totalRevenue / totalOrders),
                TotalDiscount = orders.Sum(o => o.DiscountAmount),
                CashRevenue = orders.Where(o => IsCash(o.PaymentMethod)).Sum(FinalOf),
                TransferRevenue = orders.Where(o => !IsCash(o.PaymentMethod)).Sum(FinalOf),

                // TIÊU CHÍ 44: Các chỉ số Dashboard KPI Summary Cards
                TotalCustomers = totalCustomers,
                LowStockCount = lowStockCount,
                TodayRevenue = todayRevenue,
                TodayOrders = todayOrders
            };

            // Dữ liệu cho Biểu đồ cột Doanh thu theo ngày (Tiêu chí 45)
            var daily = orders
                .GroupBy(o => o.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(FinalOf), Orders = g.Count() })
                .ToList();

            // Dữ liệu Top sản phẩm bán chạy nhất (Tiêu chí 46)
            var topProducts = lines
                .GroupBy(l => l.ProductId)
                .Select(g => new
                {
                    ProductName = productNames.TryGetValue(g.Key, out var n) ? n : $"Sản phẩm #{g.Key}",
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.LineTotal > 0 ? x.LineTotal : x.UnitPrice * x.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            // =====================================================================
            // YÊU CẦU CỦA THẦY: Cơ cấu Doanh thu theo Nhóm hàng cho Biểu đồ tròn (Pie Chart)
            // =====================================================================
            var categoryShares = lines
                .GroupBy(l => productCategoryMap.TryGetValue(l.ProductId, out var cName) ? cName : "Khác")
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(x => x.LineTotal > 0 ? x.LineTotal : x.UnitPrice * x.Quantity),
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            // Danh sách đơn hàng (giữ lại ở API để đảm bảo tính đầy đủ)
            var orderRows = orders.Select(o => new
            {
                o.OrderCode,
                o.CreatedAt,
                o.CashierUsername,
                CustomerName = o.CustomerId != null && customerNames.TryGetValue(o.CustomerId.Value, out var cn) ? cn : null,
                o.PaymentMethod,
                o.DiscountAmount,
                FinalAmount = FinalOf(o)
            }).ToList();

            return Ok(new 
            { 
                summary, 
                daily, 
                topProducts, 
                categoryShares, 
                orders = orderRows 
            });
        }

        // =========================================================================
        // GET: api/reports/dashboard-summary
        // Tiêu chí 44: Thống kê tức thì 4 chỉ số KPI quan trọng cho Dashboard
        // 1. Doanh thu hôm nay (VNĐ)
        // 2. Tổng số hóa đơn đã xuất
        // 3. Số lượng khách hàng mới đăng ký
        // 4. Số mặt hàng sắp hết hàng (tồn kho <= 10)
        // =========================================================================
        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // 1. Thẻ 1: Doanh thu hôm nay (VNĐ)
            var todayRevenue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow && o.Status != "Cancelled")
                .SumAsync(o => (decimal?)o.FinalAmount) ?? 0m;

            // 2. Thẻ 2: Tổng số hóa đơn đã xuất hôm nay
            var todayOrdersCount = await _context.Orders
                .AsNoTracking()
                .CountAsync(o => o.CreatedAt >= today && o.CreatedAt < tomorrow && o.Status != "Cancelled");

            // 3. Thẻ 3: Số lượng khách hàng đã đăng ký
            var totalCustomers = await _context.Customers
                .AsNoTracking()
                .CountAsync();

            // 4. Thẻ 4: Số mặt hàng sắp hết hàng (tồn kho <= 10)
            var lowStockCount = await _context.Products
                .AsNoTracking()
                .CountAsync(p => p.StockQuantity <= 10);

            return Ok(new
            {
                TodayRevenue = todayRevenue,
                TodayOrdersCount = todayOrdersCount,
                TotalCustomers = totalCustomers,
                LowStockProductsCount = lowStockCount
            });
        }

        // =========================================================================
        // GET: api/reports/top-best-sellers
        // Tiêu chí 46: Thống kê Top 5 mặt hàng tiêu thụ nhanh nhất
        // Kỹ thuật LINQ: GroupBy(od => od.ProductId) -> OrderByDescending(Sum Quantity) -> Take(5)
        // =========================================================================
        [HttpGet("top-best-sellers")]
        public async Task<IActionResult> GetTopBestSellers()
        {
            var top5 = await _context.OrderItems
                .AsNoTracking()
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantitySold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.LineTotal > 0 ? od.LineTotal : od.UnitPrice * od.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantitySold) // Sắp xếp giảm dần theo số lượng tiêu thụ
                .Take(5)                                    // Lấy đúng 5 sản phẩm đầu bảng
                .ToListAsync();

            var productIds = top5.Select(x => x.ProductId).ToList();
            var productNames = await _context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId, p => p.ProductName);

            var result = top5.Select((item, index) => new
            {
                Rank = index + 1,
                item.ProductId,
                ProductName = productNames.TryGetValue(item.ProductId, out var name) ? name : $"Sản phẩm #{item.ProductId}",
                item.TotalQuantitySold,
                item.TotalRevenue
            });

            return Ok(result);
        }

        // =========================================================================
        // GET: api/reports/sp-revenue?from=2026-09-01&to=2026-10-31
        // Tiêu chí 34 & 45: Gọi Stored Procedure trên SQL Server bằng ADO.NET CommandType.StoredProcedure
        // gom nhóm GROUP BY CAST(CreatedAt AS DATE) để tính doanh thu theo ngày
        // =========================================================================
        [HttpGet("sp-revenue")]
        public async Task<IActionResult> GetRevenueByStoredProcedure([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var toDate = to ?? DateTime.Today;
            var fromDate = from ?? toDate.AddDays(-30);

            if (toDate.Date < fromDate.Date)
                return BadRequest(new { message = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu." });

            // 1. Tự động kiểm tra và tạo Stored Procedure trên SQL Server nếu chưa tồn tại
            string createSpSql = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetDailyRevenue')
                BEGIN
                    EXEC('
                        CREATE PROCEDURE sp_GetDailyRevenue
                            @FromDate DATE,
                            @ToDate DATE
                        AS
                        BEGIN
                            SET NOCOUNT ON;
                            SELECT 
                                CAST(CreatedAt AS DATE) AS [Date],
                                COUNT(OrderId) AS [OrdersCount],
                                ISNULL(SUM(FinalAmount), 0) AS [TotalRevenue]
                            FROM Orders
                            WHERE CAST(CreatedAt AS DATE) >= @FromDate 
                              AND CAST(CreatedAt AS DATE) <= @ToDate
                              AND Status != ''Cancelled''
                            GROUP BY CAST(CreatedAt AS DATE)
                            ORDER BY [Date] ASC;
                        END
                    ')
                END";
            await _context.Database.ExecuteSqlRawAsync(createSpSql);

            // 2. Gọi Stored Procedure bằng ADO.NET thuần: SqlConnection, SqlCommand, SqlDataReader
            var connectionString = _context.Database.GetConnectionString();
            var list = new List<object>();

            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("sp_GetDailyRevenue", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.Date) { Value = fromDate.Date });
            cmd.Parameters.Add(new SqlParameter("@ToDate", SqlDbType.Date) { Value = toDate.Date });

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    Date = reader.GetDateTime(reader.GetOrdinal("Date")).ToString("yyyy-MM-dd"),
                    OrdersCount = reader.GetInt32(reader.GetOrdinal("OrdersCount")),
                    TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue"))
                });
            }

            return Ok(list);
        }
    }
}
