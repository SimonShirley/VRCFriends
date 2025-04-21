using System.Net;
using System.Security;

namespace VRCFriends.Extensions
{
    public static class StringExtensions
    {
        // convert a secure string into a normal plain text string
        public static string ToPlainString(this SecureString secureStr) => new NetworkCredential(string.Empty, secureStr).Password;

        // convert a plain text string into a secure string
        public static SecureString ToSecureString(this string plainStr)
        {
            var secStr = new SecureString();
            secStr.Clear();
            
            foreach (char c in plainStr.ToCharArray())
                secStr.AppendChar(c);

            return secStr;
        }
    }
}
