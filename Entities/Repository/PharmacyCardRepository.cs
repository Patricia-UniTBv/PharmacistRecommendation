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
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            card.PatientId = patient.Id;
            _context.PharmacyCards.Add(card);
            await _context.SaveChangesAsync();

            return card;
        }
    }
}
