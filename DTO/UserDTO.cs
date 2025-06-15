using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class UserDTO
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;
        public string? Password { get; set; }
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!; // "Admin", "Pharmacist", "Assistant"

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Ncm { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int PharmacyId { get; set; }
    }
}
