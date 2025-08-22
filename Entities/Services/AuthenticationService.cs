using DTO;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;

namespace Entities.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecureStorageService _secureStorage;
        private UserDTO? _currentUser;
        private string? _currentToken;

        public event EventHandler<AuthResult> AuthenticationStateChanged;

        public AuthenticationService(IUserRepository userRepository, ISecureStorageService secureStorage)
        {
            _userRepository = userRepository;
            _secureStorage = secureStorage;
        }

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    var emptyResult = new AuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Username and password are required."
                    };
                    AuthenticationStateChanged?.Invoke(this, emptyResult);
                    return emptyResult;
                }

                var users = await _userRepository.GetAllAsync();
                var user = users.FirstOrDefault(u => u.Username?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

                if (user == null)
                {
                    var notFoundResult = new AuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid username or password."
                    };
                    AuthenticationStateChanged?.Invoke(this, notFoundResult);
                    return notFoundResult;
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    var invalidPasswordResult = new AuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid username or password."
                    };
                    AuthenticationStateChanged?.Invoke(this, invalidPasswordResult);
                    return invalidPasswordResult;
                }

                var userDto = new UserDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    Ncm = user.Ncm,
                    PharmacyId = user.PharmacyId
                };

                var token = GenerateToken(user.Id, user.Username, user.Role);

                _currentUser = userDto;
                _currentToken = token;

                await _secureStorage.SetAsync("auth_token", token);
                await _secureStorage.SetAsync("user_id", user.Id.ToString());
                await _secureStorage.SetAsync("username", user.Username ?? string.Empty);
                await _secureStorage.SetAsync("user_role", user.Role ?? string.Empty);

                var successResult = new AuthResult
                {
                    IsSuccess = true,
                    User = userDto,
                    Token = token
                };

                AuthenticationStateChanged?.Invoke(this, successResult);
                return successResult;
            }
            catch (Exception ex)
            {
                var errorResult = new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Login failed: {ex.Message}"
                };
                AuthenticationStateChanged?.Invoke(this, errorResult);
                return errorResult;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                _secureStorage.Remove("auth_token");
                _secureStorage.Remove("user_id");
                _secureStorage.Remove("username");
                _secureStorage.Remove("user_role");

                _currentUser = null;
                _currentToken = null;

                var logoutResult = new AuthResult
                {
                    IsSuccess = false, 
                    ErrorMessage = null
                };

                AuthenticationStateChanged?.Invoke(this, logoutResult);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                if (_currentUser != null && !string.IsNullOrEmpty(_currentToken))
                {
                    return true;
                }

                var token = await _secureStorage.GetAsync("auth_token");
                var userIdStr = await _secureStorage.GetAsync("user_id");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null)
                    {
                        _currentUser = new UserDTO
                        {
                            Id = user.Id,
                            Username = user.Username,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Phone = user.Phone,
                            Role = user.Role,
                            Ncm = user.Ncm,
                            PharmacyId = user.PharmacyId
                        };
                        _currentToken = token;
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserDTO?> GetCurrentUserAsync()
        {
            if (_currentUser != null)
            {
                return _currentUser;
            }

            if (await IsAuthenticatedAsync())
            {
                return _currentUser;
            }

            return null;
        }

        private string GenerateToken(int userId, string? username, string? role)
        {
            var tokenData = $"{userId}|{username}|{role}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
            return Convert.ToBase64String(tokenBytes);
        }
    }
}