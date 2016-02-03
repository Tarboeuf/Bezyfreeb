using System;
using System.Linq;
using Windows.Security.Cryptography;
using System.Text;
using Windows.UI.Popups;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace BezyFB_UWP.Lib.Helpers
{
    public static class Helper
    {
        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public static string GetMd5Hash(string input)
        {
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);

            var md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var buffHash = md5.HashData(buffUtf8Msg);
            string strHashBase64 = CryptographicBuffer.EncodeToHexString(buffHash);
            return strHashBase64;
            //byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            //return string.Concat(data.Select(b => string.Format("{0:X2}", b).ToLower()));
        }

        public static string Encode(string input, string key)
        {
            byte[] byteKey = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(input);


            MacAlgorithmProvider objMacProb = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
            var hmacKey = objMacProb.CreateKey(byteKey.AsBuffer());
            IBuffer buffHMAC = CryptographicEngine.Sign(hmacKey, messageBytes.AsBuffer());
            return string.Concat(buffHMAC.ToArray().Select(b => string.Format("{0:X2}", b).ToLower()));
            //HashAlgorithmNames.Sha1
            //using (var hmacsha1 = new HMACSHA1(byteKey, false))
            //{
            //    hmacsha1.Initialize();

            //    byte[] hashmessage = hmacsha1.ComputeHash(byteInput);
            //    return string.Concat(hashmessage.Select(b => string.Format("{0:X2}", b).ToLower()));
            //}
        }

        public static string MessageBuffer { get; set; }
        public static bool AffichageErreurMessageBox { get; private set; }

        public static void AddMessageBuffer(string message)
        {
            if (!string.IsNullOrEmpty(MessageBuffer))
                MessageBuffer += Environment.NewLine;
            MessageBuffer += message;
        }

        public static async void AfficherMessage(string message)
        {
            var md = new MessageDialog(message);
            await md.ShowAsync();
        }

        public static async Task<YesNo> ShowYesNoDialog(string content)
        {

            var dialog = new MessageDialog(content,"Question");

            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = YesNo.Yes });
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = YesNo.No });

            //if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            //{
            //    // Adding a 3rd command will crash the app when running on Mobile !!!
            //    dialog.Commands.Add(new Windows.UI.Popups.UICommand("Maybe later") { Id = 2 });
            //}

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            var result = await dialog.ShowAsync();
            return (YesNo)result.Id;
        }

        
    }
    public enum YesNo
    {
        Yes,
        No
    }
}