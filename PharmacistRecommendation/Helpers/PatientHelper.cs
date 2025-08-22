
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
