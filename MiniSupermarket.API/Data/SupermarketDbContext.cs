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
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, Barcode = "BC001", ProductName = "Rau cải ngọt hữu cơ (500g)", Price = 25000, StockQuantity = 50, CategoryId = 1 },
                new Product { ProductId = 2, Barcode = "BC002", ProductName = "Cà chua bi Organic (300g)", Price = 35000, StockQuantity = 40, CategoryId = 1 },
                new Product { ProductId = 3, Barcode = "BC003", ProductName = "Táo xanh VN cao cấp (1kg)", Price = 65000, StockQuantity = 30, CategoryId = 2 },
                new Product { ProductId = 4, Barcode = "BC004", ProductName = "Chuối tiêu hữu cơ xuất khẩu (Nải)", Price = 30000, StockQuantity = 25, CategoryId = 2 },
                new Product { ProductId = 5, Barcode = "BC005", ProductName = "Thịt heo ba chỉ sinh học (500g)", Price = 85000, StockQuantity = 20, CategoryId = 3 },
                new Product { ProductId = 6, Barcode = "BC006", ProductName = "Phi lê cá hồi Na Uy tươi (300g)", Price = 120000, StockQuantity = 15, CategoryId = 3 },
                new Product { ProductId = 7, Barcode = "BC007", ProductName = "Gạo lứt huyết rồng hữu cơ (1kg)", Price = 45000, StockQuantity = 60, CategoryId = 4 },
                new Product { ProductId = 8, Barcode = "BC008", ProductName = "Hạt điều rang muối dinh dưỡng (250g)", Price = 95000, StockQuantity = 35, CategoryId = 4 },
                new Product { ProductId = 9, Barcode = "BC009", ProductName = "Nước ép táo nguyên chất đậm đặc (500ml)", Price = 40000, StockQuantity = 45, CategoryId = 5 },
                new Product { ProductId = 10, Barcode = "BC010", ProductName = "Trà thảo mộc hoa cúc mật ong tự nhiên (Hộp)", Price = 75000, StockQuantity = 50, CategoryId = 5 },
                new Product { ProductId = 11, Barcode = "BC011", ProductName = "Nước khoáng thiên nhiên không gas (Thùng 24 chai)", Price = 96000, StockQuantity = 40, CategoryId = 6 },
                new Product { ProductId = 12, Barcode = "BC012", ProductName = "Sữa tươi thanh trùng 100% nguyên chất (900ml)", Price = 42000, StockQuantity = 55, CategoryId = 7 },
                new Product { ProductId = 13, Barcode = "BC013", ProductName = "Bánh quy bơ thượng hạng cao cấp (Hộp thiếc)", Price = 135000, StockQuantity = 25, CategoryId = 8 },
                new Product { ProductId = 14, Barcode = "BC014", ProductName = "Mì tôm chua cay hảo hạng (Lốc 5 gói)", Price = 22000, StockQuantity = 100, CategoryId = 9 },
                new Product { ProductId = 15, Barcode = "BC015", ProductName = "Dầu ăn thực vật cao cấp tinh luyện (Lít)", Price = 58000, StockQuantity = 70, CategoryId = 10 }
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
        }
    }
}