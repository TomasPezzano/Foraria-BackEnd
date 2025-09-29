using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class Votación : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categoryPoll",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoryPoll", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "resultPoll",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Percentage = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resultPoll", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "poll",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    User_id = table.Column<int>(type: "int", nullable: false),
                    CategoryPoll_id = table.Column<int>(type: "int", nullable: false),
                    ResultPoll_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poll", x => x.Id);
                    table.ForeignKey(
                        name: "FK_poll_categoryPoll_CategoryPoll_id",
                        column: x => x.CategoryPoll_id,
                        principalTable: "categoryPoll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_poll_resultPoll_ResultPoll_id",
                        column: x => x.ResultPoll_id,
                        principalTable: "resultPoll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_poll_user_User_id",
                        column: x => x.User_id,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pollOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Poll_id = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pollOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pollOption_poll_Poll_id",
                        column: x => x.Poll_id,
                        principalTable: "poll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Poll_id = table.Column<int>(type: "int", nullable: false),
                    User_id = table.Column<int>(type: "int", nullable: false),
                    PollOption_id = table.Column<int>(type: "int", nullable: false),
                    VotedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vote_pollOption_PollOption_id",
                        column: x => x.PollOption_id,
                        principalTable: "pollOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vote_poll_Poll_id",
                        column: x => x.Poll_id,
                        principalTable: "poll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vote_user_User_id",
                        column: x => x.User_id,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_poll_CategoryPoll_id",
                table: "poll",
                column: "CategoryPoll_id");

            migrationBuilder.CreateIndex(
                name: "IX_poll_ResultPoll_id",
                table: "poll",
                column: "ResultPoll_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_poll_User_id",
                table: "poll",
                column: "User_id");

            migrationBuilder.CreateIndex(
                name: "IX_pollOption_Poll_id",
                table: "pollOption",
                column: "Poll_id");

            migrationBuilder.CreateIndex(
                name: "IX_vote_Poll_id",
                table: "vote",
                column: "Poll_id");

            migrationBuilder.CreateIndex(
                name: "IX_vote_PollOption_id",
                table: "vote",
                column: "PollOption_id");

            migrationBuilder.CreateIndex(
                name: "IX_vote_User_id",
                table: "vote",
                column: "User_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vote");

            migrationBuilder.DropTable(
                name: "pollOption");

            migrationBuilder.DropTable(
                name: "poll");

            migrationBuilder.DropTable(
                name: "categoryPoll");

            migrationBuilder.DropTable(
                name: "resultPoll");
        }
    }
}
