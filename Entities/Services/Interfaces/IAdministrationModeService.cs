using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface IAdministrationModeService
    {
        Task<IEnumerable<AdministrationMode>> GetAllAsync();
        Task AddAsync(AdministrationMode mode);
        Task UpdateAsync(AdministrationMode mode);
        Task DeleteAsync(int id);
    }

}
