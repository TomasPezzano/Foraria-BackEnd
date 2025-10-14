using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class ReservasDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "reserves",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ResidenceId",
                table: "claim",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_claim_ResidenceId",
                table: "claim",
                column: "ResidenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_residence_ResidenceId",
                table: "claim",
                column: "ResidenceId",
                principalTable: "residence",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_claim_residence_ResidenceId",
                table: "claim");

            migrationBuilder.DropIndex(
                name: "IX_claim_ResidenceId",
                table: "claim");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "reserves");

            migrationBuilder.DropColumn(
                name: "ResidenceId",
                table: "claim");
        }
    }
}
