using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IPharmacyCardService
    {
        Task<PharmacyCard> CreateCardAsync(string code, int pharmacyId, string firstName, string lastName, string? cnp, string? cid, string? email, string? phone, string? gender, DateTime? birthdate);
    }
}
