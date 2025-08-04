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
            return await _context.Pharmacies
                  .Where(p => p.Id == pharmacyId).FirstAsync();
        }

        public async Task UpdatePharmacyAsync(Pharmacy pharmacy)
        {
            _context.Pharmacies.Update(pharmacy);
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(Pharmacy pharmacy)
        {
            _context.Pharmacies.Add(pharmacy);
            await _context.SaveChangesAsync();
        }
    }
}
