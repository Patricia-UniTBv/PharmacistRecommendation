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
            return await _context.Patients
                .Include(p => p.PharmacyCards)
                .FirstOrDefaultAsync(p => p.Cnp == cnp);
        }

        public async Task<Patient?> GetByNameAsync(string? firstName, string? lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return null;

            var normalizedFirst = firstName.Trim().ToLower();
            var normalizedLast = lastName.Trim().ToLower();

            return await _context.Patients
                .Include(p => p.PharmacyCards)
                .FirstOrDefaultAsync(p => p.FirstName.ToLower().Trim() == normalizedFirst &&
                                          p.LastName.ToLower().Trim() == normalizedLast);
        }

        public async Task<Patient?> GetByCardCodeAsync(string cardCode)
        {
            return await _context.Patients
                .Include(p => p.PharmacyCards)
                .FirstOrDefaultAsync(p => p.PharmacyCards.Any(pc => pc.Code == cardCode));
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _context.Patients
                  .Include(p => p.PharmacyCards)
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
