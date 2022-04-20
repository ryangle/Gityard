using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Gityard.Web;
public class BasicAuthenticationOptions : AuthenticationSchemeOptions, IOptions<BasicAuthenticationOptions>
{
    public BasicAuthenticationOptions Value => this;
    public string? Realm { get; set; }
}
