using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services
{
    public class ImportConfigurationService : IImportConfigurationService
    {
        private readonly IImportConfigurationRepository _repository;

        public ImportConfigurationService(IImportConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<ImportConfiguration?> GetAsync()
        {
            return await _repository.GetAsync();
        }

        public async Task<ImportConfiguration?> GetById(int id)
        {
            return await _repository.GetById(id);
        }

        public async Task AddAsync(ImportConfiguration config)
        {
            var existing = await _repository.GetAsync();
            if (existing != null)
                throw new InvalidOperationException("Config already exists.");

            await _repository.AddAsync(config);
        }

        public async Task UpdateAsync(ImportConfiguration config)
        {
            var existing = await _repository.GetAsync();
            if (existing == null)
                throw new InvalidOperationException("Config does not exist.");

            config.Id = existing.Id; // asigură corectitudinea Id-ului
            await _repository.UpdateAsync(config);
        }
    }

}
