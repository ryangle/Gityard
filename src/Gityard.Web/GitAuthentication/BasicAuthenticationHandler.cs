using Gityard.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Gityard.Web;

public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
{
    private UserServcie _userServcie;
    public BasicAuthenticationHandler(UserServcie userServcie, IOptionsMonitor<BasicAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        _userServcie = userServcie;
    }
    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.NoResult();
        }

        string authHeader = Request.Headers["Authorization"];
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        string token = authHeader.Substring("Basic ".Length).Trim();
        string credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        string[] credentials = credentialString.Split(':');

        if (credentials.Length != 2)
        {
            return AuthenticateResult.Fail("More than two strings seperated by colons found");
        }

        ClaimsPrincipal? principal = await SignInAsync(credentials[0], credentials[1]);

        if (principal != null)
        {
            AuthenticationTicket ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), BasicAuthenticationDefaults.AuthenticationScheme);
            return AuthenticateResult.Success(ticket);
        }

        return AuthenticateResult.Fail("Wrong credentials supplied");
    }
    private Task<ClaimsPrincipal?> SignInAsync(string userName, string password)
    {
        var result = _userServcie.Login(userName, password);
        if (!string.IsNullOrEmpty(result.Data))
        {
            var identity = new ClaimsIdentity(BasicAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.Name, userName));
            var principal = new ClaimsPrincipal(identity);
            return Task.FromResult<ClaimsPrincipal?>(principal);
        }
        return Task.FromResult<ClaimsPrincipal?>(null);
    }
    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        return base.HandleForbiddenAsync(properties);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        string headerValue = $"{BasicAuthenticationDefaults.AuthenticationScheme} realm=\"{Options.Realm}\"";
        Response.Headers.Append(Microsoft.Net.Http.Headers.HeaderNames.WWWAuthenticate, headerValue);
        return base.HandleChallengeAsync(properties);
    }
}

