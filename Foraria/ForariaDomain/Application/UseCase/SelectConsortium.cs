using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface ISelectConsortium
{
    Task<(bool Success, string Message, Consortium? Consortium)> Execute(int userId, int consortiumId);
}

public class SelectConsortium : ISelectConsortium
{
    private readonly IConsortiumRepository _consortiumRepository;
    private readonly IUserRepository _userRepository;

    public SelectConsortium(
        IConsortiumRepository consortiumRepository,
        IUserRepository userRepository)
    {
        _consortiumRepository = consortiumRepository;
        _userRepository = userRepository;
    }

    public async Task<(bool Success, string Message, Consortium? Consortium)> Execute(int userId, int consortiumId)
    {
        var consortium = await _consortiumRepository.FindByIdWithoutFilters(consortiumId);
        if (consortium == null)
            throw new NotFoundException($"El consorcio con ID {consortiumId} no existe.");

        var userConsortiumIds = _userRepository.GetConsortiumIdsByUserId(userId);

        if (userConsortiumIds.Count == 0)
            throw new BusinessException("No tienes consorcios asignados.");

        if (!userConsortiumIds.Contains(consortiumId))
            throw new ForbiddenAccessException(
                $"No tienes acceso al consorcio '{consortium.Name}'. " +
                $"Solo puedes acceder a los consorcios: {string.Join(", ", userConsortiumIds)}");

        var user = await _userRepository.GetByIdWithoutFilters(userId);
        if (user == null)
            throw new NotFoundException("Usuario no encontrado.");

        var allowedRoles = new[] { "Administrador", "Consejo" };
        if (!allowedRoles.Contains(user.Role.Description))
            throw new ForbiddenAccessException(
                "Solo usuarios con rol Administrador o Consejo pueden cambiar de consorcio activo.");

        return (
            Success: true,
            Message: $"Consorcio '{consortium.Name}' seleccionado correctamente.",
            Consortium: consortium
        );
    }
}
