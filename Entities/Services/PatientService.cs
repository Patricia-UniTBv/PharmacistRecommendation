using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services
{
    public class PatientService: IPatientService
    {
        private readonly IPatientRepository _repository;

        public PatientService(IPatientRepository repository)
        {
            _repository = repository;
        }

        public async Task<Patient?> GetPatientByCardCodeAsync(string cardCode)
        {
            return await _repository.GetByCardCodeAsync(cardCode);
        }
    }
}
