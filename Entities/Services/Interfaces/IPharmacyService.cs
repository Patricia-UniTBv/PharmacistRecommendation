using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IPharmacyService
    {
        Task<Pharmacy?> GetByIdAsync(int id);
        Task<Pharmacy?> GetExistingPharmacyAsync();
        //Task AddPharmacyAsync(Pharmacy pharmacy);
        Task<string> GetConsentTemplateAsync(int pharmacyId);
        Task ResetConsentTemplateAsync(int pharmacyId);
        Task UpdateConsentTemplateAsync(int pharmacyId, string newTemplate);

        Task<bool> HasAnyPharmacyAsync();
        Task AddOrUpdatePharmacyAsync(Pharmacy pharmacy);
        Task<int> GetPharmacyId();
    }
}
