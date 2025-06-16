using DTO;
using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.ApplicationServices;
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
        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _repo.GetAllAsync();
            return users.Select(u => new UserDTO
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                Username = u.Username,
                Ncm = u.Ncm,
                Role = u.Role,
                PharmacyId = u.PharmacyId
            });
        }

        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return null;

            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Username = user.Username,
                Ncm = user.Ncm,
                Role = user.Role,
                PharmacyId = user.PharmacyId
            };
        }
        public async Task<int> AddUserAsync(UserDTO dto)
        {
            var user = new Models.User
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
        public async Task UpdateUserAsync(UserDTO dto)
        {
            var existing = await _repo.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new Exception("Utilizatorul nu a fost găsit.");

            existing.FirstName = dto.FirstName;
            existing.LastName = dto.LastName;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            existing.Username = dto.Username;
            existing.Ncm = dto.Ncm;
            existing.Role = dto.Role;
            existing.PharmacyId = dto.PharmacyId;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteUserAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
