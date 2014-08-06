// Créer par : tkahn
// Le : 24-06-2014

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using BezyFB.Helpers;
using BezyFB.Properties;
using Newtonsoft.Json.Linq;

namespace BezyFB.Freebox
{
    public class Freebox
    {
        private String SessionToken { get; set; }

        public bool ConnectNewFreebox()
        {
            Settings.Default.IpFreebox = "mafreebox.freebox.fr";

            if (!TesterConnexionFreebox())
                return false;
            JsonObject.Parse("");
            if (!GenererAppToken())
                return false;

            if (!GenererSessionToken())
                return false;

            var json = ApiConnector.Call("http://mafreebox.freebox.fr/api/v2/connection/config/", WebMethod.Get, "application/json", null, null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var j = JObject.Parse(json);

            Settings.Default.IpFreebox = (string)j["result"]["remote_access_ip"] + ":" + (string)j["result"]["remote_access_port"];

            return true;
        }

        private bool TesterConnexionFreebox()
        {
            Socket s = new Socket(AddressFamily.InterNetwork,
                                  SocketType.Stream,
                                  ProtocolType.Tcp);

            try
            {
                s.Connect(Settings.Default.IpFreebox, 80);
            }
            catch (Exception ex)
            {
                if (Settings.Default.AffichageErreurMessageBox)
                    MessageBox.Show(ex.Message);
                else
                    Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        private bool GenererSessionToken()
        {
            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v2/login/", WebMethod.Get);
            var challenge = (string)JObject.Parse(json)["result"]["challenge"];

            var password = Helper.Encode(challenge, Settings.Default.TokenFreebox);

            json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v2/login/session/", WebMethod.Post, "application/json",
                                     new JObject { { "password", password }, { "app_id", Settings.Default.AppId } }.ToString());
            var session = JObject.Parse(json);

            if (session == null)
                return false;

            SessionToken = (string)session["result"]["session_token"];

            return true;
        }

        private bool GenererAppToken()
        {
            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v2/login/authorize/", WebMethod.Post, "application/json",
                                         new JObject
                                             {
                                                 {"app_id", Settings.Default.AppId}, {"app_name", Settings.Default.AppName},
                                                 {"app_version", Settings.Default.AppVersion}, {"device_name", Environment.MachineName}
                                             }.ToString());

            var apptokenrequest = JObject.Parse(json);

            var appToken = (string)apptokenrequest["result"]["app_token"];
            var trackId = (string)apptokenrequest["result"]["track_id"];
            String result;
            do
            {
                json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v2/login/authorize/" + trackId, WebMethod.Get);
                var apptokenstatus = JObject.Parse(json);
                result = (string)apptokenstatus["result"]["status"];
                Thread.Sleep(500);
            } while (result == "pending");

            if (result != "granted")
                return false;

            Settings.Default.TokenFreebox = appToken;
            return true;
        }

        public string Deconnexion()
        {
            try
            {
                if (String.IsNullOrEmpty(SessionToken))
                    GenererSessionToken();

                var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v2/login/logout/", WebMethod.Post, null, null,
                                             null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

                return JObject.Parse(json).ToString();
            }
            catch (Exception ex)
            {
                if (Settings.Default.AffichageErreurMessageBox)
                    MessageBox.Show(ex.Message);
                else
                    Console.WriteLine(ex.Message);
                return null;
            }
        }

        public string CreerDossier(string directory, string parent)
        {
            if (String.IsNullOrEmpty(SessionToken))
                GenererSessionToken();

            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v2/fs/mkdir/", WebMethod.Post,
                                         "application/x-www-form-urlencoded", new JObject { { "parent", Helper.EncodeTo64(parent) }, { "dirname", directory } }.ToString(),
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            return JObject.Parse(json).ToString();
        }

        public List<string> Ls(string directory)
        {
            if (String.IsNullOrEmpty(SessionToken))
                GenererSessionToken();

            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v2/fs/ls/" + Helper.EncodeTo64(directory),
                                         WebMethod.Get, "application/x-www-form-urlencoded", null, null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            var jsonObject = JObject.Parse(json);

            if (!(bool)jsonObject["success"])
            {
                if (Settings.Default.AffichageErreurMessageBox)
                    MessageBox.Show((string)jsonObject["msg"]);
                else
                    Console.WriteLine((string)jsonObject["msg"]);
                return null;
            }
            var result = jsonObject["result"];

            return result.Select(t => t["name"].ToString()).ToList();
        }

        public string Download(String magnetUrl, string directory)
        {
            if (String.IsNullOrEmpty(SessionToken))
                GenererSessionToken();

            var pathDir = Settings.Default.PathVideo;

            foreach (var s in directory.Split('\\', '/').Where(s => !String.IsNullOrEmpty(s)))
            {
                CreerDossier(s, pathDir);
                pathDir += "/" + s;
            }

            var path = System.Web.HttpUtility.UrlEncode(magnetUrl);
            var content = "download_url=" + path + "\r\n&download_dir=" + Helper.EncodeTo64(pathDir);
            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v2/downloads/add/", WebMethod.Post,
                                         "application/x-www-form-urlencoded", content,
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var jsonObject = JObject.Parse(json);
            if (!(bool)jsonObject["success"])
            {
                if (Settings.Default.AffichageErreurMessageBox)
                    MessageBox.Show((string)jsonObject["msg"]);
                else
                    Console.WriteLine((string)jsonObject["msg"]);
                return null;
            }

            return ((int)jsonObject["result"]["id"]).ToString();
        }

        public string UploadFile(string inputFile, string outputDir, string outputFileName)
        {
            if (String.IsNullOrEmpty(SessionToken))
                GenererSessionToken();

            var pathDir = Settings.Default.PathVideo;

            foreach (var s in outputDir.Split('\\', '/').Where(s => !String.IsNullOrEmpty(s)))
            {
                CreerDossier(s, pathDir);
                pathDir += "/" + s;
            }

            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v1/upload/", WebMethod.Post, "application/json",
                                         new JObject { { "dirname", Helper.EncodeTo64(pathDir) }, { "upload_name", outputFileName } }.ToString(), null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            try
            {
                var id = (string)JObject.Parse(json)["result"]["id"];

                string text = File.ReadAllText(inputFile);

                const string boundary = "----WebKitFormBoundary0Qvwx7fycAF2CWmh";

                json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v1/upload/" + id + "/send", WebMethod.Post, "multipart/form-data; boundary=" + boundary,
                                         "--" + boundary + Environment.NewLine +
                                         "Content-Disposition: form-data; name=\"" + outputFileName + "\"; filename=\"" + outputFileName + "\"" + Environment.NewLine +
                                         "Content-Type: text/plain" + Environment.NewLine + Environment.NewLine +
                                         text + Environment.NewLine +
                                         "--" + boundary + "--",
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            }
            catch (Exception ex)
            {
                if (Settings.Default.AffichageErreurMessageBox)
                    MessageBox.Show(ex.Message);
                else
                    Console.WriteLine(ex.Message);
            }

            return JObject.Parse(json).ToString();
        }

        public string GetFileNameDownloaded(string idDownload)
        {
            if (String.IsNullOrEmpty(SessionToken))
                GenererSessionToken();

            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v2/downloads/" + idDownload, WebMethod.Get,
                                         "application/x-www-form-urlencoded", "",
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var jsonObject = JObject.Parse(json);
            if (!(bool)jsonObject["success"])
            {
                if (Settings.Default.AffichageErreurMessageBox)
                    MessageBox.Show((string)jsonObject["msg"]);
                else
                    Console.WriteLine((string)jsonObject["msg"]);
                return null;
            }
            return ((string)jsonObject["result"]["name"]);
        }
    }
}