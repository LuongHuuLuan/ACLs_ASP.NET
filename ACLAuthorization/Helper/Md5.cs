using System.Security.Cryptography;
using System.Text;

namespace ACLAuthorization.Utils
{
    public static class Md5
    {

        public static string GennerateMD5(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}
