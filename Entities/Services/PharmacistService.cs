using DTO;
using Entities.Models;
using Entities.Repository.Interfaces;
using BCrypt.Net;
using Entities.Services.Interfaces;

namespace Entities.Services
{
    public class PharmacistService: IPharmacistService
    {
        private readonly IPharmacistRepository _repo;
        public PharmacistService(IPharmacistRepository repo) => _repo = repo;
        //public async Task<int> AddPharmacistAsync(PharmacistDTO dto)
        //{
        //    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        //    var entity = new Pharmacist
        //    {
        //        FirstName = dto.FirstName, 
        //        LastName = dto.LastName,
        //        Ncm = dto.Ncm,
        //        Phone = dto.Phone,
        //        Email = dto.Email,
        //        Password = hashedPassword,
        //        //PharmacyId = 1 //
        //    };

        //    return await _repo.AddAsync(entity);
        //}
    }
}
