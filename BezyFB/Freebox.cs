// Créer par : tkahn
// Le : 24-06-2014

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using BezyFB.Properties;
using Newtonsoft.Json.Linq;

namespace BezyFB
{
    public class Freebox
    {
        private const string _ADDR_FREEBOX = "http://88.120.230.2/";

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
                {
                    apptokenstatus = AppTokenStatus(trackId);
                    Thread.Sleep(500);
                }
            }
            else
            {
                var tr = new StreamReader("appid");

                appToken = tr.ReadLine();

                tr.Close();

                challenge = (string)ChallengeRequest()["result"]["challenge"];
            }

            JObject session = SessionRequest(Settings.Default.AppId, appToken, challenge);
        }

        public static string AjouterTorrent(JObject session, string torrentPath, string directory)
        {
            if (session != null)
            {
                var sessionRequest = (string)session["result"]["session_token"];

                string output = ApiCall(_ADDR_FREEBOX + "/api/v2/downloads/", "", "application/json", sessionRequest, "GET");
                JObject o = JObject.Parse(output);
                Console.WriteLine(o.ToString());

                string content = "download_url=" + torrentPath + "&download_dir=" + directory;
                string outputDown = ApiCall(_ADDR_FREEBOX + "/api/v2/downloads/add/", content, "application/x-www-form-urlencoded", sessionRequest, "POST");
                JObject obj = JObject.Parse(outputDown);
                Console.WriteLine(obj.ToString());

                string id = obj.GetValue("result").First["id"].ToString();

                outputDown = ApiCall(_ADDR_FREEBOX + "/api/v2/downloads/add/", id, "application/x-www-form-urlencoded", sessionRequest, "POST");
                obj = JObject.Parse(outputDown);
                string nomFichier = obj.GetValue("result").First["name"].ToString();
                Logout(sessionRequest);
                return nomFichier;
            }
            return null;
        }

        public static void GenererToken()
        {
            if (!File.Exists("appid"))
            {
                JObject apptokenrequest = AppTokenRequest();
                string appToken = (string)apptokenrequest["result"]["app_token"];
                var trackId = (string)apptokenrequest["result"]["track_id"];

                TextWriter tw = new StreamWriter("appid");
                tw.WriteLine(appToken);
                tw.Close();

                JObject apptokenstatus = AppTokenStatus(trackId);

                while ((string)apptokenstatus["result"]["status"] == "pending")
                {
                    apptokenstatus = AppTokenStatus(trackId);
                    Thread.Sleep(500);
                }
            }
        }

        private static JObject AppTokenRequest()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(_ADDR_FREEBOX + "/api/v2/login/authorize/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var o = new JObject
                    {
                        {"app_id", Settings.Default.AppId},
                        {"app_name", Settings.Default.AppName},
                        {"app_version", Settings.Default.AppVersion},
                        {"device_name", Environment.MachineName}
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
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(_ADDR_FREEBOX + "/api/v2/login/authorize/" + trackid);
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
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(_ADDR_FREEBOX + "/api/v2/login/");
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
            ApiCall(_ADDR_FREEBOX + "/api/v2/login/logout/", "", "application/json", sessionid, "POST");
        }

        private static JObject SessionRequest(string appid, string apptoken, string challenge)
        {
            string password = Encode(challenge, apptoken);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(_ADDR_FREEBOX + "/api/v2/login/session/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var o = new JObject
                    {
                        {"password", password},
                        {"app_id", appid}
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