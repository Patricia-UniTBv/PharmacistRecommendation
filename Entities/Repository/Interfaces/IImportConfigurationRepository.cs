using Entities.Models;
namespace Entities.Repository.Interfaces
{
    public interface IImportConfigurationRepository
    {
        Task<ImportConfiguration?> GetAsync();
        Task<ImportConfiguration?> GetById(int id);
        Task AddAsync(ImportConfiguration config);
        Task UpdateAsync(ImportConfiguration config);
    }
}
