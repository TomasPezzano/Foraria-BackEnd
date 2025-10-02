using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase;

public interface ILoginUser
{
    Task<LoginResponseDto> Login(LoginRequestDto loginDto);
}
public class LoginUser : ILoginUser
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHash _passwordHash;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUser(
        IUserRepository userRepository,
        IPasswordHash passwordHash,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHash = passwordHash;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponseDto> Login(LoginRequestDto loginDto)
    {
        // 1. Validate user exists
        var user = await _userRepository.GetByEmailWithRole(loginDto.Email);
        if (user == null)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // 2. Validate password
        var isPasswordValid = _passwordHash.VerifyPassword(loginDto.Password, user.Password);
        if (!isPasswordValid)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // 3. Generate JWT token
        var token = _jwtTokenGenerator.Generate(
            user.Id,
            user.Mail,
            user.Role_id,
            user.Role.Description,
            user.RequiresPasswordChange
        );

        // 4. Return success response
        return new LoginResponseDto
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            RequiresPasswordChange = user.RequiresPasswordChange,
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Mail,
                FirstName = user.Name,
                LastName = user.LastName,
                RoleId = user.Role_id,
                RoleName = user.Role.Description
            }
        };
    }
}
