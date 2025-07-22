using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Helpers.Services
{
    public class ReceiptImportModel
    {
        public List<ReceiptDrugModel> Medications { get; set; } = new List<ReceiptDrugModel>();
    }

}
