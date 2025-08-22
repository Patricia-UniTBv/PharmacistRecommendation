using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Repository
{
    public class PharmacyRepository : IPharmacyRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public PharmacyRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }
        
        public async Task<Pharmacy> GetById(int pharmacyId)
        {
            var pharmacy = await _context.Pharmacies
                  .Where(p => p.Id == pharmacyId)
                  .FirstOrDefaultAsync();
            
            if (pharmacy == null)
            {
                throw new InvalidOperationException($"Farmacia cu ID-ul {pharmacyId} nu a fost găsită. Vă rugăm să verificați configurarea sistemului.");
            }
                
            return pharmacy;
        }

        public async Task<Pharmacy?> GetFirstPharmacyAsync()
        {
            return await _context.Pharmacies.FirstOrDefaultAsync();
        }

        public async Task UpdatePharmacyAsync(Pharmacy pharmacy)
        {
            _context.Pharmacies.Update(pharmacy);
            await _context.SaveChangesAsync();
        }

        public async Task EnsureDefaultPharmacyAsync()
        {
            if (!await _context.Pharmacies.AnyAsync())
            {
                var defaultPharmacy = new Pharmacy
                {
                    Name = "Farmacia Demo",
                    Address = "Strada Principala nr. 1",
                    CUI = "12345678",
                    Email = "admin@farmacia.ro",
                    Phone = "0123456789",
                    ConsentTemplate = null
                };

                _context.Pharmacies.Add(defaultPharmacy);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAsync(Pharmacy pharmacy)
        {
            _context.Pharmacies.Add(pharmacy);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasAnyPharmacyAsync()
        {
            var hasAny = await _context.Pharmacies.AnyAsync();
            return hasAny;
        }

        public async Task<int> GetPharmacyId()
        {
            var pharmacy = await _context.Pharmacies
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            if (pharmacy == null)
                throw new Exception("Nu există farmacii în baza de date");

            return pharmacy.Id;
        }
    }
}
