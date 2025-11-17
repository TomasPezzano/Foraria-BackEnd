using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class changesUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdministratorId",
                table: "consortium",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_consortium_AdministratorId",
                table: "consortium",
                column: "AdministratorId");

            migrationBuilder.AddForeignKey(
                name: "FK_consortium_user_AdministratorId",
                table: "consortium",
                column: "AdministratorId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_consortium_user_AdministratorId",
                table: "consortium");

            migrationBuilder.DropIndex(
                name: "IX_consortium_AdministratorId",
                table: "consortium");

            migrationBuilder.DropColumn(
                name: "AdministratorId",
                table: "consortium");
        }
    }
}
