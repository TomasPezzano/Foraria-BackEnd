using Foraria.Application.Services;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using ForariaDomain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Foraria.Controllers;

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
    private readonly IPermissionService _permissionService;

    public UserController(
        IRegisterUser registerUserService,
        ILoginUser loginUserService,
        ILogoutUser logoutUserService,
        IRefreshTokenUseCase refreshTokenUseCase,
        IUpdateUserFirstTime updateUserFirstTime,
        ILocalFileStorageService fileStorageService,
        IGetUserByEmail getUserByEmail,
        IGetUserById getUserById,
        IGetTotalTenantUsers getTotalTenantUsers,
        IGetTotalOwnerUsers getTotalOwnerUsers,
        IGetUsersByConsortium getUsersByConsortium,
        IResetPassword resetPassword,
        IForgotPassword forgotPassword,
        IPermissionService permissionService)
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
        _permissionService = permissionService;
    }


    [HttpPost("register")]
    [SwaggerOperation(Summary = "Registra un nuevo usuario.", Description = "Crea un usuario con residencia asignada y genera una contraseña temporal.")]
    [ProducesResponseType(typeof(RegisterUserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto request)
    {
        //await _permissionService.EnsurePermissionAsync(User, "Users.Register");

        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos del usuario no son válidos.");

        //if (request.ResidenceId.HasValue && !request.ConsortiumId.HasValue)
        //{
        //    // Obtener la residencia para saber a qué consorcio pertenece
        //    var residence = await _residenceRepository.GetByIdWithoutFilters(request.ResidenceId.Value);
        //    if (residence == null)
        //        throw new NotFoundException("Residencia no encontrada.");

        //    // Verificar que el usuario actual tenga acceso a ese consorcio
        //    var currentUserConsortiums = _permissionService.GetUserConsortiumIds(User);
        //    if (!currentUserConsortiums.Contains(residence.ConsortiumId))
        //        throw new ForbiddenAccessException(
        //            "No tienes permiso para registrar usuarios en este consorcio.");
        //}

        var user = new User
        {
            Name = request.FirstName,
            LastName = request.LastName,
            Mail = request.Email,
            PhoneNumber = request.PhoneNumber,
            Role_id = request.RoleId,
        };

        var result = await _registerUserService.Register(user, request.ResidenceId, request.ConsortiumId);

        var response = new RegisterUserResponseDto
        {
            Id = result.Id,
            Email = result.Mail,
            FirstName = result.Name,
            LastName = result.LastName,
            PhoneNumber = result.PhoneNumber,
            RoleId = result.Role_id,
            TemporaryPassword = result.Password,
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
    [SwaggerOperation(Summary = "Inicia sesión de usuario.", Description = "Autentica al usuario y devuelve tokens JWT y de refresco.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos de inicio de sesión no son válidos.");

        var ipAddress = GetIpAddress();

        var usuarioLogeado = await _getUserByEmail.Execute(request.Email);

        if (usuarioLogeado == null)
            throw new UnauthorizedException("Credenciales inválidas.");

        var result = await _loginUserService.Login(usuarioLogeado, request.Password, ipAddress);

        return Ok(result);
    }

    [Authorize(Policy = "OwnerAndTenant")]
    [HttpPost("update-first-time")]
    [SwaggerOperation(Summary = "Completa el registro inicial del usuario.", Description = "Permite actualizar los datos personales y contraseña en el primer inicio.")]
    [ProducesResponseType(typeof(UpdateUserFirstTimeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateFirstTime([FromForm] UpdateUserFirstTimeRequestDto request)
    {
        await _permissionService.EnsurePermissionAsync(User, "Users.UpdateFirstTime");

        if (!ModelState.IsValid)
            throw new DomainValidationException("Datos inválidos para actualización inicial.");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedException("Token inválido.");

        if (request.NewPassword != request.ConfirmNewPassword)
            throw new DomainValidationException("Las contraseñas nuevas no coinciden.");

        if (string.IsNullOrWhiteSpace(request.Dni) || !long.TryParse(request.Dni, out _))
            throw new DomainValidationException("El DNI es inválido.");

        var user = await _getUserById.Execute(userId);

        string? photoPath = null;
        if (request.Photo != null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var maxFileSize = 5 * 1024 * 1024;

            var photoResult = await _fileStorageService.SavePhotoAsync(request.Photo, "user-photos", allowedExtensions, maxFileSize);
            if (!photoResult.Success)
                throw new DomainValidationException(photoResult.ErrorMessage ?? "Error al guardar la foto.");

            photoPath = photoResult.FilePath;
        }

        var ipAddress = GetIpAddress();
        user.Name = request.FirstName;
        user.LastName = request.LastName;
        user.Dni = long.Parse(request.Dni);
        user.Photo = photoPath;

        var result = await _updateUserFirstTime.Update(user, request.CurrentPassword, request.NewPassword, ipAddress);

        if (!result.Success)
            throw new BusinessException(result.Message);

        return Ok(new UpdateUserFirstTimeResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            Token = result.AccessToken,
            RefreshToken = result.RefreshToken
        });
    }

    [Authorize(Policy = "All")]
    [HttpPost("refresh")]
    [SwaggerOperation(Summary = "Refresca el token JWT.", Description = "Devuelve un nuevo token JWT y de refresco si el actual es válido.")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        await _permissionService.EnsurePermissionAsync(User, "Users.RefreshToken");

        if (!ModelState.IsValid)
            throw new DomainValidationException("Solicitud de refresh token inválida.");

        var ipAddress = GetIpAddress();
        var result = await _refreshTokenUseCase.Refresh(request.RefreshToken, ipAddress);

        if (!result.Success)
            throw new UnauthorizedException(result.Message);

        return Ok(result);
    }

    [Authorize(Policy = "All")]
    [HttpPost("logout")]
    [SwaggerOperation(Summary = "Cierra sesión del usuario.", Description = "Invalida el refresh token y cierra la sesión activa.")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        await _permissionService.EnsurePermissionAsync(User, "Users.Logout");

        if (!ModelState.IsValid)
            throw new DomainValidationException("Solicitud inválida de logout.");

        var ipAddress = GetIpAddress();
        await _logoutUserService.Logout(request.RefreshToken, ipAddress);

        return Ok(new { message = "Logout successful" });
    }

    [Authorize(Policy = "ConsortiumAndAdmin")]
    [HttpGet("count")]
    [SwaggerOperation(Summary = "Obtiene la cantidad total de usuarios.", Description = "Devuelve el total de usuarios registrados en el sistema.")]
    public async Task<IActionResult> GetUsersCount(int consortiumId)
    {
        await _permissionService.EnsurePermissionAsync(User, "Users.ViewCount");

        var totalUsers = await _registerUserService.GetAllUsersInNumber();
        return Ok(new { totalUsers });
    }

    [HttpGet]
    [Authorize(Policy = "All")]
    [SwaggerOperation(Summary = "Obtiene un usuario por su ID.", Description = "Devuelve los datos básicos del usuario solicitado.")]
    public async Task<IActionResult> GetUserById([FromQuery] int id)
    {
        await _permissionService.EnsurePermissionAsync(User, "Users.ViewById");

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
            Photo = user.Photo,
            Email = user.Mail,
            PhoneNumber = user.PhoneNumber,
            Dni = user.Dni,
            RoleId = user.Role_id,
            RoleDescription = user.Role.Description,
            ResidenceId = residenceId,
            Floor = residence?.Floor,
            NumberFloor = residence?.Number,
            ConsortiumId = consortiumId,
            Success = true
        };

        return Ok(response);
    }

    [HttpGet("totalTenants")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(Summary = "Obtiene el total de inquilinos por consorcio.", Description = "Devuelve el número de usuarios con rol de inquilino en un consorcio.")]
    public async Task<IActionResult> GetTotalTenantsByConsortiumIdAsync([FromQuery] int consortiumId)
    {
        await _permissionService.EnsurePermissionAsync(User, "Users.ViewTotalTenants");

        var totalTenants = await _getTotalTenantUsers.ExecuteAsync();
        return Ok(new { totalTenants });
    }

    [HttpGet("totalOwners")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(Summary = "Obtiene el total de propietarios por consorcio.", Description = "Devuelve el número de usuarios con rol de propietario en un consorcio.")]
    public async Task<IActionResult> GetTotalOwnersByConsortiumIdAsync([FromQuery] int consortiumId)
    {
        await _permissionService.EnsurePermissionAsync(User, "Users.ViewTotalOwners");

        var totalOwners = await _getTotalOwnerUsers.ExecuteAsync();
        return Ok(new { totalOwners });
    }

    [HttpGet("consortium/{consortiumId}")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(Summary = "Obtiene los usuarios de un consorcio.", Description = "Devuelve la lista completa de usuarios y sus residencias asociadas.")]
    public async Task<IActionResult> GetUsersByConsortium(int consortiumId)
    {
        await _permissionService.EnsurePermissionAsync(User, "Users.ViewByConsortium");

        var users = await _getUsersByConsortium.ExecuteAsync();

        var usersDto = users.Select(u => new UserDetailDto
        {
            Id = u.Id,
            FirstName = u.Name,
            LastName = u.LastName,
            Mail = u.Mail,
            PhoneNumber = u.PhoneNumber,
            Role = u.Role.Description,
            ConsortiumId = consortiumId,
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

    [HttpPost("forgot-password")]
    [SwaggerOperation(Summary = "Solicita restablecer contraseña.", Description = "Envía un correo al usuario con un enlace de restablecimiento de contraseña.")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Solicitud de recuperación inválida.");

        var result = await _forgotPassword.Execute(request.Email, GetIpAddress());
        return Ok(new { message = result.Message });
    }

    [HttpPost("reset-password")]
    [SwaggerOperation(Summary = "Restablece la contraseña del usuario.", Description = "Permite establecer una nueva contraseña usando el token recibido por correo.")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Solicitud inválida de restablecimiento de contraseña.");

        var result = await _resetPassword.Execute(request.Token, request.NewPassword, GetIpAddress());

        if (!result.Success)
            throw new BusinessException(result.Message);

        return Ok(new { message = result.Message });
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}