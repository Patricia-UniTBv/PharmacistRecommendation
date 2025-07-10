using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
