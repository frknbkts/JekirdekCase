using JekirdekCase.Interfaces;
using JekirdekCase.Models;
using JekirdekCase.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace JekirdekCase.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<UserService> _logger;
        private readonly JwtSettings _jwtSettings;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            ILogger<UserService> logger,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings), "JWT settings are not configured.");
        }
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("GetUserByUsernameAsync called with empty username.");
                return null;
            }
            _logger.LogInformation("Fetching user by username: {Username}", username);
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<(User? user, string? errorMessage)> RegisterUserAsync(User userToRegister, string password)
        {
            if (userToRegister == null) return (null, "User data cannot be null.");
            if (string.IsNullOrWhiteSpace(password)) return (null, "Password cannot be empty.");
            if (string.IsNullOrWhiteSpace(userToRegister.Username)) return (null, "Username cannot be empty.");
            if (string.IsNullOrWhiteSpace(userToRegister.Role)) return (null, "User role cannot be empty.");


            var existingUser = await _userRepository.GetByUsernameAsync(userToRegister.Username);
            if (existingUser != null)
            {
                _logger.LogWarning("Attempted to register user with existing username: {Username}", userToRegister.Username);
                return (null, $"Username '{userToRegister.Username}' is already taken.");
            }

            userToRegister.PasswordHash = _passwordHasher.HashPassword(userToRegister, password);
            userToRegister.CreatedAt = DateTime.UtcNow;
            userToRegister.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _userRepository.AddAsync(userToRegister);
                await _userRepository.SaveChangesAsync();
                _logger.LogInformation("User registered successfully with username: {Username}", userToRegister.Username);

                var registeredUserDto = new User { Id = userToRegister.Id, Username = userToRegister.Username, Role = userToRegister.Role, CreatedAt = userToRegister.CreatedAt, UpdatedAt = userToRegister.UpdatedAt };
                return (registeredUserDto, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user {Username}", userToRegister.Username);
                return (null, "An error occurred during registration. Please try again.");
            }
        }

        public async Task<(bool success, string? token, string? errorMessage)> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, null, "Username and password are required.");

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existing username: {Username}", username);
                return (false, null, "Invalid username or password.");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Invalid password attempt for username: {Username}", username);
                return (false, null, "Invalid username or password.");
            }

            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, password);
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();
            }

            var token = GenerateJwtToken(user);
            _logger.LogInformation("User {Username} logged in successfully.", username);
            return (true, token, null);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }
    }
}