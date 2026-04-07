using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public UserRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetFirstUserByRoleAsync(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return null;

            return await _context.Users
                                 .Where(u => u.Role.ToLower() == role.ToLower())
                                 .OrderBy(u => u.Id)
                                 .FirstOrDefaultAsync();
        }

        public async Task<int> AddAsync(User entity)
        {
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }
        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
