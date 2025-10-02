using Entities.Models;

namespace Entities.Repository.Interfaces
{
    public interface IPharmacyCardRepository
    {
        Task<PharmacyCard?> GetByPatientAndPharmacyAsync(int patientId, int pharmacyId);
        Task<PharmacyCard> UpdateAsync(PharmacyCard card);
        Task<PharmacyCard> AddCardWithPatientAsync(PharmacyCard card, Patient patient);
        Task<PharmacyCard?> GetByCodeAsync(string code);
    }
}
