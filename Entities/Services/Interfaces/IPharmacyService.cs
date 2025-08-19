using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface IPharmacyService
    {
        Task<Pharmacy?> GetByIdAsync(int id);
        Task AddPharmacyAsync(Pharmacy pharmacy);
        Task<string> GetConsentTemplateAsync(int pharmacyId);
        Task ResetConsentTemplateAsync(int pharmacyId);
        Task UpdateConsentTemplateAsync(int pharmacyId, string newTemplate);

        Task<bool> HasAnyPharmacyAsync();

        Task<int> GetPharmacyId();
    }
}
