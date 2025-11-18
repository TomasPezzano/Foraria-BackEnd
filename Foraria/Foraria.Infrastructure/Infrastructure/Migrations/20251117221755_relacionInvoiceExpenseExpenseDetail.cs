using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class relacionInvoiceExpenseExpenseDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseDetailByResidences_Expenses_ExpenseId",
                table: "ExpenseDetailByResidences");

            migrationBuilder.DropForeignKey(
                name: "FK_invoice_Expenses_ExpenseId",
                table: "invoice");

            migrationBuilder.DropIndex(
                name: "IX_invoice_ExpenseId",
                table: "invoice");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseDetailByResidences_ExpenseId",
                table: "ExpenseDetailByResidences");

            migrationBuilder.DropColumn(
                name: "ExpenseId",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "ExpenseId",
                table: "ExpenseDetailByResidences");

            migrationBuilder.CreateTable(
                name: "ExpenseAndExpenseDetail",
                columns: table => new
                {
                    ExpenseDetailByResidenceId = table.Column<int>(type: "int", nullable: false),
                    ExpenseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseAndExpenseDetail", x => new { x.ExpenseDetailByResidenceId, x.ExpenseId });
                    table.ForeignKey(
                        name: "FK_ExpenseAndExpenseDetail_ExpenseDetailByResidences_ExpenseDetailByResidenceId",
                        column: x => x.ExpenseDetailByResidenceId,
                        principalTable: "ExpenseDetailByResidences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseAndExpenseDetail_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseInvoice",
                columns: table => new
                {
                    ExpenseId = table.Column<int>(type: "int", nullable: false),
                    InvoicesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseInvoice", x => new { x.ExpenseId, x.InvoicesId });
                    table.ForeignKey(
                        name: "FK_ExpenseInvoice_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseInvoice_invoice_InvoicesId",
                        column: x => x.InvoicesId,
                        principalTable: "invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseAndExpenseDetail_ExpenseId",
                table: "ExpenseAndExpenseDetail",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseInvoice_InvoicesId",
                table: "ExpenseInvoice",
                column: "InvoicesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpenseAndExpenseDetail");

            migrationBuilder.DropTable(
                name: "ExpenseInvoice");

            migrationBuilder.AddColumn<int>(
                name: "ExpenseId",
                table: "invoice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExpenseId",
                table: "ExpenseDetailByResidences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_ExpenseId",
                table: "invoice",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetailByResidences_ExpenseId",
                table: "ExpenseDetailByResidences",
                column: "ExpenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseDetailByResidences_Expenses_ExpenseId",
                table: "ExpenseDetailByResidences",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_Expenses_ExpenseId",
                table: "invoice",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
