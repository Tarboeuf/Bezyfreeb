using System;
using System.Net;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;


namespace TarboeufEzBsFb
{
    public class Freebox
    {
        public static void Main2()
        {
            string appToken;
            string challenge;

            if (!File.Exists("appid"))
            {
                JObject apptokenrequest = AppTokenRequest();
                appToken = (string)apptokenrequest["result"]["app_token"];
                var trackId = (string)apptokenrequest["result"]["track_id"];

                TextWriter tw = new StreamWriter("appid");
                tw.WriteLine(appToken);
                tw.Close();

                JObject apptokenstatus = AppTokenStatus(trackId);
                challenge = (string)apptokenstatus["result"]["challenge"];

                while ((string)apptokenstatus["result"]["status"] == "pending")
                    apptokenstatus = AppTokenStatus(trackId);
            }
            else
            {
                var tr = new StreamReader("appid");

                appToken = tr.ReadLine();

                tr.Close();

                challenge = (string)ChallengeRequest()["result"]["challenge"];
            }

            JObject session = SessionRequest("fr.freebox.testapp", appToken, challenge);

            if (session != null)
            {
                var sessionRequest = (string)session["result"]["session_token"];

                string output = ApiCall("http://mafreebox.freebox.fr/api/v2/downloads/", "", "application/json", sessionRequest, "GET");
                JObject o = JObject.Parse(output);
                Console.WriteLine(o.ToString());

                const string content = "download_url=http%3A%2F%2Fmirror.ovh.net%2Fubuntu-releases%2F14.04%2Fubuntu-14.04-desktop-amd64.iso.torrent";
                string outputDown = ApiCall("http://mafreebox.freebox.fr/api/v2/downloads/add/", content, "application/x-www-form-urlencoded", sessionRequest, "POST");
                JObject obj = JObject.Parse(outputDown);
                Console.WriteLine(obj.ToString());

                Logout(sessionRequest);
            }
        }

        private static JObject AppTokenRequest()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://mafreebox.freebox.fr/api/v2/login/authorize/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var o = new JObject
                {
                    { "app_id", "fr.freebox.testapp" },
                    { "app_name", "Test App" },
                    { "app_version", "1.0" },
                    { "device_name", "PC-Tristan" }
                };

                streamWriter.Write(o.ToString());
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = httpWebRequest.GetResponse().GetResponseStream();
                if (httpResponse == null) return null;
                using (var streamReader = new StreamReader(httpResponse))
                {
                    var json = streamReader.ReadToEnd();
                    return JObject.Parse(json);
                }
            }
        }

        private static JObject AppTokenStatus(string trackid)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://mafreebox.freebox.fr/api/v2/login/authorize/" + trackid);
            httpWebRequest.Method = "GET";

            var httpResponse = httpWebRequest.GetResponse().GetResponseStream();
            if (httpResponse == null) return null;
            using (var streamReader = new StreamReader(httpResponse))
            {
                var json = streamReader.ReadToEnd();
                return JObject.Parse(json);
            }
        }

        private static JObject ChallengeRequest()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://mafreebox.freebox.fr/api/v2/login/");
            httpWebRequest.Method = "GET";

            var httpResponse = httpWebRequest.GetResponse().GetResponseStream();
            if (httpResponse == null) return null;
            using (var streamReader = new StreamReader(httpResponse))
            {
                var json = streamReader.ReadToEnd();
                return JObject.Parse(json);
            }
        }

        private static void Logout(string sessionid)
        {
            ApiCall("http://mafreebox.freebox.fr/api/v2/login/logout/", "", "application/json", sessionid, "POST");
        }

        private static JObject SessionRequest(string appid, string apptoken, string challenge)
        {
            string password = Encode(challenge, apptoken);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://mafreebox.freebox.fr/api/v2/login/session/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var o = new JObject
                {
                    { "password", password },
                    { "app_id", appid }
                };

                streamWriter.Write(o.ToString());
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = httpWebRequest.GetResponse().GetResponseStream();
                if (httpResponse == null) return null;
                using (var streamReader = new StreamReader(httpResponse))
                {
                    var json = streamReader.ReadToEnd();
                    return JObject.Parse(json);
                }
            }
        }

        private static string ApiCall(string url, string content, string contentType, string sessionid, string method)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = method;
            httpWebRequest.ContentType = contentType;
            httpWebRequest.Headers.Add("X-Fbx-App-Auth", sessionid);

            if (!String.IsNullOrEmpty(content))
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(content);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }

            Stream httpResponse = httpWebRequest.GetResponse().GetResponseStream();
            if (null == httpResponse) return null;
            using (var streamReader = new StreamReader(httpResponse))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static string Encode(string input, string key)
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