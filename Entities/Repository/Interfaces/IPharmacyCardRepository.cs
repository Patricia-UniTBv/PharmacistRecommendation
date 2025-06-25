using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Repository.Interfaces
{
    public interface IPharmacyCardRepository
    {
        Task<PharmacyCard> AddCardWithPatientAsync(PharmacyCard card, Patient patient);
    }
}
