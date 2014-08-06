// Créer par : pepinat
// Le : 25-06-2014

using System;
using System.Linq;
using System.Text;
using System.Windows;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace BezyFB.Helpers
{
    public static class Helper
    {
        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        //public static string GetMd5Hash(HashAlgorithm md5Hash, string input)
        //{
        //    byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        //    return string.Concat(data.Select(b => string.Format("{0:X2}", b).ToLower()));
        //}
        public static string GetMd5Hash(string str)
        {
            try
            {
                var alg = HashAlgorithmProvider.OpenAlgorithm("MD5");
                IBuffer buff = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
                var hashed = alg.HashData(buff);
                var res = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, hashed);
                return res;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Encode(string input, string key)
        {
            byte[] byteKey = Encoding.UTF8.GetBytes(key);
            byte[] byteInput = Encoding.UTF8.GetBytes(input);
            var algo = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
            BinaryStringEncoding encoding = BinaryStringEncoding.Utf8;
            var buffMsg = CryptographicBuffer.ConvertStringToBinary(input, encoding);
            var hmacsha1 = algo.CreateHash(buffMsg);
            
                hmacsha1.Append(buffMsg);
                return hmacsha1.GetValueAndReset().ToString();
                //byte[] hashmessage = hmacsha1.ComputeHash(byteInput);
                //return string.Concat(hashmessage.Select(b => string.Format("{0:X2}", b).ToLower()));
            
        }

        
        private static void CreateHMAC(
            String strMsg,
            out IBuffer buffMsg,
            out CryptographicKey hmacKey,
            out IBuffer buffHMAC)
        {
            // Create a MacAlgorithmProvider object for the specified algorithm.
            MacAlgorithmProvider objMacProv = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);

            // Demonstrate how to retrieve the name of the algorithm used.
            String strNameUsed = objMacProv.AlgorithmName;

            // Create a buffer that contains the message to be signed.
            BinaryStringEncoding encoding = BinaryStringEncoding.Utf8;
            buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);

            // Create a key to be signed with the message.
            IBuffer buffKeyMaterial = CryptographicBuffer.GenerateRandom(objMacProv.MacLength);
            hmacKey = objMacProv.CreateKey(buffKeyMaterial);

            // Sign the key and message together.
            buffHMAC = CryptographicEngine.Sign(hmacKey, buffMsg);

            // Verify that the HMAC length is correct for the selected algorithm
            if (buffHMAC.Length != objMacProv.MacLength)
            {
                throw new Exception("Error computing digest");
            }
        }
    }
}