using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventMarketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixStaticSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f8456f2d-ea47-492c-a892-5b6a44c917f9",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEDq2owkK3iyjkqsnKRslKTQwOoej062hYwQsuiC+o18fqpG1y2grENKeMyMdL/vilw==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f8456f2d-ea47-492c-a892-5b6a44c917f9",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENo5td2mavorh5Ir/iVIrqRI2mpa0xT7KZStA3aIcGtZOwh0A6kW+/f2lLqcv1hG+Q==");
        }
    }
}
