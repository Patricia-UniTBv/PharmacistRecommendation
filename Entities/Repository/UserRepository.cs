using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public UserRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(User entity)
        {
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }
    }
}
