using Foraria.Domain.Repository;
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
        // 1. Validate email doesn't exist
        if (await _userRepository.ExistsEmail(userDto.Email))
        {
            return new UserDto
            {
                Success = false,
                Message = "Email is already registered in the system"
            };
        }

        // 2. Validate role exists
        var roleExists = await _roleRepository.Exists(userDto.RoleId);
        if (!roleExists)
        {
            return new UserDto
            {
                Success = false,
                Message = "Selected role is not valid"
            };
        }

        // 3. Validate and get residences (if provided)
        List<Residence> residenceEntities = new List<Residence>();
        if (userDto.Residences != null && userDto.Residences.Any())
        {
            foreach (var residenceDto in userDto.Residences)
            {
                // Validar que la residencia existe
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

                // Obtener la entidad Residence de la base de datos
                var residence = await _residenceRepository.GetById(residenceDto.Id.Value);
                if (residence != null)
                {
                    residenceEntities.Add(residence);
                }
            }
        }

        // 4. Generate temporary password
        var temporaryPassword = await _generatePasswordUseCase.Generate();
        var passwordHash = _passwordHashUseCase.HashPassword(temporaryPassword);

        // 5. Parse phone number
        if (!long.TryParse(userDto.Phone.Replace(" ", "").Replace("-", ""), out long phoneNumber))
        {
            return new UserDto
            {
                Success = false,
                Message = "Phone format is not valid"
            };
        }

        // 6. Create user with residences
        var newUser = new User
        {
            Name = userDto.FirstName,
            LastName = userDto.LastName,
            Mail = userDto.Email,
            Password = passwordHash,
            PhoneNumber = phoneNumber,
            Role_id = userDto.RoleId,
            RequiresPasswordChange = true,
            Residence = residenceEntities  // Asignar las entidades Residence
        };

        // 7. Save to database
        var savedUser = await _userRepository.Add(newUser);

        // 8. Send email with credentials 
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
            // TODO: Implement proper logging
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }

        // 9. Convert residences back to DTOs for response
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