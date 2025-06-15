using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PharmacistDTO
    {
        private string v1;
        private string v2;
        private string? v3;
        private string? v4;
        private string? v5;
        private string? v6;

        public PharmacistDTO(string v1, string v2, string? v3, string? v4, string? v5, string? v6, string? password)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.v4 = v4;
            this.v5 = v5;
            this.v6 = v6;
            Password = password;
        }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Ncm { get; set; } = null!;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }
    }
}
