using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheeseAndThankYou.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductImageToPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Products",
                newName: "Photo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Photo",
                table: "Products",
                newName: "Image");
        }
    }
}
