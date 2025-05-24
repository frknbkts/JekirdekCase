using JekirdekCase.Data;
using JekirdekCase.Interfaces;
using JekirdekCase.Models;
using Microsoft.EntityFrameworkCore;

namespace JekirdekCase.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Customer>> GetCustomersFilteredAsync(
            string? name,
            string? email,
            DateTime? registrationDateFrom,
            DateTime? registrationDateTo,
            string? region)
        {
            var query = _dbSet.AsQueryable(); // _dbSet, base class'tan geliyor

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => (c.FirstName + " " + c.LastName).ToLower().Contains(name.ToLower()) ||
                                         c.FirstName.ToLower().Contains(name.ToLower()) ||
                                         c.LastName.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(c => c.Email.ToLower().Contains(email.ToLower()));
            }

            if (registrationDateFrom.HasValue)
            {
                query = query.Where(c => c.RegistrationDate.Date >= registrationDateFrom.Value.Date);
            }

            if (registrationDateTo.HasValue)
            {
                query = query.Where(c => c.RegistrationDate.Date <= registrationDateTo.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(region))
            {
                query = query.Where(c => c.Region != null && c.Region.ToLower().Contains(region.ToLower()));
            }

            return await query.ToListAsync();
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
        }
    }
}
