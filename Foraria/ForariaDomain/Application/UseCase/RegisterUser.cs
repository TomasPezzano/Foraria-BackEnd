using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public interface IRegisterUser
{
    Task<int> GetAllUsersInNumber();
    Task<UserDto> Register(User user, int residenceId);
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

    public async Task<UserDto> Register(User user, int residenceId)
    {
        if (await _userRepository.ExistsEmail(user.Mail))
        {
            return new UserDto
            {
                Success = false,
                Message = "Email is already registered in the system"
            };
        }

        Role role = await _roleRepository.GetById(user.Role_id);
        if (role == null)
        {
            return new UserDto
            {
                Success = false,
                Message = "Selected role is not valid"
            };
        }

        var residenceExists = await _residenceRepository.Exists(residenceId);
        if (!residenceExists)
        {
            return new UserDto
            {
                Success = false,
                Message = $"Residence with ID {residenceId} does not exist"
            };
        }

        var residence = await _residenceRepository.GetById(residenceId);
        if (residence == null)
        {
            return new UserDto
            {
                Success = false,
                Message = "Error retrieving residence information"
            };
        }

        var roleAlreadyAssigned = await _userRepository.ExistsUserWithRoleInResidence(residenceId, user.Role.Description);
        if (roleAlreadyAssigned)
        {
            return new UserDto
            {
                Success = false,
                Message = $"This residence already has a user with the role '{role.Description}' assigned. Each residence can only have one user per role."
            };
        }

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

        var residenceDtos = new List<ResidenceDto>
        {
            new ResidenceDto
            {
                Id = residence.Id,
                Number = residence.Number,
                Floor = residence.Floor,
                Tower = residence.Tower
            }
        };

        return new UserDto
        {
            Success = true,
            Message = "User registered successfully. An email has been sent with the credentials.",
            Id = savedUser.Id,
            Email = savedUser.Mail,
            FirstName = user.Name,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            RoleId = savedUser.Role_id,
            TemporaryPassword = temporaryPassword,
            Residences = residenceDtos
        };
    }

    public async Task<int> GetAllUsersInNumber()
    {
        var totalUsers = await _userRepository.GetAllInNumber();
        return totalUsers; 
    }
}