using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CommonStandardLib
{
    public class Cryptographic : ICryptographic
    {
        public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public string GetMd5Hash(string input)
        {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            var bytes = provider.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Encoding.UTF8.GetString(bytes);
        }

        public string Encode(string input, string key)
        {
            byte[] byteKey = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(input);

            HMACSHA1 hmacsha1 = new HMACSHA1(byteKey);

            var bytes = hmacsha1.ComputeHash(messageBytes);
            
            return string.Concat(bytes.Select(b => $"{b:X2}".ToLower()));
        }

    }
}