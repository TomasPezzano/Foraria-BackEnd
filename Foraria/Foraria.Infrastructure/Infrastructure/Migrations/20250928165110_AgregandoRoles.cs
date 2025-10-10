using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class AgregandoRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rol_id",
                table: "user",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "rolId",
                table: "user",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "rol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_rol_rolId",
                table: "user");

            migrationBuilder.DropTable(
                name: "rol");

            migrationBuilder.DropIndex(
                name: "IX_user_rolId",
                table: "user");

            migrationBuilder.DropColumn(
                name: "Rol_id",
                table: "user");

            migrationBuilder.DropColumn(
                name: "rolId",
                table: "user");
        }
    }
}
