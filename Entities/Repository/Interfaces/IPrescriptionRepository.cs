using Entities.Models;

namespace Entities.Repository.Interfaces
{
    public interface IPrescriptionRepository
    {
        Task<List<Prescription>> GetAllAsync();
        Task<Prescription?> GetByIdAsync(int id);
        Task<List<Prescription>> GetByPatientCnpOrCidAsync(string cnpOrCid);
        Task AddAsync(Prescription prescription);
        Task UpdateAsync(Prescription prescription);
        Task DeleteAsync(int id);
    }
}
