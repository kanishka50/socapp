using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CozyComfort.Manufacturer.API.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CompanyName", "CreatedAt", "CreatedBy", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "Phone", "Role", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 3, "System Integration", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "distributor-api@cozycomfort.com", "Distributor", true, "API Service", "$2a$11$XYZ123...", null, 6, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
