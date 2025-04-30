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
    public class PatientRepository : IPatientRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public PatientRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task<Patient?> GetByCardCodeAsync(string cardCode)
        {
            return await _context.PharmacyCards
                .Where(pc => pc.Code == cardCode)
                .Select(pc => pc.Patient)
                .FirstOrDefaultAsync();
        }
    }
}
