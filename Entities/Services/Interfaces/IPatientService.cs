using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IPatientService
    {
        Task<Patient?> GetPatientByCardCodeAsync(string cardCode);
        Task<Patient> GetOrCreatePatientAsync(Patient dto);
        Task<Patient?> GetPatientByCnpAsync(string cnp);
        Task<Patient?> GetByIdAsync(int id);
    }
}
