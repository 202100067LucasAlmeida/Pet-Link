using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetLink.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthDocumentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthDocument_AnimalListings_AnimalListingId",
                table: "HealthDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HealthDocument",
                table: "HealthDocument");

            migrationBuilder.RenameTable(
                name: "HealthDocument",
                newName: "HealthDocuments");

            migrationBuilder.RenameIndex(
                name: "IX_HealthDocument_AnimalListingId",
                table: "HealthDocuments",
                newName: "IX_HealthDocuments_AnimalListingId");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "HealthDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "HealthDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerifiedByAdminId",
                table: "HealthDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HealthDocuments",
                table: "HealthDocuments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_HealthDocuments_VerifiedByAdminId",
                table: "HealthDocuments",
                column: "VerifiedByAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthDocuments_AnimalListings_AnimalListingId",
                table: "HealthDocuments",
                column: "AnimalListingId",
                principalTable: "AnimalListings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HealthDocuments_Users_VerifiedByAdminId",
                table: "HealthDocuments",
                column: "VerifiedByAdminId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthDocuments_AnimalListings_AnimalListingId",
                table: "HealthDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_HealthDocuments_Users_VerifiedByAdminId",
                table: "HealthDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HealthDocuments",
                table: "HealthDocuments");

            migrationBuilder.DropIndex(
                name: "IX_HealthDocuments_VerifiedByAdminId",
                table: "HealthDocuments");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "HealthDocuments");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "HealthDocuments");

            migrationBuilder.DropColumn(
                name: "VerifiedByAdminId",
                table: "HealthDocuments");

            migrationBuilder.RenameTable(
                name: "HealthDocuments",
                newName: "HealthDocument");

            migrationBuilder.RenameIndex(
                name: "IX_HealthDocuments_AnimalListingId",
                table: "HealthDocument",
                newName: "IX_HealthDocument_AnimalListingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HealthDocument",
                table: "HealthDocument",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthDocument_AnimalListings_AnimalListingId",
                table: "HealthDocument",
                column: "AnimalListingId",
                principalTable: "AnimalListings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
