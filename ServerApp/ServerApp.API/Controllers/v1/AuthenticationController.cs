using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ServerApp.API.Extensions;
using ServerApp.Contracts.Logging;
using ServerApp.Contracts.Repositories;
using ServerApp.Domain.DTOs;
using ServerApp.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ServerApp.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILoggerManager _logger;
    private readonly IUserRepository _userRepo;

    public AuthenticationController(IConfiguration config, ILoggerManager logger, IUserRepository userRepo)
    {
        _config = config;
        _logger = logger;
        _userRepo = userRepo;
    }

    /// <summary>
    /// Handle Login Requests
    /// </summary>
    /// <param name="creds">Username and password</param>
    /// <returns>Returns the connected user and JWT token if successful</returns>
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] AuthenticationCredsDto creds)
    {
        IActionResult response;

        try
        {
            // Log debug message before authentication attempt
            _logger.LogDebug($"Login attempt for user: {creds.Username}");

            // Authenticate user
            var authenticatedUser = await Authenticate(creds);

            if (authenticatedUser != null)
            {
                // If authentication is successful, generate JWT
                var tokenString = GenerateJWT(authenticatedUser);

                // Log successful login info
                _logger.LogInfo($"User {creds.Username} successfully logged in.");

                // Return token and user information
                response = Ok(new { token = tokenString, authenticatedUser });
            }
            else
            {
                // Log unauthorized access attempt
                _logger.LogWarn($"Failed login attempt for user: {creds.Username}");

                // Return unauthorized status
                response = Unauthorized("Username or Password incorrect.");
            }
        }
        catch (Exception ex)
        {
            // Log error details
            _logger.LogError(ex, $"An error occurred during login for user: {creds.Username}");
            return StatusCode(500, "Internal server error");
        }

        return response;
    }

    /// <summary>
    /// Authenticate User based on provided credentials
    /// </summary>
    /// <param name="creds">User credentials (username & password)</param>
    /// <returns>Authenticated user object if credentials are valid, otherwise null</returns>
    private async Task<User?> Authenticate(AuthenticationCredsDto creds)
    {
        try
        {
            // Log debug message before querying the user repository
            _logger.LogDebug($"Attempting to authenticate user: {creds.Username}");

            // Query repository for the user with matching username and hashed password
            var res = await _userRepo.GetByConditionAsync(user =>
                            user.Username == creds.Username
                            && user.Password == StringExtentions.GenerateMD5Hash(creds.Password));

            // Select the first matched user and map to User object (excluding sensitive info like password)
            var signedUser = res.Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName
            }).FirstOrDefault();

            // Log the result of the authentication attempt
            if (signedUser != null)
            {
                _logger.LogInfo($"User {creds.Username} authenticated successfully.");
            }
            else
            {
                _logger.LogWarn($"Authentication failed for user: {creds.Username}.");
            }

            return signedUser;
        }
        catch (Exception ex)
        {
            // Log error details
            _logger.LogError(ex, $"An error occurred during authentication for user: {creds.Username}");
            throw;
        }
    }

    /// <summary>
    /// Generate JSON Web Token (JWT) for authenticated users
    /// </summary>
    /// <param name="user">Authenticated user object</param>
    /// <returns>JWT Token string</returns>
    private string GenerateJWT(User user)
    {
        try
        {
            // Log debug message before generating JWT
            _logger.LogDebug($"Generating JWT for user: {user.Username}");

            // Get the security key from appsettings.json
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));

            // Create signing credentials using the security key
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Define claims for the token (e.g., username)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Name, user.FullName)
            };

            // Generate JWT token
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMonths(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Log token generation success
            _logger.LogInfo($"JWT generated successfully for user: {user.Username}");

            return tokenString;
        }
        catch (Exception ex)
        {
            // Log error if JWT generation fails
            _logger.LogError(ex, $"An error occurred while generating JWT for user: {user.Username}");
            throw;
        }
    }
}