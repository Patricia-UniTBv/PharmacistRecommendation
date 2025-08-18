using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface IEmailConfigurationService
    {
        Task<EmailConfiguration?> GetByPharmacyIdAsync(int pharmacyId);
        Task AddAsync(EmailConfiguration config);
        Task UpdateAsync(EmailConfiguration config);
    }

}
