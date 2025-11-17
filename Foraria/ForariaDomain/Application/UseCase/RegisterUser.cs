using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IRegisterUser
{
    Task<int> GetAllUsersInNumber(int consortiumId);
    Task<User> Register(User user, int? residenceId = null, int? consortiumId = null);
}

public class RegisterUser : IRegisterUser
{
    private readonly IUserRepository _userRepository;
    private readonly IGeneratePassword _generatePasswordUseCase;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHash _passwordHashUseCase;
    private readonly ISendEmail _emailUseCase;
    private readonly IResidenceRepository _residenceRepository;
    private readonly IConsortiumRepository _consortiumRepository;

    public RegisterUser(
        IUserRepository userRepository,
        IGeneratePassword generatePasswordService,
        IRoleRepository roleRepository,
        IPasswordHash passwordHash,
        ISendEmail sendEmail,
        IResidenceRepository residenceRepository,
        IConsortiumRepository consortiumRepository)
    {
        _userRepository = userRepository;
        _generatePasswordUseCase = generatePasswordService;
        _roleRepository = roleRepository;
        _passwordHashUseCase = passwordHash;
        _emailUseCase = sendEmail;
        _residenceRepository = residenceRepository;
        _consortiumRepository = consortiumRepository;
    }

    public async Task<User> Register(User user, int? residenceId = null, int? consortiumId = null)
    {
        if (await _userRepository.ExistsEmail(user.Mail))
            throw new BusinessException("El email ya está registrado en el sistema.");

        Role role = await _roleRepository.GetById(user.Role_id);
        if (role == null)
            throw new NotFoundException("El rol seleccionado no es válido.");

        bool isAdministrator = role.Description == "Administrador";
        Residence? residence = null;

        if (isAdministrator)
        {
            if (!consortiumId.HasValue)
                throw new BusinessException("El rol Administrador requiere un Consortium ID.");

            if (!await _consortiumRepository.ExistsWithoutFilters(consortiumId.Value))
                throw new NotFoundException($"Consorcio con ID {consortiumId.Value} no existe.");

            if (await _consortiumRepository.HasAdministrator(consortiumId.Value))
                throw new BusinessException("El consorcio ya tiene un administrador asignado.");

            user.Residences = new List<Residence?>();
        }
        else
        {
            if (!residenceId.HasValue)
                throw new BusinessException($"El rol '{role.Description}' requiere una residencia asignada.");

            residence = await _residenceRepository.GetByIdWithoutFilters(residenceId.Value);
            if (residence == null)
                throw new NotFoundException($"Residencia con ID {residenceId.Value} no existe.");

            var roleAlreadyAssigned = await _userRepository.ExistsUserWithRoleInResidence(residenceId.Value, role.Description);

            if (roleAlreadyAssigned)
                throw new BusinessException(
                    $"Esta residencia ya tiene un usuario con el rol '{role.Description}' asignado.");

            user.Residences = new List<Residence?> { residence };
        }

        user.HasPermission = role.Description == "Propietario";

        var temporaryPassword = await _generatePasswordUseCase.Generate();
        var passwordHash = _passwordHashUseCase.Execute(temporaryPassword);
        user.Password = passwordHash;
        user.RequiresPasswordChange = true;

        var savedUser = await _userRepository.Add(user);
        
        if (isAdministrator && consortiumId.HasValue)
        {
            await _consortiumRepository.AssignAdministrator(consortiumId.Value, savedUser.Id);
        }

        try
        {
            await _emailUseCase.SendWelcomeEmail(
                user.Mail,
                user.Name,
                user.LastName,
                temporaryPassword);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar email: {ex.Message}");
        }

        var residenceList = new List<Residence>();
        if (residence != null)
        {
            residenceList.Add(new Residence
            {
                Id = residence.Id,
                Number = residence.Number,
                Floor = residence.Floor,
                Tower = residence.Tower,
                ConsortiumId = residence.ConsortiumId
            });
        }

        return new User
        {
            Id = savedUser.Id,
            Mail = savedUser.Mail,
            Name = user.Name,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role_id = savedUser.Role_id,
            Password = temporaryPassword,
            Residences = residenceList,
            RequiresPasswordChange = true
        };
    }

    public async Task<int> GetAllUsersInNumber(int consortiumId)
    {
        var totalUsers = await _userRepository.GetAllInNumber(consortiumId);
        return totalUsers;
    }
}