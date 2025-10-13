using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public interface IRegisterUser
{
    Task<UserDto> Register(UserDto userDto);
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

    public async Task<UserDto> Register(UserDto userDto)
    {
        if (await _userRepository.ExistsEmail(userDto.Email))
        {
            return new UserDto
            {
                Success = false,
                Message = "Email is already registered in the system"
            };
        }

        var roleExists = await _roleRepository.Exists(userDto.RoleId);
        if (!roleExists)
        {
            return new UserDto
            {
                Success = false,
                Message = "Selected role is not valid"
            };
        }

        List<Residence> residenceEntities = new List<Residence>();
        if (userDto.Residences != null && userDto.Residences.Any())
        {
            foreach (var residenceDto in userDto.Residences)
            {
                if (!residenceDto.Id.HasValue)
                {
                    return new UserDto
                    {
                        Success = false,
                        Message = "All residences must have a valid ID"
                    };
                }

                var residenceExists = await _residenceRepository.Exists(residenceDto.Id.Value);
                if (!residenceExists)
                {
                    return new UserDto
                    {
                        Success = false,
                        Message = $"Residence with ID {residenceDto.Id} does not exist"
                    };
                }

                var residence = await _residenceRepository.GetById(residenceDto.Id.Value);
                if (residence != null)
                {
                    residenceEntities.Add(residence);
                }
            }
        }

        var temporaryPassword = await _generatePasswordUseCase.Generate();
        var passwordHash = _passwordHashUseCase.HashPassword(temporaryPassword);

        if (!long.TryParse(userDto.Phone.Replace(" ", "").Replace("-", ""), out long phoneNumber))
        {
            return new UserDto
            {
                Success = false,
                Message = "Phone format is not valid"
            };
        }

        var newUser = new User
        {
            Name = userDto.FirstName,
            LastName = userDto.LastName,
            Mail = userDto.Email,
            Password = passwordHash,
            PhoneNumber = phoneNumber,
            Role_id = userDto.RoleId,
            RequiresPasswordChange = true,
            Residences = residenceEntities 
        };

        var savedUser = await _userRepository.Add(newUser);

        try
        {
            await _emailUseCase.SendWelcomeEmail(
                userDto.Email,
                userDto.FirstName,
                userDto.LastName,
                temporaryPassword);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }

        var residenceDtos = residenceEntities.Select(r => new ResidenceDto
        {
            Id = r.Id,
            Number = r.Number,
            Floor = r.Floor,
            Tower = r.Tower
        }).ToList();

        return new UserDto
        {
            Success = true,
            Message = "User registered successfully. An email has been sent with the credentials.",
            Id = savedUser.Id,
            Email = savedUser.Mail,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Phone = userDto.Phone,
            RoleId = savedUser.Role_id,
            TemporaryPassword = temporaryPassword,
            Residences = residenceDtos
        };
    }
}