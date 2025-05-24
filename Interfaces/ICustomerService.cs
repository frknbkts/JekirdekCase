using JekirdekCase.Dtos;
using JekirdekCase.Models;

namespace JekirdekCase.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto?> GetCustomerByEmailAsync(string email);

        Task<CustomerDto?> CreateCustomerAsync(CreateCustomerDto createCustomerDto); // DTO alır, DTO döner
        Task<bool> UpdateCustomerAsync(UpdateCustomerDto updateCustomerDto); // DTO alır
        Task<bool> DeleteCustomerAsync(int id);

        Task<IEnumerable<CustomerDto>> GetCustomersFilteredAsync(
            string? name,
            string? email,
            DateTime? registrationDateFrom,
            DateTime? registrationDateTo,
            string? region);
    }
}
