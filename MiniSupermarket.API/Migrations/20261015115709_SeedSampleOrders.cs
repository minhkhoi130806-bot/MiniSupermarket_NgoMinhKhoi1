using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiniSupermarket.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedSampleOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "OrderItemId", "OrderId", "ProductId", "Quantity", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 1, 1, 3, 25000m },
                    { 2, 2, 8, 1, 95000m },
                    { 3, 3, 3, 1, 65000m },
                    { 4, 4, 6, 1, 120000m },
                    { 5, 5, 5, 1, 85000m },
                    { 6, 6, 7, 2, 45000m },
                    { 7, 7, 8, 1, 95000m },
                    { 8, 8, 4, 2, 30000m },
                    { 9, 9, 3, 2, 65000m },
                    { 10, 10, 9, 1, 40000m },
                    { 11, 10, 1, 1, 25000m },
                    { 12, 10, 4, 1, 30000m },
                    { 13, 11, 14, 2, 22000m },
                    { 14, 11, 1, 1, 25000m },
                    { 15, 12, 11, 1, 96000m },
                    { 16, 13, 2, 1, 35000m },
                    { 17, 13, 9, 1, 40000m },
                    { 18, 14, 13, 1, 135000m },
                    { 19, 14, 1, 1, 25000m },
                    { 20, 15, 8, 1, 95000m },
                    { 21, 16, 1, 2, 25000m },
                    { 22, 17, 11, 1, 96000m },
                    { 23, 18, 13, 1, 135000m },
                    { 24, 19, 14, 4, 22000m },
                    { 25, 20, 6, 1, 120000m },
                    { 26, 20, 1, 1, 20000m }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderId", "CreatedAt", "CustomerId", "OrderCode", "PaymentMethod", "Status", "TotalAmount" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 9, 20, 8, 30, 0, 0, DateTimeKind.Unspecified), 1, "HD000001", "Cash", "Completed", 75000m },
                    { 2, new DateTime(2026, 9, 20, 9, 15, 0, 0, DateTimeKind.Unspecified), 2, "HD000002", "Cash", "Completed", 95000m },
                    { 3, new DateTime(2026, 9, 20, 10, 0, 0, 0, DateTimeKind.Unspecified), 3, "HD000003", "Card", "Completed", 65000m },
                    { 4, new DateTime(2026, 9, 21, 8, 45, 0, 0, DateTimeKind.Unspecified), 4, "HD000004", "Cash", "Completed", 120000m },
                    { 5, new DateTime(2026, 9, 21, 11, 20, 0, 0, DateTimeKind.Unspecified), 5, "HD000005", "Card", "Completed", 85000m },
                    { 6, new DateTime(2026, 9, 22, 9, 10, 0, 0, DateTimeKind.Unspecified), 6, "HD000006", "Cash", "Completed", 90000m },
                    { 7, new DateTime(2026, 9, 22, 14, 5, 0, 0, DateTimeKind.Unspecified), 7, "HD000007", "Cash", "Completed", 95000m },
                    { 8, new DateTime(2026, 9, 23, 8, 35, 0, 0, DateTimeKind.Unspecified), 8, "HD000008", "Card", "Completed", 60000m },
                    { 9, new DateTime(2026, 9, 23, 10, 25, 0, 0, DateTimeKind.Unspecified), 9, "HD000009", "Cash", "Completed", 130000m },
                    { 10, new DateTime(2026, 9, 24, 15, 30, 0, 0, DateTimeKind.Unspecified), 10, "HD000010", "Cash", "Completed", 105000m },
                    { 11, new DateTime(2026, 9, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), 11, "HD000011", "Card", "Completed", 70000m },
                    { 12, new DateTime(2026, 9, 25, 13, 15, 0, 0, DateTimeKind.Unspecified), 12, "HD000012", "Cash", "Completed", 110000m },
                    { 13, new DateTime(2026, 9, 26, 10, 40, 0, 0, DateTimeKind.Unspecified), 13, "HD000013", "Cash", "Completed", 80000m },
                    { 14, new DateTime(2026, 9, 26, 16, 20, 0, 0, DateTimeKind.Unspecified), 14, "HD000014", "Card", "Completed", 155000m },
                    { 15, new DateTime(2026, 9, 27, 11, 10, 0, 0, DateTimeKind.Unspecified), 15, "HD000015", "Cash", "Completed", 95000m },
                    { 16, new DateTime(2026, 9, 28, 8, 20, 0, 0, DateTimeKind.Unspecified), null, "HD000016", "Cash", "Completed", 50000m },
                    { 17, new DateTime(2026, 9, 29, 14, 45, 0, 0, DateTimeKind.Unspecified), null, "HD000017", "Card", "Completed", 96000m },
                    { 18, new DateTime(2026, 9, 30, 9, 35, 0, 0, DateTimeKind.Unspecified), 1, "HD000018", "Cash", "Completed", 135000m },
                    { 19, new DateTime(2026, 10, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), 2, "HD000019", "Cash", "Completed", 88000m },
                    { 20, new DateTime(2026, 10, 2, 17, 10, 0, 0, DateTimeKind.Unspecified), 3, "HD000020", "Card", "Completed", 140000m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 20);
        }
    }
}
