using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Helpers.Services
{
    public class PrescriptionDrugModel
    {
        public int Index { get; set; }
        public string DisplayText { get; set; }
        public string Name { get; set; }
        public string Concentration { get; set; }
        public string PharmaceuticalForm { get; set; }
        public string Morning { get; set; }
        public string Noon { get; set; }
        public string Evening { get; set; }
        public string Night { get; set; }
        public string AdministrationMode { get; set; }
        public string Dose { get; set; }
        public string DiseaseCode { get; set; }
    }
}