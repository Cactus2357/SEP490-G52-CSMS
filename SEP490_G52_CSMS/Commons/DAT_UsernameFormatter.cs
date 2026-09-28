using System.Globalization;
using System.Text;

namespace SEP490_G52_CSMS.Commons
{
    public static class DAT_UsernameFormatter
    {
        public static string Format(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            // Convert to lowercase first
            string lowerName = name.Trim().ToLowerInvariant();

            // Replace 'đ' and 'Đ' manually since normal form normalization does not map them to 'd'
            lowerName = lowerName.Replace("đ", "d").Replace("Đ", "d");

            // Normalize to FormD (decomposes characters, separate base letters from diacritics/accents)
            string normalized = lowerName.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            for (int i = 0; i < normalized.Length; i++)
            {
                char c = normalized[i];
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

                // Skip non-spacing marks (which represent diacritics/accents)
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    // Only keep alphanumeric characters (remove spaces and special symbols)
                    if (char.IsLetterOrDigit(c))
                    {
                        sb.Append(c);
                    }
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
