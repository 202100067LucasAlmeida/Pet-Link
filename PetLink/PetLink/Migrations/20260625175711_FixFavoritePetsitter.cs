using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetLink.Migrations
{
    /// <inheritdoc />
    public partial class FixFavoritePetsitter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoritePetsitters_Users_PetsitterId",
                table: "FavoritePetsitters");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoritePetsitters_Petsitters_PetsitterId",
                table: "FavoritePetsitters",
                column: "PetsitterId",
                principalTable: "Petsitters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoritePetsitters_Petsitters_PetsitterId",
                table: "FavoritePetsitters");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoritePetsitters_Users_PetsitterId",
                table: "FavoritePetsitters",
                column: "PetsitterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
