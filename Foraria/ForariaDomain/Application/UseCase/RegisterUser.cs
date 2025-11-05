using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using ForariaDomain;
using ForariaDomain.Exceptions;

namespace Foraria.Application.UseCase;

public interface IRegisterUser
{
    Task<int> GetAllUsersInNumber();
    Task<User> Register(User user, int residenceId);
}

public class RegisterUser : IRegisterUser
{
    private readonly IUserRepository _userRepository;
    private readonly IGeneratePassword _generatePasswordUseCase;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHash _passwordHashUseCase;
    private readonly ISendEmail _emailUseCase;
    private readonly IResidenceRepository _residenceRepository;

    public RegisterUser(
        IUserRepository userRepository,
        IGeneratePassword generatePasswordService,
        IRoleRepository roleRepository,
        IPasswordHash passwordHash,
        ISendEmail sendEmail,
        IResidenceRepository residenceRepository)
    {
        _userRepository = userRepository;
        _generatePasswordUseCase = generatePasswordService;
        _roleRepository = roleRepository;
        _passwordHashUseCase = passwordHash;
        _emailUseCase = sendEmail;
        _residenceRepository = residenceRepository;
    }

    public async Task<User> Register(User user, int residenceId)
    {
        if (await _userRepository.ExistsEmail(user.Mail))
            throw new BusinessException("Email is already registered in the system");

        Role role = await _roleRepository.GetById(user.Role_id);
        if (role == null)
            throw new NotFoundException("Selected role is not valid");

        var residenceExists = await _residenceRepository.Exists(residenceId);
        if (!residenceExists)
            throw new NotFoundException($"Residence with ID {residenceId} does not exist");

        var residence = await _residenceRepository.GetById(residenceId);
        if (residence == null)
            throw new BusinessException("Error retrieving residence information");

        var roleAlreadyAssigned = await _userRepository.ExistsUserWithRoleInResidence(residenceId, role.Description);
        if (roleAlreadyAssigned)
            throw new BusinessException($"This residence already has a user with the role '{role.Description}' assigned.");


        var temporaryPassword = await _generatePasswordUseCase.Generate();
        var passwordHash = _passwordHashUseCase.HashPassword(temporaryPassword);
        user.Password = passwordHash;
        user.Residences = new List<Residence> { residence };
        user.RequiresPasswordChange = true;

        var savedUser = await _userRepository.Add(user);

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
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }

        var residenceList = new List<Residence>
        {
            new Residence
            {
                Id = residence.Id,
                Number = residence.Number,
                Floor = residence.Floor,
                Tower = residence.Tower
            }
        };

        return new User
        {
            Id = savedUser.Id,
            Mail = savedUser.Mail,
            Name = user.Name,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role_id = savedUser.Role_id,
            Password = temporaryPassword,
            Residences = residenceList
        };
    }

    public async Task<int> GetAllUsersInNumber()
    {
        var totalUsers = await _userRepository.GetAllInNumber();
        return totalUsers; 
    }
}