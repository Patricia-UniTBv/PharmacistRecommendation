using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services
{
    public class PrescriptionMedicationService : IPrescriptionMedicationService
    {
        private readonly IPrescriptionMedicationRepository _repository;

        public PrescriptionMedicationService(IPrescriptionMedicationRepository repository)
        {
            _repository = repository;
        }

        public Task<List<PrescriptionMedication>> GetMedicationsByPrescriptionIdAsync(int prescriptionId)
            => _repository.GetAllByPrescriptionIdAsync(prescriptionId);

        public Task AddMedicationAsync(PrescriptionMedication medication)
            => _repository.AddAsync(medication);

        public Task UpdateMedicationAsync(PrescriptionMedication medication)
            => _repository.UpdateAsync(medication);

        public Task DeleteMedicationAsync(int id)
            => _repository.DeleteAsync(id);
    }

}
