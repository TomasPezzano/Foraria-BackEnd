using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foraria.Migrations
{
    /// <inheritdoc />
    public partial class necesaryMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "FK_refreshToken_user_UserId",
                table: "refreshToken");

            migrationBuilder.DropForeignKey(
                name: "FK_supplier_consortium_ConsortiumId",
                table: "supplier");

            migrationBuilder.DropForeignKey(
                name: "FK_supplier_supplierCategory_SupplierCategoryId",
                table: "supplier");

            migrationBuilder.DropForeignKey(
                name: "FK_supplierContract_supplier_SupplierId",
                table: "supplierContract");

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
                newName: "Expenses");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_ResidenceId",
                table: "payment",
                newName: "IX_payment_ResidenceId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_PaymentMethodId",
                table: "payment",
                newName: "IX_payment_PaymentMethodId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_ExpenseId",
                table: "payment",
                newName: "IX_payment_ExpenseId");

            migrationBuilder.RenameIndex(
                name: "IX_Expense_ResidenceId",
                table: "Expenses",
                newName: "IX_Expenses_ResidenceId");

            migrationBuilder.RenameIndex(
                name: "IX_Expense_ConsortiumId",
                table: "Expenses",
                newName: "IX_Expenses_ConsortiumId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_paymentMethod",
                table: "paymentMethod",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payment",
                table: "payment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Expenses",
                table: "Expenses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_expenseDetail_Expenses_ExpenseId",
                table: "expenseDetail",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_consortium_ConsortiumId",
                table: "Expenses",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_residence_ResidenceId",
                table: "Expenses",
                column: "ResidenceId",
                principalTable: "residence",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_Expenses_ExpenseId",
                table: "payment",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_paymentMethod_PaymentMethodId",
                table: "payment",
                column: "PaymentMethodId",
                principalTable: "paymentMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_residence_ResidenceId",
                table: "payment",
                column: "ResidenceId",
                principalTable: "residence",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_refreshToken_user_UserId",
                table: "refreshToken",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_supplier_consortium_ConsortiumId",
                table: "supplier",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_supplier_supplierCategory_SupplierCategoryId",
                table: "supplier",
                column: "SupplierCategoryId",
                principalTable: "supplierCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_supplierContract_supplier_SupplierId",
                table: "supplierContract",
                column: "SupplierId",
                principalTable: "supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_expenseDetail_Expenses_ExpenseId",
                table: "expenseDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_consortium_ConsortiumId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_residence_ResidenceId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_Expenses_ExpenseId",
                table: "payment");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_paymentMethod_PaymentMethodId",
                table: "payment");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_residence_ResidenceId",
                table: "payment");

            migrationBuilder.DropForeignKey(
                name: "FK_refreshToken_user_UserId",
                table: "refreshToken");

            migrationBuilder.DropForeignKey(
                name: "FK_supplier_consortium_ConsortiumId",
                table: "supplier");

            migrationBuilder.DropForeignKey(
                name: "FK_supplier_supplierCategory_SupplierCategoryId",
                table: "supplier");

            migrationBuilder.DropForeignKey(
                name: "FK_supplierContract_supplier_SupplierId",
                table: "supplierContract");

            migrationBuilder.DropPrimaryKey(
                name: "PK_paymentMethod",
                table: "paymentMethod");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payment",
                table: "payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Expenses",
                table: "Expenses");

            migrationBuilder.RenameTable(
                name: "paymentMethod",
                newName: "PaymentMethod");

            migrationBuilder.RenameTable(
                name: "payment",
                newName: "Payment");

            migrationBuilder.RenameTable(
                name: "Expenses",
                newName: "Expense");

            migrationBuilder.RenameIndex(
                name: "IX_payment_ResidenceId",
                table: "Payment",
                newName: "IX_Payment_ResidenceId");

            migrationBuilder.RenameIndex(
                name: "IX_payment_PaymentMethodId",
                table: "Payment",
                newName: "IX_Payment_PaymentMethodId");

            migrationBuilder.RenameIndex(
                name: "IX_payment_ExpenseId",
                table: "Payment",
                newName: "IX_Payment_ExpenseId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_ResidenceId",
                table: "Expense",
                newName: "IX_Expense_ResidenceId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_ConsortiumId",
                table: "Expense",
                newName: "IX_Expense_ConsortiumId");

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
                name: "FK_refreshToken_user_UserId",
                table: "refreshToken",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_supplier_consortium_ConsortiumId",
                table: "supplier",
                column: "ConsortiumId",
                principalTable: "consortium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_supplier_supplierCategory_SupplierCategoryId",
                table: "supplier",
                column: "SupplierCategoryId",
                principalTable: "supplierCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_supplierContract_supplier_SupplierId",
                table: "supplierContract",
                column: "SupplierId",
                principalTable: "supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
