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
    }
}
