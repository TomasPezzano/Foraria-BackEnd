using System.Security.Claims;
using Foraria.Application.Services;
using ForariaDomain.Exceptions;
using Xunit;

namespace Foraria.Tests.Application.Services
{
    public class PermissionServiceTests
    {
        private readonly PermissionService _service = new();

        private static ClaimsPrincipal CreateUser(string? role = null, bool authenticated = true)
        {
            var identity = authenticated
                ? new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, role ?? "")
                }, authenticationType: "TestAuthType")
                : new ClaimsIdentity();

            return new ClaimsPrincipal(identity);
        }



        [Fact]
        public async Task EnsurePermissionAsync_Should_Throw_If_User_Not_Authenticated()
        {
            var user = CreateUser(role: "Administrador", authenticated: false);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _service.EnsurePermissionAsync(user, "Polls.ViewAll"));
        }


        [Fact]
        public async Task EnsurePermissionAsync_Should_Throw_If_Role_Not_Registered()
        {
            var user = CreateUser(role: "Visitante", authenticated: true);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                _service.EnsurePermissionAsync(user, "Polls.ViewAll"));
        }

        [Fact]
        public async Task EnsurePermissionAsync_Should_Throw_If_Permission_Not_In_Role()
        {
            var user = CreateUser(role: "Inquilino", authenticated: true);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                _service.EnsurePermissionAsync(user, "Expenses.Create")); 
        }

        [Fact]
        public async Task EnsurePermissionAsync_Should_Pass_If_Role_Has_Permission()
        {
            var user = CreateUser(role: "Administrador", authenticated: true);

            var ex = await Record.ExceptionAsync(() =>
                _service.EnsurePermissionAsync(user, "Polls.ViewAll"));

            Assert.Null(ex);
        }

        [Fact]
        public void AllRoles_Should_Have_Permissions_Defined()
        {
            var permissionsField = typeof(PermissionService)
                .GetField("PermissionsByRole", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

            var permissions = (Dictionary<string, HashSet<string>>)permissionsField.GetValue(null)!;

            Assert.NotEmpty(permissions);

            foreach (var (role, perms) in permissions)
            {
                Assert.False(string.IsNullOrWhiteSpace(role), "El nombre del rol no puede estar vacío");
                Assert.NotEmpty(perms);
            }
        }

        [Fact]
        public void Consortium_Should_Have_All_Admin_Permissions()
        {
            var permissionsField = typeof(PermissionService)
                .GetField("PermissionsByRole", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

            var permissions = (Dictionary<string, HashSet<string>>)permissionsField.GetValue(null)!;

            var adminPerms = permissions["Administrador"];
            var consorcioPerms = permissions["Consorcio"];

            var missing = adminPerms.Except(consorcioPerms).ToList();

            Assert.True(!missing.Any(),
                $"El rol Consorcio no tiene todos los permisos del Administrador. Faltan: {string.Join(", ", missing)}");
        }
    }
}
