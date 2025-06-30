using System.Text.RegularExpressions;

namespace QsfpEyeDiagram
{
    internal static class ComPortStringHelper
    {
        private static readonly Regex ComPortRegex = new Regex("^COM[0-9]+$", RegexOptions.IgnoreCase & RegexOptions.Compiled & RegexOptions.CultureInvariant);

        internal static bool GetComPortNumber(string comPortName, out int number)
        {
            if (comPortName != null && ComPortRegex.IsMatch(comPortName))
            {
                number = int.Parse(comPortName.Substring(3));
                return true;
            }

            number = -1;
            return false;
        }
    }
}
