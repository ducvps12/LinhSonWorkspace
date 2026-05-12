using System.Linq;
using LinhSonWorkspace.Data;
using LinhSonWorkspace.Models;
using Microsoft.EntityFrameworkCore;

namespace LinhSonWorkspace.Services
{
    /// <summary>
    /// Handles user authentication and authorization.
    /// </summary>
    public class AuthService
    {
        /// <summary>
        /// Authenticates a user by username and password.
        /// Returns the User object if successful, null otherwise.
        /// </summary>
        public User? Login(string username, string password)
        {
            using var context = new AppDbContext();

            // Find user by username, include Role for authorization
            var user = context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username == username && u.IsActive);

            if (user == null) return null;

            // Verify password using BCrypt
            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return isValid ? user : null;
        }
    }
}
