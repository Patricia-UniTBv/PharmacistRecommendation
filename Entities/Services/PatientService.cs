using Entities.Models;
using Entities.Repository;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Services
{
    public class PatientService: IPatientService
    {
        private readonly IPatientRepository _repository;

        public PatientService(IPatientRepository repository)
        {
            _repository = repository;
        }

        public async Task<Patient> GetOrCreatePatientAsync(string cardNumber, Patient dto)
        {
            Patient? patient = null;
  
            patient = await GetPatientAsync(cardNumber, dto.Cnp, dto.FirstName, dto.LastName);
            

            if (patient == null)
            {
                patient = new Patient
                {
                    FirstName = string.IsNullOrWhiteSpace(dto.FirstName) ? "-" : dto.FirstName,
                    LastName = string.IsNullOrWhiteSpace(dto.LastName) ? "-" : dto.LastName,
                    Cnp = dto.Cnp,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Birthdate = dto.Birthdate,
                    Gender = dto.Gender
                };

                patient = await _repository.AddAsync(patient);
            }
            else
            {
                patient.FirstName = string.IsNullOrWhiteSpace(dto.FirstName) ? patient.FirstName : dto.FirstName;
                patient.LastName = string.IsNullOrWhiteSpace(dto.LastName) ? patient.LastName : dto.LastName;
                patient.Gender = string.IsNullOrWhiteSpace(dto.Gender) ? patient.Gender : dto.Gender;
                patient.Email = dto.Email ?? patient.Email;
                patient.Phone = dto.Phone ?? patient.Phone;
                patient.Birthdate = dto.Birthdate ?? patient.Birthdate;

                await _repository.UpdateAsync(patient);
            }

            return patient;
        }

        public async Task<Patient?> GetPatientByCardCodeAsync(string cardCode)
        {
            return await _repository.GetByCardCodeAsync(cardCode);
        }

        public async Task<Patient?> GetPatientByCnpAsync(string cnp)
        {
            return await _repository.GetByCnpAsync(cnp);
        }

        public async Task<Patient?> GetPatientAsync(string? cardCode = null, string? cnp = null, string? firstName = null, string? lastName = null)
        {
            Patient? patient = null;

            // 1. Caută după cardCode
            if (!string.IsNullOrWhiteSpace(cardCode))
            {
                patient = await _repository.GetByCardCodeAsync(cardCode);
            }

            // 2. Caută după CNP, dacă nu a fost găsit deja
            if (patient == null && !string.IsNullOrWhiteSpace(cnp))
            {
                patient = await _repository.GetByCnpAsync(cnp);
            }

            // 3. Caută după nume + prenume, dacă nu a fost găsit deja
            if (patient == null && !string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            {
                patient = await _repository.GetByNameAsync(firstName.Trim(), lastName.Trim());
            }

            // Dacă există pacient, actualizează câmpurile cu cele noi
            if (patient != null)
            {
                if (!string.IsNullOrWhiteSpace(firstName)) patient.FirstName = firstName.Trim();
                if (!string.IsNullOrWhiteSpace(lastName)) patient.LastName = lastName.Trim();
                if (!string.IsNullOrWhiteSpace(cnp)) patient.Cnp = cnp.Trim();

                await _repository.UpdateAsync(patient);
            }

            return patient;
        }

        public async Task<Patient?> GetByCnpAsync(string cnp)
        {
            return await _repository.GetByCnpAsync(cnp);
        }

        public async Task<Patient?> GetByCardCodeAsync(string cardCode)
        {
            return await _repository.GetByCardCodeAsync(cardCode);
        }

        public async Task<Patient?> GetByNameAsync(string firstName, string lastName)
        {
            return await _repository.GetByNameAsync(firstName, lastName);
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

    }
}
