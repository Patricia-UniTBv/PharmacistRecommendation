using Entities.Models;

namespace Entities.Repository.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByCardCodeAsync(string cardCode);
        Task<Patient?> GetByIdAsync(int id);
    }
}
