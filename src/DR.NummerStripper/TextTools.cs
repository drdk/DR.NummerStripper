using System;
using System.Text.RegularExpressions;

namespace DR.NummerStripper
{
    public static class TextTools
    {
        private static readonly Regex _escapedUncPath = new Regex(@"^\\{4}", RegexOptions.Compiled);
        

        private static readonly Regex _prdNbr = new Regex(@"^([01])(?:-|)(\d{3})(?:-|)(\d{2})(?:-|)(\d{4})(?:-|)(\d)$",
            RegexOptions.Compiled);

        public static bool IsProductionNumber(this string value) => _prdNbr.IsMatch(value);

        public static bool IsWhatsOnProductionNumber(this string value) =>
            _prdNbr.IsMatch(value) && value.Contains("-");

        public static string ToWhatsOnProductionNumber(this string value)
        {
            
            var m = _prdNbr.Match(value);
            if (!m.Success)
                throw new ArgumentException("not a production number", nameof(value));
            var g = m.Groups;
            return $"{g[1]}-{g[2]}-{g[3]}-{g[4]}-{g[5]}";
        }

        public static string ToCleanProductionNumber(this string value)
        {
            var m = _prdNbr.Match(value);
            if (!m.Success)
                throw new ArgumentException("not a production number", nameof(value));
            var g = m.Groups;
            return $"{g[1]}{g[2]}{g[3]}{g[4]}{g[5]}";
        }

        public static bool IsEscapedUncPath(this string value) => _escapedUncPath.IsMatch(value);

        public static string UnescapedUncPath(this string value)
        {
            var m = _escapedUncPath.Match(value);

            if (!m.Success)
                throw new ArgumentException("not escaped unc path", nameof(value));

            return value.Replace(@"\\", @"\");
        }

    }
}
