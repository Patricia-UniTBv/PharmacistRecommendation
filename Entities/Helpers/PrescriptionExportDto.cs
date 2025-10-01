using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Helpers
{
    public class PrescriptionExportDto
    {
        public string PharmacyName { get; set; }
        public string PharmacyAdress { get; set; }
        public string PharmacyCUI { get; set; }
        public string PharmacyPhone { get; set; }
        public string PharmacyEmail { get; set; }
        public string PacientCard { get; set; }
        public string CardAderenta { get; set; }
        public string RetetaType { get; set; }
        public string Note { get; set; }
        public string ConfirmareAdresareMedic { get; set; }
        public string ConfirmareRidicareReteta { get; set; }
        public string MedicParafa { get; set; }
        public string MedicEmail { get; set; }
        public string SerieReteta { get; set; }
        public string NrReteta { get; set; }
        public List<MedicationExportDto> Reteta { get; set; }

    }
}
