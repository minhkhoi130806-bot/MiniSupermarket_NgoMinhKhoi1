using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiniSupermarket.API.Migrations
{
    /// <inheritdoc />
    public partial class InitPureFoodStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RewardPoints = table.Column<int>(type: "int", nullable: false),
                    MembershipRank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "Description" },
                values: new object[,]
                {
                    { 1, "Rau củ quả hữu cơ", "Rau, củ, quả tươi sạch" },
                    { 2, "Trái cây tươi", "Trái cây các loại" },
                    { 3, "Thịt & Hải sản sinh học", "Thịt heo, cá hồi tươi sống" },
                    { 4, "Gạo & Đồ khô dinh dưỡng", "Gạo lứt, hạt khô" },
                    { 5, "Nước ép & Trà thảo mộc", "Đồ uống tự nhiên" }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Address", "CustomerName", "MembershipRank", "PhoneNumber", "RewardPoints" },
                values: new object[,]
                {
                    { 1, "TP. Hồ Chí Minh", "Ngô Minh Khôi", "Kim Cương", "0909999999", 999 },
                    { 2, "Quận 1, TP.HCM", "Nguyễn Văn An", "Vàng", "0901112233", 150 },
                    { 3, "Quận 3, TP.HCM", "Trần Thị Bình", "Bạc", "0902223344", 80 },
                    { 4, "Quận 7, TP.HCM", "Lê Hoàng Cường", "Chuẩn", "0903334455", 20 },
                    { 5, "Quận Bình Thạnh, TP.HCM", "Phạm Thị Dung", "Kim Cương", "0904445566", 320 },
                    { 6, "Thành phố Thủ Đức", "Hoàng Văn Em", "Bạc", "0905556677", 50 },
                    { 7, "Quận 10, TP.HCM", "Vũ Thị Phương", "Chuẩn", "0906667788", 10 },
                    { 8, "Quận Tân Bình, TP.HCM", "Đặng Văn Giang", "Vàng", "0907778899", 210 },
                    { 9, "Quận Phú Nhuận, TP.HCM", "Bùi Thị Hoa", "Kim Cương", "0908889900", 450 },
                    { 10, "Quận Gò Vấp, TP.HCM", "Ngô Văn Hùng", "Bạc", "0909990011", 90 },
                    { 11, "Biên Hòa, Đồng Nai", "Dương Thị Kim", "Chuẩn", "0911122233", 30 },
                    { 12, "Dĩ An, Bình Dương", "Lý Văn Long", "Vàng", "0912233445", 180 },
                    { 13, "Quận 4, TP.HCM", "Hồ Thị Mai", "Chuẩn", "0913344556", 5 },
                    { 14, "Quận 5, TP.HCM", "Phan Văn Nam", "Bạc", "0914455667", 120 },
                    { 15, "Quận 11, TP.HCM", "Trương Thị Quỳnh", "Vàng", "0915566778", 290 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Barcode", "CategoryId", "Price", "ProductName", "StockQuantity" },
                values: new object[,]
                {
                    { 1, "BC001", 1, 25000m, "Rau cải ngọt hữu cơ (500g)", 50 },
                    { 2, "BC002", 1, 35000m, "Cà chua bi Organic (300g)", 40 },
                    { 3, "BC003", 2, 65000m, "Táo xanh VN (1kg)", 30 },
                    { 4, "BC004", 2, 30000m, "Chuối tiêu hữu cơ (Nải)", 25 },
                    { 5, "BC005", 3, 85000m, "Thịt heo ba chỉ sinh học (500g)", 20 },
                    { 6, "BC006", 3, 120000m, "Phi lê cá hồi phi lê (300g)", 15 },
                    { 7, "BC007", 4, 45000m, "Gạo lứt huyết rồng hữu cơ (1kg)", 60 },
                    { 8, "BC008", 4, 95000m, "Hạt điều rang muối dinh dưỡng (250g)", 35 },
                    { 9, "BC009", 5, 40000m, "Nước ép táo nguyên chất (500ml)", 45 },
                    { 10, "BC010", 5, 75000m, "Trà thảo mộc hoa cúc mật ong (Hộp)", 50 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
