using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class mp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "payment",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoPaymentId",
                table: "payment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferenceId",
                table: "payment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "payment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StatusDetail",
                table: "payment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "MercadoPagoPaymentId",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "PreferenceId",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "StatusDetail",
                table: "payment");
        }
    }
}
