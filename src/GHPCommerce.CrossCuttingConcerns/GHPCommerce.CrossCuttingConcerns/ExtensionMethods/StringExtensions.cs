using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GHPCommerce.CrossCuttingConcerns.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string Left(this string value, int length)
        {
            length = Math.Abs(length);
            return string.IsNullOrEmpty(value) ? value : value.Substring(0, Math.Min(value.Length, length));
        }

        public static int GetNumber(this string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                return 0;
            Regex re = new Regex(@"\d+");
            Match m = re.Match(value);

            return m.Success ? Int32.Parse(m.Value ) : 0;
        }

        public static string Right(this string value, int length)
        {
            length = Math.Abs(length);
            return string.IsNullOrEmpty(value) ? value : value.Substring(value.Length - Math.Min(value.Length, length));
        }

        public static bool In(this string value, List<string> list)
        {
            return list.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool NotIn(this string value, List<string> list)
        {
            return !In(value, list);
        }

        public static bool EqualsIgnoreCase(this string source, string toCheck)
        {
            return string.Equals(source, toCheck, StringComparison.OrdinalIgnoreCase);
        }

        public static string ToBase64(this string src)
        {
            byte[] b = Encoding.UTF8.GetBytes(src);
            return Convert.ToBase64String(b);
        }

        public static string ToBase64(this string src, Encoding encoding)
        {
            byte[] b = encoding.GetBytes(src);
            return Convert.ToBase64String(b);
        }

        public static string FromBase64String(this string src)
        {
            byte[] b = Convert.FromBase64String(src);
            return Encoding.UTF8.GetString(b);
        }

        public static string FromBase64String(this string src, Encoding encoding)
        {
            byte[] b = Convert.FromBase64String(src);
            return encoding.GetString(b);
        }

        public static string Remove(this string source, params string[] removedValues)
        {
            removedValues.ToList().ForEach(x => source = source.Replace(x, ""));
            return source;
        }
        public static  string UppercaseFirst(this string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }
        public static string PascalToKebabCaseMessage(this string value)
        {
            return pascalToKebabCase(value,"message");           
        }
        public static string PascalToKebabCaseActivity(this string value)
        {
            return pascalToKebabCase(value, "activity");
        }
        private static string pascalToKebabCase(string value,string postfix)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var result = Regex.Replace(
                    value,
                    "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                    "-$1",
                    RegexOptions.Compiled)
                .Trim()
                .ToLower();

            var segments = result.Split('-');
            if (segments[segments.Length - 1] != postfix)
                return result;
            return result.Substring(0, result.Length - 8);
        }
         public static string ToFriendlyCase(this string PascalString)
         {
                return Regex.Replace(PascalString, "(?!^)([A-Z])", " $1");
        }
        public static  object ConvertTo(this  string term,Type type)
        {
            return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(term);
        }
    }
}
