// Créer par : tkahn
// Le : 24-06-2014

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using BezyFB.Helpers;
using BezyFB.Properties;
using Newtonsoft.Json.Linq;

namespace BezyFB.Freebox
{
    public static class Freebox
    {
        public static string GetPublicIp()
        {
            String direction = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            {
                var r = response.GetResponseStream();
                if (r != null)
                {
                    using (var stream = new StreamReader(r))
                    {
                        direction = stream.ReadToEnd();
                    }
                }
            }

            //Search for the ip in the html
            int first = direction.IndexOf("Address: ") + 9;
            int last = direction.LastIndexOf("</body>");
            direction = direction.Substring(first, last - first);

            return direction;
        }

        private static bool TestToken()
        {
            if (String.IsNullOrEmpty(Settings.Default.TokenFreebox))
            {
                Settings.Default.IpFreebox = "http://mafreebox.freebox.fr";

                var token = GenererToken();
                if (String.IsNullOrEmpty(token))
                    return false;

                Settings.Default.TokenFreebox = token;
                Settings.Default.IpFreebox = GetPublicIp();
            }
            return true;
        }

        public static void Download(String magnetUrl, string directory)
        {
            if (!TestToken()) return;
            var challenge = (string)ChallengeRequest()["result"]["challenge"];

            JObject session = SessionRequest(Settings.Default.AppId, Settings.Default.TokenFreebox, challenge);

            if (session != null)
            {
                var sessionRequest = (string)session["result"]["session_token"];

                try
                {
                    var path = System.Web.HttpUtility.UrlEncode(magnetUrl);
                    string content = "download_url=" + path + "\r\n&download_dir=" +
                                     Helper.EncodeTo64(Settings.Default.PathVideo + directory, Encoding.UTF8);
                    ApiConnector.Call(Settings.Default.TokenFreebox + "/api/v2/downloads/add/", WebMethod.Post,
                        "application/x-www-form-urlencoded", content,
                        null,
                        new List<Tuple<string, string>>
                        {
                            new Tuple<string, string>("X-Fbx-App-Auth", sessionRequest)
                        });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                // Logout
                ApiConnector.Call(Settings.Default.IpFreebox + "/api/v2/login/logout/", WebMethod.Post, null, null,
                    null,
                    new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", sessionRequest) });
            }
        }

        public static string GenererToken()
        {
            JObject apptokenrequest = AppTokenRequest();
            var appToken = (string)apptokenrequest["result"]["app_token"];
            var trackId = (string)apptokenrequest["result"]["track_id"];

            JObject apptokenstatus = AppTokenStatus(trackId);

            var result = (string)apptokenstatus["result"]["status"];
            while (result == "pending")
            {
                apptokenstatus = AppTokenStatus(trackId);
                result = (string)apptokenstatus["result"]["status"];
                Thread.Sleep(500);
            }

            return result == "granted" ? appToken : String.Empty;
        }

        private static JObject AppTokenRequest()
        {
            var o = new JObject
                {
                    {"app_id", Settings.Default.AppId},
                    {"app_name", Settings.Default.AppName},
                    {"app_version", Settings.Default.AppVersion},
                    {"device_name", Environment.MachineName}
                };

            var json = ApiConnector.Call(Settings.Default.IpFreebox + "/api/v2/login/authorize/", WebMethod.Post, "application/json", o.ToString());
            return JObject.Parse(json);
        }

        private static JObject AppTokenStatus(string trackid)
        {
            var json = ApiConnector.Call(Settings.Default.IpFreebox + "/api/v2/login/authorize/" + trackid, WebMethod.Get);
            return JObject.Parse(json);
        }

        private static JObject ChallengeRequest()
        {
            var json = ApiConnector.Call(Settings.Default.IpFreebox + "/api/v2/login/", WebMethod.Get);
            return JObject.Parse(json);
        }

        private static JObject SessionRequest(string appid, string apptoken, string challenge)
        {
            string password = Encode(challenge, apptoken);

            var o = new JObject
                {
                    {"password", password},
                    {"app_id", appid}
                };

            var json = ApiConnector.Call(Settings.Default.IpFreebox + "/api/v2/login/session/", WebMethod.Post, "application/json", o.ToString());
            return JObject.Parse(json);
        }

        private static string Encode(string input, string key)
        {
            var encoding = new UTF8Encoding();

            byte[] byteKey = encoding.GetBytes(key);
            byte[] byteInput = encoding.GetBytes(input);

            using (var hmacsha1 = new HMACSHA1(byteKey, false))
            {
                hmacsha1.Initialize();

                byte[] hashmessage = hmacsha1.ComputeHash(byteInput);
                return string.Concat(hashmessage.Select(b => string.Format("{0:X2}", b).ToLower()));
            }
        }
    }
}