using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class UserDocumentAndPoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "userDocument",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "poll",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "poll",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_poll_ApprovedByUserId",
                table: "poll",
                column: "ApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_poll_user_ApprovedByUserId",
                table: "poll",
                column: "ApprovedByUserId",
                principalTable: "user",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_poll_user_ApprovedByUserId",
                table: "poll");

            migrationBuilder.DropIndex(
                name: "IX_poll_ApprovedByUserId",
                table: "poll");

            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "userDocument");

            migrationBuilder.DropColumn(
                name: "HasPermission",
                table: "user");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "poll");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "poll");
        }
    }
}
