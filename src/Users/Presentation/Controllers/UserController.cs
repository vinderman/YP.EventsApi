using Application.Services.UserService;
using Microsoft.AspNetCore.Mvc;
using Presentation.Contracts;

namespace Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController: ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    { 
        _userService = userService;  
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterUserDto registerUserDto, CancellationToken ct)
    {
        await _userService.Register(registerUserDto.login, registerUserDto.password, registerUserDto.Role, ct);

        return NoContent();
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