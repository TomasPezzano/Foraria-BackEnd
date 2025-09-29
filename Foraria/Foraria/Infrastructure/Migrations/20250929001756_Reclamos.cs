using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class Reclamos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_rol_Rol_id",
                table: "user");

            migrationBuilder.DropTable(
                name: "rol");

            migrationBuilder.RenameColumn(
                name: "Telefono",
                table: "user",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "Rol_id",
                table: "user",
                newName: "Role_id");

            migrationBuilder.RenameColumn(
                name: "Foto",
                table: "user",
                newName: "Photo");

            migrationBuilder.RenameIndex(
                name: "IX_user_Rol_id",
                table: "user",
                newName: "IX_user_Role_id");

            migrationBuilder.CreateTable(
                name: "consortium",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consortium", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "responsibleSector",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_responsibleSector", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "claimResponse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ResponsibleSector_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claimResponse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_claimResponse_responsibleSector_ResponsibleSector_id",
                        column: x => x.ResponsibleSector_id,
                        principalTable: "responsibleSector",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claimResponse_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "claim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Archive = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    User_id = table.Column<int>(type: "int", nullable: false),
                    ClaimResponse_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_claim_claimResponse_ClaimResponse_id",
                        column: x => x.ClaimResponse_id,
                        principalTable: "claimResponse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_user_User_id",
                        column: x => x.User_id,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_claim_ClaimResponse_id",
                table: "claim",
                column: "ClaimResponse_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_claim_User_id",
                table: "claim",
                column: "User_id");

            migrationBuilder.CreateIndex(
                name: "IX_claimResponse_ResponsibleSector_id",
                table: "claimResponse",
                column: "ResponsibleSector_id");

            migrationBuilder.CreateIndex(
                name: "IX_claimResponse_UserId",
                table: "claimResponse",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_role_Role_id",
                table: "user",
                column: "Role_id",
                principalTable: "role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_role_Role_id",
                table: "user");

            migrationBuilder.DropTable(
                name: "claim");

            migrationBuilder.DropTable(
                name: "consortium");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "claimResponse");

            migrationBuilder.DropTable(
                name: "responsibleSector");

            migrationBuilder.RenameColumn(
                name: "Role_id",
                table: "user",
                newName: "Rol_id");

            migrationBuilder.RenameColumn(
                name: "Photo",
                table: "user",
                newName: "Foto");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "user",
                newName: "Telefono");

            migrationBuilder.RenameIndex(
                name: "IX_user_Role_id",
                table: "user",
                newName: "IX_user_Rol_id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_user_rol_Rol_id",
                table: "user",
                column: "Rol_id",
                principalTable: "rol",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
