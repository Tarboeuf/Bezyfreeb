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
using BezyFB.Properties;
using Newtonsoft.Json.Linq;

namespace BezyFB
{
    public static class Freebox
    {
        //private const string _ADDR_FREEBOX = "http://88.120.230.2/";

        private const string _ADDR_FREEBOX = "http://mafreebox.freebox.fr/";

        private const string _PATH_VIDEO = "Disque dur/Vidéos/";

        public static void Download(String magnetUrl, string directory)
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
            AjouterTorrent(session, magnetUrl, directory);
        }

        public static void AjouterTorrent(JObject session, string torrentPath, string directory)
        {
            if (session != null)
            {
                var sessionRequest = (string)session["result"]["session_token"];

                // List download
                /*string output = ApiConnector.Call(_ADDR_FREEBOX + "/api/v2/downloads/", WebMethod.Get, "application/json", null,
                    null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", sessionRequest) });
                string output = ApiCall(_ADDR_FREEBOX + "/api/v2/downloads/", "", "application/json", sessionRequest, "GET");
                JObject o = JObject.Parse(output);
                Console.WriteLine(o.ToString());*/

                // Download
                //const string content = "download_url=http%3A%2F%2Fmirror.ovh.net%2Fubuntu-releases%2F14.04%2Fubuntu-14.04-desktop-amd64.iso.torrent";
                try
                {
                    var path = System.Web.HttpUtility.UrlEncode(torrentPath);
                    string content = "download_url=" + path + "\r\n&download_dir=" + Helper.EncodeTo64(_PATH_VIDEO + directory, Encoding.UTF8);
                    string outputDown = ApiConnector.Call(_ADDR_FREEBOX + "api/v2/downloads/add/", WebMethod.Post, "application/x-www-form-urlencoded", content,
                                                          null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", sessionRequest) });
                    var obj = JObject.Parse(outputDown);
                    Console.WriteLine(obj.ToString());
                    string idDL = (string)obj["result"]["id"];
                    Console.WriteLine(idDL);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                //outputDown = ApiConnector.Call(_ADDR_FREEBOX + "api/v2/downloads/" + idDL, WebMethod.Post, null, null,
                //                               null, new List<Tuple<string, string>> {new Tuple<string, string>("X-Fbx-App-Auth", sessionRequest)});
                //obj = JObject.Parse(outputDown);
                //string nomFichier = (string) obj["result"]["name"];

                // Logout
                ApiConnector.Call(_ADDR_FREEBOX + "api/v2/login/logout/", WebMethod.Post, null, null,
                                  null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", sessionRequest) });

                //return nomFichier;
            }
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
            var o = new JObject
                {
                    {"app_id", Settings.Default.AppId},
                    {"app_name", Settings.Default.AppName},
                    {"app_version", Settings.Default.AppVersion},
                    {"device_name", Environment.MachineName}
                };

            var json = ApiConnector.Call(_ADDR_FREEBOX + "api/v2/login/authorize/", WebMethod.Post, "application/json", o.ToString());
            return JObject.Parse(json);
        }

        private static JObject AppTokenStatus(string trackid)
        {
            var json = ApiConnector.Call(_ADDR_FREEBOX + "api/v2/login/authorize/" + trackid, WebMethod.Get);
            return JObject.Parse(json);
        }

        private static JObject ChallengeRequest()
        {
            var json = ApiConnector.Call(_ADDR_FREEBOX + "api/v2/login/", WebMethod.Get);
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

            var json = ApiConnector.Call(_ADDR_FREEBOX + "api/v2/login/session/", WebMethod.Post, "application/json", o.ToString());
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