using Entities.Models;

namespace Entities.Repository.Interfaces
{
    public interface IPatientRepository
    {
        Task<List<Patient>> GetAllAsync();
        Task<Patient?> GetByCardCodeAsync(string cardCode);
        Task<Patient?> GetByIdAsync(int id);
        Task<Patient?> GetByCnpAsync(string cnp);
        Task<Patient?> GetByNameAsync(string? firstName, string? lastName);
        Task<Patient> AddAsync(Patient patient);
        Task<Patient> UpdateAsync(Patient patient);
    }
}
