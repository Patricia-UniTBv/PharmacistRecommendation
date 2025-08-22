using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Repository
{
    public class AdministrationModeRepository : IAdministrationModeRepository
    {
        private readonly PharmacistRecommendationDbContext _context;
        public AdministrationModeRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdministrationMode>> GetAllAsync()
        {
            return await _context.AdministrationModes.OrderBy(x => x.Name).ToListAsync();
        }

        public async Task AddAsync(AdministrationMode mode)
        {
            _context.AdministrationModes.Add(mode);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AdministrationMode mode)
        {
            _context.AdministrationModes.Update(mode);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.AdministrationModes.FindAsync(id);
            if (entity != null)
            {
                _context.AdministrationModes.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }

}
