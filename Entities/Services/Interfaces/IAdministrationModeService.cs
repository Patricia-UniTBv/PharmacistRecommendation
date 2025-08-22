using Entities.Models;

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
