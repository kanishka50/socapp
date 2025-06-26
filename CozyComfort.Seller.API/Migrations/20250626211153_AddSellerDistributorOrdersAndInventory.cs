using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CozyComfort.Seller.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerDistributorOrdersAndInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "SellerProducts");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "CustomerOrders");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "CustomerOrderItems");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_ProductId",
                table: "CustomerOrderItems",
                newName: "IX_CustomerOrderItems_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderId",
                table: "CustomerOrderItems",
                newName: "IX_CustomerOrderItems_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SellerProducts",
                table: "SellerProducts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerOrders",
                table: "CustomerOrders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerOrderItems",
                table: "CustomerOrderItems",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SellerDistributorOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DistributorId = table.Column<int>(type: "int", nullable: false),
                    DistributorOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerDistributorOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SellerInventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerInventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerInventoryTransactions_SellerProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "SellerProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SellerDistributorOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerDistributorOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerDistributorOrderItems_SellerDistributorOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "SellerDistributorOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SellerDistributorOrderItems_SellerProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "SellerProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "SellerProducts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "SellerProducts",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$RZvBwJL7e8OadjLPNgW7x.Km7NkkdVNoLSbEVmNGbxSu.Pnw8dXLa" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$5VBm7OKqiM6XGQqATcQTb.SZeh7mr8fVyVuVC.M8FQ8g1jLOSGGBu" });

            migrationBuilder.CreateIndex(
                name: "IX_SellerDistributorOrderItems_OrderId",
                table: "SellerDistributorOrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerDistributorOrderItems_ProductId",
                table: "SellerDistributorOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerDistributorOrders_OrderNumber",
                table: "SellerDistributorOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerInventoryTransactions_ProductId_TransactionDate",
                table: "SellerInventoryTransactions",
                columns: new[] { "ProductId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SellerInventoryTransactions_TransactionDate",
                table: "SellerInventoryTransactions",
                column: "TransactionDate");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerOrderItems_CustomerOrders_OrderId",
                table: "CustomerOrderItems",
                column: "OrderId",
                principalTable: "CustomerOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerOrderItems_SellerProducts_ProductId",
                table: "CustomerOrderItems",
                column: "ProductId",
                principalTable: "SellerProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerOrderItems_CustomerOrders_OrderId",
                table: "CustomerOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerOrderItems_SellerProducts_ProductId",
                table: "CustomerOrderItems");

            migrationBuilder.DropTable(
                name: "SellerDistributorOrderItems");

            migrationBuilder.DropTable(
                name: "SellerInventoryTransactions");

            migrationBuilder.DropTable(
                name: "SellerDistributorOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SellerProducts",
                table: "SellerProducts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerOrders",
                table: "CustomerOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerOrderItems",
                table: "CustomerOrderItems");

            migrationBuilder.RenameTable(
                name: "SellerProducts",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "CustomerOrders",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "CustomerOrderItems",
                newName: "OrderItems");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerOrderItems_ProductId",
                table: "OrderItems",
                newName: "IX_OrderItems_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerOrderItems_OrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 24, 5, 36, 35, 391, DateTimeKind.Utc).AddTicks(7002));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 24, 5, 36, 35, 391, DateTimeKind.Utc).AddTicks(7014));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 6, 24, 5, 36, 35, 197, DateTimeKind.Utc).AddTicks(4471), "$2a$11$0NGK9YqKL/Klw1xBvq89cOChPHLy3g4MzbFS8VBWrkOopNgnaRdn." });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 6, 24, 5, 36, 35, 390, DateTimeKind.Utc).AddTicks(7973), "$2a$11$IWdMv4yl.Z7LqzF69Dgy8u1BWKvpsdfQlmwFdB3M11ppIdhDB42fq" });

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
