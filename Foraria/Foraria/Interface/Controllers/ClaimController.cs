using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClaimController : ControllerBase
{

    public readonly CreateClaim CreateClaim;
    public ClaimController(CreateClaim CreateClaim)
    {
        this.CreateClaim = CreateClaim;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Claim Controller");
    }

    [HttpPost]
    public IActionResult Add(string Description, string State, string Priority, string Category, string Title, string? Archive, int? User_id)
    {
        CreateClaim.Execute(Title, Description, Priority, Category, Archive, User_id);
        return Ok("Claim Controller");
    }

}
