using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventMarketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignRequirements_EventsAuthSeedSwagger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "d6f2d3fd-37be-4715-b4a0-1dfa07749026", null, "Organizer", "ORGANIZER" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("5ef0db25-4218-42ff-b42f-41a72f16d1cd"), "Live concerts and music performances", "Concert" },
                    { new Guid("6116a3e5-d4ca-40c7-bf71-74d0ba034670"), "Stage plays and theatre shows", "Theatre" },
                    { new Guid("89a8c1eb-c3ca-4cab-8953-675f7ec44ad7"), "Festivals and large public events", "Festival" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "d6f2d3fd-37be-4715-b4a0-1dfa07749026", "f8456f2d-ea47-492c-a892-5b6a44c917f9" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "d6f2d3fd-37be-4715-b4a0-1dfa07749026", "f8456f2d-ea47-492c-a892-5b6a44c917f9" });

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5ef0db25-4218-42ff-b42f-41a72f16d1cd"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6116a3e5-d4ca-40c7-bf71-74d0ba034670"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("89a8c1eb-c3ca-4cab-8953-675f7ec44ad7"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d6f2d3fd-37be-4715-b4a0-1dfa07749026");
        }
    }
}
