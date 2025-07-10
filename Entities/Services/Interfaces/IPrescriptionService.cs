using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface IPrescriptionService
    {
        Task<List<Prescription>> GetAllPrescriptionsAsync();
        Task<Prescription?> GetPrescriptionByIdAsync(int id);
        Task<List<Prescription>> GetPrescriptionsByPatientAsync(string cnpOrCid);
        Task AddPrescriptionAsync(Prescription prescription);
        Task UpdatePrescriptionAsync(Prescription prescription);
        Task DeletePrescriptionAsync(int id);
    }
}
