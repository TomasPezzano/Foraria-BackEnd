using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class ExpenseDetailByResidenceYModificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_residence_ResidenceId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_Expenses_ExpenseId",
                table: "payment");

            migrationBuilder.DropTable(
                name: "expenseDetail");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ResidenceId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ResidenceId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "ExpenseId",
                table: "payment",
                newName: "ExpenseDetailByResidenceId");

            migrationBuilder.RenameIndex(
                name: "IX_payment_ExpenseId",
                table: "payment",
                newName: "IX_payment_ExpenseDetailByResidenceId");

           

            migrationBuilder.AddColumn<double>(
                name: "Coeficient",
                table: "residence",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ExpenseId",
                table: "invoice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ExpenseDetailByResidences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalAmount = table.Column<double>(type: "float", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpenseId = table.Column<int>(type: "int", nullable: false),
                    ResidenceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseDetailByResidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseDetailByResidences_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExpenseDetailByResidences_residence_ResidenceId",
                        column: x => x.ResidenceId,
                        principalTable: "residence",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_invoice_ExpenseId",
                table: "invoice",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetailByResidences_ExpenseId",
                table: "ExpenseDetailByResidences",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetailByResidences_ResidenceId",
                table: "ExpenseDetailByResidences",
                column: "ResidenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_Expenses_ExpenseId",
                table: "invoice",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_ExpenseDetailByResidences_ExpenseDetailByResidenceId",
                table: "payment",
                column: "ExpenseDetailByResidenceId",
                principalTable: "ExpenseDetailByResidences",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_Expenses_ExpenseId",
                table: "invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_ExpenseDetailByResidences_ExpenseDetailByResidenceId",
                table: "payment");

            migrationBuilder.DropTable(
                name: "ExpenseDetailByResidences");

            migrationBuilder.DropIndex(
                name: "IX_invoice_ExpenseId",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "HasPermission",
                table: "user");

            migrationBuilder.DropColumn(
                name: "Coeficient",
                table: "residence");

            migrationBuilder.DropColumn(
                name: "ExpenseId",
                table: "invoice");

            migrationBuilder.RenameColumn(
                name: "ExpenseDetailByResidenceId",
                table: "payment",
                newName: "ExpenseId");

            migrationBuilder.RenameIndex(
                name: "IX_payment_ExpenseDetailByResidenceId",
                table: "payment",
                newName: "IX_payment_ExpenseId");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ResidenceId",
                table: "Expenses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "expenseDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpenseId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenseDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_expenseDetail_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ResidenceId",
                table: "Expenses",
                column: "ResidenceId");

            migrationBuilder.CreateIndex(
                name: "IX_expenseDetail_ExpenseId",
                table: "expenseDetail",
                column: "ExpenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_residence_ResidenceId",
                table: "Expenses",
                column: "ResidenceId",
                principalTable: "residence",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_Expenses_ExpenseId",
                table: "payment",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id");
        }
    }
}
