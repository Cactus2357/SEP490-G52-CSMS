// CSMSSystem Global Client-Side Search Engine (Fuzzy + Unicode Accent-Insensitive)
(function (window) {
    'use strict';

    var CSMSSearch = {
        /**
         * Removes Vietnamese accents, marks, and converts đ/Đ to d/D.
         */
        removeVietnameseTones: function (str) {
            if (!str) return '';
            str = String(str);
            // Normalize to NFD form to decompose accents
            str = str.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
            // Handle d with stroke
            str = str.replace(/[đĐ]/g, function (match) {
                return match === 'đ' ? 'd' : 'D';
            });
            return str.toLowerCase().trim();
        },

        /**
         * Calculates Levenshtein edit distance between two strings.
         */
        levenshteinDistance: function (s, t) {
            if (!s) return (t || '').length;
            if (!t) return s.length;

            var n = s.length;
            var m = t.length;
            var d = [];

            for (var j = 0; j <= m; j++) d[j] = j;

            for (var i = 1; i <= n; i++) {
                var prevDiagonal = d[0];
                d[0] = i;

                for (var k = 1; k <= m; k++) {
                    var temp = d[k];
                    var cost = (s.charAt(i - 1) === t.charAt(k - 1)) ? 0 : 1;
                    d[k] = Math.min(Math.min(d[k] + 1, d[k - 1] + 1), prevDiagonal + cost);
                    prevDiagonal = temp;
                }
            }

            return d[m];
        },

        /**
         * Checks if pattern is a subsequence of source.
         */
        isSubsequence: function (source, pattern) {
            if (!pattern) return true;
            if (!source) return false;

            var sIdx = 0, pIdx = 0;
            while (sIdx < source.length && pIdx < pattern.length) {
                if (source.charAt(sIdx) === pattern.charAt(pIdx)) {
                    pIdx++;
                }
                sIdx++;
            }

            return pIdx === pattern.length;
        },

        /**
         * Checks if a single source token matches a pattern token.
         */
        matchToken: function (sToken, pToken) {
            if (sToken === pToken) return true;
            if (pToken.length === 1) {
                return sToken === pToken || sToken.indexOf(pToken) === 0;
            }
            if (sToken.indexOf(pToken) === 0) return true;
            if (sToken.indexOf(pToken) !== -1) return true;
            if (pToken.length >= 3 && this.isSubsequence(sToken, pToken)) return true;

            var dist = this.levenshteinDistance(sToken, pToken);
            if (pToken.length >= 4 && dist <= 1) return true;
            if (pToken.length >= 6 && dist <= 2) return true;

            return false;
        },

        /**
         * Performs fuzzy match between source string and query.
         * Returns true if query matches source.
         */
        fuzzyMatch: function (source, query) {
            if (!query || !query.trim()) return true;
            if (!source) return false;

            var normSource = this.removeVietnameseTones(source);
            var normQuery = this.removeVietnameseTones(query);

            if (!normQuery) return true;

            // Direct substring check
            if (normSource.indexOf(normQuery) !== -1) return true;

            // Also check with 'ph' replaced with 'f' for Vietnamese phonetic convenience (e.g. ca fe / cf -> ca phe)
            var fSource = normSource.replace(/ph/g, 'f');
            var fQuery = normQuery.replace(/ph/g, 'f');
            if (fSource.indexOf(fQuery) !== -1) return true;

            var queryTokens = normQuery.split(/\s+/).filter(Boolean);
            if (queryTokens.length === 0) return true;

            var sourceTokens = normSource.split(/\s+/).filter(Boolean);
            var fSourceTokens = fSource.split(/\s+/).filter(Boolean);
            var fQueryTokens = fQuery.split(/\s+/).filter(Boolean);

            // Initials (acronym) check: e.g. "cpsd" or "cf" for "ca phe sua da"
            var initials = sourceTokens.map(function(t) { return t.charAt(0); }).join('');
            var fInitials = fSourceTokens.map(function(t) { return t.charAt(0); }).join('');
            if (this.isSubsequence(initials, normQuery) || this.isSubsequence(fInitials, fQuery)) return true;

            // Single token subsequence check against compacted source
            if (queryTokens.length === 1) {
                var qToken = queryTokens[0];
                var sourceNoSpace = normSource.replace(/\s+/g, '');
                if (this.isSubsequence(sourceNoSpace, qToken)) return true;

                var fSourceNoSpace = fSource.replace(/\s+/g, '');
                if (this.isSubsequence(fSourceNoSpace, fQuery)) return true;
            }

            // Multi-token: check if each pattern token matches a distinct source token
            var self = this;
            var used = new Array(sourceTokens.length);
            for (var u = 0; u < used.length; u++) used[u] = false;

            for (var i = 0; i < queryTokens.length; i++) {
                var pToken = queryTokens[i];
                var fpToken = fQueryTokens[i] || pToken;
                var matched = false;

                for (var j = 0; j < sourceTokens.length; j++) {
                    if (used[j]) continue;

                    var sToken = sourceTokens[j];
                    var fsToken = fSourceTokens[j] || sToken;

                    if (self.matchToken(sToken, pToken) || self.matchToken(fsToken, fpToken)) {
                        used[j] = true;
                        matched = true;
                        break;
                    }
                }

                if (!matched) {
                    return false;
                }
            }

            return true;
        },

        /**
         * Checks if query matches any of the supplied source strings.
         */
        fuzzyMatchAny: function (query, sources) {
            if (!query || !query.trim()) return true;
            if (!sources || !sources.length) return false;

            for (var i = 0; i < sources.length; i++) {
                if (this.fuzzyMatch(sources[i], query)) {
                    return true;
                }
            }

            return false;
        }
    };

    window.CSMSSearch = CSMSSearch;
})(window);

