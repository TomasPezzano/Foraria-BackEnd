using Foraria.Application.UseCase;
using Foraria.Contracts.DTOs;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Foraria.Interface.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IRegisterUser _registerUserService;
    private readonly ILoginUser _loginUserService;
    private readonly ILogoutUser _logoutUserService;
    private readonly IRefreshTokenUseCase _refreshTokenUseCase;
    private readonly IUpdateUserFirstTime _updateUserFirstTime;
    private readonly ILocalFileStorageService _fileStorageService;
    private readonly IGetUserByEmail _getUserByEmail;
    private readonly IGetUserById _getUserById;
    private readonly IGetTotalTenantUsers _getTotalTenantUsers;
    private readonly IGetTotalOwnerUsers _getTotalOwnerUsers;
    private readonly IGetUsersByConsortium _getUsersByConsortium;
    private readonly IResetPassword _resetPassword;
    private readonly IForgotPassword _forgotPassword;

    public UserController(IRegisterUser registerUserService, ILoginUser loginUserService, ILogoutUser logoutUserService, IRefreshTokenUseCase refreshTokenUseCase, IUpdateUserFirstTime updateUserFirstTime, ILocalFileStorageService fileStorageService, IGetUserByEmail getUserByEmail, IGetUserById getUserById, IGetTotalTenantUsers getTotalTenantUsers, IGetTotalOwnerUsers getTotalOwnerUsers, IGetUsersByConsortium getUsersByConsortium, IResetPassword resetPassword, IForgotPassword forgotPassword)
    {
        _registerUserService = registerUserService;
        _loginUserService = loginUserService;
        _logoutUserService = logoutUserService;
        _refreshTokenUseCase = refreshTokenUseCase;
        _updateUserFirstTime = updateUserFirstTime;
        _fileStorageService = fileStorageService;
        _getUserByEmail = getUserByEmail;
        _getUserById = getUserById;
        _getTotalTenantUsers = getTotalTenantUsers;
        _getTotalOwnerUsers = getTotalOwnerUsers;
        _getUsersByConsortium = getUsersByConsortium;
        _resetPassword = resetPassword;
        _forgotPassword = forgotPassword;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }


        var user = new User
        {
            Name = request.FirstName,
            LastName = request.LastName,
            Mail = request.Email,
            PhoneNumber = request.PhoneNumber,
            Role_id = request.RoleId,
        };

        var result = await _registerUserService.Register(user, request.ResidenceId);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        var response = new RegisterUserResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            Id = result.Id,
            Email = result.Email,
            FirstName = result.FirstName,
            LastName = result.LastName,
            PhoneNumber = result.PhoneNumber,
            RoleId = result.RoleId,
            TemporaryPassword = result.TemporaryPassword,
            Residences = result.Residences?.Select(r => new ResidenceResponseDto
            {
                Id = r.Id,
                Number = r.Number,
                Floor = r.Floor,
                Tower = r.Tower,
                Success = true
            }).ToList()
        };

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = GetIpAddress();
        User usuarioLogeado = await _getUserByEmail.Execute(request.Email);
        string passRequest = request.Password;

        var result = await _loginUserService.Login(usuarioLogeado, passRequest, ipAddress);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(result);
    }

    [Authorize(Policy = "OwnerAndTenant")]
    [HttpPost("update-first-time")]
    public async Task<IActionResult> UpdateFirstTime([FromForm] UpdateUserFirstTimeRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
    
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Token inválido" });
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return BadRequest(new { message = "Las contraseñas nuevas no coinciden" });
        }

        if (string.IsNullOrWhiteSpace(request.Dni) || !long.TryParse(request.Dni, out long dniNumber))
        {
            return BadRequest(new { message = "El DNI es inválido" });
        }

        User user = await _getUserById.Execute(userId);

        string? photoPath = null;
        if (request.Photo != null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var maxFileSize = 5 * 1024 * 1024; // 5 MB

            var photoResult = await _fileStorageService.SavePhotoAsync(
                request.Photo,
                "user-photos",
                allowedExtensions,
                maxFileSize);

            if (!photoResult.Success)
            {
                return BadRequest(new { message = photoResult.ErrorMessage ?? "Error al guardar la foto" });
            }

            photoPath = photoResult.FilePath;
        }

        var ipAddress = GetIpAddress();

        user.Name = request.FirstName;
        user.LastName = request.LastName;
        user.Dni = long.TryParse(request.Dni, out var dniValue) ? dniValue : (long?)null;
        user.Photo = photoPath;
        string currentPassword = request.CurrentPassword;
        string newPassword = request.NewPassword;


        var result = await _updateUserFirstTime.Update(user, currentPassword, newPassword, ipAddress);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        var response = new UpdateUserFirstTimeResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            Token = result.AccessToken,
            RefreshToken = result.RefreshToken
        };

        return Ok(response);
    }

    [Authorize(Policy = "All")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = GetIpAddress();
        var refreshToken = request.RefreshToken;

        var result = await _refreshTokenUseCase.Refresh(refreshToken, ipAddress);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(result);
    }

    [Authorize(Policy = "All")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = GetIpAddress();
        var refreshToken = request.RefreshToken;

        var result = await _logoutUserService.Logout(refreshToken, ipAddress);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    [Authorize(Policy = "All")]
    [HttpGet("count")]
    public async Task<IActionResult> GetUsersCount()
    {
        try
        {
            var totalUsers = await _registerUserService.GetAllUsersInNumber();
            return Ok(new { totalUsers });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al obtener cantidad de usuarios", detail = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Policy = "All")]
    public async Task<IActionResult> GetUserById([FromQuery] int id)
    {
        var user = await _getUserById.Execute(id);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }
        var residence = user.Residences?.FirstOrDefault();
        int? residenceId = residence?.Id;
        int? consortiumId = residence?.ConsortiumId;

        var response = new UserDto
        {
            Id = user.Id,
            FirstName = user.Name,
            LastName = user.LastName,
            Email = user.Mail,
            PhoneNumber = user.PhoneNumber,
            RoleId = user.Role_id,
            ResidenceId = residenceId,
            ConsortiumId = consortiumId,
            Success = true
        };
        return Ok(response);
    }

    [HttpGet ("totalTenants")]
    [Authorize(Policy = "All")]
    public async Task<IActionResult> GetTotalTenantsByConsortiumIdAsync([FromQuery] int consortiumId)
    {
        try
        {
            var totalTenants = await _getTotalTenantUsers.ExecuteAsync(consortiumId);
            return Ok(new { totalTenants });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detail = ex.Message });
        }
    }

    [HttpGet("totalOwners")]
    [Authorize(Policy = "All")]
    public async Task<IActionResult> GetTotalOwnersByConsortiumIdAsync([FromQuery] int consortiumId)
    {
        try
        {
            var totalOwners = await _getTotalOwnerUsers.ExecuteAsync(consortiumId);
            return Ok(new { totalOwners });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detail = ex.Message });
        }
    }

    [HttpGet("consortium/{consortiumId}")]
    [Authorize(Policy = "All")]
    public async Task<IActionResult> GetUsersByConsortium(int consortiumId)
    {
        try
        {
            var users = await _getUsersByConsortium.ExecuteAsync(consortiumId);

            var usersDto = users.Select(u => new UserDetailDto
            {
                Id = u.Id,
                FirstName = u.Name,
                LastName = u.LastName,
                Mail = u.Mail,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role.Description,
                Residences = u.Residences.Select(r => new ResidenceDto
                {
                    Id = r.Id,
                    Floor = r.Floor,
                    Number = r.Number,
                    ConsortiumId = r.ConsortiumId
                }).ToList()
            }).ToList();

            return Ok(usersDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al obtener usuarios del consorcio", detail = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var email = request.Email;

        var ipAddress = GetIpAddress();
        var result = await _forgotPassword.Execute(email, ipAddress);

        return Ok(new { message = result.Message });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var token = request.Token;
        var newPassworn = request.NewPassword;

        var ipAddress = GetIpAddress();
        var result = await _resetPassword.Execute(token, newPassworn, ipAddress);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }



    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}