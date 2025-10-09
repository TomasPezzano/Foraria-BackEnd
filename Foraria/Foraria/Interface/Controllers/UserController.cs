using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IRegisterUser _registerUserService;

    public UserController(IRegisterUser registerUserService)
    {
        _registerUserService = registerUserService;
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
}