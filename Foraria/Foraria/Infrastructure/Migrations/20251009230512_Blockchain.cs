using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class Blockchain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_claimResponse_user_UserId",
                table: "claimResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_expense_consortium_Id_Consortium",
                table: "expense");

            migrationBuilder.DropForeignKey(
                name: "FK_expense_residence_Id_Residence",
                table: "expense");

            migrationBuilder.DropForeignKey(
                name: "FK_expenseDetail_expense_Id_Expense",
                table: "expenseDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_expense_Id_Expense",
                table: "payment");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_paymentMethod_Id_PaymentMethod",
                table: "payment");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_residence_Id_Residence",
                table: "payment");

            migrationBuilder.DropForeignKey(
                name: "FK_reaction_message_Message_id",
                table: "reaction");

            migrationBuilder.DropForeignKey(
                name: "FK_reaction_thread_Thread_id",
                table: "reaction");

            migrationBuilder.DropForeignKey(
                name: "FK_reaction_user_User_id",
                table: "reaction");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEvent_event_EventId",
                table: "UserEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEvent_user_UserId",
                table: "UserEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResidence_residence_ResidenceId",
                table: "UserResidence");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResidence_user_UserId",
                table: "UserResidence");

            migrationBuilder.DropPrimaryKey(
                name: "PK_paymentMethod",
                table: "paymentMethod");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payment",
                table: "payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_expense",
                table: "expense");

            migrationBuilder.RenameTable(
                name: "paymentMethod",
                newName: "PaymentMethod");

            migrationBuilder.RenameTable(
                name: "payment",
                newName: "Payment");

            migrationBuilder.RenameTable(
                name: "expense",
                newName: "Expense");

            migrationBuilder.RenameColumn(
                name: "Id_Residence",
                table: "Payment",
                newName: "ResidenceId");

            migrationBuilder.RenameColumn(
                name: "Id_PaymentMethod",
                table: "Payment",
                newName: "PaymentMethodId");

            migrationBuilder.RenameColumn(
                name: "Id_Expense",
                table: "Payment",
                newName: "ExpenseId");

            migrationBuilder.RenameIndex(
                name: "IX_payment_Id_Residence",
                table: "Payment",
                newName: "IX_Payment_ResidenceId");

            migrationBuilder.RenameIndex(
                name: "IX_payment_Id_PaymentMethod",
                table: "Payment",
                newName: "IX_Payment_PaymentMethodId");

            migrationBuilder.RenameIndex(
                name: "IX_payment_Id_Expense",
                table: "Payment",
                newName: "IX_Payment_ExpenseId");

            migrationBuilder.RenameColumn(
                name: "Id_Expense",
                table: "expenseDetail",
                newName: "ExpenseId");

            migrationBuilder.RenameIndex(
                name: "IX_expenseDetail_Id_Expense",
                table: "expenseDetail",
                newName: "IX_expenseDetail_ExpenseId");

            migrationBuilder.RenameColumn(
                name: "Id_Residence",
                table: "Expense",
                newName: "ResidenceId");

            migrationBuilder.RenameColumn(
                name: "Id_Consortium",
                table: "Expense",
                newName: "ConsortiumId");

            migrationBuilder.RenameIndex(
                name: "IX_expense_Id_Residence",
                table: "Expense",
                newName: "IX_Expense_ResidenceId");

            migrationBuilder.RenameIndex(
                name: "IX_expense_Id_Consortium",
                table: "Expense",
                newName: "IX_Expense_ConsortiumId");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "expenseDetail",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(18)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<double>(
                name: "TotalAmount",
                table: "Expense",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(18)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentMethod",
                table: "PaymentMethod",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Expense",
                table: "Expense",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "blockchainProof",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PollId = table.Column<int>(type: "int", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HashHex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TxHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contract = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Network = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChainId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blockchainProof", x => x.Id);
                    table.ForeignKey(
                        name: "FK_blockchainProof_poll_PollId",
                        column: x => x.PollId,
                        principalTable: "poll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_blockchainProof_PollId",
                table: "blockchainProof",
                column: "PollId",
                unique: true,
                filter: "[PollId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_claimResponse_user_UserId",
                table: "claimResponse",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expense_consortium_ConsortiumId",
                table: "Expense",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expense_residence_ResidenceId",
                table: "Expense",
                column: "ResidenceId",
                principalTable: "residence",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_expenseDetail_Expense_ExpenseId",
                table: "expenseDetail",
                column: "ExpenseId",
                principalTable: "Expense",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Expense_ExpenseId",
                table: "Payment",
                column: "ExpenseId",
                principalTable: "Expense",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_PaymentMethod_PaymentMethodId",
                table: "Payment",
                column: "PaymentMethodId",
                principalTable: "PaymentMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_residence_ResidenceId",
                table: "Payment",
                column: "ResidenceId",
                principalTable: "residence",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_reaction_message_Message_id",
                table: "reaction",
                column: "Message_id",
                principalTable: "message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reaction_thread_Thread_id",
                table: "reaction",
                column: "Thread_id",
                principalTable: "thread",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reaction_user_User_id",
                table: "reaction",
                column: "User_id",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEvent_event_EventId",
                table: "UserEvent",
                column: "EventId",
                principalTable: "event",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEvent_user_UserId",
                table: "UserEvent",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResidence_residence_ResidenceId",
                table: "UserResidence",
                column: "ResidenceId",
                principalTable: "residence",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResidence_user_UserId",
                table: "UserResidence",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_claimResponse_user_UserId",
                table: "claimResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_Expense_consortium_ConsortiumId",
                table: "Expense");

            migrationBuilder.DropForeignKey(
                name: "FK_Expense_residence_ResidenceId",
                table: "Expense");

            migrationBuilder.DropForeignKey(
                name: "FK_expenseDetail_Expense_ExpenseId",
                table: "expenseDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Expense_ExpenseId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_PaymentMethod_PaymentMethodId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_residence_ResidenceId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_reaction_message_Message_id",
                table: "reaction");

            migrationBuilder.DropForeignKey(
                name: "FK_reaction_thread_Thread_id",
                table: "reaction");

            migrationBuilder.DropForeignKey(
                name: "FK_reaction_user_User_id",
                table: "reaction");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEvent_event_EventId",
                table: "UserEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEvent_user_UserId",
                table: "UserEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResidence_residence_ResidenceId",
                table: "UserResidence");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResidence_user_UserId",
                table: "UserResidence");

            migrationBuilder.DropTable(
                name: "blockchainProof");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentMethod",
                table: "PaymentMethod");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Expense",
                table: "Expense");

            migrationBuilder.RenameTable(
                name: "PaymentMethod",
                newName: "paymentMethod");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "payment");

            migrationBuilder.RenameTable(
                name: "Expense",
                newName: "expense");

            migrationBuilder.RenameColumn(
                name: "ResidenceId",
                table: "payment",
                newName: "Id_Residence");

            migrationBuilder.RenameColumn(
                name: "PaymentMethodId",
                table: "payment",
                newName: "Id_PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "ExpenseId",
                table: "payment",
                newName: "Id_Expense");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_ResidenceId",
                table: "payment",
                newName: "IX_payment_Id_Residence");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_PaymentMethodId",
                table: "payment",
                newName: "IX_payment_Id_PaymentMethod");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_ExpenseId",
                table: "payment",
                newName: "IX_payment_Id_Expense");

            migrationBuilder.RenameColumn(
                name: "ExpenseId",
                table: "expenseDetail",
                newName: "Id_Expense");

            migrationBuilder.RenameIndex(
                name: "IX_expenseDetail_ExpenseId",
                table: "expenseDetail",
                newName: "IX_expenseDetail_Id_Expense");

            migrationBuilder.RenameColumn(
                name: "ResidenceId",
                table: "expense",
                newName: "Id_Residence");

            migrationBuilder.RenameColumn(
                name: "ConsortiumId",
                table: "expense",
                newName: "Id_Consortium");

            migrationBuilder.RenameIndex(
                name: "IX_Expense_ResidenceId",
                table: "expense",
                newName: "IX_expense_Id_Residence");

            migrationBuilder.RenameIndex(
                name: "IX_Expense_ConsortiumId",
                table: "expense",
                newName: "IX_expense_Id_Consortium");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "expenseDetail",
                type: "float(18)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "TotalAmount",
                table: "expense",
                type: "float(18)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddPrimaryKey(
                name: "PK_paymentMethod",
                table: "paymentMethod",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payment",
                table: "payment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_expense",
                table: "expense",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_claimResponse_user_UserId",
                table: "claimResponse",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_expense_consortium_Id_Consortium",
                table: "expense",
                column: "Id_Consortium",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_expense_residence_Id_Residence",
                table: "expense",
                column: "Id_Residence",
                principalTable: "residence",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_expenseDetail_expense_Id_Expense",
                table: "expenseDetail",
                column: "Id_Expense",
                principalTable: "expense",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_expense_Id_Expense",
                table: "payment",
                column: "Id_Expense",
                principalTable: "expense",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_paymentMethod_Id_PaymentMethod",
                table: "payment",
                column: "Id_PaymentMethod",
                principalTable: "paymentMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_residence_Id_Residence",
                table: "payment",
                column: "Id_Residence",
                principalTable: "residence",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reaction_message_Message_id",
                table: "reaction",
                column: "Message_id",
                principalTable: "message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reaction_thread_Thread_id",
                table: "reaction",
                column: "Thread_id",
                principalTable: "thread",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reaction_user_User_id",
                table: "reaction",
                column: "User_id",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEvent_event_EventId",
                table: "UserEvent",
                column: "EventId",
                principalTable: "event",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEvent_user_UserId",
                table: "UserEvent",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResidence_residence_ResidenceId",
                table: "UserResidence",
                column: "ResidenceId",
                principalTable: "residence",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResidence_user_UserId",
                table: "UserResidence",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
