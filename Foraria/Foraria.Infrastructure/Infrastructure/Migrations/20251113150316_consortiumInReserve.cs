using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class consortiumInReserve : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsortiumId",
                table: "reserves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_reserves_ConsortiumId",
                table: "reserves",
                column: "ConsortiumId");

            migrationBuilder.AddForeignKey(
                name: "FK_reserves_consortium_ConsortiumId",
                table: "reserves",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reserves_consortium_ConsortiumId",
                table: "reserves");

            migrationBuilder.DropIndex(
                name: "IX_reserves_ConsortiumId",
                table: "reserves");

            migrationBuilder.DropColumn(
                name: "ConsortiumId",
                table: "reserves");
        }
    }
}
