using System.Text.RegularExpressions;

namespace ACLAuthorization.Helper
{
    public class StringConverter
    {
        public static string ConvertToColonFormat(string input)
        {
            // Sử dụng Regex để tìm và thay thế mẫu {id} và {r} thành :id và :r
            string pattern = @"\{([^}]*)\}";
            string replacement = ":$1";
            string output = Regex.Replace(input, pattern, replacement);
            return output;
        }
    }
}
