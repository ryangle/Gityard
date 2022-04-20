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
    public ResponseResult<string> Register([FromBody] GityardUserDto userGityardDto)
    {
        return _userServcie.Create(userGityardDto);
    }
    [Authorize]
    [HttpGet("user")]
    public ResponseResult<IEnumerable<GityardUserDto>> GetUsers()
    {
        return new ResponseResult<IEnumerable<GityardUserDto>> { Data = _userServcie.GetAllUser().Select(u => new GityardUserDto(u.Name, "", u.Email)) };
    }
    [HttpPost("login")]
    public ResponseResult<string> Login([FromBody] GityardUserDto userGityardDto)
    {
        return _userServcie.Login(userGityardDto.Name, userGityardDto.Password);
    }
}
