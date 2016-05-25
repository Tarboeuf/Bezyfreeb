using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using CommonPortableLib;

namespace CommonLib
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
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);

            var md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var buffHash = md5.HashData(buffUtf8Msg);
            string strHashBase64 = CryptographicBuffer.EncodeToHexString(buffHash);
            return strHashBase64;
        }

        public string Encode(string input, string key)
        {
            byte[] byteKey = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(input);
            
            MacAlgorithmProvider objMacProb = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
            var hmacKey = objMacProb.CreateKey(byteKey.AsBuffer());
            IBuffer buffHMAC = CryptographicEngine.Sign(hmacKey, messageBytes.AsBuffer());
            return string.Concat(buffHMAC.ToArray().Select(b => string.Format("{0:X2}", b).ToLower()));
        }

    }
}