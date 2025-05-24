using JekirdekCase.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JekirdekCase.Pages.Customers
{
    //[Authorize(Roles = "Admin")]
    [Authorize(Roles = "Admin,User")]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public CreateCustomerDto CustomerInput { get; set; } = new CreateCustomerDto();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {

            CustomerInput.RegistrationDate = DateTime.Today;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var jwtToken = User.FindFirstValue("jwtToken");
            if (string.IsNullOrEmpty(jwtToken))
            {
                ErrorMessage = "Kimlik do�rulama token'� bulunamad�. L�tfen tekrar giri� yap�n.";
                _logger.LogWarning("JWT token not found in claims for user {Username} during customer creation.", User.Identity?.Name);
                return Page();
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var apiBaseUrl = "https://localhost:7146";

            try
            {

                if (CustomerInput.RegistrationDate.Kind == DateTimeKind.Unspecified)
                {
                    CustomerInput.RegistrationDate = DateTime.SpecifyKind(CustomerInput.RegistrationDate, DateTimeKind.Local).ToUniversalTime();
                }
                else if (CustomerInput.RegistrationDate.Kind == DateTimeKind.Local)
                {
                    CustomerInput.RegistrationDate = CustomerInput.RegistrationDate.ToUniversalTime();
                }


                var response = await httpClient.PostAsJsonAsync($"{apiBaseUrl}/api/customers", CustomerInput);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Customer created successfully by user {Username}. Email: {Email}", User.Identity?.Name, CustomerInput.Email);
                    TempData["SuccessMessage"] = "M��teri ba�ar�yla eklendi.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                        if (errorResponse != null && errorResponse.TryGetValue("message", out var apiMessage))
                        {
                            ErrorMessage = apiMessage;
                        }
                        else
                        {
                            ErrorMessage = $"M��teri olu�turulamad�. Durum Kodu: {response.StatusCode}. Detay: {errorContent}";
                        }
                    }
                    catch
                    {
                        ErrorMessage = $"M��teri olu�turulamad�. Durum Kodu: {response.StatusCode}. Detay: {errorContent}";
                    }
                    _logger.LogWarning("Failed to create customer by user {Username}. Status: {StatusCode}, Response: {ErrorContent}", User.Identity?.Name, response.StatusCode, errorContent);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating customer by user {Username}.", User.Identity?.Name);
                ErrorMessage = "M��teri olu�turulurken beklenmedik bir hata olu�tu.";
                return Page();
            }
        }
    }
}