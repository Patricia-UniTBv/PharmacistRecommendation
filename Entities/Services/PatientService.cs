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

        public async Task<Patient> GetOrCreatePatientAsync(Patient dto)
        {
            // Căutăm pacientul după CNP
            Patient? patient = null;
            if (!string.IsNullOrWhiteSpace(dto.Cnp))
            {
                patient = await _repository.GetByCnpAsync(dto.Cnp);
            }

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

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

    }
}
