using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;

namespace Entities.Services
{
    public class PharmacyCardService: IPharmacyCardService
    {
        private readonly IPharmacyCardRepository _cardRepository;
        private readonly IPatientRepository _patientRepository;

        public PharmacyCardService(IPharmacyCardRepository cardRepository, IPatientRepository patientRepository)
        {
            _cardRepository = cardRepository;
            _patientRepository = patientRepository;
        }

        public async Task<PharmacyCard> CreateCardAsync(string code, int pharmacyId, string firstName, string lastName, string? cnp, string? cid, string? email, string? phone, string? gender, DateTime? birthdate)
        {

            Patient? patient = null;

            if (!string.IsNullOrWhiteSpace(cnp))
            {
                patient = await _patientRepository.GetByCnpAsync(cnp);
            }

            if (patient == null)
            {
                patient = new Patient
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

                patient = await _patientRepository.AddAsync(patient);
            }
            else
            {
                patient.FirstName = string.IsNullOrWhiteSpace(firstName) ? patient.FirstName : firstName;
                patient.LastName = string.IsNullOrWhiteSpace(lastName) ? patient.LastName : lastName;
                patient.Cid = string.IsNullOrWhiteSpace(cid) ? patient.Cid : cid;
                patient.Email = string.IsNullOrWhiteSpace(email) ? patient.Email : email;
                patient.Phone = string.IsNullOrWhiteSpace(phone) ? patient.Phone : phone;
                patient.Gender = string.IsNullOrWhiteSpace(gender) ? patient.Gender : gender;
                patient.Birthdate = birthdate ?? patient.Birthdate;

                await _patientRepository.UpdateAsync(patient);
            }

            var card = new PharmacyCard
            {
                Code = code,
                PharmacyId = pharmacyId,
                Patient = patient
            };

            return await _cardRepository.AddCardWithPatientAsync(card, patient);
        }

        public async Task<PharmacyCard?> GetByCodeAsync(string code)
        {
            return await _cardRepository.GetByCodeAsync(code);
        }
    }
    
}
