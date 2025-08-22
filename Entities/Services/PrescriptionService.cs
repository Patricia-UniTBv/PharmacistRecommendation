using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;

namespace Entities.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository _repository;

        public PrescriptionService(IPrescriptionRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Prescription>> GetAllPrescriptionsAsync()
            => _repository.GetAllAsync();

        public Task<Prescription?> GetPrescriptionByIdAsync(int id)
            => _repository.GetByIdAsync(id);

        public Task<List<Prescription>> GetPrescriptionsByPatientAsync(string cnpOrCid)
            => _repository.GetByPatientCnpOrCidAsync(cnpOrCid);

        public Task AddPrescriptionAsync(Prescription prescription)
            => _repository.AddAsync(prescription);

        public Task UpdatePrescriptionAsync(Prescription prescription)
            => _repository.UpdateAsync(prescription);

        public Task DeletePrescriptionAsync(int id)
            => _repository.DeleteAsync(id);
    }

}
