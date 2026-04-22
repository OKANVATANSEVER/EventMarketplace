using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventMarketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountPreferencesAndOrganizerOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "AdvertisementSlots",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserCategoryPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WantsEmail = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    WantsSms = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    UserAppId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCategoryPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCategoryPreferences_AspNetUsers_UserAppId",
                        column: x => x.UserAppId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserCategoryPreferences_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryPreferences_CategoryId",
                table: "UserCategoryPreferences",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryPreferences_UserAppId",
                table: "UserCategoryPreferences",
                column: "UserAppId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryPreferences_UserId_CategoryId",
                table: "UserCategoryPreferences",
                columns: new[] { "UserId", "CategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCategoryPreferences");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AdvertisementSlots");
        }
    }
}
