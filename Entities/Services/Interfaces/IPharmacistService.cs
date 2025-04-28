using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface IPharmacistService
    {
        Task<IEnumerable<Pharmacist>> GetPharmacistsAsync();
        Task<Pharmacist> GetPharmacistAsync(int id);
        Task<bool> AddPharmacistAsync(Pharmacist pharmacist);
        Task<bool> UpdatePharmacistAsync(Pharmacist pharmacist);
        Task<bool> DeletePharmacistAsync(int id);
    }
}
