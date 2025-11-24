using ForariaDomain.Exceptions;
using System.Security.Claims;

namespace Foraria.Application.Services
{
    public interface IPermissionService
    {
        Task EnsurePermissionAsync(ClaimsPrincipal user, string permissionKey);
    }

    public class PermissionService : IPermissionService
    {
        private static readonly Dictionary<string, HashSet<string>> PermissionsByRole = new()
        {
            ["Administrador"] = new()
            {
                "Blockchain.Notarize", "Blockchain.Verify",

                "Calls.Create", "Calls.Join", "Calls.End",
                "Calls.ViewDetails", "Calls.ViewParticipants",
                "Calls.ViewState", "Calls.UploadRecording",
                "Calls.SendMessage", "Calls.View",

                "Claims.Reject", "Claims.Respond", "Claims.View", "Claim.ViewPendingClaim",

                "Consortium.Select", "Consortium.ViewAvailable", "Consortium.AssignToAdmin", "Consortium.ViewAdminConsortiums",

                "Dashboard.ViewActivePolls", "Dashboard.ViewActiveReservations",
                "Dashboard.ViewLatestClaim", "Dashboard.ViewPendingClaims",
                "Dashboard.ViewReservationsCount", "Dashboard.ViewUpcomingReservations",
                "Dashboard.ViewUsersCount", "Dashboard.ViewCollectedExpensesPercentage",
                "Dashboard.ViewMonthlyExpenseTotal", "Dashboard.ViewPendingExpenses",

                "ExpenseDetails.Generate", "ExpenseDetails.ViewByResidence",

                "Expenses.Create", "Expenses.ViewAll",

                "Forums.Create", "Forums.Disable", "Forums.View",
                "Forums.ViewAll", "Forums.ViewThreads", "Forums.ViewWithCategory",

                "Invoices.Create", "Invoices.ViewAll",

                "Messages.Create", "Messages.DeleteOwn", "Messages.Hide",
                "Messages.UpdateOwn", "Messages.View", "Messages.ViewByThread", "Messages.ViewByUser",

                "Ocr.ProcessInvoice",

                "Payments.CreatePreference",

                "Permissions.RevokeFromTenant", "Permissions.TransferToTenant",

                "Polls.Create", "Polls.Notarize", "Polls.Update",
                "Polls.View", "Polls.ViewActiveCount", "Polls.ViewAll", "Polls.ViewResults", "Polls.ViewResultsAll",

                "Reactions.Toggle", "Reactions.ViewMessage", "Reactions.ViewThread",

                "Reserves.Create", "Reserves.ViewAll", "Reserve.ViewActiveCount",

                "Residences.Create", "Residences.View", "Residences.ViewAllByConsortium",

                "SupplierContracts.Create", "SupplierContracts.View", "SupplierContracts.ViewBySupplier", "SupplierContracts.ViewActiveContractsCount",

                "Suppliers.Create", "Suppliers.Delete", "Suppliers.View", "Suppliers.ViewAll", "Suppliers.CategoriesCount",

                "Threads.Close", "Threads.Create", "Threads.Delete", "Threads.Update",
                "Threads.View", "Threads.ViewAll", "Threads.ViewCommentCount", "Threads.ViewWithMessages",

                "Transcriptions.ViewInfo",
                "Transcriptions.Verify",

                "Users.ViewTotalOwners", "Users.ViewTotalTenants","Users.ViewById", "Users.Logout",
                "Users.RefreshToken",  "Users.ViewByConsortium", "Users.ViewCount", "Users.UpdateFirstTime",

                "UserDocuments.Create", "UserDocuments.Update", "UserDocuments.ViewAll",
                "UserDocuments.ViewByCategory", "UserDocuments.ViewLastUpload", "UserDocuments.ViewStats",

                
            },

            ["Consorcio"] = new()
            {
                "Blockchain.Notarize", "Blockchain.Verify",

                "Calls.Create", "Calls.Join", "Calls.End",
                "Calls.ViewDetails", "Calls.ViewParticipants",
                "Calls.ViewState", "Calls.UploadRecording",
                "Calls.SendMessage", "Calls.View",

                "Claims.Create", "Claims.Reject", "Claims.Respond", "Claims.View", "Claim.ViewPendingClaim",

                "Consortium.Select", "Consortium.ViewAvailable", "Consortium.AssignToAdmin", "Consortium.ViewAdminConsortiums",

                "Dashboard.ViewActivePolls", "Dashboard.ViewActiveReservations",
                "Dashboard.ViewLatestClaim", "Dashboard.ViewPendingClaims",
                "Dashboard.ViewReservationsCount", "Dashboard.ViewUpcomingReservations",
                "Dashboard.ViewUsersCount", "Dashboard.ViewCollectedExpensesPercentage",
                "Dashboard.ViewMonthlyExpenseTotal", "Dashboard.ViewPendingExpenses",

                "ExpenseDetails.Generate", "ExpenseDetails.ViewByResidence",

                "Expenses.Create", "Expenses.ViewAll",

                "Forums.Create", "Forums.Disable", "Forums.View",

                "Forums.ViewAll", "Forums.ViewThreads", "Forums.ViewWithCategory",

                "Invoices.Create", "Invoices.ViewAll",

                "Messages.Create", "Messages.DeleteOwn", "Messages.Hide",
                "Messages.UpdateOwn", "Messages.View", "Messages.ViewByThread", "Messages.ViewByUser",

                "Ocr.ProcessInvoice",

                "Payments.CreatePreference",

                "Permissions.RevokeFromTenant", "Permissions.TransferToTenant",

                "Polls.ChangeState", "Polls.Create", "Polls.Notarize", "Polls.Update",
                "Polls.View", "Polls.ViewActiveCount", "Polls.ViewAll", "Polls.ViewResults", "Polls.ViewResultsAll",

                "Reactions.Toggle", "Reactions.ViewMessage", "Reactions.ViewThread",

                "Reserves.Create", "Reserves.ViewAll", "Reserve.ViewActiveCount",

                "Residences.Create", "Residences.View", "Residences.ViewAllByConsortium",

                "SupplierContracts.Create", "SupplierContracts.View", "SupplierContracts.ViewBySupplier",  "SupplierContracts.ViewActiveContractsCount",

                "Suppliers.Create", "Suppliers.Delete", "Suppliers.View", "Suppliers.ViewAll", "Suppliers.CategoriesCount",

                "Threads.Close", "Threads.Create", "Threads.Delete", "Threads.Update",
                "Threads.View", "Threads.ViewAll", "Threads.ViewCommentCount", "Threads.ViewWithMessages",

                "Transcriptions.ViewInfo",
                "Transcriptions.Verify",

                "Users.ViewTotalOwners","Users.ViewTotalTenants" , "Users.ViewById", "Users.Logout",
                "Users.RefreshToken", "Users.ViewByConsortium", "Users.ViewCount", "Users.UpdateFirstTime",

                "UserDocuments.Create", "UserDocuments.Update", "UserDocuments.ViewAll",
                "UserDocuments.ViewByCategory", "UserDocuments.ViewLastUpload", "UserDocuments.ViewStats",

                "Votes.Cast"
            },

            ["Propietario"] = new()
            {
                "Blockchain.Verify",

                "Calls.Join", "Calls.ViewDetails", "Calls.ViewParticipants",
                "Calls.ViewState", "Calls.SendMessage", "Calls.View",

                "Claims.Create", "Claims.View", "Claim.ViewPendingClaim",

                "Consortium.ViewAvailable", 

                "Dashboard.ViewActivePolls", "Dashboard.ViewActiveReservations", 
                "Dashboard.ViewReservationsCount", "Dashboard.ViewMonthlyExpenseTotal",
                "Dashboard.ViewPendingExpenses","Dashboard.ViewUserExpenseSummary",
                "Dashboard.ViewUserMonthlyExpenseHistory", 

                "Expenses.ViewAll",

                "ExpenseDetails.ViewByResidence",

                "Forums.View", "Forums.ViewAll", "Forums.ViewThreads", "Forums.ViewWithCategory",

                "Messages.Create", "Messages.DeleteOwn", "Messages.UpdateOwn",

                "Messages.View", "Messages.ViewByThread",

                "Payments.CreatePreference",

                "Permissions.RevokeFromTenant", "Permissions.TransferToTenant",

                "Polls.Notarize", "Polls.View", "Polls.ViewActiveCount",
                "Polls.ViewAll", "Polls.ViewResults", "Polls.ViewResultsAll",

                "Reactions.Toggle", "Reactions.ViewMessage", "Reactions.ViewThread",

                "Reserves.Create", "Reserves.ViewAll", "Reserve.ViewActiveCount",

                "Suppliers.View", "Suppliers.ViewAll", "Suppliers.CategoriesCount",

                "Threads.Create", "Threads.Update", "Threads.View", "Threads.ViewAll",
                "Threads.ViewCommentCount", "Threads.ViewWithMessages",

                "Transcriptions.ViewInfo",
                "Transcriptions.Verify",

                "Users.ViewTotalOwners","Users.ViewTotalTenants","Users.ViewById", "Users.Logout",
                "Users.RefreshToken", "Users.UpdateFirstTime", "Users.ViewByConsortium",

                "UserDocuments.Create", "UserDocuments.Update", "UserDocuments.ViewAll", "Users.ViewCount",
                "UserDocuments.ViewByCategory", "UserDocuments.ViewLastUpload", "UserDocuments.ViewStats",

                "Votes.Cast"
            },

            ["Inquilino"] = new()
            {
                "Blockchain.Verify",

                "Calls.Join", "Calls.ViewDetails", "Calls.ViewParticipants",
                "Calls.ViewState", "Calls.SendMessage", "Calls.View",

                "Claims.Create", "Claims.View", "Claim.ViewPendingClaim",

                "Consortium.ViewAvailable",

                "Dashboard.ViewActivePolls", "Dashboard.ViewActiveReservations",
                "Dashboard.ViewReservationsCount", "Dashboard.ViewMonthlyExpenseTotal",
                "Dashboard.ViewPendingExpenses","Dashboard.ViewUserExpenseSummary",
                "Dashboard.ViewUserMonthlyExpenseHistory", 

                "Expenses.ViewAll",

                "ExpenseDetails.ViewByResidence",

                "Forums.View", "Forums.ViewAll", "Forums.ViewThreads", "Forums.ViewWithCategory",

                "Messages.Create", "Messages.DeleteOwn", "Messages.UpdateOwn",
                "Messages.View", "Messages.ViewByThread",

                "Payments.CreatePreference",

                "Polls.Notarize", "Polls.View", "Polls.ViewActiveCount",
                "Polls.ViewAll", "Polls.ViewResults", "Polls.ViewResultsAll",

                "Reactions.Toggle", "Reactions.ViewMessage", "Reactions.ViewThread",

                "Reserves.Create", "Reserves.ViewAll", "Reserve.ViewActiveCount",

                "Suppliers.View", "Suppliers.ViewAll", "Suppliers.CategoriesCount",

                "Threads.Create", "Threads.Update", "Threads.View", "Threads.ViewAll",
                "Threads.ViewCommentCount", "Threads.ViewWithMessages",

                "Transcriptions.ViewInfo",
                "Transcriptions.Verify",

                "Users.ViewTotalOwners","Users.ViewTotalTenants", "Users.ViewById", "Users.Logout",
                "Users.RefreshToken", "Users.UpdateFirstTime", "Users.ViewByConsortium",

                "UserDocuments.Create", "UserDocuments.Update", "UserDocuments.ViewAll", "Users.ViewCount",
                "UserDocuments.ViewByCategory", "UserDocuments.ViewLastUpload", "UserDocuments.ViewStats",

                "Votes.Cast"
            }
        };

        public Task EnsurePermissionAsync(ClaimsPrincipal user, string permissionKey)
        {
            if (user?.Identity is null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedException("Usuario no autenticado.");

            var role = user.FindFirst(ClaimTypes.Role)?.Value
                ?? throw new UnauthorizedException("No se encontró un rol en el token.");

            if (!PermissionsByRole.TryGetValue(role, out var permissions))
                throw new ForbiddenAccessException($"El rol '{role}' no tiene permisos registrados.");

            if (!permissions.Contains(permissionKey))
                throw new ForbiddenAccessException($"El rol '{role}' no tiene permiso para '{permissionKey}'.");

            return Task.CompletedTask;
        }
    }
}
