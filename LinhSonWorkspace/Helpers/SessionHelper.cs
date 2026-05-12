using LinhSonWorkspace.Models;

namespace LinhSonWorkspace.Helpers
{
    /// <summary>
    /// Manages the current logged-in user session.
    /// </summary>
    public static class SessionHelper
    {
        public static User? CurrentUser { get; set; }

        public static bool IsAdmin => CurrentUser?.Role?.RoleName == "Admin";

        public static bool IsLoggedIn => CurrentUser != null;

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
