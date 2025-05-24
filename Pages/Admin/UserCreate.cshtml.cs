using AutoMapper;
using JekirdekCase.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JekirdekCase.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserCreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserCreateModel> _logger;
        private readonly IMapper _mapper;


        public UserCreateModel(IHttpClientFactory httpClientFactory, ILogger<UserCreateModel> logger, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _mapper = mapper;
        }

        [BindProperty]
        public RegisterRequestDto NewUser { get; set; } = new RegisterRequestDto();

        [TempData] // Bu, redirect sonrasý okunacak
        public string? ResultMessage { get; set; }
        public bool IsSuccess { get; set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                IsSuccess = false;
                ResultMessage = "Lütfen formdaki hatalarý düzeltin.";
                return Page();
            }

            var adminJwtToken = User.FindFirstValue("jwtToken");
            if (string.IsNullOrEmpty(adminJwtToken))
            {
                IsSuccess = false;
                ResultMessage = "Admin kimlik doðrulama token'ý bulunamadý. Lütfen tekrar giriþ yapýn.";
                _logger.LogWarning("Admin JWT token not found in claims during user creation by admin {AdminUsername}.", User.Identity?.Name);
                return Page();
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminJwtToken);
            var apiBaseUrl = "https://localhost:7146";

            try
            {
                var response = await httpClient.PostAsJsonAsync($"{apiBaseUrl}/api/auth/register", NewUser);

                if (response.IsSuccessStatusCode)
                {
                    var createdUserDto = await response.Content.ReadFromJsonAsync<UserDto>();
                    _logger.LogInformation("Admin {AdminUsername} successfully created new user {NewUsername} with role {NewUserRole}.",
                        User.Identity?.Name, NewUser.Username, NewUser.Role);

                    IsSuccess = true;
                    ResultMessage = $"Kullanýcý '{NewUser.Username}' baþarýyla oluþturuldu (Rol: {NewUser.Role}).";
                    TempData["ResultMessage"] = ResultMessage; 
                    TempData["IsSuccess"] = IsSuccess;

                    ModelState.Clear();
                    NewUser = new RegisterRequestDto(); // Modeli sýfýrla
                    return Page();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    string apiErrorMessage = "Kullanýcý oluþturulamadý.";
                    try
                    {
                        var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                        if (errorResponse != null && errorResponse.TryGetValue("message", out var msg))
                        {
                            apiErrorMessage = msg;
                        }
                        else
                        {
                            apiErrorMessage = $"API Hatasý: {response.StatusCode}. Detay: {errorContent}";
                        }
                    }
                    catch { apiErrorMessage = $"API Hatasý: {response.StatusCode}. Detay: {errorContent}"; }

                    IsSuccess = false;
                    ResultMessage = apiErrorMessage;
                    _logger.LogWarning("Admin {AdminUsername} failed to create user {NewUsername}. Status: {StatusCode}, Response: {ErrorContent}",
                        User.Identity?.Name, NewUser.Username, response.StatusCode, errorContent);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while admin {AdminUsername} was creating user {NewUsername}.", User.Identity?.Name, NewUser.Username);
                IsSuccess = false;
                ResultMessage = "Kullanýcý oluþturulurken beklenmedik bir hata oluþtu.";
                return Page();
            }
        }
    }
}