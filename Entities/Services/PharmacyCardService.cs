using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;

namespace Entities.Services
{
    public class PharmacyCardService: IPharmacyCardService
    {
        private readonly IPharmacyCardRepository _repository;

        public PharmacyCardService(IPharmacyCardRepository repository)
        {
            _repository = repository;
        }

        public async Task<PharmacyCard> CreateCardAsync(string code, int pharmacyId, string firstName, string lastName, string? cnp, string? cid, string? email, string? phone, string? gender, DateTime? birthdate)
        {

            var patient = new Patient
            {
                FirstName = firstName,
                LastName = lastName,
                Cnp = cnp,
                Cid = cid,
                Email = email,
                Phone = phone,
                Gender = gender,
                Birthdate = birthdate
            };

            var card = new PharmacyCard
            {
                Code = code,
                PharmacyId = pharmacyId,
                Patient = patient
            };

            return await _repository.AddCardWithPatientAsync(card, patient);
        }
    }
}
