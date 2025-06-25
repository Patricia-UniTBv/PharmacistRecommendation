using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PharmacyCardDTO
    {
        public int Id { get; set; }

        public int PharmacyId { get; set; }

        public int PatientId { get; set; }

        public string? Code { get; set; }

        public DateTime IssueDate { get; set; }
    }
}
