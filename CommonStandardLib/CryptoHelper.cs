using System.Text;

namespace CommonStandardLib
{
    public static class CryptoHelper
    {
        public static string EncodeTo64(this string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
    }
}
