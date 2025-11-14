using Entities.Models;
using System.Linq.Expressions;

namespace Entities.Repository.Interfaces
{
    public interface IPharmacyRepository
    {
        Task<Pharmacy> GetById(int pharmacyId);
        Task UpdatePharmacyAsync(Pharmacy pharmacy);
        Task<Pharmacy?> GetFirstOrDefaultAsync();
        Task AddAsync(Pharmacy pharmacy);

        Task<bool> HasAnyPharmacyAsync();

        Task<int> GetPharmacyId();
        Task<Pharmacy?> GetByConditionAsync(Expression<Func<Pharmacy, bool>> predicate);
        Task UpdateAsync(Pharmacy entity);
    }
}
