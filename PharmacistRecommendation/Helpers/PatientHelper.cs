using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Helpers
{
    class PatientHelper
    {
        public static int CalculateAge(DateTime? birthdate)
        {
            if (birthdate == null) return 0;

            var today = DateTime.Today;
            var age = today.Year - birthdate.Value.Year;
            if (birthdate > today.AddYears(-age)) age--;

            return age;
        }
    }
}
