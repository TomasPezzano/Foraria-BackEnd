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

                "Claims.Create", "Claims.Reject", "Claims.Respond", "Claims.View",

                "Dashboard.ViewActivePolls", "Dashboard.ViewActiveReservations",
                "Dashboard.ViewLatestClaim", "Dashboard.ViewPendingClaims",
                "Dashboard.ViewReservationsCount", "Dashboard.ViewUpcomingReservations",
                "Dashboard.ViewUsersCount",

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

                "Reserves.Create", "Reserves.ViewAll",

                "Residences.Create", "Residences.View", "Residences.ViewAllByConsortium",

                "SupplierContracts.Create", "SupplierContracts.View", "SupplierContracts.ViewBySupplier",

                "Suppliers.Create", "Suppliers.Delete", "Suppliers.View", "Suppliers.ViewAll",

                "Threads.Close", "Threads.Create", "Threads.Delete", "Threads.Update",
                "Threads.View", "Threads.ViewAll", "Threads.ViewCommentCount", "Threads.ViewWithMessages",

                "UserDocuments.Create", "UserDocuments.Update", "UserDocuments.ViewAll",
                "UserDocuments.ViewByCategory", "UserDocuments.ViewLastUpload", "UserDocuments.ViewStats",

                "Votes.Cast"
            },

            ["Consorcio"] = new()
            {
                "Blockchain.Notarize", "Blockchain.Verify",

                "Claims.Create", "Claims.Reject", "Claims.Respond", "Claims.View",

                "Dashboard.ViewActivePolls", "Dashboard.ViewActiveReservations",
                "Dashboard.ViewLatestClaim", "Dashboard.ViewPendingClaims",
                "Dashboard.ViewReservationsCount", "Dashboard.ViewUpcomingReservations",
                "Dashboard.ViewUsersCount",

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

                "Reserves.Create", "Reserves.ViewAll",

                "Residences.Create", "Residences.View", "Residences.ViewAllByConsortium",

                "SupplierContracts.Create", "SupplierContracts.View", "SupplierContracts.ViewBySupplier",

                "Suppliers.Create", "Suppliers.Delete", "Suppliers.View", "Suppliers.ViewAll",

                "Threads.Close", "Threads.Create", "Threads.Delete", "Threads.Update",
                "Threads.View", "Threads.ViewAll", "Threads.ViewCommentCount", "Threads.ViewWithMessages",

                "UserDocuments.Create", "UserDocuments.Update", "UserDocuments.ViewAll",
                "UserDocuments.ViewByCategory", "UserDocuments.ViewLastUpload", "UserDocuments.ViewStats",

                "Votes.Cast"
            },

            ["Propietario"] = new()
            {
                "Blockchain.Verify",

                "Claims.Create", "Claims.View",

                "Dashboard.ViewActivePolls", "Dashboard.ViewActiveReservations", "Dashboard.ViewReservationsCount",

                "ExpenseDetails.ViewByResidence",

                "Forums.View", "Forums.ViewAll", "Forums.ViewThreads", "Forums.ViewWithCategory",

                "Messages.Create", "Messages.DeleteOwn", "Messages.UpdateOwn",

                "Messages.View", "Messages.ViewByThread",

                "Payments.CreatePreference",

                "Permissions.RevokeFromTenant", "Permissions.TransferToTenant",

                "Polls.Notarize", "Polls.View", "Polls.ViewActiveCount",
                "Polls.ViewAll", "Polls.ViewResults", "Polls.ViewResultsAll",

                "Reactions.Toggle", "Reactions.ViewMessage", "Reactions.ViewThread",

                "Reserves.Create", "Reserves.ViewAll",

                "Suppliers.View", "Suppliers.ViewAll",

                "Threads.Create", "Threads.Update", "Threads.View", "Threads.ViewAll",
                "Threads.ViewCommentCount", "Threads.ViewWithMessages",

                "UserDocuments.Create", "UserDocuments.Update", "UserDocuments.ViewAll",

                "UserDocuments.ViewByCategory", "UserDocuments.ViewLastUpload", "UserDocuments.ViewStats",
                "Votes.Cast"
            },

            ["Inquilino"] = new()
            {
                "Blockchain.Verify",

                "Claims.Create", "Claims.View",

                "Dashboard.ViewActivePolls", "Dashboard.ViewActiveReservations", "Dashboard.ViewReservationsCount",

                "ExpenseDetails.ViewByResidence",

                "Forums.View", "Forums.ViewAll", "Forums.ViewThreads", "Forums.ViewWithCategory",

                "Messages.Create", "Messages.DeleteOwn", "Messages.UpdateOwn",
                "Messages.View", "Messages.ViewByThread",

                "Payments.CreatePreference",

                "Polls.Notarize", "Polls.View", "Polls.ViewActiveCount",
                "Polls.ViewAll", "Polls.ViewResults", "Polls.ViewResultsAll",

                "Reactions.Toggle", "Reactions.ViewMessage", "Reactions.ViewThread",

                "Reserves.Create", "Reserves.ViewAll",

                "Suppliers.View", "Suppliers.ViewAll",

                "Threads.Create", "Threads.Update", "Threads.View", "Threads.ViewAll",
                "Threads.ViewCommentCount", "Threads.ViewWithMessages",

                "UserDocuments.Create", "UserDocuments.Update", "UserDocuments.ViewAll",

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
