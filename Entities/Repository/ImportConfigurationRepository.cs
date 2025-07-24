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
    public class ImportConfigurationRepository : IImportConfigurationRepository
    {
        private readonly PharmacistRecommendationDbContext _context;
        public ImportConfigurationRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task<ImportConfiguration?> GetAsync()
        {
            return await _context.ImportConfigurations.FirstOrDefaultAsync();
        }

        public async Task<ImportConfiguration?> GetById(int id)
        {
            return await _context.ImportConfigurations.FirstOrDefaultAsync(x => x.PharmacyId == id);
        }

        public async Task AddAsync(ImportConfiguration config)
        {
            _context.ImportConfigurations.Add(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ImportConfiguration config)
        {
            var existing = await _context.ImportConfigurations.FindAsync(config.Id);
            if (existing == null)
                throw new InvalidOperationException("Config not found.");

            existing.ReceiptPath = config.ReceiptPath;
            existing.PrescriptionPath = config.PrescriptionPath;
            await _context.SaveChangesAsync();
        }
    }
}
