using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniSupermarket.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoryDescriptionShort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "Description",
                value: "Rau củ quả tươi sạch đạt chuẩn VietGAP, an toàn cho sức khỏe.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "Description",
                value: "Trái cây nội địa và nhập khẩu tươi ngon, giàu vitamin.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "Description",
                value: "Thịt và hải sản tươi sống được kiểm định chất lượng nghiêm ngặt.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4,
                column: "Description",
                value: "Các loại gạo lứt, hạt dinh dưỡng tốt cho sức khỏe và ăn kiêng.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 5,
                column: "Description",
                value: "Đồ uống giải khát tự nhiên, trà thảo mộc thanh lọc cơ thể.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 6,
                column: "Description",
                value: "Nước giải khát và nước khoáng thiên nhiên giải nhiệt nhanh chóng.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 7,
                column: "Description",
                value: "Sữa tươi, sữa chua và phô mai bổ sung canxi tối ưu.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 8,
                column: "Description",
                value: "Bánh quy, kẹo dẻo và snack cao cấp từ các thương hiệu uy tín.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 9,
                column: "Description",
                value: "Mì tôm, phở khô và cháo ăn liền tiện lợi cho bữa ăn nhanh.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 10,
                column: "Description",
                value: "Nước mắm, hạt nêm và dầu ăn giúp nâng tầm hương vị bữa cơm.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 11,
                column: "Description",
                value: "Các món ăn chế biến sẵn đông lạnh tiết kiệm thời gian nấu nướng.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 12,
                column: "Description",
                value: "Dầu gội, sữa tắm và sản phẩm vệ sinh an toàn cho da.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 13,
                column: "Description",
                value: "Nước giặt, nước rửa chén và dung dịch vệ sinh nhà cửa sạch sẽ.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 14,
                column: "Description",
                value: "Đồ dùng nhựa, giấy vệ sinh và vật dụng sinh hoạt hàng ngày.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 15,
                column: "Description",
                value: "Thực phẩm bổ sung vitamin và sữa bột nâng cao sức khỏe toàn diện.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 6,
                column: "Description",
                value: "Các dòng nước giải khát có gas thương hiệu lớn và nước khoáng thiên nhiên đóng chai giàu khoáng chất giúp giải nhiệt nhanh chóng trong những ngày nắng nóng.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 7,
                column: "Description",
                value: "Sữa tươi thanh trùng, sữa chua ăn, sữa chua uống và các sản phẩm từ phô mai nguyên chất bổ sung canxi tối ưu, hỗ trợ phát triển chiều cao và tăng cường hệ miễn dịch.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 8,
                column: "Description",
                value: "Thiên đường bánh quy, kẹo dẻo trái cây, snack cao cấp nhập khẩu từ các thương hiệu uy tín, mang đến hương vị thơm ngon hấp dẫn cho những giờ phút giải trí.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 9,
                column: "Description",
                value: "Các loại mì ăn liền cao cấp, phở khô, hủ tiếu và cháo dinh dưỡng đóng gói tiện lợi, đáp ứng nhanh chóng bữa ăn nhanh gọn nhưng vẫn đảm bảo dinh dưỡng.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 10,
                column: "Description",
                value: "Hệ thống gia vị hoàn chỉnh bao gồm nước mắm truyền thống, hạt nêm hữu cơ, dầu ăn thực vật tinh luyện giúp nâng tầm hương vị cho mọi bữa cơm gia đình.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 11,
                column: "Description",
                value: "Các món ăn chế biến sẵn đông lạnh như chả giò, xúc xích, sủi cảo, cá viên giúp tiết kiệm tối đa thời gian nấu nướng cho người bận rộn.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 12,
                column: "Description",
                value: "Sản phẩm dầu gội, sữa tắm, kem đánh răng và xà phòng đến từ các thương hiệu uy tín toàn cầu, an toàn và dịu nhẹ cho làn da nhạy cảm.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 13,
                column: "Description",
                value: "Nước giặt, nước xả vải, nước rửa chén và dung dịch vệ sinh nhà cửa đậm đà hương thơm, đánh bay vết bẩn cứng đầu và bảo vệ không gian sống.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 14,
                column: "Description",
                value: "Các sản phẩm nhựa gia dụng cao cấp, màng bọc thực phẩm, giấy vệ sinh, khăn giấy rút phục vụ sinh hoạt hàng ngày với độ bền cao và an toàn.");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 15,
                column: "Description",
                value: "Các dòng thực phẩm bổ sung vitamin tổng hợp, sữa bột dinh dưỡng cho người cao tuổi và người tập luyện thể hình, nâng cao sức khỏe toàn diện.");
        }
    }
}
