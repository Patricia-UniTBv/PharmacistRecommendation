using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IPatientService
    {
        Task<Patient?> GetPatientByCardCodeAsync(string cardCode);
        Task<Patient> GetOrCreatePatientAsync(Patient dto);
        Task<Patient?> GetPatientByCnpAsync(string cnp);
        Task<Patient?> GetPatientAsync(string? cardCode = null, string? cnp = null);
        Task<Patient?> GetByIdAsync(int id);
    }
}
