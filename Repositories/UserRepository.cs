using JekirdekCase.Data;
using JekirdekCase.Interfaces;
using JekirdekCase.Models;
using Microsoft.EntityFrameworkCore;

namespace JekirdekCase.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }
    }
}
