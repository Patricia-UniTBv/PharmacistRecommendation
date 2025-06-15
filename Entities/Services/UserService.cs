using DTO;
using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        public UserService(IUserRepository repo) => _repo = repo;
        public async Task<int> AddUserAsync(UserDTO dto)
        {
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role, // "Pharmacist" sau "Assistant"
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Ncm = dto.Ncm,
                Email = dto.Email,
                Phone = dto.Phone,
                PharmacyId = dto.PharmacyId
            };

            return await _repo.AddAsync(user);
        }

    }
}
