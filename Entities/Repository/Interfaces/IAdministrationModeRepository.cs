using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Repository.Interfaces
{
    public interface IAdministrationModeRepository
    {
        Task<IEnumerable<AdministrationMode>> GetAllAsync();
        Task AddAsync(AdministrationMode mode);
        Task UpdateAsync(AdministrationMode mode);
        Task DeleteAsync(int id);
    }

}
