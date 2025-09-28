using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IPatientService
    {
        Task<Patient?> GetPatientByCardCodeAsync(string cardCode);
        Task<Patient> GetOrCreatePatientAsync(string cardNumber, Patient dto);
        Task<Patient?> GetPatientByCnpAsync(string cnp);
        Task<Patient?> GetPatientAsync(string? cardCode = null, string? cnp = null, string? firstName = null, string? lastName = null);
        Task<Patient?> GetByCnpAsync(string cnp);
        Task<Patient?> GetByCardCodeAsync(string cardCode);
        Task<Patient?> GetByNameAsync(string firstName, string lastName);
        Task<Patient?> GetByIdAsync(int id);
    }
}
