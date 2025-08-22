using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IPrescriptionMedicationService
    {
        Task<List<PrescriptionMedication>> GetMedicationsByPrescriptionIdAsync(int prescriptionId);
        Task AddMedicationAsync(PrescriptionMedication medication);
        Task UpdateMedicationAsync(PrescriptionMedication medication);
        Task DeleteMedicationAsync(int id);
    }
}
