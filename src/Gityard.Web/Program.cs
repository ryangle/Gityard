using Gityard;
using Gityard.Application;
using Gityard.Application.Services;
using Gityard.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Host.UseSerilog();

builder.Services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));

builder.Services.Configure<GityardOptions>(configuration.GetSection(nameof(GityardOptions)));
builder.Services.Configure<DbOptions>(configuration.GetSection(nameof(DbOptions)));


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        var config = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
        var sceKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecKey));
        opt.TokenValidationParameters = new()
        {
            IssuerSigningKey = sceKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddTransient<UserServcie>();
builder.Services.AddTransient<GitRepositoryService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Gityard Api", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Description = " ‰»Î£∫Bearer <Token>",
        Name = "Authorization",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme },
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    };

    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, scheme);

    OpenApiSecurityRequirement requirement = new();
    requirement[scheme] = new List<string>();
    c.AddSecurityRequirement(requirement);
});

var app = builder.Build();

//app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gityard Api");
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();