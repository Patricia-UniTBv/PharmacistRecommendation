using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface IPharmacyCardService
    {
        Task<PharmacyCard> CreateCardAsync(string code, int pharmacyId, string firstName, string lastName, string? cnp, string? email, string? phone, string? gender, DateOnly? birthdate);
    }
}
