// IMedicationService.cs
using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IMedicationService
    {
        Task<List<Medication>> GetAllMedicationsAsync();
        Task<List<Medication>> SearchMedicationsAsync(string searchTerm);
        Task<Medication> AddMedicationAsync(Medication medication);
        Task<Medication> UpdateMedicationAsync(Medication medication);
        Task DeleteMedicationAsync(int id);
    }
}