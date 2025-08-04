using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // Add this method to get the first available pharmacy
        public async Task<Pharmacy?> GetFirstPharmacyAsync()
        {
            return await _context.Pharmacies.FirstOrDefaultAsync();
        }

        public async Task UpdatePharmacyAsync(Pharmacy pharmacy)
        {
            _context.Pharmacies.Update(pharmacy);
            await _context.SaveChangesAsync();
        }

        // Method to ensure at least one pharmacy exists in the database
        public async Task EnsureDefaultPharmacyAsync()
        {
            // Check if any pharmacy exists
            if (!await _context.Pharmacies.AnyAsync())
            {
                // If no pharmacy exists, insert a default one
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
    }
}
