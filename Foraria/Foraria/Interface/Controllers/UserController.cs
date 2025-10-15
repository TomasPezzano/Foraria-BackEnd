using Foraria.Application.UseCase;
using Foraria.Contracts.DTOs;
using Foraria.Interface.DTOs;
using ForariaDomain;
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
    private readonly IFileStorageService _fileStorageService;

    public UserController(IRegisterUser registerUserService, ILoginUser loginUserService, ILogoutUser logoutUserService, IRefreshTokenUseCase refreshTokenUseCase, IUpdateUserFirstTime updateUserFirstTime, IFileStorageService fileStorageService)
    {
        _registerUserService = registerUserService;
        _loginUserService = loginUserService;
        _logoutUserService = logoutUserService;
        _refreshTokenUseCase = refreshTokenUseCase;
        _updateUserFirstTime = updateUserFirstTime;
        _fileStorageService = fileStorageService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userDto = new UserDto
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            RoleId = request.RoleId,
            Residences = request.Residences
        };

        var result = await _registerUserService.Register(userDto);

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
            Phone = result.Phone,
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
        var result = await _loginUserService.Login(request, ipAddress);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(result);
    }

    [Authorize] 
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
        var result = await _updateUserFirstTime.Update(
            userId: userId,
            currentPassword: request.CurrentPassword,
            newPassword: request.NewPassword,
            firstName: request.FirstName,
            lastName: request.LastName,
            dni: dniNumber,
            photoPath: photoPath,
            ipAddress: ipAddress
        );

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

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = GetIpAddress();
        var result = await _refreshTokenUseCase.Refresh(request.RefreshToken, ipAddress);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = GetIpAddress();
        var result = await _logoutUserService.Logout(request.RefreshToken, ipAddress);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        await _registerUserService.GetAllUsers();
        return Ok();
    }

    // Helper method to get client IP address
    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }


}