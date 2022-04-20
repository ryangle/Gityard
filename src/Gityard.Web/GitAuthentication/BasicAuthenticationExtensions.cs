using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Gityard.Web;
public static class BasicAuthenticationExtensions
{
    public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder)
        => builder.AddBasic(BasicAuthenticationDefaults.AuthenticationScheme, _ => { _.Realm = "Gityard"; });

    public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, Action<BasicAuthenticationOptions> configureOptions)
        => builder.AddBasic(BasicAuthenticationDefaults.AuthenticationScheme, configureOptions);

    public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, string authenticationScheme, Action<BasicAuthenticationOptions> configureOptions)
        => builder.AddBasic(authenticationScheme, displayName: null, configureOptions: configureOptions);

    public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, string authenticationScheme, string? displayName, Action<BasicAuthenticationOptions> configureOptions)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptions<BasicAuthenticationOptions>, BasicAuthenticationOptions>());
        return builder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
    }
}
