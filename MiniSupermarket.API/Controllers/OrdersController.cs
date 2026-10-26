using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private const decimal VndPerPoint = 10000m;   // 10.000đ = 1 điểm

        private readonly SupermarketDbContext _context;

        public OrdersController(SupermarketDbContext context)
        {
            _context = context;
        }

        // API thanh toán đơn hàng POS
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                return BadRequest(new { message = "Giỏ hàng đang trống!" });

            if (request.Items.Any(i => i.Quantity <= 0))
                return BadRequest(new { message = "Số lượng sản phẩm phải lớn hơn 0!" });

            // Kiểm tra tài khoản thu ngân
            var cashier = await _context.Users.FirstOrDefaultAsync(
                u => u.Username == request.CashierUsername && u.IsActive);

            if (cashier == null ||
                (!string.Equals(cashier.Role, "Admin", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(cashier.Role, "Cashier", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = "Tài khoản không có quyền thanh toán!" });
            }

            // Tìm khách hàng theo số điện thoại nếu có
            Customer? customer = null;
            if (!string.IsNullOrWhiteSpace(request.CustomerPhone))
            {
                string phone = request.CustomerPhone.Trim();
                customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phone);

                if (customer == null)
                    return BadRequest(new { message = "Không tìm thấy khách hàng!" });
            }

            // Gộp các sản phẩm trùng nhau
            var groupedItems = request.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
                .ToList();

            var productIds = groupedItems.Select(i => i.ProductId).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            if (products.Count != productIds.Count)
                return BadRequest(new { message = "Có sản phẩm không tồn tại!" });

            // Kiểm tra tồn kho và tính tạm tính theo giá trong database
            decimal totalAmount = 0;
            int totalItems = 0;

            foreach (var item in groupedItems)
            {
                var product = products.First(p => p.ProductId == item.ProductId);

                if (product.StockQuantity < item.Quantity)
                    return BadRequest(new { message = $"Sản phẩm {product.ProductName} không đủ tồn kho!" });

                totalAmount += product.Price * item.Quantity;
                totalItems += item.Quantity;
            }

            // Tiêu chí 47: Đổi điểm thưởng trừ tiền hóa đơn (1 điểm = 1.000 đ)
            // Khách có điểm tích lũy, có thể dùng điểm để trừ tiền (vd: 50 điểm = giảm 50.000đ)
            decimal pointsDiscount = 0;
            if (customer != null && request.PointsToRedeem > 0)
            {
                if (request.PointsToRedeem > customer.RewardPoints)
                {
                    return BadRequest(new { message = $"Khách hàng chỉ có {customer.RewardPoints} điểm, không đủ để đổi {request.PointsToRedeem} điểm!" });
                }
                pointsDiscount = request.PointsToRedeem * 1000m;
            }

            // Giảm giá do POS gửi lên + giảm giá từ điểm thưởng, giới hạn trong khoảng 0 đến tạm tính
            decimal discount = Math.Min(Math.Max(0, request.DiscountAmount + pointsDiscount), totalAmount);
            decimal finalAmount = totalAmount - discount;

            // Hình thức thanh toán
            bool isCash = string.Equals(request.PaymentMethod, "CASH", StringComparison.OrdinalIgnoreCase);
            string paymentMethod = isCash ? "Cash" : "Transfer";

            // Tiền khách đưa / tiền thừa
            decimal received = isCash && request.AmountReceived.HasValue
                ? request.AmountReceived.Value
                : finalAmount;

            if (received < finalAmount)
                return BadRequest(new { message = "Số tiền khách đưa chưa đủ!" });

            decimal change = received - finalAmount;

            // Điểm thưởng tính trên số tiền thực trả (chỉ khi có khách hàng)
            int earnedPoints = customer == null ? 0 : (int)Math.Floor(finalAmount / VndPerPoint);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    OrderCode = $"HD{DateTime.Now:yyyyMMddHHmmssfff}",
                    CustomerId = customer?.CustomerId,
                    CreatedAt = DateTime.Now,
                    TotalItems = totalItems,
                    CashierId = cashier.Id,
                    CashierUsername = cashier.Username,
                    TotalAmount = totalAmount,
                    DiscountAmount = discount,
                    FinalAmount = finalAmount,
                    AmountReceived = received,
                    ChangeAmount = change,
                    RewardPoints = earnedPoints,
                    PaymentMethod = paymentMethod,
                    Status = "Completed"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Lưu chi tiết đơn hàng và trừ tồn kho
                foreach (var item in groupedItems)
                {
                    var product = products.First(p => p.ProductId == item.ProductId);

                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        LineTotal = product.Price * item.Quantity
                    });

                    product.StockQuantity -= item.Quantity;
                }

                // Tiêu chí 47 & 35: Cập nhật điểm trong cùng một Transaction: trừ điểm đã đổi và cộng điểm mới
                int totalPoints = 0;
                if (customer != null)
                {
                    if (request.PointsToRedeem > 0)
                    {
                        customer.RewardPoints -= request.PointsToRedeem; // Trừ điểm đã dùng đổi tiền
                    }
                    customer.RewardPoints += earnedPoints; // Cộng điểm tích lũy của đơn hàng mới
                    totalPoints = customer.RewardPoints;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Thanh toán thành công!",
                    orderId = order.OrderId,
                    orderCode = order.OrderCode,
                    totalAmount = order.TotalAmount,
                    discountAmount = order.DiscountAmount,
                    finalAmount = order.FinalAmount,
                    redeemedPoints = request.PointsToRedeem,
                    pointsDiscount = pointsDiscount,
                    earnedPoints,
                    totalPoints
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi khi lưu đơn hàng! Giao dịch đã được tự động hoàn tác (Rollback)." });
            }
        }
    }

    // Dữ liệu nhận từ FormPOS
    public class CheckoutRequest
    {
        public string CashierUsername { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string PaymentMethod { get; set; } = "CASH";
        public decimal DiscountAmount { get; set; }
        public int PointsToRedeem { get; set; } = 0; // Tiêu chí 47: Số điểm muốn đổi trừ tiền (1 điểm = 1.000đ)
        public decimal? AmountReceived { get; set; }
        public List<CheckoutItemRequest> Items { get; set; } = new();
    }

    public class CheckoutItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}