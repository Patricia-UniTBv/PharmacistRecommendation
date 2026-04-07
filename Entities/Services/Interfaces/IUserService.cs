using DTO;

namespace Entities.Services.Interfaces
{
    public interface IUserService
    {
        Task<int> AddUserAsync(UserDTO dto);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<bool> IsUsernameTakenAsync(string username, int excludeUserId);
        Task<UserDTO?> GetEffectivePharmacistAsync(UserDTO? currentUser);
        Task DeleteUserAsync(int userId);
        Task UpdateUserAsync(UserDTO dto);
    }
}
