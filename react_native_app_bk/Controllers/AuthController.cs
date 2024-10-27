using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using react_native_app_bk.Dtos;
using react_native_app_bk.Models;
using react_native_app_bk.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _userService = userService;
        _configuration = configuration;
        _logger = logger;
    }

    // Action to register a new user
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        // Verifies if the email already exists
        if (await _userService.EmailExists(model.Email))
        {
            return BadRequest("The email is already in use.");   
        }

        // Create the password hash
        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        await _userService.CreateUser(user);
        return Ok("User successfully registered.");
    }

    // Action to login a user

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        _logger.LogInformation("Login attempt for email: {Email}", model.Email);

        var user = await _userService.GetUserByEmail(model.Email);

        // Check if the user exists
        if (user == null)
        {
            _logger.LogWarning("Login failed: Invalid email - {Email}", model.Email);
            return Unauthorized("Invalid email or password.");
        }

        // Check if the password is correct
        if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Incorrect password for email - {Email}", model.Email);
            return Unauthorized("Invalid email or password.");
        }

        // Create the JWT token
        var token = GenerateJwtToken(user);
        _logger.LogInformation("Login successful for email: {Email}", model.Email);
        return Ok(new { Token = token });
    }

    [HttpPost("check-auth")]
    public IActionResult CheckAuth()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer"))
        {
            return Unauthorized("No token provided.");
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            return Ok("Authenticated");
        }
        catch
        {
            return Unauthorized("Invalid token.");
        }   
    }

    // Method to generate the JWT token
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}