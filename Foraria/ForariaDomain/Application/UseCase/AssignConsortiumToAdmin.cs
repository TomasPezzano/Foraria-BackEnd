using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IAssignConsortiumToAdmin
{
    Task<(bool Success, string Message, User Admin, List<Consortium> Consortiums)> Execute(
        int administratorId,
        int consortiumId);
}

public class AssignConsortiumToAdmin : IAssignConsortiumToAdmin
{
    private readonly IConsortiumRepository _consortiumRepository;
    private readonly IUserRepository _userRepository;

    public AssignConsortiumToAdmin(
        IConsortiumRepository consortiumRepository,
        IUserRepository userRepository)
    {
        _consortiumRepository = consortiumRepository;
        _userRepository = userRepository;
    }

    public async Task<(bool Success, string Message, User Admin, List<Consortium> Consortiums)> Execute(
        int administratorId,
        int consortiumId)
    {
        var admin = await _userRepository.GetByIdWithoutFilters(administratorId);

        if (admin == null)
            throw new NotFoundException($"Usuario con ID {administratorId} no encontrado.");

        if (admin.Role.Description != "Administrador")
            throw new BusinessException(
                $"El usuario '{admin.Name} {admin.LastName}' no tiene rol de Administrador. " +
                $"Solo se pueden asignar consorcios a usuarios con rol Administrador.");

        var consortium = await _consortiumRepository.FindByIdWithoutFilters(consortiumId);

        if (consortium == null)
            throw new NotFoundException($"Consorcio con ID {consortiumId} no encontrado.");

        if (await _consortiumRepository.HasAdministrator(consortiumId))
        {
            if (await _consortiumRepository.IsAdministratorAssigned(consortiumId, administratorId))
            {
                throw new BusinessException(
                    $"El administrador '{admin.Name} {admin.LastName}' ya está asignado al consorcio '{consortium.Name}'.");
            }
            else
            {
                var currentConsortium = await _consortiumRepository.FindByIdWithoutFilters(consortiumId);
                var currentAdmin = currentConsortium?.Administrator;

                throw new BusinessException(
                    $"El consorcio '{consortium.Name}' ya tiene un administrador asignado: " +
                    $"{currentAdmin?.Name} {currentAdmin?.LastName} (ID: {currentAdmin?.Id}).");
            }
        }

        await _consortiumRepository.AssignAdministrator(consortiumId, administratorId);

        var allConsortiums = await _consortiumRepository.GetConsortiumsByAdministrator(administratorId);

        return (
            Success: true,
            Message: $"Consorcio '{consortium.Name}' asignado exitosamente a '{admin.Name} {admin.LastName}'.",
            Admin: admin,
            Consortiums: allConsortiums
        );
    }
}
