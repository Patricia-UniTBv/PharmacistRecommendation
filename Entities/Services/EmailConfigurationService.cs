using Entities.Data;
using Entities.Models;
using Entities.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services
{
    public class EmailConfigurationService : IEmailConfigurationService
    {
        private readonly PharmacistRecommendationDbContext _context;

        public EmailConfigurationService(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task<EmailConfiguration?> GetByPharmacyIdAsync(int pharmacyId)
        {
            return await _context.EmailConfigurations
                .FirstOrDefaultAsync(e => e.PharmacyId == pharmacyId);
        }

        public async Task AddAsync(EmailConfiguration config)
        {
            _context.EmailConfigurations.Add(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EmailConfiguration config)
        {
            _context.EmailConfigurations.Update(config);
            await _context.SaveChangesAsync();
        }
    }

}
