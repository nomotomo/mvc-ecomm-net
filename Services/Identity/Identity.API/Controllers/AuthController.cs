using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity.API.Data;
using Identity.API.DTOS;
using Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        // consider moving into its own dedicated mapper
        var user = new ApplicationUser
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            UserName = registerDto.UserName,
            Email = registerDto.Email
        };
        
        var result = await _userManager.CreateAsync(user, registerDto.Password);
        _logger.LogInformation("User {UserName} register attempted.", user.UserName);
        if (!result.Succeeded)
        { 
            _logger.LogError($"User {user.UserName} failed to register.");
            return BadRequest(result.Errors);
        }
        _logger.LogInformation("User {UserName} registered successfully.", user.UserName);
        return Ok("Registration successful");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.password))
        {
            _logger.LogWarning("Login attempt failed for email {Email}. User not found.", loginDto.email);
            return Unauthorized("Invalid email or password.");
        }
        var token = GenerateToken(user);
        _logger.LogInformation("User {UserName} logged in successfully.", user.UserName);
        return Ok(new { Token = token });
    }

    private string GenerateToken(ApplicationUser user)
    {
        // Token generation logic here (e.g., using JWT)
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email?? string.Empty),
            new Claim("uid", user.Id),
        };
        
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"])),
            signingCredentials: creds);
        _logger.LogInformation("User {UserName} logged in and token is generated", user.Email);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}