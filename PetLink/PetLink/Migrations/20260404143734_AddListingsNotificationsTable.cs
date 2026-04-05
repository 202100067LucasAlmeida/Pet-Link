using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetLink.Migrations
{
    /// <inheritdoc />
    public partial class AddListingsNotificationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListingsNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnimalListingId = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingsNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingsNotifications_AnimalListings_AnimalListingId",
                        column: x => x.AnimalListingId,
                        principalTable: "AnimalListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListingsNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListingsNotifications_AnimalListingId",
                table: "ListingsNotifications",
                column: "AnimalListingId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingsNotifications_UserId",
                table: "ListingsNotifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListingsNotifications");
        }
    }
}
