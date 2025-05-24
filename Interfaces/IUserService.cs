using JekirdekCase.Models;

namespace JekirdekCase.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<(User? user, string? errorMessage)> RegisterUserAsync(User userToRegister, string password);
        Task<(bool success, string? token, string? errorMessage)> LoginAsync(string username, string password);
    }
}
