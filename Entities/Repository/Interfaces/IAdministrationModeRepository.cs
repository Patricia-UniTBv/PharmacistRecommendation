using Entities.Models;

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
