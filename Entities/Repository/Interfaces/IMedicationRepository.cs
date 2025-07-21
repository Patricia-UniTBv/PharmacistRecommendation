// IMedicationRepository.cs
using Entities.Models;

namespace Entities.Repository.Interfaces
{
    public interface IMedicationRepository
    {
        Task<List<Medication>> GetAllAsync();
        Task<List<Medication>> SearchAsync(string searchTerm);
        Task<Medication> GetByIdAsync(int id);
        Task<Medication> AddAsync(Medication medication);
        Task<Medication> UpdateAsync(Medication medication);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<Medication>> GetByCodCIMAsync(string codCIM);
        Task<List<Medication>> GetByAtcCodeAsync(string atcCode);

        // New methods for CSV import functionality
        Task<List<Medication>> GetByDataSourceAsync(string dataSource);
        Task<List<Medication>> BatchAddAsync(List<Medication> medications);
        Task<List<Medication>> BatchUpdateAsync(List<Medication> medications);
        Task<Medication> UpdateCodCIMAsync(int medicationId, string newCodCIM, string oldCodCIM);
    }
}

