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
    public class PharmacyService : IPharmacyService
    {
        private readonly IPharmacyRepository _repository;

        public PharmacyService(IPharmacyRepository repository)
        {
            _repository = repository;
        }
        public async Task<Pharmacy?> GetByIdAsync(int id)
        {
            return await _repository.GetById(id);
        }
    }
}
