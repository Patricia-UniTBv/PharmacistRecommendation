using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;

namespace Entities.Services
{
    public class AdministrationModeService : IAdministrationModeService
    {
        private readonly IAdministrationModeRepository _repo;
        public AdministrationModeService(IAdministrationModeRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<AdministrationMode>> GetAllAsync() => _repo.GetAllAsync();
        public Task AddAsync(AdministrationMode mode) => _repo.AddAsync(mode);
        public Task UpdateAsync(AdministrationMode mode) => _repo.UpdateAsync(mode);
        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
    }

}
