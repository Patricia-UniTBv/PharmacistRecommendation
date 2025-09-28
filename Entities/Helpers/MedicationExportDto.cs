using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Helpers
{
    public class MedicationExportDto
    {
        public string MedicineName { get; set; }
        public string MedicineMorning { get; set; }
        public string MedicineLunch { get; set; }
        public string MedicineEvening { get; set; }
        public string MedicineNight { get; set; }
        public string MedicineAdministration { get; set; }
    }
}
