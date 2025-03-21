using System.Text;
using BasketService.Application.Interfaces;
using BasketService.Application.Services;
using BasketService.Domain.Interfaces;
using BasketService.Infrastructure.Repositories;
using BasketService.Domain.Strategies;
using BasketService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["JWT:Secret"] ?? "no_secret_key")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddAuthorization();

// Register repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IBasketService, BasketService.Application.Services.BasketService>();
builder.Services.AddScoped<IBasketItemRepository, BasketItemRepository>();
builder.Services.AddScoped<IBasketHistoryRepository, BasketHistoryRepository>();
builder.Services.AddScoped<IBasketHistoryService, BasketHistoryService>();
builder.Services.AddSingleton<IEnumerable<IDiscountStrategy>>(sp => new List<IDiscountStrategy>
{
    new AppleDiscountStrategy(),
    new SoupBreadDiscountStrategy()
});

// Register database setup service
builder.Services.AddHostedService<DbSetup>();

var app = builder.Build();

app.Urls.Clear();

var serverHost = app.Configuration["Server:Host"];
var serverPort = app.Configuration["Server:Port"];

// If configured, set the app URL
if (!string.IsNullOrEmpty(serverHost) && !string.IsNullOrEmpty(serverPort))
{
    app.Urls.Add($"http://{serverHost}:{serverPort}");
}
else
{
    // Fallback to default
    app.Urls.Add("http://*:8081");
}

// Allowing this since it's not a production ready application
// In a production scenario we would have to set the hosts which are allowed
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseDefaultFiles();
app.UseStaticFiles();

var clientAppDistPath = Path.Combine(Directory.GetCurrentDirectory(), "Web", "ClientApp", "dist", "client-app", "browser");

if (!Directory.Exists(clientAppDistPath))
{
    clientAppDistPath = Path.Combine(Directory.GetCurrentDirectory(), "Web", "ClientApp", "dist", "client-app");
}

if (Directory.Exists(clientAppDistPath))
{
    Console.WriteLine($"Found Angular dist path: {clientAppDistPath}");
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(clientAppDistPath),
        RequestPath = ""
    });

    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(clientAppDistPath)
    });
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.Run();