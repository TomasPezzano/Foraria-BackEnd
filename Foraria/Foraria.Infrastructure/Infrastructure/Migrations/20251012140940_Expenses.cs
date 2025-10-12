using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class Expenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsortiumId",
                table: "residence",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "poll",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "poll",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_residence_ConsortiumId",
                table: "residence",
                column: "ConsortiumId");

            migrationBuilder.AddForeignKey(
                name: "FK_residence_consortium_ConsortiumId",
                table: "residence",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_residence_consortium_ConsortiumId",
                table: "residence");

            migrationBuilder.DropIndex(
                name: "IX_residence_ConsortiumId",
                table: "residence");

            migrationBuilder.DropColumn(
                name: "ConsortiumId",
                table: "residence");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "poll");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "poll");
        }
    }
}
