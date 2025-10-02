using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IRegisterUser _registerUserService;
    private readonly ILoginUser _loginUserService;

    public UserController(IRegisterUser registerUserService, ILoginUser loginUserService)
    {
        _registerUserService = registerUserService;
        _loginUserService = loginUserService;
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

        var result = await _loginUserService.Login(request);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(result);
    }
}