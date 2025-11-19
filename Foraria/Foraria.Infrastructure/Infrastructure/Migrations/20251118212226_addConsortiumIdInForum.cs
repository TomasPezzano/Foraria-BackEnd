using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class addConsortiumIdInForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsortiumId",
                table: "forum",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_forum_ConsortiumId",
                table: "forum",
                column: "ConsortiumId");

            migrationBuilder.AddForeignKey(
                name: "FK_forum_consortium_ConsortiumId",
                table: "forum",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_forum_consortium_ConsortiumId",
                table: "forum");

            migrationBuilder.DropIndex(
                name: "IX_forum_ConsortiumId",
                table: "forum");

            migrationBuilder.DropColumn(
                name: "ConsortiumId",
                table: "forum");
        }
    }
}
