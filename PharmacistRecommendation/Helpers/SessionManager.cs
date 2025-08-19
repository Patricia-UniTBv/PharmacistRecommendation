using DTO;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Helpers
{
    public static class SessionManager
    {
        public static UserDTO CurrentUser { get; private set; }

        public static void SetCurrentUser(UserDTO user)
        {
            CurrentUser = user;
        }

        public static int? GetCurrentPharmacyId()
        {
            return CurrentUser?.PharmacyId;
        }
    }
}
