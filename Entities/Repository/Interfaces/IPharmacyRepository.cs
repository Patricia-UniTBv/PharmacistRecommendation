using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Repository.Interfaces
{
    public interface IPharmacyRepository
    {
        Task<Pharmacy> GetById(int pharmacyId);
        Task UpdatePharmacyAsync(Pharmacy pharmacy);
        Task AddAsync(Pharmacy pharmacy);

        Task<bool> HasAnyPharmacyAsync();

        Task<int> GetPharmacyId();
    }
}
