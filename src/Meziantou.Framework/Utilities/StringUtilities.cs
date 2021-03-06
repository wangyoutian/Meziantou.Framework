﻿using System;
using System.Globalization;
using System.Text;
using System.Diagnostics.Contracts;

namespace Meziantou.Framework.Utilities
{
    public static class StringUtilities
    {
        [Pure]
        public static string Nullify(this string str, bool trim)
        {
            if (str == null)
                return null;

            if (trim)
            {
                str = str.Trim();
            }

            if (string.IsNullOrEmpty(str))
                return null;

            return str;
        }

        [Pure]
        public static bool EqualsIgnoreCase(this string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        [Pure]
        public static bool ContainsIgnoreCase(this string str, string value)
        {
            if (str == null)
                return value == null;

            return str.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        [Pure]
        public static string RemoveDiacritics(this string str)
        {
            if (str == null)
                return null;

            var normalizedString = str.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        [Pure]
        public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison)
        {
            if (str == null)
                return null;

            var sb = new StringBuilder();

            var previousIndex = 0;
            var index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str, previousIndex, index - previousIndex);
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }

            sb.Append(str.Substring(previousIndex));
            return sb.ToString();
        }
    }
}
