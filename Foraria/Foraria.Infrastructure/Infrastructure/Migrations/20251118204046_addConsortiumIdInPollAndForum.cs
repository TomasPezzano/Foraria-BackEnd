using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class addConsortiumIdInPollAndForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsortiumId",
                table: "thread",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsortiumId",
                table: "Calls",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_thread_ConsortiumId",
                table: "thread",
                column: "ConsortiumId");

            migrationBuilder.CreateIndex(
                name: "IX_Calls_ConsortiumId",
                table: "Calls",
                column: "ConsortiumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Calls_consortium_ConsortiumId",
                table: "Calls",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_thread_consortium_ConsortiumId",
                table: "thread",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calls_consortium_ConsortiumId",
                table: "Calls");

            migrationBuilder.DropForeignKey(
                name: "FK_thread_consortium_ConsortiumId",
                table: "thread");

            migrationBuilder.DropIndex(
                name: "IX_thread_ConsortiumId",
                table: "thread");

            migrationBuilder.DropIndex(
                name: "IX_Calls_ConsortiumId",
                table: "Calls");

            migrationBuilder.DropColumn(
                name: "ConsortiumId",
                table: "thread");

            migrationBuilder.DropColumn(
                name: "ConsortiumId",
                table: "Calls");
        }
    }
}
