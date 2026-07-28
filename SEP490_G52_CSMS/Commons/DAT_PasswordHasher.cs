using System.Security.Cryptography;

namespace SEP490_G52_CSMS.Commons
{
    public static class DAT_PasswordHasher
    {
        private const int SaltSize = 16; // 128-bit salt
        private const int KeySize = 32;  // 256-bit subkey
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        /// <summary>
        /// Hashes a password using PBKDF2.
        /// </summary>
        public static string HashPassword(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);

            // Format: Iterations.SaltBase64.HashBase64
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifies a password against a PBKDF2 hash.
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (hashedPassword == null) return false;

            try
            {
                var parts = hashedPassword.Split('.', 3);
                if (parts.Length != 3) return false;

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] hash = Convert.FromBase64String(parts[2]);

                byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithm, hash.Length);

                return CryptographicOperations.FixedTimeEquals(hash, inputHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
