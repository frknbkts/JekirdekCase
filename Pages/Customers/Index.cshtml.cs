using JekirdekCase.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace JekirdekCase.Pages.Customers
{
    [Authorize(Roles = "Admin,User")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public IList<CustomerDto> Customers { get; set; } = new List<CustomerDto>();
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? SearchEmail { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? SearchRegion { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? SearchRegistrationDateFrom { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? SearchRegistrationDateTo { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            var jwtToken = User.FindFirstValue("jwtToken");
            if (string.IsNullOrEmpty(jwtToken))
            {
                ErrorMessage = "Kimlik do�rulama token'� bulunamad�. L�tfen tekrar giri� yap�n.";
                _logger.LogWarning("JWT token not found in claims for user {Username}.", User.Identity?.Name);
                return RedirectToPage("/Login");
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var apiBaseUrl = "https://localhost:7146";

            var queryParams = new Dictionary<string, string?>();
            if (!string.IsNullOrWhiteSpace(SearchName)) queryParams["name"] = SearchName;
            if (!string.IsNullOrWhiteSpace(SearchEmail)) queryParams["email"] = SearchEmail;
            if (!string.IsNullOrWhiteSpace(SearchRegion)) queryParams["region"] = SearchRegion;
            if (SearchRegistrationDateFrom.HasValue) queryParams["registrationDateFrom"] = SearchRegistrationDateFrom.Value.ToString("yyyy-MM-dd");
            if (SearchRegistrationDateTo.HasValue) queryParams["registrationDateTo"] = SearchRegistrationDateTo.Value.ToString("yyyy-MM-dd");

            var queryString = string.Join("&", queryParams.Where(kvp => kvp.Value != null)
                                .Select(kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));

            var requestUrl = $"{apiBaseUrl}/api/customers";
            if (!string.IsNullOrEmpty(queryString))
            {
                requestUrl += $"?{queryString}";
            }


            try
            {
                var response = await httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var customersList = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
                    Customers = customersList ?? new List<CustomerDto>();
                    _logger.LogInformation("Successfully retrieved {Count} customers for user {Username}.", Customers.Count, User.Identity?.Name);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Unauthorized or Forbidden access to customer list for user {Username}. Status: {StatusCode}", User.Identity?.Name, response.StatusCode);
                    ErrorMessage = "M��teri listesine eri�im yetkiniz bulunmamaktad�r.";
                    return RedirectToPage("/AccessDenied");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to retrieve customers. Status: {StatusCode}, User: {Username}, Response: {ErrorContent}", response.StatusCode, User.Identity?.Name, errorContent);
                    ErrorMessage = $"M��teriler getirilirken bir hata olu�tu. Durum Kodu: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching customers for user {Username}.", User.Identity?.Name);
                ErrorMessage = "M��teriler getirilirken beklenmedik bir hata olu�tu.";
            }
            return Page();
        }
    }
}