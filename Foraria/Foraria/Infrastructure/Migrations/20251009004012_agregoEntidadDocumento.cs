using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class agregoEntidadDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "userDocument",
                newName: "Category");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "userDocument",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "userDocument");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "userDocument",
                newName: "Type");
        }
    }
}
