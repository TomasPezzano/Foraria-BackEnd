using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class ModificoError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userDocument_residence_Residence_id",
                table: "userDocument");

            migrationBuilder.RenameColumn(
                name: "Residence_id",
                table: "userDocument",
                newName: "Consortium_id");

            migrationBuilder.RenameIndex(
                name: "IX_userDocument_Residence_id",
                table: "userDocument",
                newName: "IX_userDocument_Consortium_id");

            migrationBuilder.AddForeignKey(
                name: "FK_userDocument_consortium_Consortium_id",
                table: "userDocument",
                column: "Consortium_id",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userDocument_consortium_Consortium_id",
                table: "userDocument");

            migrationBuilder.RenameColumn(
                name: "Consortium_id",
                table: "userDocument",
                newName: "Residence_id");

            migrationBuilder.RenameIndex(
                name: "IX_userDocument_Consortium_id",
                table: "userDocument",
                newName: "IX_userDocument_Residence_id");

            migrationBuilder.AddForeignKey(
                name: "FK_userDocument_residence_Residence_id",
                table: "userDocument",
                column: "Residence_id",
                principalTable: "residence",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
