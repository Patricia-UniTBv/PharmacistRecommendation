using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
