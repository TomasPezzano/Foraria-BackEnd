using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class RolFluentApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_rol_rolId",
                table: "user");

            migrationBuilder.DropIndex(
                name: "IX_user_rolId",
                table: "user");

            migrationBuilder.DropColumn(
                name: "rolId",
                table: "user");

            migrationBuilder.CreateIndex(
                name: "IX_user_Rol_id",
                table: "user",
                column: "Rol_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_rol_Rol_id",
                table: "user",
                column: "Rol_id",
                principalTable: "rol",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_rol_Rol_id",
                table: "user");

            migrationBuilder.DropIndex(
                name: "IX_user_Rol_id",
                table: "user");

            migrationBuilder.AddColumn<int>(
                name: "rolId",
                table: "user",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_user_rolId",
                table: "user",
                column: "rolId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_rol_rolId",
                table: "user",
                column: "rolId",
                principalTable: "rol",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
