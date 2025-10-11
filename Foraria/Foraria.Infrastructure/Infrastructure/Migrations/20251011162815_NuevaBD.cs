using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class NuevaBD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_supplier_consortium_ConsortiumId",
                table: "supplier");

            migrationBuilder.DropIndex(
                name: "IX_supplier_ConsortiumId",
                table: "supplier");

            migrationBuilder.DropColumn(
                name: "ConsortiumId",
                table: "supplier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsortiumId",
                table: "supplier",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_ConsortiumId",
                table: "supplier",
                column: "ConsortiumId");

            migrationBuilder.AddForeignKey(
                name: "FK_supplier_consortium_ConsortiumId",
                table: "supplier",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id");
        }
    }
}
