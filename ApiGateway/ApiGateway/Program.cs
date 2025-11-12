using ApiGateway.Middleware;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// jwt setting
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var key = jwtSection["SecretKey"];
if (string.IsNullOrEmpty(key))
{
    throw new InvalidOperationException("JWT SecretKey is missing from configuration. Please ensure 'JwtSettings:SecretKey' is set.");
}
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// add jwt to ocelot
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOcelot();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
await app.UseOcelot();
app.Run();