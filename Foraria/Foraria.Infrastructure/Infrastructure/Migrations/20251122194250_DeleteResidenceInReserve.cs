using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class DeleteResidenceInReserve : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reserves_residence_Residence_id",
                table: "reserves");

            migrationBuilder.DropIndex(
                name: "IX_reserves_Residence_id",
                table: "reserves");

            migrationBuilder.DropColumn(
                name: "Residence_id",
                table: "reserves");

            migrationBuilder.AddColumn<int>(
                name: "ResidenceId",
                table: "reserves",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reserves_ResidenceId",
                table: "reserves",
                column: "ResidenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_reserves_residence_ResidenceId",
                table: "reserves",
                column: "ResidenceId",
                principalTable: "residence",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reserves_residence_ResidenceId",
                table: "reserves");

            migrationBuilder.DropIndex(
                name: "IX_reserves_ResidenceId",
                table: "reserves");

            migrationBuilder.DropColumn(
                name: "ResidenceId",
                table: "reserves");

            migrationBuilder.AddColumn<int>(
                name: "Residence_id",
                table: "reserves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_reserves_Residence_id",
                table: "reserves",
                column: "Residence_id");

            migrationBuilder.AddForeignKey(
                name: "FK_reserves_residence_Residence_id",
                table: "reserves",
                column: "Residence_id",
                principalTable: "residence",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
