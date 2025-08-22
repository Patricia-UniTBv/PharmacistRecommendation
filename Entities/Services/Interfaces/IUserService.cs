using DTO;

namespace Entities.Services.Interfaces
{
    public interface IUserService
    {
        Task<int> AddUserAsync(UserDTO dto);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task DeleteUserAsync(int userId);
        Task UpdateUserAsync(UserDTO dto);
    }
}
