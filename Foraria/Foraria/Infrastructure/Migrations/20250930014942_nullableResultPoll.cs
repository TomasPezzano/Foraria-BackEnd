using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class nullableResultPoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_poll_ResultPoll_id",
                table: "poll");

            migrationBuilder.AlterColumn<int>(
                name: "ResultPoll_id",
                table: "poll",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_poll_ResultPoll_id",
                table: "poll",
                column: "ResultPoll_id",
                unique: true,
                filter: "[ResultPoll_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_poll_ResultPoll_id",
                table: "poll");

            migrationBuilder.AlterColumn<int>(
                name: "ResultPoll_id",
                table: "poll",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_poll_ResultPoll_id",
                table: "poll",
                column: "ResultPoll_id",
                unique: true);
        }
    }
}
