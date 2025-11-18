using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class CallAndCallTranscript : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CallTranscriptId",
                table: "blockchainProof",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Calls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CallParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CallId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallParticipants_Calls_CallId",
                        column: x => x.CallId,
                        principalTable: "Calls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CallTranscripts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CallId = table.Column<int>(type: "int", nullable: false),
                    TranscriptPath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AudioPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TranscriptHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AudioHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BlockchainTxHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallTranscripts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallTranscripts_Calls_CallId",
                        column: x => x.CallId,
                        principalTable: "Calls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_blockchainProof_CallTranscriptId",
                table: "blockchainProof",
                column: "CallTranscriptId");

            migrationBuilder.CreateIndex(
                name: "IX_CallParticipants_CallId",
                table: "CallParticipants",
                column: "CallId");

            migrationBuilder.CreateIndex(
                name: "IX_CallTranscripts_CallId",
                table: "CallTranscripts",
                column: "CallId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_blockchainProof_CallTranscripts_CallTranscriptId",
                table: "blockchainProof",
                column: "CallTranscriptId",
                principalTable: "CallTranscripts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_blockchainProof_CallTranscripts_CallTranscriptId",
                table: "blockchainProof");

            migrationBuilder.DropTable(
                name: "CallParticipants");

            migrationBuilder.DropTable(
                name: "CallTranscripts");

            migrationBuilder.DropTable(
                name: "Calls");

            migrationBuilder.DropIndex(
                name: "IX_blockchainProof_CallTranscriptId",
                table: "blockchainProof");

            migrationBuilder.DropColumn(
                name: "CallTranscriptId",
                table: "blockchainProof");
        }
    }
}
