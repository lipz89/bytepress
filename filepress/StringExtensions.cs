using System;

namespace filepress
{
    public static class StringExtensions
    {
        private const long OneKb = 1024;
        private const long OneMb = OneKb * 1024;
        private const long OneGb = OneMb * 1024;
        private const long OneTb = OneGb * 1024;

        public static string ToPrettySize(this int value, int decimalPlaces = 0)
        {
            return ((long)value).ToPrettySize(decimalPlaces);
        }

        public static string ToPrettySize(this long value, int decimalPlaces = 0)
        {
            var asTb = Math.Round((double)value / OneTb, decimalPlaces);
            var asGb = Math.Round((double)value / OneGb, decimalPlaces);
            var asMb = Math.Round((double)value / OneMb, decimalPlaces);
            var asKb = Math.Round((double)value / OneKb, decimalPlaces);
            string chosenValue = asTb > 1 ? $"{asTb}TB"
                : asGb > 1 ? $"{asGb}GB"
                    : asMb > 1 ? $"{asMb}MB"
                        : asKb > 1 ? $"{asKb}KB"
                            : $"{Math.Round((double)value, decimalPlaces)}B";
            return chosenValue;
        }
    }
}
