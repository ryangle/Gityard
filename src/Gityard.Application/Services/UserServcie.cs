using Gityard.Application.Dtos;
using Gityard.Application.Models;
using LiteDB;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace Gityard.Application.Services;

public class UserServcie
{
    private readonly LiteDatabase _database;
    private readonly IMemoryCache _memoryCache;
    public ILiteCollection<GityardUser> Users => _database.GetCollection<GityardUser>();
    private JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
    private JwtOptions _jwtOptions;
    public UserServcie(IOptionsSnapshot<DbOptions> dbOptions, IOptionsSnapshot<JwtOptions> jwtOptions, IMemoryCache memoryCache)
    {
        _jwtOptions = jwtOptions.Value;
        _database = new LiteDatabase(dbOptions.Value.DefaultConnection);
        Users.EnsureIndex(x => x.Id);

        _memoryCache = memoryCache;
    }

    public List<GityardUser> GetAllUser()
    {
        return Users.FindAll().ToList();
    }
    public GityardUser? Get(string id)
    {
        var cacheKey = $"user-{id}";
        if (!_memoryCache.TryGetValue(cacheKey, out GityardUser user))
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(3));
            var temp = Users.Find(x => x.Id == id).FirstOrDefault();
            if (temp == null)
            {
                return null;
            }
            user = temp;
            _memoryCache.Set(cacheKey, user, cacheEntryOptions);
        }
        return user;

    }
    public GityardUserDto? Get(string username, string password)
    {
        return Users
            .Find(x => x.Name == username && x.Password == password)
            .Select(u => new GityardUserDto(u.Name, "", u.Email))
            .FirstOrDefault();
    }
    public ResponseResult<string> Login(string username, string password)
    {
        var user = Get(username, password);
        if (user == null)
        {
            return new ResponseResult<string> { Data = "" };
        }
        return CreateToken(user.Name);
    }
    public void Delete(string id)
    {
        Users.Delete(id);
    }

    public ResponseResult<string> Create(GityardUserDto userDto)
    {
        if (userDto == null || string.IsNullOrEmpty(userDto.Name) || string.IsNullOrEmpty(userDto.Password))
        {
            return new ResponseResult<string> { Data = "" };
        }
        var userExist = Users.Find(x => x.Name == userDto.Name).FirstOrDefault();
        if (userExist != null)
        {
            return new ResponseResult<string> { Data = "" };
        }
        GityardUser user = new()
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = userDto.Name,
            Email = userDto.Email,
            Password = userDto.Password
        };
        var b = Users.Insert(user);
        return new ResponseResult<string> { Data = b.ToString() };
    }

    private ResponseResult<string> CreateToken(string userName)
    {
        var expire = DateTime.UtcNow.AddSeconds(_jwtOptions.ExpireSeconds);

        var claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.Name, userName));
        claims.Add(new Claim("role", "org"));


        var sceKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecKey));
        var credentials = new SigningCredentials(sceKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(claims: claims, expires: expire, signingCredentials: credentials);


        var token = _tokenHandler.WriteToken(tokenDescriptor);

        return new ResponseResult<string> { Data = token };
    }
    public IEnumerable<string> ValidToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return new List<string>();
        }
        try
        {
            TokenValidationParameters parameters = new();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecKey));
            parameters.IssuerSigningKey = securityKey;
            parameters.ValidateIssuer = false;
            parameters.ValidateAudience = false;
            ClaimsPrincipal claimsPrincipal = _tokenHandler.ValidateToken(token, parameters, out var securityToken);

            return claimsPrincipal.Claims.Select(c => c.Value);
        }
        catch (Exception)
        {
            return new List<string>();
        }
    }
}
