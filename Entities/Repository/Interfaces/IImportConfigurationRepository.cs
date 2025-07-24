using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Repository.Interfaces
{
    public interface IImportConfigurationRepository
    {
        Task<ImportConfiguration?> GetAsync();
        Task<ImportConfiguration?> GetById(int id);
        Task AddAsync(ImportConfiguration config);
        Task UpdateAsync(ImportConfiguration config);
    }
}
