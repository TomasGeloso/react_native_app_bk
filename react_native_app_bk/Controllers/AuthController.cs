namespace react_native_app_bk.Controllers;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using react_native_app_bk.Models.RefreshToken;
using react_native_app_bk.Models.RefreshToken.Dtos;
using react_native_app_bk.Models.User;
using react_native_app_bk.Models.User.Dtos;
using react_native_app_bk.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthController(IUserService userService, IConfiguration configuration, ILogger<AuthController> logger, IRefreshTokenService refreshTokenService)
    {
        _userService = userService;
        _configuration = configuration;
        _logger = logger;
        _refreshTokenService = refreshTokenService;
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
    public async Task<IActionResult> Login([FromBody] LoginDto model, [FromHeader(Name = "deviceInfo")] string deviceInfo)
    {
        // Check if the model is valid
        if (!ModelState.IsValid)
        {
            _logger.LogError("Invalid model state.");
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

            // Check if the user has a refresh token already
            var alreadyRefreshToken = await _refreshTokenService.GetRefreshToken(user.Id);
            // If the user has a refresh token, delete it
            if (alreadyRefreshToken != null)
            {
                await _refreshTokenService.DeleteRefreshToken(alreadyRefreshToken.User_Id, alreadyRefreshToken.Device);
            }

            // Create the access JWT token and the refresh token
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                User_Id = user.Id,
                Refresh_Token = newRefreshToken,
                Refresh_Token_Expiry = DateTime.Now.AddMinutes(1),
                Device = deviceInfo
            };

            await _refreshTokenService.AddRefreshToken(refreshToken);   // Save the refresh token in the database

            _logger.LogInformation("Login successful for email: {Email}", model.Email);
            return Ok(new {
                AccessToken = newAccessToken    // Return the access token to the user
            });
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

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromHeader(Name = "Authorization")] string authorizationHeader, [FromHeader(Name = "Device")] string deviceInfo)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer"))
        {
            _logger.LogWarning("No token provided.");
            return Unauthorized(new { error = "invalid_token", message = "No token provided." });
        }

        var accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

        try
        {
            // Get the user from the access token
            var principal = GetPrincipalFromExpiredToken(accessToken);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError("Invalid user id in the token.");
                return Unauthorized(new { error = "invalid_token", message = "Invalid token." });
            }

            // Get the refresh token from the database
            var refreshToken = await _refreshTokenService.GetRefreshToken(userId);

            if(refreshToken == null)
            {
                _logger.LogWarning("Refresh token not found in the database.");
                return Unauthorized(new { error = "refresh_token_not_found", message = "Refresh token not found." });
            }

            // Check if the refresh token is valid
            if (refreshToken.Refresh_Token_Expiry <= DateTime.Now)
            {
                await _refreshTokenService.DeleteRefreshToken(userId, deviceInfo);
                _logger.LogWarning("Invalid or expired refresh token in the DataBase.");
                return Unauthorized(new { error = "expired_refresh_token", message = "Invalid or expired refresh token." });
            }

            // Generate a new access token and refresh token
            var user = await _userService.GetUserById(userId);

            if(user == null)
            {
                _logger.LogError("User not found.");
                return Unauthorized(new { error = "invalid_token", message = "Invalid token." });
            }

            var newAccessToken = GenerateJwtToken(user);

            var newRefreshToken = new RefreshToken
            {
                User_Id = user.Id,
                Refresh_Token = GenerateRefreshToken(),
                Refresh_Token_Expiry = DateTime.Now.AddDays(1),
                Device = deviceInfo
            };

            await _refreshTokenService.AddRefreshToken(newRefreshToken);   // Save the refresh token in the database

            return Ok(new
            {
                AccessToken = newAccessToken
            });

        }
        catch(SecurityTokenException ex)
        {
            _logger.LogError(ex, "Invalid token.");
            return Unauthorized(new { error = "invalid_token", message = "Invalid token." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while refreshing the token.");
            return StatusCode(500, "An internal error occurred.");
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
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(20),
            signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]))
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}