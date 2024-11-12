namespace react_native_app_bk.Controllers;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using react_native_app_bk.Models.User;
using react_native_app_bk.Models.User.Dtos;
using react_native_app_bk.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        // Check if the model is valid
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Check if the email is already in use
            if (await _userService.EmailExists(model.Email))
            {
                return BadRequest("The email is already in use.");
            }

            // Create the user
            var user = new User
            {
                Username = model.Name,
                Email = model.Email,
                PasswordHash = BCrypt.HashPassword(model.Password)
            };

            await _userService.CreateUser(user);

            return Ok("User successfully registered.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error ocurred during user registration.");
            return StatusCode(500, "An error ocurred while creating the user.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error ocurred during user registration.");
            return StatusCode(500, "An error occurred while registering the user.");
        }
    }

    // Action to login a user
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        // Check if the model is valid
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _userService.GetUserByEmail(model.Email);

            // Check if the the password is correct
            if (!BCrypt.Verify(model.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email: {Email}", model.Email);
                return Unauthorized("Invalid email or password.");
            }

            // Create the JWT token
            var token = GenerateJwtToken(user);

            _logger.LogInformation("Login successful for email: {Email}", model.Email);
            return Ok(new { Token = token });
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found");
            return Unauthorized("Invalid email or password."); // We don't want to give hints about the existence of the email
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error ocurred during user login for email: {Email}.", model.Email);
            return StatusCode(500, "An internal error ocurred.");
        }
    }


    // Action to check if the token is valid
    [HttpPost("check-auth")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CheckAuth([FromHeader] string Authorization)
    {
        string keyString = _configuration["Jwt:Key"] ?? string.Empty;   // Use environment variable for key in production

        if (string.IsNullOrEmpty(keyString))
        {
            _logger.LogError("JWT key not found.");
            return StatusCode(500, "JWT key not found.");
        }

        if (Authorization == null || !Authorization.StartsWith("Bearer"))
        {
            return Unauthorized("No token provided.");
        }

        var token = Authorization.Substring("Bearer ".Length).Trim();
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            _logger.LogInformation("Token validated.");
            return Ok("Authenticated");
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "Invalid token.");
            return Unauthorized("Invalid token.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while validating the token.");
            return StatusCode(500, "An unexpected error occurred while validating the token.");
        }
    }

    // Method to generate the JWT token
    private string GenerateJwtToken(User user)
    {
        string keyString = _configuration["Jwt:Key"] ?? string.Empty;   // Use environment variable for key in production

        if (string.IsNullOrEmpty(keyString))
        {
            _logger.LogError("JWT key not found.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
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