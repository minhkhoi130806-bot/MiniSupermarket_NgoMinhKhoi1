using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Data
{
    public class SupermarketDbContext : DbContext
    {
        public SupermarketDbContext(DbContextOptions<SupermarketDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public DbSet<User> Users { get; set; }
        // Don hang va chi tiet don hang
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Dữ liệu mẫu danh mục
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Rau củ quả hữu cơ", Description = "Rau củ quả tươi sạch đạt chuẩn VietGAP, an toàn cho sức khỏe." },
                new Category { CategoryId = 2, CategoryName = "Trái cây tươi", Description = "Trái cây nội địa và nhập khẩu tươi ngon, giàu vitamin." },
                new Category { CategoryId = 3, CategoryName = "Thịt & Hải sản sinh học", Description = "Thịt và hải sản tươi sống được kiểm định chất lượng nghiêm ngặt." },
                new Category { CategoryId = 4, CategoryName = "Gạo & Đồ khô dinh dưỡng", Description = "Các loại gạo lứt, hạt dinh dưỡng tốt cho sức khỏe và ăn kiêng." },
                new Category { CategoryId = 5, CategoryName = "Nước ép & Trà thảo mộc", Description = "Đồ uống giải khát tự nhiên, trà thảo mộc thanh lọc cơ thể." },
                new Category { CategoryId = 6, CategoryName = "Đồ uống có gas & Nước khoáng", Description = "Nước giải khát và nước khoáng thiên nhiên giải nhiệt nhanh chóng." },
                new Category { CategoryId = 7, CategoryName = "Sữa & Sản phẩm từ sữa", Description = "Sữa tươi, sữa chua và phô mai bổ sung canxi tối ưu." },
                new Category { CategoryId = 8, CategoryName = "Bánh kẹo & Đồ ăn vặt", Description = "Bánh quy, kẹo dẻo và snack cao cấp từ các thương hiệu uy tín." },
                new Category { CategoryId = 9, CategoryName = "Mì gói & Thực phẩm ăn liền", Description = "Mì tôm, phở khô và cháo ăn liền tiện lợi cho bữa ăn nhanh." },
                new Category { CategoryId = 10, CategoryName = "Gia vị & Dầu ăn cao cấp", Description = "Nước mắm, hạt nêm và dầu ăn giúp nâng tầm hương vị bữa cơm." },
                new Category { CategoryId = 11, CategoryName = "Thực phẩm đông lạnh tiện lợi", Description = "Các món ăn chế biến sẵn đông lạnh tiết kiệm thời gian nấu nướng." },
                new Category { CategoryId = 12, CategoryName = "Đồ dùng chăm sóc cá nhân", Description = "Dầu gội, sữa tắm và sản phẩm vệ sinh an toàn cho da." },
                new Category { CategoryId = 13, CategoryName = "Hóa chất & Tẩy rửa gia dụng", Description = "Nước giặt, nước rửa chén và dung dịch vệ sinh nhà cửa sạch sẽ." },
                new Category { CategoryId = 14, CategoryName = "Đồ dùng gia đình thiết yếu", Description = "Đồ dùng nhựa, giấy vệ sinh và vật dụng sinh hoạt hàng ngày." },
                new Category { CategoryId = 15, CategoryName = "Sản phẩm dinh dưỡng chuyên biệt", Description = "Thực phẩm bổ sung vitamin và sữa bột nâng cao sức khỏe toàn diện." }
            );

            // 1. Dữ liệu mẫu danh mục thực phẩm hữu cơ
            // --- 3. DỮ LIỆU MẪU SẢN PHẨM THỰC PHẨM HỮU CƠ (Products) ---
            // Du lieu mau san pham voi barcode EAN-13 gom 13 chu so
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, Barcode = "8930000000019", ProductName = "Rau cải ngọt hữu cơ (500g)", Price = 25000, StockQuantity = 50, CategoryId = 1 },
                new Product { ProductId = 2, Barcode = "8930000000026", ProductName = "Cà chua bi Organic (300g)", Price = 35000, StockQuantity = 40, CategoryId = 1 },
                new Product { ProductId = 3, Barcode = "8930000000033", ProductName = "Táo xanh VN cao cấp (1kg)", Price = 65000, StockQuantity = 30, CategoryId = 2 },
                new Product { ProductId = 4, Barcode = "8930000000040", ProductName = "Chuối tiêu hữu cơ xuất khẩu (Nải)", Price = 30000, StockQuantity = 25, CategoryId = 2 },
                new Product { ProductId = 5, Barcode = "8930000000057", ProductName = "Thịt heo ba chỉ sinh học (500g)", Price = 85000, StockQuantity = 20, CategoryId = 3 },
                new Product { ProductId = 6, Barcode = "8930000000064", ProductName = "Phi lê cá hồi Na Uy tươi (300g)", Price = 120000, StockQuantity = 15, CategoryId = 3 },
                new Product { ProductId = 7, Barcode = "8930000000071", ProductName = "Gạo lứt huyết rồng hữu cơ (1kg)", Price = 45000, StockQuantity = 60, CategoryId = 4 },
                new Product { ProductId = 8, Barcode = "8930000000088", ProductName = "Hạt điều rang muối dinh dưỡng (250g)", Price = 95000, StockQuantity = 35, CategoryId = 4 },
                new Product { ProductId = 9, Barcode = "8930000000095", ProductName = "Nước ép táo nguyên chất đậm đặc (500ml)", Price = 40000, StockQuantity = 45, CategoryId = 5 },
                new Product { ProductId = 10, Barcode = "8930000000101", ProductName = "Trà thảo mộc hoa cúc mật ong tự nhiên (Hộp)", Price = 75000, StockQuantity = 50, CategoryId = 5 },
                new Product { ProductId = 11, Barcode = "8930000000118", ProductName = "Nước khoáng thiên nhiên không gas (Thùng 24 chai)", Price = 96000, StockQuantity = 40, CategoryId = 6 },
                new Product { ProductId = 12, Barcode = "8930000000125", ProductName = "Sữa tươi thanh trùng 100% nguyên chất (900ml)", Price = 42000, StockQuantity = 55, CategoryId = 7 },
                new Product { ProductId = 13, Barcode = "8930000000132", ProductName = "Bánh quy bơ thượng hạng cao cấp (Hộp thiếc)", Price = 135000, StockQuantity = 25, CategoryId = 8 },
                new Product { ProductId = 14, Barcode = "8930000000149", ProductName = "Mì tôm chua cay hảo hạng (Lốc 5 gói)", Price = 22000, StockQuantity = 100, CategoryId = 9 },
                new Product { ProductId = 15, Barcode = "8930000000156", ProductName = "Dầu ăn thực vật cao cấp tinh luyện (Lít)", Price = 58000, StockQuantity = 70, CategoryId = 10 }
            );

            // 2. Dữ liệu mẫu 15 khách hàng (Có Ngô Minh Khôi ở vị trí VIP đầu tiên)
            // 2. Dữ liệu mẫu 15 khách hàng (Có Ngô Minh Khôi ở vị trí VIP đầu tiên, địa chỉ đầy đủ số nhà, tên đường)
            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, CustomerName = "Ngô Minh Khôi", PhoneNumber = "0909999999", Address = "123 Đường Lê Lợi, Phường Bến Thành, Quận 1, TP.HCM", RewardPoints = 999, MembershipRank = "Kim Cương" },
                new Customer { CustomerId = 2, CustomerName = "Nguyễn Văn An", PhoneNumber = "0901112233", Address = "45 Đường Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM", RewardPoints = 150, MembershipRank = "Vàng" },
                new Customer { CustomerId = 3, CustomerName = "Trần Thị Bình", PhoneNumber = "0902223344", Address = "78 Đường Cách Mạng Tháng 8, Phường Võ Thị Sáu, Quận 3, TP.HCM", RewardPoints = 80, MembershipRank = "Bạc" },
                new Customer { CustomerId = 4, CustomerName = "Lê Hoàng Cường", PhoneNumber = "0903334455", Address = "12 Đường Nguyễn Thị Thập, Phường Tân Phú, Quận 7, TP.HCM", RewardPoints = 20, MembershipRank = "Chuẩn" },
                new Customer { CustomerId = 5, CustomerName = "Phạm Thị Dung", PhoneNumber = "0904445566", Address = "90 Đường Đinh Bộ Lĩnh, Phường 26, Quận Bình Thạnh, TP.HCM", RewardPoints = 320, MembershipRank = "Kim Cương" },
                new Customer { CustomerId = 6, CustomerName = "Hoàng Văn Em", PhoneNumber = "0905556677", Address = "567 Đường Võ Văn Ngân, Phường Linh Chiểu, Thành phố Thủ Đức, TP.HCM", RewardPoints = 50, MembershipRank = "Bạc" },
                new Customer { CustomerId = 7, CustomerName = "Vũ Thị Phương", PhoneNumber = "0906667788", Address = "34 Đường 3 Tháng 2, Phường 12, Quận 10, TP.HCM", RewardPoints = 10, MembershipRank = "Chuẩn" },
                new Customer { CustomerId = 8, CustomerName = "Đặng Văn Giang", PhoneNumber = "0907778899", Address = "88 Đường Cộng Hòa, Phường 4, Quận Tân Bình, TP.HCM", RewardPoints = 210, MembershipRank = "Vàng" },
                new Customer { CustomerId = 9, CustomerName = "Bùi Thị Hoa", PhoneNumber = "0908889900", Address = "21 Đường Phan Xích Long, Phường 2, Quận Phú Nhuận, TP.HCM", RewardPoints = 450, MembershipRank = "Kim Cương" },
                new Customer { CustomerId = 10, CustomerName = "Ngô Văn Hùng", PhoneNumber = "0909990011", Address = "99 Đường Nguyễn Oanh, Phường 17, Quận Gò Vấp, TP.HCM", RewardPoints = 90, MembershipRank = "Bạc" },
                new Customer { CustomerId = 11, CustomerName = "Dương Thị Kim", PhoneNumber = "0911122233", Address = "102 Đường Nguyễn Ái Quốc, Phường Tân Tiến, Thành phố Biên Hòa, Đồng Nai", RewardPoints = 30, MembershipRank = "Chuẩn" },
                new Customer { CustomerId = 12, CustomerName = "Lý Văn Long", PhoneNumber = "0912233445", Address = "456 Đại lộ Bình Dương, Phường An Thạnh, Thành phố Dĩ An, Bình Dương", RewardPoints = 180, MembershipRank = "Vàng" },
                new Customer { CustomerId = 13, CustomerName = "Hồ Thị Mai", PhoneNumber = "0913344556", Address = "15 Đường Hoàng Diệu, Phường 6, Quận 4, TP.HCM", RewardPoints = 5, MembershipRank = "Chuẩn" },
                new Customer { CustomerId = 14, CustomerName = "Phan Văn Nam", PhoneNumber = "0914455667", Address = "67 Đường An Dương Vương, Phường 8, Quận 5, TP.HCM", RewardPoints = 120, MembershipRank = "Bạc" },
                new Customer { CustomerId = 15, CustomerName = "Trương Thị Quỳnh", PhoneNumber = "0915566778", Address = "321 Đường Lạc Long Quân, Phường 5, Quận 11, TP.HCM", RewardPoints = 290, MembershipRank = "Vàng" }
            );


            // 3. Du lieu mau 15 tai khoan nguoi dung
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin01", Password = "123456", FullName = "Quản trị viên 01", Role = "Admin", IsActive = true },
                new User { Id = 2, Username = "admin02", Password = "123456", FullName = "Quản trị viên 02", Role = "Admin", IsActive = true },
                new User { Id = 3, Username = "cashier01", Password = "123456", FullName = "Thu ngân 01", Role = "Cashier", IsActive = true },
                new User { Id = 4, Username = "cashier02", Password = "123456", FullName = "Thu ngân 02", Role = "Cashier", IsActive = true },
                new User { Id = 5, Username = "cashier03", Password = "123456", FullName = "Thu ngân 03", Role = "Cashier", IsActive = true },
                new User { Id = 6, Username = "cashier04", Password = "123456", FullName = "Thu ngân 04", Role = "Cashier", IsActive = true },
                new User { Id = 7, Username = "cashier05", Password = "123456", FullName = "Thu ngân 05", Role = "Cashier", IsActive = true },
                new User { Id = 8, Username = "cashier06", Password = "123456", FullName = "Thu ngân 06", Role = "Cashier", IsActive = true },
                new User { Id = 9, Username = "ware01", Password = "123456", FullName = "Thủ kho 01", Role = "Warehouse", IsActive = true },
                new User { Id = 10, Username = "ware02", Password = "123456", FullName = "Thủ kho 02", Role = "Warehouse", IsActive = true },
                new User { Id = 11, Username = "ware03", Password = "123456", FullName = "Thủ kho 03", Role = "Warehouse", IsActive = true },
                new User { Id = 12, Username = "ware04", Password = "123456", FullName = "Thủ kho 04", Role = "Warehouse", IsActive = true },
                new User { Id = 13, Username = "ware05", Password = "123456", FullName = "Thủ kho 05", Role = "Warehouse", IsActive = true },
                new User { Id = 14, Username = "admin_backup", Password = "123456", FullName = "Quản trị dự phòng", Role = "Admin", IsActive = true },
                new User { Id = 15, Username = "supervisor", Password = "123456", FullName = "Giám sát", Role = "Admin", IsActive = true }
            );


            // 4. Du lieu mau 20 don hang

            // 4. Du lieu mau 20 don hang
            modelBuilder.Entity<Order>().HasData(
                new Order { OrderId = 1, OrderCode = "HD000001", CustomerId = 1, CreatedAt = new DateTime(2026, 9, 20, 8, 30, 0), TotalItems = 3, CashierId = 3, CashierUsername = "cashier01", TotalAmount = 75000, DiscountAmount = 0, FinalAmount = 75000, AmountReceived = 80000, ChangeAmount = 5000, RewardPoints = 7, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 2, OrderCode = "HD000002", CustomerId = 2, CreatedAt = new DateTime(2026, 9, 20, 9, 15, 0), TotalItems = 1, CashierId = 4, CashierUsername = "cashier02", TotalAmount = 95000, DiscountAmount = 0, FinalAmount = 95000, AmountReceived = 100000, ChangeAmount = 5000, RewardPoints = 9, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 3, OrderCode = "HD000003", CustomerId = 3, CreatedAt = new DateTime(2026, 9, 20, 10, 0, 0), TotalItems = 1, CashierId = 5, CashierUsername = "cashier03", TotalAmount = 65000, DiscountAmount = 0, FinalAmount = 65000, AmountReceived = 65000, ChangeAmount = 0, RewardPoints = 6, PaymentMethod = "Card", Status = "Completed" },
                new Order { OrderId = 4, OrderCode = "HD000004", CustomerId = 4, CreatedAt = new DateTime(2026, 9, 21, 8, 45, 0), TotalItems = 1, CashierId = 6, CashierUsername = "cashier04", TotalAmount = 120000, DiscountAmount = 0, FinalAmount = 120000, AmountReceived = 120000, ChangeAmount = 0, RewardPoints = 12, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 5, OrderCode = "HD000005", CustomerId = 5, CreatedAt = new DateTime(2026, 9, 21, 11, 20, 0), TotalItems = 1, CashierId = 7, CashierUsername = "cashier05", TotalAmount = 85000, DiscountAmount = 0, FinalAmount = 85000, AmountReceived = 85000, ChangeAmount = 0, RewardPoints = 8, PaymentMethod = "Card", Status = "Completed" },
                new Order { OrderId = 6, OrderCode = "HD000006", CustomerId = 6, CreatedAt = new DateTime(2026, 9, 22, 9, 10, 0), TotalItems = 2, CashierId = 8, CashierUsername = "cashier06", TotalAmount = 90000, DiscountAmount = 0, FinalAmount = 90000, AmountReceived = 90000, ChangeAmount = 0, RewardPoints = 9, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 7, OrderCode = "HD000007", CustomerId = 7, CreatedAt = new DateTime(2026, 9, 22, 14, 5, 0), TotalItems = 1, CashierId = 3, CashierUsername = "cashier01", TotalAmount = 95000, DiscountAmount = 0, FinalAmount = 95000, AmountReceived = 100000, ChangeAmount = 5000, RewardPoints = 9, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 8, OrderCode = "HD000008", CustomerId = 8, CreatedAt = new DateTime(2026, 9, 23, 8, 35, 0), TotalItems = 2, CashierId = 4, CashierUsername = "cashier02", TotalAmount = 60000, DiscountAmount = 0, FinalAmount = 60000, AmountReceived = 60000, ChangeAmount = 0, RewardPoints = 6, PaymentMethod = "Card", Status = "Completed" },
                new Order { OrderId = 9, OrderCode = "HD000009", CustomerId = 9, CreatedAt = new DateTime(2026, 9, 23, 10, 25, 0), TotalItems = 2, CashierId = 5, CashierUsername = "cashier03", TotalAmount = 130000, DiscountAmount = 0, FinalAmount = 130000, AmountReceived = 130000, ChangeAmount = 0, RewardPoints = 13, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 10, OrderCode = "HD000010", CustomerId = 10, CreatedAt = new DateTime(2026, 9, 24, 15, 30, 0), TotalItems = 3, CashierId = 6, CashierUsername = "cashier04", TotalAmount = 95000, DiscountAmount = 0, FinalAmount = 95000, AmountReceived = 100000, ChangeAmount = 5000, RewardPoints = 9, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 11, OrderCode = "HD000011", CustomerId = 11, CreatedAt = new DateTime(2026, 9, 25, 9, 0, 0), TotalItems = 3, CashierId = 7, CashierUsername = "cashier05", TotalAmount = 69000, DiscountAmount = 0, FinalAmount = 69000, AmountReceived = 69000, ChangeAmount = 0, RewardPoints = 6, PaymentMethod = "Card", Status = "Completed" },
                new Order { OrderId = 12, OrderCode = "HD000012", CustomerId = 12, CreatedAt = new DateTime(2026, 9, 25, 13, 15, 0), TotalItems = 1, CashierId = 8, CashierUsername = "cashier06", TotalAmount = 96000, DiscountAmount = 0, FinalAmount = 96000, AmountReceived = 100000, ChangeAmount = 4000, RewardPoints = 9, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 13, OrderCode = "HD000013", CustomerId = 13, CreatedAt = new DateTime(2026, 9, 26, 10, 40, 0), TotalItems = 2, CashierId = 3, CashierUsername = "cashier01", TotalAmount = 75000, DiscountAmount = 0, FinalAmount = 75000, AmountReceived = 80000, ChangeAmount = 5000, RewardPoints = 7, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 14, OrderCode = "HD000014", CustomerId = 14, CreatedAt = new DateTime(2026, 9, 26, 16, 20, 0), TotalItems = 2, CashierId = 4, CashierUsername = "cashier02", TotalAmount = 160000, DiscountAmount = 0, FinalAmount = 160000, AmountReceived = 160000, ChangeAmount = 0, RewardPoints = 16, PaymentMethod = "Card", Status = "Completed" },
                new Order { OrderId = 15, OrderCode = "HD000015", CustomerId = 15, CreatedAt = new DateTime(2026, 9, 27, 11, 10, 0), TotalItems = 1, CashierId = 5, CashierUsername = "cashier03", TotalAmount = 95000, DiscountAmount = 0, FinalAmount = 95000, AmountReceived = 100000, ChangeAmount = 5000, RewardPoints = 9, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 16, OrderCode = "HD000016", CustomerId = null, CreatedAt = new DateTime(2026, 9, 28, 8, 20, 0), TotalItems = 2, CashierId = 6, CashierUsername = "cashier04", TotalAmount = 50000, DiscountAmount = 0, FinalAmount = 50000, AmountReceived = 50000, ChangeAmount = 0, RewardPoints = 5, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 17, OrderCode = "HD000017", CustomerId = null, CreatedAt = new DateTime(2026, 9, 29, 14, 45, 0), TotalItems = 1, CashierId = 7, CashierUsername = "cashier05", TotalAmount = 96000, DiscountAmount = 0, FinalAmount = 96000, AmountReceived = 96000, ChangeAmount = 0, RewardPoints = 9, PaymentMethod = "Card", Status = "Completed" },
                new Order { OrderId = 18, OrderCode = "HD000018", CustomerId = 1, CreatedAt = new DateTime(2026, 9, 30, 9, 35, 0), TotalItems = 1, CashierId = 8, CashierUsername = "cashier06", TotalAmount = 135000, DiscountAmount = 0, FinalAmount = 135000, AmountReceived = 140000, ChangeAmount = 5000, RewardPoints = 13, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 19, OrderCode = "HD000019", CustomerId = 2, CreatedAt = new DateTime(2026, 10, 1, 12, 0, 0), TotalItems = 4, CashierId = 3, CashierUsername = "cashier01", TotalAmount = 88000, DiscountAmount = 0, FinalAmount = 88000, AmountReceived = 90000, ChangeAmount = 2000, RewardPoints = 8, PaymentMethod = "Cash", Status = "Completed" },
                new Order { OrderId = 20, OrderCode = "HD000020", CustomerId = 3, CreatedAt = new DateTime(2026, 10, 2, 17, 10, 0), TotalItems = 2, CashierId = 4, CashierUsername = "cashier02", TotalAmount = 140000, DiscountAmount = 0, FinalAmount = 140000, AmountReceived = 140000, ChangeAmount = 0, RewardPoints = 14, PaymentMethod = "Card", Status = "Completed" }
            );



            // 5. Du lieu mau chi tiet don hang
            modelBuilder.Entity<OrderItem>().HasData(
                new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 1, Quantity = 3, UnitPrice = 25000, LineTotal = 75000 },
                new OrderItem { OrderItemId = 2, OrderId = 2, ProductId = 8, Quantity = 1, UnitPrice = 95000, LineTotal = 95000 },
                new OrderItem { OrderItemId = 3, OrderId = 3, ProductId = 3, Quantity = 1, UnitPrice = 65000, LineTotal = 65000 },
                new OrderItem { OrderItemId = 4, OrderId = 4, ProductId = 6, Quantity = 1, UnitPrice = 120000, LineTotal = 120000 },
                new OrderItem { OrderItemId = 5, OrderId = 5, ProductId = 5, Quantity = 1, UnitPrice = 85000, LineTotal = 85000 },
                new OrderItem { OrderItemId = 6, OrderId = 6, ProductId = 7, Quantity = 2, UnitPrice = 45000, LineTotal = 90000 },
                new OrderItem { OrderItemId = 7, OrderId = 7, ProductId = 8, Quantity = 1, UnitPrice = 95000, LineTotal = 95000 },
                new OrderItem { OrderItemId = 8, OrderId = 8, ProductId = 4, Quantity = 2, UnitPrice = 30000, LineTotal = 60000 },
                new OrderItem { OrderItemId = 9, OrderId = 9, ProductId = 3, Quantity = 2, UnitPrice = 65000, LineTotal = 130000 },
                new OrderItem { OrderItemId = 10, OrderId = 10, ProductId = 9, Quantity = 1, UnitPrice = 40000, LineTotal = 40000 },
                new OrderItem { OrderItemId = 11, OrderId = 10, ProductId = 1, Quantity = 1, UnitPrice = 25000, LineTotal = 25000 },
                new OrderItem { OrderItemId = 12, OrderId = 10, ProductId = 4, Quantity = 1, UnitPrice = 30000, LineTotal = 30000 },
                new OrderItem { OrderItemId = 13, OrderId = 11, ProductId = 14, Quantity = 2, UnitPrice = 22000, LineTotal = 44000 },
                new OrderItem { OrderItemId = 14, OrderId = 11, ProductId = 1, Quantity = 1, UnitPrice = 25000, LineTotal = 25000 },
                new OrderItem { OrderItemId = 15, OrderId = 12, ProductId = 11, Quantity = 1, UnitPrice = 96000, LineTotal = 96000 },
                new OrderItem { OrderItemId = 16, OrderId = 13, ProductId = 2, Quantity = 1, UnitPrice = 35000, LineTotal = 35000 },
                new OrderItem { OrderItemId = 17, OrderId = 13, ProductId = 9, Quantity = 1, UnitPrice = 40000, LineTotal = 40000 },
                new OrderItem { OrderItemId = 18, OrderId = 14, ProductId = 13, Quantity = 1, UnitPrice = 135000, LineTotal = 135000 },
                new OrderItem { OrderItemId = 19, OrderId = 14, ProductId = 1, Quantity = 1, UnitPrice = 25000, LineTotal = 25000 },
                new OrderItem { OrderItemId = 20, OrderId = 15, ProductId = 8, Quantity = 1, UnitPrice = 95000, LineTotal = 95000 },
                new OrderItem { OrderItemId = 21, OrderId = 16, ProductId = 1, Quantity = 2, UnitPrice = 25000, LineTotal = 50000 },
                new OrderItem { OrderItemId = 22, OrderId = 17, ProductId = 11, Quantity = 1, UnitPrice = 96000, LineTotal = 96000 },
                new OrderItem { OrderItemId = 23, OrderId = 18, ProductId = 13, Quantity = 1, UnitPrice = 135000, LineTotal = 135000 },
                new OrderItem { OrderItemId = 24, OrderId = 19, ProductId = 14, Quantity = 4, UnitPrice = 22000, LineTotal = 88000 },
                new OrderItem { OrderItemId = 25, OrderId = 20, ProductId = 6, Quantity = 1, UnitPrice = 120000, LineTotal = 120000 },
                new OrderItem { OrderItemId = 26, OrderId = 20, ProductId = 1, Quantity = 1, UnitPrice = 20000, LineTotal = 20000 }
            );




        }
    }
}