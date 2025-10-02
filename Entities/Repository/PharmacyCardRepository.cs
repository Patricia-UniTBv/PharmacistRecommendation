using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Repository
{
    public class PharmacyCardRepository: IPharmacyCardRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public PharmacyCardRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task<PharmacyCard?> GetByPatientAndPharmacyAsync(int patientId, int pharmacyId)
        {
            return await _context.PharmacyCards
                .Include(c => c.Patient)
                .FirstOrDefaultAsync(c => c.PatientId == patientId && c.PharmacyId == pharmacyId);
        }

        public async Task<PharmacyCard> UpdateAsync(PharmacyCard card)
        {
            _context.PharmacyCards.Update(card);
            await _context.SaveChangesAsync();
            return card;
        }

      
        public async Task<PharmacyCard> AddCardWithPatientAsync(PharmacyCard card, Patient patient)
        {
            if (patient.Id == 0)
            {
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Patients.Update(patient);
                await _context.SaveChangesAsync();
            }

            card.PatientId = patient.Id;
            _context.PharmacyCards.Add(card);
            await _context.SaveChangesAsync();

            return card;
        }

        public async Task<PharmacyCard?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            return await _context.PharmacyCards
                .Include(pc => pc.Patient)
                .FirstOrDefaultAsync(pc => pc.Code == code.Trim());
        }
    }
}
