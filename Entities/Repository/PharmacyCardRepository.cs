using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;

namespace Entities.Repository
{
    public class PharmacyCardRepository: IPharmacyCardRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public PharmacyCardRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
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
    }
}
