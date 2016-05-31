using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CommonPortableLib;

namespace BezyFB.Helpers
{
    public class Cryptographic : ICryptographic
    {
        public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public string GetMd5Hash(string input)
        {
            HashAlgorithm md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(data.Select(b => $"{b:X2}".ToLower()));
        }

        public string Encode(string input, string key)
        {
            byte[] byteKey = Encoding.UTF8.GetBytes(key);
            byte[] byteInput = Encoding.UTF8.GetBytes(input);

            using (var hmacsha1 = new HMACSHA1(byteKey, false))
            {
                hmacsha1.Initialize();

                byte[] hashmessage = hmacsha1.ComputeHash(byteInput);
                return string.Concat(hashmessage.Select(b => string.Format("{0:X2}", b).ToLower()));
            }
        }
        
    }
}