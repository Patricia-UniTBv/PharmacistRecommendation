using Entities.Models;

namespace Entities.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<int> AddAsync(User entity);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
    }
}
