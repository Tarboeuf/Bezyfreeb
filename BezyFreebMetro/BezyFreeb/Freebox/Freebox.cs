// Créer par : tkahn
// Le : 24-06-2014

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Windows.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using BezyFB.Helpers;
using Windows.Data.Json;
using BezyFreebMetro;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;

namespace BezyFB.Freebox
{
    public class Freebox
    {
        private String SessionToken { get; set; }

        public async Task<bool> ConnectNewFreebox()
        {
            AppSettings.Default.IpFreebox = "mafreebox.freebox.fr";

            if (!TesterConnexionFreebox())
                return false;

            if (string.IsNullOrEmpty(AppSettings.Default.TokenFreebox))
                if (!await GenererAppToken())
                    return false;

            if (!await GenererSessionToken())
                return false;

            var json = await ApiConnector.Call("http://mafreebox.freebox.fr/api/v2/connection/config/", WebMethod.Get, "application/json", null, null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var j = JsonObject.Parse(json).GetObject();

            AppSettings.Default.IpFreebox = j["result"].GetObject()["remote_access_ip"].GetString() + ":" + j["result"].GetObject()["remote_access_port"].GetNumber();

            return true;
        }

        private bool TesterConnexionFreebox()
        {
            //Socket s = new Socket(AddressFamily.InterNetwork,
            //                      SocketType.Stream,
            //                      ProtocolType.Tcp);

            //try
            //{
            //    s.Connect(Settings.Default.IpFreebox, 80);
            //}
            //catch (Exception ex)
            //{
            //    if (Settings.Default.AffichageErreurMessageBox)
            //        MessageBox.Show(ex.Message);
            //    else
            //        Console.WriteLine(ex.Message);
            //    return false;
            //}
            return true;
        }

        private async Task<bool> GenererSessionToken()
        {
            var json = await ApiConnector.Call("http://" + AppSettings.Default.IpFreebox + "/api/v2/login/", WebMethod.Get);
            var challenge = JsonObject.Parse(json).GetObject()["result"].GetObject()["challenge"].GetString();

            string password = Helper.Sha1Encrypt(challenge, AppSettings.Default.TokenFreebox);

            json = await ApiConnector.Call("http://" + AppSettings.Default.IpFreebox + "/api/v2/login/session/", WebMethod.Post, "application/json",
                                     new JsonObject { { "password", JsonValue.CreateStringValue(password) }, { "app_id", JsonValue.CreateStringValue(AppSettings.Default.AppId) } }.Stringify());
            var session = JsonObject.Parse(json);

            if (session == null)
                return false;

            SessionToken = (string)session["result"].GetObject()["session_token"].GetString();

            return true;
        }

        private async Task<bool> GenererAppToken()
        {
            var hostNames = NetworkInformation.GetHostNames();

            var localName = hostNames.FirstOrDefault(name => name.DisplayName.Contains(".local"));

            var computerName = localName.DisplayName.Replace(".local", "");
            var newjson = new JsonObject
                                             {
                                                 {"app_id", JsonValue.CreateStringValue(AppSettings.Default.AppId)}, {"app_name", JsonValue.CreateStringValue(AppSettings.Default.AppName)},
                                                 {"app_version", JsonValue.CreateStringValue(AppSettings.Default.AppVersion)}, {"device_name", JsonValue.CreateStringValue(computerName)}
                                             };

            var json = await ApiConnector.Call("http://" + AppSettings.Default.IpFreebox + "/api/v2/login/authorize/", WebMethod.Post, "application/json", newjson.Stringify());

            var apptokenrequest = JsonObject.Parse(json).GetObject();

            var appToken = apptokenrequest["result"].GetObject()["app_token"].GetString();
            var trackId = apptokenrequest["result"].GetObject()["track_id"].GetNumber();
            String result;
            do
            {
                json = await ApiConnector.Call("http://" + AppSettings.Default.IpFreebox + "/api/v2/login/authorize/" + trackId, WebMethod.Get);
                var apptokenstatus = JsonObject.Parse(json).GetObject();
                result = (string)apptokenstatus["result"].GetObject()["status"].GetString();
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            } while (result == "pending");

            if (result != "granted")
                return false;

            AppSettings.Default.TokenFreebox = appToken;
            return true;
        }

        public async Task<string> Deconnexion()
        {
            string message = null;
            try
            {
                if (String.IsNullOrEmpty(SessionToken))
                    await GenererSessionToken();

                var json = await ApiConnector.Call("http://" + AppSettings.Default.IpFreebox + "/api/v2/login/logout/", WebMethod.Post, null, null,
                                             null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

                return JsonObject.Parse(json).ToString();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (null != message)
            {
                MessageDialog md = new MessageDialog(message);
                await md.ShowAsync();
            }
            return null;
        }

        public async Task CreerDossier(string directory, string parent)
        {
            if (string.IsNullOrEmpty(parent))
                return;

            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + AppSettings.Default.IpFreebox + "/api/v2/fs/mkdir/", WebMethod.Post,
                                         "application/x-www-form-urlencoded", new JsonObject { { "parent", JsonValue.CreateStringValue(Helper.EncodeTo64(parent)) }, { "dirname", JsonValue.CreateStringValue(directory) } }.Stringify(),
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

        }

        public async Task<List<string>> Ls(string directory)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + AppSettings.Default.IpFreebox + "/api/v2/fs/ls/" + Helper.EncodeTo64(directory),
                                         WebMethod.Get, "application/x-www-form-urlencoded", null, null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            var jsonObject = JsonObject.Parse(json).GetObject();

            if (!(bool)jsonObject["success"].GetBoolean())
            {
                MessageDialog md = new MessageDialog((string)jsonObject["msg"].GetString());
                await md.ShowAsync();
                return new List<string>();
            }
            var result = jsonObject["result"].GetArray();

            return result.Select(t => t.GetObject()["name"].GetString()).ToList();
        }

        public async Task<string> Download(String magnetUrl, string directory)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var pathDir = AppSettings.Default.PathVideo;

            foreach (var s in directory.Split('\\', '/').Where(s => !String.IsNullOrEmpty(s)))
            {
                await CreerDossier(s, pathDir);
                pathDir += "/" + s;
            }

            var path = Uri.EscapeUriString(magnetUrl);
            var content = "download_url=" + path + "\r\n&download_dir=" + Helper.EncodeTo64(pathDir);
            var json = await ApiConnector.Call("http://" + AppSettings.Default.IpFreebox + "/api/v2/downloads/add/", WebMethod.Post,
                                         "application/x-www-form-urlencoded", content,
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var jsonObject = JsonObject.Parse(json).GetObject();
            if (!(bool)jsonObject["success"].GetBoolean())
            {
                MessageDialog md = new MessageDialog((string)jsonObject["msg"].GetString());
                await md.ShowAsync();
                return null;
            }

            return ((int)jsonObject["result"].GetObject()["id"].GetNumber()).ToString();
        }

        public async Task<string> UploadFile(string inputFile, string outputDir, string outputFileName, string fileContent)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var pathDir = AppSettings.Default.PathVideo;

            foreach (var s in outputDir.Split('\\', '/').Where(s => !String.IsNullOrEmpty(s)))
            {
                await CreerDossier(s, pathDir);
                pathDir += "/" + s;
            }

            var json = await ApiConnector.Call("http://" + AppSettings.Default.IpFreebox + "/api/v1/upload/", WebMethod.Post, "application/json",
                                         new JsonObject { { "dirname", JsonValue.CreateStringValue(Helper.EncodeTo64(pathDir)) }, { "upload_name", JsonValue.CreateStringValue(outputFileName) } }.Stringify(), null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            string message = null;
            try
            {
                var id = JsonObject.Parse(json)["result"].GetObject()["id"].GetNumber();

                string text = fileContent;

                const string boundary = "----WebKitFormBoundary0Qvwx7fycAF2CWmh";

                json = await ApiConnector.Call("http://" + AppSettings.Default.IpFreebox + "/api/v1/upload/" + id + "/send", WebMethod.Post, "multipart/form-data; boundary=" + boundary,
                                         "--" + boundary + Environment.NewLine +
                                         "Content-Disposition: form-data; name=\"" + outputFileName + "\"; filename=\"" + outputFileName + "\"" + Environment.NewLine +
                                         "Content-Type: text/plain" + Environment.NewLine + Environment.NewLine +
                                         text + Environment.NewLine +
                                         "--" + boundary + "--",
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }


            if (null != message)
            {
                MessageDialog md = new MessageDialog(message);
                await md.ShowAsync();
                return null;
            }

            return JsonObject.Parse(json).ToString();
        }

        public async Task<string> GetFileNameDownloaded(string idDownload)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + AppSettings.Default.IpFreebox + "/api/v2/downloads/" + idDownload, WebMethod.Get,
                                         "application/x-www-form-urlencoded", "",
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var jsonObject = JsonObject.Parse(json).GetObject();
            if (!(bool)jsonObject["success"].GetBoolean())
            {
                MessageDialog md = new MessageDialog((string)jsonObject["msg"].GetString());
                await md.ShowAsync();
                return null;
            }
            return ((string)jsonObject["result"].GetObject()["name"].GetString());
        }
    }
}