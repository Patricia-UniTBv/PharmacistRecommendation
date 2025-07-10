using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Repository.Interfaces
{
    public interface IPrescriptionRepository
    {
        Task<List<Prescription>> GetAllAsync();
        Task<Prescription?> GetByIdAsync(int id);
        Task<List<Prescription>> GetByPatientCnpOrCidAsync(string cnpOrCid);
        Task AddAsync(Prescription prescription);
        Task UpdateAsync(Prescription prescription);
        Task DeleteAsync(int id);
    }
}
