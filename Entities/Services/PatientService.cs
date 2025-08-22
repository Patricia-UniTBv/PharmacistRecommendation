using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;

namespace Entities.Services
{
    public class PatientService: IPatientService
    {
        private readonly IPatientRepository _repository;

        public PatientService(IPatientRepository repository)
        {
            _repository = repository;
        }
        public async Task<Patient?> GetPatientByCardCodeAsync(string cardCode)
        {
            return await _repository.GetByCardCodeAsync(cardCode);
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

    }
}
