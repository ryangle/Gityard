using Gityard;
using Gityard.Application;
using Gityard.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Host.UseSerilog();

builder.Services.Configure<GityardSettings>(configuration.GetSection(nameof(GityardSettings)));

builder.Services.AddTransient<GitRepositoryService>();
builder.Services.AddDbContext<GityardContext>(options => options.UseSqlite(connectionString));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Gityard Api", Version = "v1" });
});

var app = builder.Build();

//app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gityard Api");
});
app.MapControllers();

app.Run();