using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;

namespace Entities.Repository
{
    public class PharmacistRepository: IPharmacistRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public PharmacistRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Pharmacist entity)
        {
            _context.Pharmacists.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }
    }
}
