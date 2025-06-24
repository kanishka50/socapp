using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CozyComfort.Distributor.API.Migrations
{
    /// <inheritdoc />
    public partial class FixedDistributorDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$QJk7fvgHt5KnrHcE9dGhF.8MJYl4ZzPu1CeQq2AvBnIiYHXM4QNYi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$rQf8Fx8Kz2Wn7vJ4RjPmE.9lK3qS2hF1GxN5pT7dM8aC6bV9eWxYi");
        }
    }
}
