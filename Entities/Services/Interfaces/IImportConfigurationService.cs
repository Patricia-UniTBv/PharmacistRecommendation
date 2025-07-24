using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface IImportConfigurationService
    {
        Task<ImportConfiguration?> GetAsync();
        Task<ImportConfiguration?> GetById(int id);
        Task AddAsync(ImportConfiguration config);
        Task UpdateAsync(ImportConfiguration config);
    }
}
