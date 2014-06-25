// Créer par : pepinat
// Le : 25-06-2014

using System.Text;

namespace BezyFB
{
    public static class Helper
    {
        public static string EncodeTo64(string toEncode, Encoding encoding = null)
        {
            if (null == encoding)
                encoding = ASCIIEncoding.ASCII;
            byte[] toEncodeAsBytes = encoding.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
    }
}