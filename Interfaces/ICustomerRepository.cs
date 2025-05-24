using JekirdekCase.Models;

namespace JekirdekCase.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<IEnumerable<Customer>> GetCustomersFilteredAsync(
              string? name,
              string? email,
              DateTime? registrationDateFrom,
              DateTime? registrationDateTo,
              string? region);

        Task<Customer?> GetByEmailAsync(string email);
    }
}
