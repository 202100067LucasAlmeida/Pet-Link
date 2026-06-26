using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetLink.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCommentFromReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "Comment",
        table: "Reviews");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "Comment",
        table: "Reviews",
        type: "nvarchar(500)",
        maxLength: 500,
        nullable: false,
        defaultValue: "");
}
    }
}
