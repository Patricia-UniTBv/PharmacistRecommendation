using DTO;

namespace Entities.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthResult> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<UserDTO?> GetCurrentUserAsync();
        event EventHandler<AuthResult> AuthenticationStateChanged;
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public UserDTO? User { get; set; }
        public string? Token { get; set; }
    }
}