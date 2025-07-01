using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface IPatientService
    {
        Task<Patient?> GetPatientByCardCodeAsync(string cardCode);
    }
}
