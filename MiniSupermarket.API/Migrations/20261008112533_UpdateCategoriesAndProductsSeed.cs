using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiniSupermarket.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoriesAndProductsSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "Description",
                value: "Các loại rau, củ, quả tươi sạch được canh tác theo phương pháp hữu cơ đạt chuẩn VietGAP, không sử dụng thuốc trừ sâu hóa học hay chất kích thích tăng trưởng, đảm bảo an toàn tuyệt đối cho sức khỏe gia đình.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "Description",
                value: "Trái cây nội địa và nhập khẩu cao cấp được tuyển chọn khắt khe từng trái, đảm bảo độ tươi ngon tự nhiên, giàu vitamin, khoáng chất và hương vị thơm ngon đặc trưng.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "Description",
                value: "Thịt heo, thịt bò và các loại hải sản tươi sống được nuôi trồng theo công nghệ sinh học sạch, kiểm định nghiêm ngặt từ khâu nguyên liệu đến thành phẩm đông lạnh và tươi.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4,
                column: "Description",
                value: "Các dòng sản phẩm gạo lứt, gạo nương, các loại hạt dinh dưỡng và đồ khô cung cấp nguồn năng lượng lành mạnh, giàu chất xơ và phù hợp cho người ăn kiêng, thực dưỡng.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 5,
                column: "Description",
                value: "Đồ uống giải khát tự nhiên từ trái cây tươi nguyên chất không pha đường hóa học kết hợp các loại trà thảo mộc thiên nhiên giúp thanh lọc cơ thể và thư giãn tinh thần.");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "Description" },
                values: new object[,]
                {
                    { 6, "Đồ uống có gas & Nước khoáng", "Các dòng nước giải khát có gas thương hiệu lớn và nước khoáng thiên nhiên đóng chai giàu khoáng chất giúp giải nhiệt nhanh chóng trong những ngày nắng nóng." },
                    { 7, "Sữa & Sản phẩm từ sữa", "Sữa tươi thanh trùng, sữa chua ăn, sữa chua uống và các sản phẩm từ phô mai nguyên chất bổ sung canxi tối ưu, hỗ trợ phát triển chiều cao và tăng cường hệ miễn dịch." },
                    { 8, "Bánh kẹo & Đồ ăn vặt", "Thiên đường bánh quy, kẹo dẻo trái cây, snack cao cấp nhập khẩu từ các thương hiệu uy tín, mang đến hương vị thơm ngon hấp dẫn cho những giờ phút giải trí." },
                    { 9, "Mì gói & Thực phẩm ăn liền", "Các loại mì ăn liền cao cấp, phở khô, hủ tiếu và cháo dinh dưỡng đóng gói tiện lợi, đáp ứng nhanh chóng bữa ăn nhanh gọn nhưng vẫn đảm bảo dinh dưỡng." },
                    { 10, "Gia vị & Dầu ăn cao cấp", "Hệ thống gia vị hoàn chỉnh bao gồm nước mắm truyền thống, hạt nêm hữu cơ, dầu ăn thực vật tinh luyện giúp nâng tầm hương vị cho mọi bữa cơm gia đình." },
                    { 11, "Thực phẩm đông lạnh tiện lợi", "Các món ăn chế biến sẵn đông lạnh như chả giò, xúc xích, sủi cảo, cá viên giúp tiết kiệm tối đa thời gian nấu nướng cho người bận rộn." },
                    { 12, "Đồ dùng chăm sóc cá nhân", "Sản phẩm dầu gội, sữa tắm, kem đánh răng và xà phòng đến từ các thương hiệu uy tín toàn cầu, an toàn và dịu nhẹ cho làn da nhạy cảm." },
                    { 13, "Hóa chất & Tẩy rửa gia dụng", "Nước giặt, nước xả vải, nước rửa chén và dung dịch vệ sinh nhà cửa đậm đà hương thơm, đánh bay vết bẩn cứng đầu và bảo vệ không gian sống." },
                    { 14, "Đồ dùng gia đình thiết yếu", "Các sản phẩm nhựa gia dụng cao cấp, màng bọc thực phẩm, giấy vệ sinh, khăn giấy rút phục vụ sinh hoạt hàng ngày với độ bền cao và an toàn." },
                    { 15, "Sản phẩm dinh dưỡng chuyên biệt", "Các dòng thực phẩm bổ sung vitamin tổng hợp, sữa bột dinh dưỡng cho người cao tuổi và người tập luyện thể hình, nâng cao sức khỏe toàn diện." }
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                column: "ProductName",
                value: "Táo xanh VN cao cấp (1kg)");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4,
                column: "ProductName",
                value: "Chuối tiêu hữu cơ xuất khẩu (Nải)");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 6,
                column: "ProductName",
                value: "Phi lê cá hồi Na Uy tươi (300g)");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 9,
                column: "ProductName",
                value: "Nước ép táo nguyên chất đậm đặc (500ml)");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 10,
                column: "ProductName",
                value: "Trà thảo mộc hoa cúc mật ong tự nhiên (Hộp)");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Barcode", "CategoryId", "Price", "ProductName", "StockQuantity" },
                values: new object[,]
                {
                    { 11, "BC011", 6, 96000m, "Nước khoáng thiên nhiên không gas (Thùng 24 chai)", 40 },
                    { 12, "BC012", 7, 42000m, "Sữa tươi thanh trùng 100% nguyên chất (900ml)", 55 },
                    { 13, "BC013", 8, 135000m, "Bánh quy bơ thượng hạng cao cấp (Hộp thiếc)", 25 },
                    { 14, "BC014", 9, 22000m, "Mì tôm chua cay hảo hạng (Lốc 5 gói)", 100 },
                    { 15, "BC015", 10, 58000m, "Dầu ăn thực vật cao cấp tinh luyện (Lít)", 70 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "Description",
                value: "Rau, củ, quả tươi sạch");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "Description",
                value: "Trái cây các loại");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "Description",
                value: "Thịt heo, cá hồi tươi sống");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4,
                column: "Description",
                value: "Gạo lứt, hạt khô");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 5,
                column: "Description",
                value: "Đồ uống tự nhiên");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                column: "ProductName",
                value: "Táo xanh VN (1kg)");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4,
                column: "ProductName",
                value: "Chuối tiêu hữu cơ (Nải)");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 6,
                column: "ProductName",
                value: "Phi lê cá hồi phi lê (300g)");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 9,
                column: "ProductName",
                value: "Nước ép táo nguyên chất (500ml)");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 10,
                column: "ProductName",
                value: "Trà thảo mộc hoa cúc mật ong (Hộp)");
        }
    }
}
