using System;
using System.Text.RegularExpressions;

namespace UnderMineControl.Loader.Core.Models
{
    public class WildCard : IComparable
    {
        public string OriginalString { get; private set; }

        public string RegexString { get; private set; }

        public WildCard(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                OriginalString = null;
                RegexString = null;
                return;
            }

            OriginalString = pattern;
            RegexString = RegexStringWildCard(pattern);
        }

        public bool Matches(string value)
        {
            if (string.IsNullOrEmpty(value) &&
                string.IsNullOrEmpty(RegexString))
                return true;

            if (string.IsNullOrEmpty(value) ||
                string.IsNullOrEmpty(RegexString))
                return false;

            if (OriginalString == value)
                return true;

            return Regex.IsMatch(value, RegexString);
        }

        public int CompareTo(object obj)
        {
            return Equals(obj) ? 0 : 1;
        }

        public override string ToString()
        {
            return OriginalString;
        }

        public override bool Equals(object obj)
        {
            return Matches(obj.ToString());
        }

        public override int GetHashCode()
        {
            return OriginalString.GetHashCode();
        }

        public static implicit operator string(WildCard card)
        {
            return card.OriginalString;
        }

        public static implicit operator WildCard(string pattern)
        {
            return new WildCard(pattern);
        }

        public static bool operator ==(WildCard card, string value)
        {
            return card.Matches(value);
        }

        public static bool operator !=(WildCard card, string value)
        {
            return !card.Matches(value);
        }

        public static bool operator ==(string value, WildCard card)
        {
            return card.Matches(value);
        }

        public static bool operator !=(string value, WildCard card)
        {
            return !card.Matches(value);
        }

        public static string RegexStringWildCard(string pattern)
        {
            return "^" + Regex.Escape(pattern).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
    }
}
