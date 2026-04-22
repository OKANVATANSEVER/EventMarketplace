using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventMarketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPopularVenuesAndSlotTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveSinceUtc",
                table: "FeaturedListings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "FeaturedListings",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusChangedAtUtc",
                table: "FeaturedListings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TotalActiveDurationSeconds",
                table: "FeaturedListings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveSinceUtc",
                table: "AdvertisementSlots",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "AdvertisementSlots",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusChangedAtUtc",
                table: "AdvertisementSlots",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TotalActiveDurationSeconds",
                table: "AdvertisementSlots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "PopularVenues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tag = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PopularVenues", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PopularVenues");

            migrationBuilder.DropColumn(
                name: "ActiveSinceUtc",
                table: "FeaturedListings");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "FeaturedListings");

            migrationBuilder.DropColumn(
                name: "LastStatusChangedAtUtc",
                table: "FeaturedListings");

            migrationBuilder.DropColumn(
                name: "TotalActiveDurationSeconds",
                table: "FeaturedListings");

            migrationBuilder.DropColumn(
                name: "ActiveSinceUtc",
                table: "AdvertisementSlots");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "AdvertisementSlots");

            migrationBuilder.DropColumn(
                name: "LastStatusChangedAtUtc",
                table: "AdvertisementSlots");

            migrationBuilder.DropColumn(
                name: "TotalActiveDurationSeconds",
                table: "AdvertisementSlots");
        }
    }
}
