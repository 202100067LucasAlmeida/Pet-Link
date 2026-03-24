using BCrypt.Net;

namespace PetLink.Models
{
    public static class UserHashHelpers
    {
        /// <summary>
        /// Generates a BCrypt hash for the given plaintext password.
        /// Uses work factor of 12 (secure default).
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifies a plaintext password against a BCrypt hash.
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;
            
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}

