using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IEmailConfigurationService
    {
        Task<EmailConfiguration?> GetByPharmacyIdAsync(int pharmacyId);
        Task AddAsync(EmailConfiguration config);
        Task UpdateAsync(EmailConfiguration config);
    }

}
