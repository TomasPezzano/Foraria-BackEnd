using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class claimYConsortium : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsortiumId",
                table: "claim",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_claim_ConsortiumId",
                table: "claim",
                column: "ConsortiumId");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_consortium_ConsortiumId",
                table: "claim",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_claim_consortium_ConsortiumId",
                table: "claim");

            migrationBuilder.DropIndex(
                name: "IX_claim_ConsortiumId",
                table: "claim");

            migrationBuilder.DropColumn(
                name: "ConsortiumId",
                table: "claim");
        }
    }
}
