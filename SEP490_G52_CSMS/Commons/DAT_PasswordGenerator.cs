using System;
using System.Security.Cryptography;
using System.Text;

namespace SEP490_G52_CSMS.Commons
{
    public static class DAT_PasswordGenerator
    {
        private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lower = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string Special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        public static string Generate(int length = 10)
        {
            if (length < 4) throw new ArgumentException("Length must be at least 4.", nameof(length));

            var chars = new StringBuilder();
            
            // Ensure at least one of each required group
            chars.Append(GetRandomChar(Upper));
            chars.Append(GetRandomChar(Lower));
            chars.Append(GetRandomChar(Digits));
            chars.Append(GetRandomChar(Special));

            string allPossible = Upper + Lower + Digits + Special;
            for (int i = 4; i < length; i++)
            {
                chars.Append(GetRandomChar(allPossible));
            }

            // Shuffle the characters
            var charArray = chars.ToString().ToCharArray();
            Shuffle(charArray);
            return new string(charArray);
        }

        private static char GetRandomChar(string pool)
        {
            return pool[RandomNumberGenerator.GetInt32(pool.Length)];
        }

        private static void Shuffle<T>(T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = RandomNumberGenerator.GetInt32(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}
