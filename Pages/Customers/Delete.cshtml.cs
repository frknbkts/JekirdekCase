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
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IHttpClientFactory httpClientFactory, ILogger<DeleteModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public CustomerDto? Customer { get; set; }                                               

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
                _logger.LogWarning("OnGetAsync called with null ID for customer deletion by user {Username}.", User.Identity?.Name);
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
                    Customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
                    if (Customer == null)
                    {
                        ErrorMessage = $"M��teri (ID: {id}) bulunamad� veya API'den bo� yan�t geldi.";
                        _logger.LogWarning("Customer with ID {CustomerId} not found or API returned null for deletion confirmation by user {Username}.", id, User.Identity?.Name);
                    }
                    else
                    {
                        _logger.LogInformation("Customer data fetched for deletion confirmation. ID: {CustomerId}, User: {Username}", id, User.Identity?.Name);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ErrorMessage = $"M��teri (ID: {id}) bulunamad�.";
                    _logger.LogWarning("Customer with ID {CustomerId} not found (404) for deletion confirmation by user {Username}.", id, User.Identity?.Name);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"M��teri bilgileri getirilirken hata olu�tu. Durum: {response.StatusCode}";
                    _logger.LogWarning("Failed to fetch customer ID {CustomerId} for deletion by user {Username}. Status: {StatusCode}, Response: {ErrorContent}", id, User.Identity?.Name, response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching customer ID {CustomerId} for deletion by user {Username}.", id, User.Identity?.Name);
                ErrorMessage = "M��teri bilgileri getirilirken beklenmedik bir hata olu�tu.";
            }

            if (Customer == null && string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = $"M��teri (ID: {id}) bulunamad�.";
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (Customer == null || Customer.Id == 0)
            {
                ErrorMessage = "Silinecek m��teri ID'si bulunamad�.";
                _logger.LogWarning("OnPostAsync called with null or invalid Customer.Id for deletion by user {Username}.", User.Identity?.Name);
                return Page();
            }
            int customerIdToDelete = Customer.Id;


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
                var response = await httpClient.DeleteAsync($"{apiBaseUrl}/api/customers/{customerIdToDelete}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Customer ID {CustomerId} deleted successfully by user {Username}.", customerIdToDelete, User.Identity?.Name);
                    TempData["SuccessMessage"] = "M��teri ba�ar�yla silindi.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"M��teri silinemedi. Durum Kodu: {response.StatusCode}. Detay: {errorContent}";
                    _logger.LogWarning("Failed to delete customer ID {CustomerId} by user {Username}. Status: {StatusCode}, Response: {ErrorContent}", customerIdToDelete, User.Identity?.Name, response.StatusCode, errorContent);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting customer ID {CustomerId} by user {Username}.", customerIdToDelete, User.Identity?.Name);
                ErrorMessage = "M��teri silinirken beklenmedik bir hata olu�tu.";
                return Page();
            }
        }
    }
}