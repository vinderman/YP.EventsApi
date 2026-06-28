using Microsoft.AspNetCore.Mvc;
using Yp.EventsApi.Application.Services.UserService;
using Yp.EventsApi.Presentation.Contracts;

namespace Yp.EventsApi.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController: ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    { 
        _userService = userService;  
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterUserDto registerUserDto, CancellationToken ct)
    {
        await _userService.Register(registerUserDto.login, registerUserDto.password, registerUserDto.Role, ct);

        return Created();
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(string login, string password, CancellationToken ct)
    {
        var token = await _userService.Login(login, password, ct);
        
        return Ok(token);
    }
}