using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IPatientService
    {
        Task<Patient?> GetPatientByCardCodeAsync(string cardCode);
        Task<Patient?> GetByIdAsync(int id);
    }
}
