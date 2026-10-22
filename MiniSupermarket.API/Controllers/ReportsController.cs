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
    public class ReportsController : ControllerBase
    {
        private readonly SupermarketDbContext _context;

        public ReportsController(SupermarketDbContext context) => _context = context;

        // GET /api/reports/revenue?from=2026-10-01&to=2026-10-22
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

            // Toàn bộ chi tiết đơn (để tính lại tiền khi cột tổng bị trống)
            var lines = await _context.OrderItems.AsNoTracking()
                .Where(i => orderIds.Contains(i.OrderId))
                .Select(i => new { i.OrderId, i.ProductId, i.Quantity, i.LineTotal, i.UnitPrice })
                .ToListAsync();

            var lineSums = lines
                .GroupBy(l => l.OrderId)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.LineTotal > 0 ? l.LineTotal : l.UnitPrice * l.Quantity));

            // Thành tiền thực của đơn: ưu tiên FinalAmount, thiếu thì lấy TotalAmount - giảm giá,
            // vẫn thiếu thì cộng từ chi tiết đơn.
            decimal FinalOf(Order o)
            {
                if (o.FinalAmount > 0) return o.FinalAmount;
                if (o.TotalAmount > 0) return Math.Max(0, o.TotalAmount - o.DiscountAmount);
                return lineSums.TryGetValue(o.OrderId, out var s) ? Math.Max(0, s - o.DiscountAmount) : 0;
            }

            // Tên sản phẩm cho Top bán chạy
            var productIds = lines.Select(l => l.ProductId).Distinct().ToList();
            var productNames = await _context.Products.AsNoTracking()
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId, p => p.ProductName);

            bool IsCash(string? m) => string.Equals(m, "CASH", StringComparison.OrdinalIgnoreCase);

            decimal totalRevenue = orders.Sum(FinalOf);
            int totalOrders = orders.Count;

            var summary = new
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalItems = lines.Sum(l => l.Quantity),
                AverageOrderValue = totalOrders == 0 ? 0 : Math.Round(totalRevenue / totalOrders),
                TotalDiscount = orders.Sum(o => o.DiscountAmount),
                CashRevenue = orders.Where(o => IsCash(o.PaymentMethod)).Sum(FinalOf),
                TransferRevenue = orders.Where(o => !IsCash(o.PaymentMethod)).Sum(FinalOf)
            };

            var daily = orders
                .GroupBy(o => o.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(FinalOf), Orders = g.Count() })
                .ToList();

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

            return Ok(new { summary, daily, topProducts, orders = orderRows });
        }
    }
}