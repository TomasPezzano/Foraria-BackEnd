using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class ForumState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_thread_forum_Forum_id",
                table: "thread");

            migrationBuilder.DropForeignKey(
                name: "FK_thread_user_User_id",
                table: "thread");

            migrationBuilder.RenameColumn(
                name: "User_id",
                table: "thread",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Forum_id",
                table: "thread",
                newName: "ForumId");

            migrationBuilder.RenameIndex(
                name: "IX_thread_User_id",
                table: "thread",
                newName: "IX_thread_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_thread_Forum_id",
                table: "thread",
                newName: "IX_thread_ForumId");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "forum",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_thread_forum_ForumId",
                table: "thread",
                column: "ForumId",
                principalTable: "forum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_thread_user_UserId",
                table: "thread",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_thread_forum_ForumId",
                table: "thread");

            migrationBuilder.DropForeignKey(
                name: "FK_thread_user_UserId",
                table: "thread");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "forum");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "thread",
                newName: "User_id");

            migrationBuilder.RenameColumn(
                name: "ForumId",
                table: "thread",
                newName: "Forum_id");

            migrationBuilder.RenameIndex(
                name: "IX_thread_UserId",
                table: "thread",
                newName: "IX_thread_User_id");

            migrationBuilder.RenameIndex(
                name: "IX_thread_ForumId",
                table: "thread",
                newName: "IX_thread_Forum_id");

            migrationBuilder.AddForeignKey(
                name: "FK_thread_forum_Forum_id",
                table: "thread",
                column: "Forum_id",
                principalTable: "forum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_thread_user_User_id",
                table: "thread",
                column: "User_id",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
