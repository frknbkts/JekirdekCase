using JekirdekCase.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace JekirdekCase.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Kullanici adi gereklidir.")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Sifre gereklidir.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            ReturnUrl = returnUrl;
            if (User.Identity.IsAuthenticated)
            {

            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var httpClient = _httpClientFactory.CreateClient();

            var apiBaseUrl = "https://localhost:7146";

            var loginRequest = new LoginRequestDto
            {
                Username = Input.Username,
                Password = Input.Password
            };

            try
            {
                var response = await httpClient.PostAsJsonAsync($"{apiBaseUrl}/api/auth/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        _logger.LogInformation("User {Username} logged in successfully via API.", Input.Username);

                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(loginResponse.Token);

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, jwtToken.Subject ?? Input.Username),
                            new Claim("jwtToken", loginResponse.Token)
                        };

                        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role);
                        if (roleClaim != null)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
                        }
                        else
                        {
                            _logger.LogWarning("Role claim not found in JWT for user {Username}.", Input.Username);
                        }

                        var nameIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                        if (nameIdClaim != null)
                        {
                            claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdClaim.Value));
                        }


                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        _logger.LogInformation("User {Username} signed in with cookie.", Input.Username);
                        return LocalRedirect(ReturnUrl);
                    }
                    else
                    {
                        ErrorMessage = "API'den gecersiz yanit alindi";
                        _logger.LogWarning("Invalid token response from API for user {Username}.", Input.Username);
                        return Page();
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API login failed for user {Username}. Status: {StatusCode}, Response: {ErrorContent}", Input.Username, response.StatusCode, errorContent);
                    // API'den gelen hata mesaj�n� g�stermeye �al��al�m
                    try
                    {
                        var errorDto = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                        if (errorDto != null && errorDto.TryGetValue("message", out var apiMessage))
                        {
                            ErrorMessage = apiMessage;
                        }
                        else
                        {
                            ErrorMessage = $"Giris basarisiz. Durum Kodu: {response.StatusCode}";
                        }
                    }
                    catch
                    {
                        ErrorMessage = $"Giris basarisiz. Durum Kodu: {response.StatusCode}";
                    }
                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API login request failed for user {Username}.", Input.Username);
                ErrorMessage = "Giris servisine ulasilamadi. Lutfen daha sonra tekrar deneyin.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login for user {Username}.", Input.Username);
                ErrorMessage = "Beklenmedik bir hata olustu.";
                return Page();
            }
        }
    }
}