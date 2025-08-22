using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IImportConfigurationService
    {
        Task<ImportConfiguration?> GetAsync();
        Task<ImportConfiguration?> GetById(int id);
        Task AddAsync(ImportConfiguration config);
        Task UpdateAsync(ImportConfiguration config);
    }
}
