using Entities.Models;

namespace Entities.Repository.Interfaces
{
    public interface IPrescriptionMedicationRepository
    {
        Task<List<PrescriptionMedication>> GetAllByPrescriptionIdAsync(int prescriptionId);
        Task AddAsync(PrescriptionMedication medication);
        Task UpdateAsync(PrescriptionMedication medication);
        Task DeleteAsync(int id);
    }
}
