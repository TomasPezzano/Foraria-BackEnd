using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class invoiceInResidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResidenceId",
                table: "invoice",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_ResidenceId",
                table: "invoice",
                column: "ResidenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_residence_ResidenceId",
                table: "invoice",
                column: "ResidenceId",
                principalTable: "residence",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_residence_ResidenceId",
                table: "invoice");

            migrationBuilder.DropIndex(
                name: "IX_invoice_ResidenceId",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "ResidenceId",
                table: "invoice");
        }
    }
}
