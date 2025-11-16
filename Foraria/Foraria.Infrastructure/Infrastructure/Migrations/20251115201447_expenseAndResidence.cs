using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class expenseAndResidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpenseResidence",
                columns: table => new
                {
                    ExpenseId = table.Column<int>(type: "int", nullable: false),
                    ResidenceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseResidence", x => new { x.ExpenseId, x.ResidenceId });
                    table.ForeignKey(
                        name: "FK_ExpenseResidence_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseResidence_residence_ResidenceId",
                        column: x => x.ResidenceId,
                        principalTable: "residence",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseResidence_ResidenceId",
                table: "ExpenseResidence",
                column: "ResidenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpenseResidence");
        }
    }
}
