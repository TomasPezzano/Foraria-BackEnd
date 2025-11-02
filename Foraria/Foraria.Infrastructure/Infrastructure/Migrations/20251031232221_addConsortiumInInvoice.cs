using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class addConsortiumInInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsortiumId",
                table: "invoice",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_ConsortiumId",
                table: "invoice",
                column: "ConsortiumId");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_consortium_ConsortiumId",
                table: "invoice",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_consortium_ConsortiumId",
                table: "invoice");

            migrationBuilder.DropIndex(
                name: "IX_invoice_ConsortiumId",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "ConsortiumId",
                table: "invoice");
        }
    }
}
