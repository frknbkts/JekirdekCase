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
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public UpdateCustomerDto? CustomerInput { get; set; }

        public string? ErrorMessage { get; set; }

        private async Task<string?> GetJwtTokenAsync()
        {
            var jwtToken = User.FindFirstValue("jwtToken");
            if (string.IsNullOrEmpty(jwtToken))
            {
                _logger.LogWarning("JWT token not found in claims for user {Username}.", User.Identity?.Name);
                ErrorMessage = "Kimlik do�rulama token'� bulunamad�. L�tfen tekrar giri� yap�n.";
            }
            return jwtToken;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                ErrorMessage = "M��teri ID'si belirtilmedi.";
                return Page();
            }

            var jwtToken = await GetJwtTokenAsync();
            if (string.IsNullOrEmpty(jwtToken))
            {
                return RedirectToPage("/Login");
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var apiBaseUrl = "https://localhost:7146";

            try
            {
                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/customers/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var customerDto = await response.Content.ReadFromJsonAsync<CustomerDto>();
                    if (customerDto != null)
                    {
                        CustomerInput = new UpdateCustomerDto
                        {
                            Id = customerDto.Id,
                            FirstName = customerDto.FirstName,
                            LastName = customerDto.LastName,
                            Email = customerDto.Email,
                            Region = customerDto.Region,
                            RegistrationDate = customerDto.RegistrationDate
                        };
                        _logger.LogInformation("Customer data fetched for editing. ID: {CustomerId}, User: {Username}", id, User.Identity?.Name);
                    }
                    else
                    {
                        ErrorMessage = $"M��teri (ID: {id}) bulunamad� veya API'den bo� yan�t geldi.";
                        _logger.LogWarning("Customer with ID {CustomerId} not found or API returned null for user {Username}.", id, User.Identity?.Name);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ErrorMessage = $"M��teri (ID: {id}) bulunamad�.";
                    _logger.LogWarning("Customer with ID {CustomerId} not found (404) for user {Username}.", id, User.Identity?.Name);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"M��teri bilgileri getirilirken hata olu�tu. Durum: {response.StatusCode}";
                    _logger.LogWarning("Failed to fetch customer ID {CustomerId} for editing by user {Username}. Status: {StatusCode}, Response: {ErrorContent}", id, User.Identity?.Name, response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching customer ID {CustomerId} for editing by user {Username}.", id, User.Identity?.Name);
                ErrorMessage = "M��teri bilgileri getirilirken beklenmedik bir hata olu�tu.";
            }

            if (CustomerInput == null && string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = $"M��teri (ID: {id}) bulunamad�.";
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (CustomerInput == null || id != CustomerInput.Id)
            {
                ErrorMessage = "ID uyu�mazl��� veya m��teri verisi eksik.";
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var jwtToken = await GetJwtTokenAsync();
            if (string.IsNullOrEmpty(jwtToken))
            {
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

                var response = await httpClient.PutAsJsonAsync($"{apiBaseUrl}/api/customers/{CustomerInput.Id}", CustomerInput);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Customer ID {CustomerId} updated successfully by user {Username}.", CustomerInput.Id, User.Identity?.Name);
                    TempData["SuccessMessage"] = "M��teri ba�ar�yla g�ncellendi.";
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
                            ErrorMessage = $"M��teri g�ncellenemedi. Durum Kodu: {response.StatusCode}. Detay: {errorContent}";
                        }
                    }
                    catch
                    {
                        ErrorMessage = $"M��teri g�ncellenemedi. Durum Kodu: {response.StatusCode}. Detay: {errorContent}";
                    }
                    _logger.LogWarning("Failed to update customer ID {CustomerId} by user {Username}. Status: {StatusCode}, Response: {ErrorContent}", CustomerInput.Id, User.Identity?.Name, response.StatusCode, errorContent);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating customer ID {CustomerId} by user {Username}.", CustomerInput.Id, User.Identity?.Name);
                ErrorMessage = "M��teri g�ncellenirken beklenmedik bir hata olu�tu.";
                return Page();
            }
        }
    }
}