using Entities.Models;

namespace Entities.Repository.Interfaces
{
    public interface IPharmacyCardRepository
    {
        Task<PharmacyCard> AddCardWithPatientAsync(PharmacyCard card, Patient patient);
        Task<PharmacyCard?> GetByCodeAsync(string code);
    }
}
