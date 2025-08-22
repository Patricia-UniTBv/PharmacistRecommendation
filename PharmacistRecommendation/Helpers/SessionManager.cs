using DTO;

namespace PharmacistRecommendation.Helpers
{
    public static class SessionManager
    {
        public static UserDTO CurrentUser { get; private set; }

        public static void SetCurrentUser(UserDTO user)
        {
            CurrentUser = user;
        }

        public static int? GetCurrentUserId()
        {
            return CurrentUser.Id;
        }

        public static int? GetCurrentPharmacyId()
        {
            return CurrentUser?.PharmacyId;
        }
    }
}
