using Photo_Job_Save_Manager.Models;

namespace Photo_Job_Save_Manager.Services
{
    public interface IAuthService
    {
        Task<bool> IsUserLoggedInAsync();
        Task<User?> GetCurrentUserAsync();
        Task<bool> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(string username, string email, string password);
        Task<bool> SendPasswordResetEmailAsync(string email);
        Task<bool> VerifyEmailAsync();
        Task LogoutAsync();
        Task<bool> IsEmailVerifiedAsync();
    }
} 