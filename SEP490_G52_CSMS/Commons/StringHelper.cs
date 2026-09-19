using System.Globalization;
using System.Text;

namespace SEP490_G52_CSMS.Commons
{
    public static class StringHelper
    {
        public static string RemoveDiacritics(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    if (c == 'đ') stringBuilder.Append('d');
                    else if (c == 'Đ') stringBuilder.Append('D');
                    else stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower().Trim();
        }

        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s.Length;

            int n = s.Length;
            int m = t.Length;
            int[] d = new int[m + 1];

            for (int j = 0; j <= m; j++) d[j] = j;

            for (int i = 1; i <= n; i++)
            {
                int prevDiagonal = d[0];
                d[0] = i;

                for (int j = 1; j <= m; j++)
                {
                    int temp = d[j];
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    d[j] = Math.Min(Math.Min(d[j] + 1, d[j - 1] + 1), prevDiagonal + cost);
                    prevDiagonal = temp;
                }
            }

            return d[m];
        }

        public static bool IsSubsequence(string source, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return true;
            if (string.IsNullOrEmpty(source)) return false;

            int sIdx = 0, pIdx = 0;
            while (sIdx < source.Length && pIdx < pattern.Length)
            {
                if (source[sIdx] == pattern[pIdx])
                {
                    pIdx++;
                }
                sIdx++;
            }

            return pIdx == pattern.Length;
        }

        private static bool MatchToken(string sToken, string pToken)
        {
            if (sToken == pToken) return true;
            if (pToken.Length == 1)
            {
                return sToken == pToken || sToken.StartsWith(pToken);
            }
            if (sToken.StartsWith(pToken)) return true;
            if (sToken.Contains(pToken)) return true;
            if (pToken.Length >= 3 && IsSubsequence(sToken, pToken)) return true;

            int dist = LevenshteinDistance(sToken, pToken);
            if (pToken.Length >= 4 && dist <= 1) return true;
            if (pToken.Length >= 6 && dist <= 2) return true;

            return false;
        }

        public static bool FuzzyMatch(string? source, string? pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return true;
            if (string.IsNullOrWhiteSpace(source)) return false;

            var normSource = RemoveDiacritics(source);
            var normPattern = RemoveDiacritics(pattern);

            if (normSource.Contains(normPattern)) return true;

            // Also check with 'ph' replaced with 'f' for Vietnamese phonetic convenience (e.g. ca fe / cf -> ca phe)
            var fSource = normSource.Replace("ph", "f");
            var fPattern = normPattern.Replace("ph", "f");

            if (fSource.Contains(fPattern)) return true;

            var patternTokens = normPattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (patternTokens.Length == 0) return true;

            var sourceTokens = normSource.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Initials (acronym) check: e.g. "cpsd" or "cf" for "ca phe sua da"
            var initials = string.Concat(sourceTokens.Where(t => t.Length > 0).Select(t => t[0]));
            var fInitials = string.Concat(fSource.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Where(t => t.Length > 0).Select(t => t[0]));

            if (IsSubsequence(initials, normPattern) || IsSubsequence(fInitials, fPattern)) return true;

            // Single token subsequence check against full source without spaces
            if (patternTokens.Length == 1)
            {
                var pToken = patternTokens[0];
                var sourceNoSpace = normSource.Replace(" ", "");
                if (IsSubsequence(sourceNoSpace, pToken)) return true;

                var fSourceNoSpace = fSource.Replace(" ", "");
                if (IsSubsequence(fSourceNoSpace, fPattern)) return true;
            }

            // Check if each pattern token matches a distinct source token
            var fSourceTokens = fSource.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var fPatternTokens = fPattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var used = new bool[sourceTokens.Length];
            for (int pIdx = 0; pIdx < patternTokens.Length; pIdx++)
            {
                var pToken = patternTokens[pIdx];
                var fpToken = fPatternTokens[pIdx];
                bool tokenMatched = false;

                for (int sIdx = 0; sIdx < sourceTokens.Length; sIdx++)
                {
                    if (used[sIdx]) continue;

                    var sToken = sourceTokens[sIdx];
                    var fsToken = fSourceTokens[sIdx];

                    if (MatchToken(sToken, pToken) || MatchToken(fsToken, fpToken))
                    {
                        used[sIdx] = true;
                        tokenMatched = true;
                        break;
                    }
                }

                if (!tokenMatched)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool FuzzyMatchAny(string? pattern, params string?[] sources)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return true;
            if (sources == null || sources.Length == 0) return false;

            foreach (var s in sources)
            {
                if (FuzzyMatch(s, pattern)) return true;
            }

            return false;
        }
    }
}
