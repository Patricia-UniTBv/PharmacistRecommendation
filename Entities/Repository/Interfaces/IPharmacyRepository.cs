using Entities.Models;

namespace Entities.Repository.Interfaces
{
    public interface IPharmacyRepository
    {
        Task<Pharmacy> GetById(int pharmacyId);
        Task UpdatePharmacyAsync(Pharmacy pharmacy);
        Task AddAsync(Pharmacy pharmacy);

        Task<bool> HasAnyPharmacyAsync();

        Task<int> GetPharmacyId();
    }
}
