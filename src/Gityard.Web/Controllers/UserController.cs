using Gityard.Application.Dtos;
using Gityard.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Gityard.Web.Controllers;

[Produces("application/json")]
[ApiController]
[Route("")]
public class UserController : ControllerBase
{
    private UserServcie _userServcie;
    public UserController(UserServcie userServcie)
    {
        _userServcie = userServcie;
    }

    [HttpPost("user")]
    public string Register([FromBody] GityardUserDto userGityardDto)
    {
        return _userServcie.Create(userGityardDto);
    }
    [Authorize]
    [HttpGet("user")]
    public IEnumerable<GityardUserDto> GetUsers()
    {
        return _userServcie.GetAllUser().Select(u => new GityardUserDto(u.Name, "", u.Email));
    }
    [HttpPost("login")]
    public string Login([FromBody] GityardUserDto userGityardDto)
    {
        return _userServcie.Login(userGityardDto.Name, userGityardDto.Password);
    }
    [HttpPost("valid")]
    public IEnumerable<string> Login([FromBody] string token)
    {
        return _userServcie.ValidToken(token);
    }

}
