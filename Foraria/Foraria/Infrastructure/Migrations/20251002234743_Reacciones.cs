using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class Reacciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_id = table.Column<int>(type: "int", nullable: false),
                    Message_id = table.Column<int>(type: "int", nullable: true),
                    Thread_id = table.Column<int>(type: "int", nullable: true),
                    ReactionType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reaction_message_Message_id",
                        column: x => x.Message_id,
                        principalTable: "message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reaction_thread_Thread_id",
                        column: x => x.Thread_id,
                        principalTable: "thread",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reaction_user_User_id",
                        column: x => x.User_id,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reaction_Message_id",
                table: "reaction",
                column: "Message_id");

            migrationBuilder.CreateIndex(
                name: "IX_reaction_Thread_id",
                table: "reaction",
                column: "Thread_id");

            migrationBuilder.CreateIndex(
                name: "IX_reaction_User_id_Message_id_Thread_id",
                table: "reaction",
                columns: new[] { "User_id", "Message_id", "Thread_id" },
                unique: true,
                filter: "[Message_id] IS NOT NULL AND [Thread_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reaction");
        }
    }
}
