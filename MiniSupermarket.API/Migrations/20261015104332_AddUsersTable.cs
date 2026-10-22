using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiniSupermarket.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "FullName", "IsActive", "Password", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "Quản trị viên 01", true, "123456", "Admin", "admin01" },
                    { 2, "Quản trị viên 02", true, "123456", "Admin", "admin02" },
                    { 3, "Thu ngân 01", true, "123456", "Cashier", "cashier01" },
                    { 4, "Thu ngân 02", true, "123456", "Cashier", "cashier02" },
                    { 5, "Thu ngân 03", true, "123456", "Cashier", "cashier03" },
                    { 6, "Thu ngân 04", true, "123456", "Cashier", "cashier04" },
                    { 7, "Thu ngân 05", true, "123456", "Cashier", "cashier05" },
                    { 8, "Thu ngân 06", true, "123456", "Cashier", "cashier06" },
                    { 9, "Thủ kho 01", true, "123456", "Warehouse", "ware01" },
                    { 10, "Thủ kho 02", true, "123456", "Warehouse", "ware02" },
                    { 11, "Thủ kho 03", true, "123456", "Warehouse", "ware03" },
                    { 12, "Thủ kho 04", true, "123456", "Warehouse", "ware04" },
                    { 13, "Thủ kho 05", true, "123456", "Warehouse", "ware05" },
                    { 14, "Quản trị dự phòng", true, "123456", "Admin", "admin_backup" },
                    { 15, "Giám sát", true, "123456", "Admin", "supervisor" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
