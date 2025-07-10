using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
