using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CozyComfort.Manufacturer.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManufacturerOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DistributorId = table.Column<int>(type: "int", nullable: false),
                    DistributorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DistributorOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturerOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrentStock = table.Column<int>(type: "int", nullable: false),
                    ReservedStock = table.Column<int>(type: "int", nullable: false),
                    ProductionCapacityPerDay = table.Column<int>(type: "int", nullable: false),
                    ManufacturingCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinStockLevel = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturerOrderItems",
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
                    table.PrimaryKey("PK_ManufacturerOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturerOrderItems_ManufacturerOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ManufacturerOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManufacturerOrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantityRequired = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CostPerUnit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductMaterials_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "CurrentStock", "Description", "ImageUrl", "IsActive", "LeadTimeDays", "ManufacturingCost", "Material", "MinStockLevel", "Name", "Price", "ProductionCapacityPerDay", "ReservedStock", "SKU", "Size", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Wool Blankets", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 50, "Premium 100% Merino wool blanket, perfect for cold nights", "/images/luxury-wool-queen.jpg", true, 3, 75.00m, "100% Merino Wool", 20, "Luxury Wool Blanket - Queen Size", 199.99m, 10, 0, "LWB-Q-001", "Queen (90x90 inches)", null, null },
                    { 2, "Cotton Blankets", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 75, "Soft and breathable 100% organic cotton blanket", "/images/cotton-comfort-king.jpg", true, 2, 55.00m, "100% Organic Cotton", 25, "Cotton Comfort Blanket - King Size", 149.99m, 15, 5, "CCB-K-001", "King (108x90 inches)", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CompanyName", "CreatedAt", "CreatedBy", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "Phone", "Role", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin@cozycomfort.com", "System", true, "Administrator", "$2a$11$ZGsLwskCPFNMmT8lEV2ELetqSMN5XC1nBR1eEI8FmGN5dQdB2Ibvy", null, 1, null, null },
                    { 2, "Cozy Comfort Manufacturing", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "manufacturer@cozycomfort.com", "John", true, "Manufacturer", "$2a$11$Q3p1VQDLzq.VQDlzq.VQDlzq.VQ3p1VQDLzq.VQDlzq.VQ3p1VQ", null, 2, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId",
                table: "InventoryTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturerOrderItems_OrderId",
                table: "ManufacturerOrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturerOrderItems_ProductId",
                table: "ManufacturerOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturerOrders_OrderNumber",
                table: "ManufacturerOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductMaterials_ProductId",
                table: "ProductMaterials",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "ManufacturerOrderItems");

            migrationBuilder.DropTable(
                name: "ProductMaterials");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ManufacturerOrders");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
