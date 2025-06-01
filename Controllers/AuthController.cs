using AutoMapper;
using JekirdekCase.Configuration;
using JekirdekCase.Dtos;
using JekirdekCase.Interfaces;
using JekirdekCase.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JekirdekCase.Controllers
{
    [Route("api/[controller]")] // API route: /api/auth
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;


        public AuthController(
            IUserService userService,
            ILogger<AuthController> logger,
            IOptions<JwtSettings> jwtSettings,
            IMapper mapper)
        {
            _userService = userService;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
            _mapper = mapper;
        }
        [HttpPost("register")]
        // ESKİ: [Authorize(Roles = "Admin")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userToRegister = new User // DTO'dan User modeline basit bir map'leme
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                Role = registerDto.Role
            };

            var (registeredUser, errorMessage) = await _userService.RegisterUserAsync(userToRegister, registerDto.Password);

            if (registeredUser == null || !string.IsNullOrEmpty(errorMessage))
            {
                _logger.LogWarning("User registration failed for {Username}: {Error}", registerDto.Username, errorMessage);
                return BadRequest(new { message = errorMessage });
            }
            var userResponseDto = _mapper.Map<UserDto>(registeredUser);

            _logger.LogInformation("User {Username} registered successfully.", registerDto.Username);
            return Ok(userResponseDto);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, token, errorMessage) = await _userService.LoginAsync(loginDto.Username, loginDto.Password);

            if (!success || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Login failed for {Username}: {Error}", loginDto.Username, errorMessage);
                return Unauthorized(new { message = errorMessage ?? "Invalid credentials." });
            }

            _logger.LogInformation("User {Username} logged in successfully.", loginDto.Username);

            var tokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            var user = await _userService.GetUserByUsernameAsync(loginDto.Username);

            return Ok(new LoginResponseDto(
                token: token,
                expiration: tokenExpiration,
                username: user?.Username ?? loginDto.Username,
                role: user?.Role ?? "User" // rolü user nesnesinden alioz
            ));
        }
    }
}
