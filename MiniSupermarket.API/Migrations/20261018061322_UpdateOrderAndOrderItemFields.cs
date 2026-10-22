using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniSupermarket.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderAndOrderItemFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountReceived",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CashierId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CashierUsername",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ChangeAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RewardPoints",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalItems",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotal",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 1,
                column: "LineTotal",
                value: 75000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 2,
                column: "LineTotal",
                value: 95000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 3,
                column: "LineTotal",
                value: 65000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 4,
                column: "LineTotal",
                value: 120000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 5,
                column: "LineTotal",
                value: 85000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 6,
                column: "LineTotal",
                value: 90000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 7,
                column: "LineTotal",
                value: 95000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 8,
                column: "LineTotal",
                value: 60000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 9,
                column: "LineTotal",
                value: 130000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 10,
                column: "LineTotal",
                value: 40000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 11,
                column: "LineTotal",
                value: 25000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 12,
                column: "LineTotal",
                value: 30000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 13,
                column: "LineTotal",
                value: 44000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 14,
                column: "LineTotal",
                value: 25000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 15,
                column: "LineTotal",
                value: 96000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 16,
                column: "LineTotal",
                value: 35000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 17,
                column: "LineTotal",
                value: 40000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 18,
                column: "LineTotal",
                value: 135000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 19,
                column: "LineTotal",
                value: 25000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 20,
                column: "LineTotal",
                value: 95000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 21,
                column: "LineTotal",
                value: 50000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 22,
                column: "LineTotal",
                value: 96000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 23,
                column: "LineTotal",
                value: 135000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 24,
                column: "LineTotal",
                value: 88000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 25,
                column: "LineTotal",
                value: 120000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: 26,
                column: "LineTotal",
                value: 20000m);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 1,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 80000m, 3, "cashier01", 5000m, 0m, 75000m, 7, 3 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 2,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 100000m, 4, "cashier02", 5000m, 0m, 95000m, 9, 1 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 3,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 65000m, 5, "cashier03", 0m, 0m, 65000m, 6, 1 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 4,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 120000m, 6, "cashier04", 0m, 0m, 120000m, 12, 1 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 5,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 85000m, 7, "cashier05", 0m, 0m, 85000m, 8, 1 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 6,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 90000m, 8, "cashier06", 0m, 0m, 90000m, 9, 2 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 7,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 100000m, 3, "cashier01", 5000m, 0m, 95000m, 9, 1 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 8,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 60000m, 4, "cashier02", 0m, 0m, 60000m, 6, 2 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 9,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 130000m, 5, "cashier03", 0m, 0m, 130000m, 13, 2 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 10,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalAmount", "TotalItems" },
                values: new object[] { 100000m, 6, "cashier04", 5000m, 0m, 95000m, 9, 95000m, 3 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 11,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalAmount", "TotalItems" },
                values: new object[] { 69000m, 7, "cashier05", 0m, 0m, 69000m, 6, 69000m, 3 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 12,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalAmount", "TotalItems" },
                values: new object[] { 100000m, 8, "cashier06", 4000m, 0m, 96000m, 9, 96000m, 1 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 13,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalAmount", "TotalItems" },
                values: new object[] { 80000m, 3, "cashier01", 5000m, 0m, 75000m, 7, 75000m, 2 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 14,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalAmount", "TotalItems" },
                values: new object[] { 160000m, 4, "cashier02", 0m, 0m, 160000m, 16, 160000m, 2 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 15,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 100000m, 5, "cashier03", 5000m, 0m, 95000m, 9, 1 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 16,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 50000m, 6, "cashier04", 0m, 0m, 50000m, 5, 2 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 17,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 96000m, 7, "cashier05", 0m, 0m, 96000m, 9, 1 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 18,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 140000m, 8, "cashier06", 5000m, 0m, 135000m, 13, 1 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 19,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 90000m, 3, "cashier01", 2000m, 0m, 88000m, 8, 4 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 20,
                columns: new[] { "AmountReceived", "CashierId", "CashierUsername", "ChangeAmount", "DiscountAmount", "FinalAmount", "RewardPoints", "TotalItems" },
                values: new object[] { 140000m, 4, "cashier02", 0m, 0m, 140000m, 14, 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountReceived",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CashierId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CashierUsername",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ChangeAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RewardPoints",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TotalItems",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LineTotal",
                table: "OrderItems");

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 10,
                column: "TotalAmount",
                value: 105000m);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 11,
                column: "TotalAmount",
                value: 70000m);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 12,
                column: "TotalAmount",
                value: 110000m);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 13,
                column: "TotalAmount",
                value: 80000m);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 14,
                column: "TotalAmount",
                value: 155000m);
        }
    }
}
