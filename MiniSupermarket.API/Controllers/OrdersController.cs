using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [Authorize]
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

        // =========================================================================
        // TIÊU CHÍ 25: PHÂN TRANG DỮ LIỆU MÁY CHỦ (GET api/orders)
        // Áp dụng cặp lệnh LINQ .Skip((page - 1) * pageSize) và .Take(pageSize)
        // để phân trang phía server, trả về tổng số bản ghi và tổng số trang.
        // Hỗ trợ tham số exportAll=true để lấy toàn bộ dữ liệu khi xuất Excel.
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 8,
            [FromQuery] bool exportAll = false)
        {
            var query = _context.Orders.AsNoTracking().AsQueryable();

            // 1. Lọc theo khoảng ngày lập hóa đơn nếu người dùng chọn
            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                query = query.Where(o => o.CreatedAt >= fromDate);
            }
            if (to.HasValue)
            {
                var toDate = to.Value.Date.AddDays(1); // Lấy trọn vẹn cả ngày kết thúc
                query = query.Where(o => o.CreatedAt < toDate);
            }

                        // 2. Tìm kiếm theo mã đơn hàng, tên thu ngân hoặc thông tin khách hàng
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                var matchedCustomerIds = await _context.Customers
                    .Where(c => c.CustomerName.Contains(keyword) || c.PhoneNumber.Contains(keyword))
                    .Select(c => c.CustomerId)
                    .ToListAsync();

                query = query.Where(o => o.OrderCode.Contains(keyword) ||
                                         o.CashierUsername.Contains(keyword) ||
                                         (o.CustomerId.HasValue && matchedCustomerIds.Contains(o.CustomerId.Value)));
            }

            // 3. TIÊU CHÍ 25: Đếm tổng số bản ghi và tính tổng số trang
            int totalRecords = await query.CountAsync();
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 8;
            int totalPages = totalRecords > 0 ? (int)Math.Ceiling((double)totalRecords / pageSize) : 1;

            // 4. TIÊU CHÍ 25: Phân trang bằng cặp lệnh LINQ .Skip() và .Take()
            var pagedQuery = query.OrderByDescending(o => o.CreatedAt);

            List<Order> orders;
            if (exportAll)
            {
                orders = await pagedQuery.ToListAsync(); // Lấy toàn bộ khi xuất Excel
            }
            else
            {
                orders = await pagedQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            // 5. Nạp danh sách tên và số điện thoại khách hàng tương ứng
            var customerIds = orders.Where(o => o.CustomerId.HasValue)
                                    .Select(o => o.CustomerId!.Value)
                                    .Distinct()
                                    .ToList();

            var customers = await _context.Customers.AsNoTracking()
                .Where(c => customerIds.Contains(c.CustomerId))
                .ToDictionaryAsync(c => c.CustomerId, c => new { c.CustomerName, c.PhoneNumber });

            // 6. Định dạng dữ liệu trả về cho giao diện WinForms
            var result = orders.Select(o =>
            {
                string customerName = "Khách vãng lai";
                string customerPhone = "";

                if (o.CustomerId.HasValue && customers.TryGetValue(o.CustomerId.Value, out var cust))
                {
                    customerName = cust.CustomerName;
                    customerPhone = cust.PhoneNumber;
                }
                else if (o.CustomerId.HasValue)
                {
                    customerName = $"Khách #{o.CustomerId.Value}";
                }

                return new
                {
                    o.OrderId,
                    o.OrderCode,
                    o.CreatedAt,
                    o.TotalItems,
                    o.CashierId,
                    o.CashierUsername,
                    CustomerId = o.CustomerId,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    o.TotalAmount,
                    o.DiscountAmount,
                    o.FinalAmount,
                    o.AmountReceived,
                    o.ChangeAmount,
                    o.RewardPoints,
                    o.PaymentMethod,
                    o.Status
                };
            }).ToList();

            return Ok(new
            {
                currentPage = page,
                pageSize = pageSize,
                totalRecords = totalRecords,
                totalPages = totalPages,
                data = result
            });
        }

        // =========================================================================
        // TIÊU CHÍ BỔ SUNG: XEM CHI TIẾT ĐƠN HÀNG THEO ID (GET api/orders/{id})
        // Trả về thông tin hóa đơn và toàn bộ danh sách sản phẩm nằm trong hóa đơn đó
        // =========================================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound(new { message = $"Không tìm thấy đơn hàng với mã ID #{id}!" });

            // Lấy thông tin khách hàng nếu có
            string customerName = "Khách vãng lai";
            string customerPhone = "";
            string customerAddress = "";

            if (order.CustomerId.HasValue)
            {
                var cust = await _context.Customers.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CustomerId == order.CustomerId.Value);
                if (cust != null)
                {
                    customerName = cust.CustomerName;
                    customerPhone = cust.PhoneNumber;
                    customerAddress = cust.Address ?? "";
                }
                else
                {
                    customerName = $"Khách hàng #{order.CustomerId.Value}";
                }
            }

            // Lấy danh sách các dòng chi tiết sản phẩm của đơn hàng
            var items = await _context.OrderItems.AsNoTracking()
                .Where(i => i.OrderId == id)
                .ToListAsync();

            var productIds = items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products.AsNoTracking()
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId, p => new { p.ProductName, p.Barcode });

            var itemDetails = items.Select(i => new
            {
                i.OrderItemId,
                i.ProductId,
                ProductName = products.TryGetValue(i.ProductId, out var prod) ? prod.ProductName : $"Sản phẩm #{i.ProductId}",
                Barcode = products.TryGetValue(i.ProductId, out var p2) ? p2.Barcode : "",
                i.Quantity,
                i.UnitPrice,
                i.LineTotal
            }).ToList();

            return Ok(new
            {
                order.OrderId,
                order.OrderCode,
                order.CreatedAt,
                order.CashierUsername,
                order.CustomerId,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                CustomerAddress = customerAddress,
                order.PaymentMethod,
                order.TotalItems,
                order.TotalAmount,
                order.DiscountAmount,
                order.FinalAmount,
                order.AmountReceived,
                order.ChangeAmount,
                order.RewardPoints,
                order.Status,
                Items = itemDetails
            });
        }

        // =========================================================================
        // TIÊU CHÍ BỔ SUNG: XEM CHI TIẾT ĐƠN HÀNG THEO MÃ ĐƠN (GET api/orders/code/{orderCode})
        // =========================================================================
        [HttpGet("code/{orderCode}")]
        public async Task<IActionResult> GetOrderByCode(string orderCode)
        {
            var order = await _context.Orders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode.Trim());

            if (order == null)
                return NotFound(new { message = $"Không tìm thấy đơn hàng mang mã {orderCode}!" });

            return await GetOrderById(order.OrderId);
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

            // Tính tạm tính theo đơn giá trong CSDL và tổng số lượng mặt hàng
            decimal totalAmount = 0;
            int totalItems = 0;

            foreach (var item in groupedItems)
            {
                var product = products.First(p => p.ProductId == item.ProductId);
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

                // =========================================================================
                // TIÊU CHÍ 35: GIAO DỊCH THANH TOÁN AN TOÀN TRONG TRANSACTION (2.0 / 2.0 ĐIỂM)
                // Lưu hóa đơn, trừ tồn kho và cập nhật điểm tích lũy trọn gói trong 1 Transaction.
                // Khi kho hết hàng: Tự ném ngoại lệ -> kích hoạt transaction.RollbackAsync(),
                // đảm bảo KHÔNG lưu bất kỳ hóa đơn hay dòng chi tiết rác nào vào CSDL!
                // =========================================================================

                // Bước 2 & 3: Thêm chi tiết đơn hàng và kiểm tra/trừ tồn kho trực tiếp trong Transaction
                foreach (var item in groupedItems)
                {
                    var product = products.First(p => p.ProductId == item.ProductId);

                    // TIÊU CHÍ 35: Kiểm tra tồn kho ngay trong Transaction.
                    // Nếu mặt hàng không đủ số lượng -> Ném ngoại lệ để kích hoạt Rollback ngay lập tức!
                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Sản phẩm '{product.ProductName}' không đủ tồn kho! (Hiện còn: {product.StockQuantity}, Cần: {item.Quantity})");
                    }

                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        LineTotal = product.Price * item.Quantity
                    });

                    // Trừ tồn kho sản phẩm trong Transaction
                    product.StockQuantity -= item.Quantity;
                }

                // Bước 4: Cập nhật điểm tích lũy trong cùng Transaction: trừ điểm đã đổi và cộng điểm mới
                int totalPoints = 0;
                if (customer != null)
                {
                    if (request.PointsToRedeem > 0)
                    {
                        if (customer.RewardPoints < request.PointsToRedeem)
                        {
                            throw new InvalidOperationException(
                                $"Khách hàng không đủ điểm tích lũy để đổi! (Hiện có: {customer.RewardPoints}, Cần đổi: {request.PointsToRedeem})");
                        }
                        customer.RewardPoints -= request.PointsToRedeem; // Trừ điểm đã dùng đổi tiền
                    }

                    customer.RewardPoints += earnedPoints; // Cộng điểm tích lũy của đơn hàng mới
                    totalPoints = customer.RewardPoints;
                }

                // Lưu toàn bộ thay đổi (Order, OrderItems, Products, Customers) xuống Transaction
                await _context.SaveChangesAsync();

                // Xác nhận giao dịch thành công (Commit)
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
            catch (InvalidOperationException ex)
            {
                // TIÊU CHÍ 35: Khi kho hết hàng hoặc không đủ điểm -> Tự động Rollback, hủy Order vừa thêm!
                await transaction.RollbackAsync();
                return BadRequest(new { message = $"Thanh toán thất bại: {ex.Message} Giao dịch đã được tự động hoàn tác (Rollback) an toàn." });
            }
            catch (Exception ex)
            {
                // Khi xảy ra bất kỳ lỗi hệ thống hoặc ngoại lệ CSDL nào khác -> Tự động Rollback
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = $"Lỗi hệ thống khi lưu đơn hàng ({ex.Message})! Giao dịch đã được tự động hoàn tác (Rollback)." });
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