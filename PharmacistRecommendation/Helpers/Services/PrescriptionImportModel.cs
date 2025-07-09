using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PharmacistRecommendation.Helpers.Services
{
    public class PrescriptionImportModel
    {
        public string PatientCnp { get; set; }
        public string PrescriptionSeries { get; set; }
        public string PrescriptionNumber { get; set; }
        public string DoctorStamp { get; set; }
        public string Diagnosis { get; set; }
        public List<PrescriptionDrugModel> Drugs { get; set; } = new();
    }
}
