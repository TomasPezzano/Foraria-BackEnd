using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class addRelationPollConsortium : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsortiumId",
                table: "poll",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_poll_ConsortiumId",
                table: "poll",
                column: "ConsortiumId");

            migrationBuilder.AddForeignKey(
                name: "FK_poll_consortium_ConsortiumId",
                table: "poll",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_poll_consortium_ConsortiumId",
                table: "poll");

            migrationBuilder.DropIndex(
                name: "IX_poll_ConsortiumId",
                table: "poll");

            migrationBuilder.DropColumn(
                name: "ConsortiumId",
                table: "poll");
        }
    }
}
