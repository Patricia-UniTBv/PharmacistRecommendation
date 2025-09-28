using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public PatientRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task<Patient> AddAsync(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }
        public async Task<Patient?> GetByCnpAsync(string cnp)
        {
            return await _context.Patients.FirstOrDefaultAsync(p => p.Cnp == cnp);
        }

        public async Task<Patient?> GetByNameAsync(string? firstName, string? lastName)
        {
            return await _context.Patients
    .FirstOrDefaultAsync(p => p.FirstName.ToLower().Trim() == firstName.ToLower().Trim() &&
                              p.LastName.ToLower().Trim() == lastName.ToLower().Trim());

        }

        public async Task<Patient?> GetByCardCodeAsync(string cardCode)
        {
            return await _context.PharmacyCards
                .Where(pc => pc.Code == cardCode)
                .Select(pc => pc.Patient)
                .FirstOrDefaultAsync();
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _context.Patients
                  .Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Patient> UpdateAsync(Patient patient)
        {
            _context.Patients.Update(patient);

            await _context.SaveChangesAsync();

            return patient;
        }
    }
}
