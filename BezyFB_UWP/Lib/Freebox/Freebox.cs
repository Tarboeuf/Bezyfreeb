// Créer par : tkahn
// Le : 24-06-2014

using BezyFB_UWP.Lib.BetaSerie;
using BezyFB_UWP.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Storage;

namespace BezyFB_UWP.Lib.Freebox
{
    public class Freebox
    {
        private Settings current;

        public Freebox(Settings current)
        {
            this.current = current;
            IpFreebox = current.FreeboxIp;
            AppId = Settings.AppId;
            AppName = Settings.AppName;
            AppVersion = Settings.AppVersion;
            PathVideo = current.PathVideo;
            MachineName = GetMachineName();
        }

        private string GetMachineName()
        {
            var hostNames = NetworkInformation.GetHostNames();
            var hostName = hostNames.FirstOrDefault(name => name.Type == HostNameType.DomainName)?.DisplayName ?? "???";
            return hostName;
        }

        private string SessionToken { get; set; }
        public string IpFreebox { get; set; }
        public string TokenFreebox
        {
            get { return ApplicationData.Current.LocalSettings.Values["TokenFreebox"] as string; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["TokenFreebox"] = value;
            }
        }
        public string AppId { get; private set; }
        public string AppName { get; private set; }
        public string AppVersion { get; private set; }
        public string PathVideo { get; private set; }
        public string MachineName { get; private set; }

        public async Task<bool> ConnectNewFreebox()
        {
            IpFreebox = "mafreebox.freebox.fr";

            if (!TesterConnexionFreebox())
                return false;

            if (!await GenererAppToken())
                return false;

            if (!await GenererSessionToken())
                return false;

            var json = await ApiConnector.Call("http://mafreebox.freebox.fr/api/v3/connection/config/", WebMethod.Get, "application/json", null, null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var j = JsonObject.Parse(json);

            IpFreebox = j["result"].GetObject()["remote_access_ip"].GetString() + ":" + j["result"].GetObject()["remote_access_port"];

            return true;
        }

        private bool TesterConnexionFreebox()
        {
            //Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
            //try
            //{
            //    //IpFreebox, 80
            //    var eventargs = new SocketAsyncEventArgs();
            //    eventargs.RemoteEndPoint = new EndPoint();
            //    return s.ConnectAsync();
            //}
            //catch (Exception ex)
            //{
            //    Helper.AfficherMessage(IpFreebox + " (TesterConnexionFreebox) : " + ex.Message);
            //    return false;
            //}
        }

        private async Task<bool> GenererSessionToken()
        {
            if (IpFreebox.StartsWith("http:"))
                return false;

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/login/", WebMethod.Get);
            if (json == null)
                return false;
            var challenge = JsonObject.Parse(json)["result"].GetObject()["challenge"].GetString();

            var password = Helper.Encode(challenge, TokenFreebox);

            json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/login/session/", WebMethod.Post, "application/json",
                                     new JsonObject() { { "password", JsonValue.CreateStringValue(password) }, { "app_id", JsonValue.CreateStringValue(AppId) } }.ToString());
            var session = JsonObject.Parse(json);

            if (session == null)
                return false;

            SessionToken = session["result"].GetObject()["session_token"].GetString();

            return true;
        }

        private async Task<bool> GenererAppToken()
        {
            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/login/authorize/", WebMethod.Post, "application/json",
                                         new JsonObject
                                             {
                                                 {"app_id", JsonValue.CreateStringValue(AppId)}, {"app_name", JsonValue.CreateStringValue(AppName)},
                                                 {"app_version", JsonValue.CreateStringValue(AppVersion)}, {"device_name", JsonValue.CreateStringValue(MachineName)}
                                             }.ToString());

            var apptokenrequest = JsonObject.Parse(json);

            var appToken = apptokenrequest["result"].GetObject()["app_token"].GetString();
            var trackId = apptokenrequest["result"].GetObject()["track_id"].GetNumber();
            String result;
            do
            {
                json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/login/authorize/" + trackId, WebMethod.Get);
                var apptokenstatus = JsonObject.Parse(json);
                result = apptokenstatus["result"].GetObject()["status"].GetString();
                await Task.Delay(TimeSpan.FromSeconds(0.5));
            } while (result == "pending");

            if (result != "granted")
                return false;

            TokenFreebox = appToken;
            return true;
        }

        public async Task<string> Deconnexion()
        {
            try
            {
                if (String.IsNullOrEmpty(SessionToken))
                    return null;

                var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/login/logout/", WebMethod.Post, null, null,
                                             null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

                return JsonObject.Parse(json).ToString();
            }
            catch (Exception ex)
            {
                Helper.AfficherMessage(IpFreebox + " (Deconnexion) : " + ex.Message);
                return null;
            }
        }

        public async Task<string> CreerDossier(string directory, string parent)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/fs/mkdir/", WebMethod.Post,
                                         "application/x-www-form-urlencoded", new JsonObject
                                         {
                                             { "parent", JsonValue.CreateStringValue(Helper.EncodeTo64(parent)) },
                                             { "dirname", JsonValue.CreateStringValue(directory)}
                                         }.ToString(),
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            return JsonObject.Parse(json).ToString();
        }

        public async Task<List<string>> Ls(string directory, bool onlyFolder)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/fs/ls/" + Helper.EncodeTo64(directory) + "?onlyFolder=" + (onlyFolder ? 1 : 0),
                                         WebMethod.Get, "application/x-www-form-urlencoded", null, null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            if (json == null)
                return new List<string>();

            var jsonObject = JsonObject.Parse(json);

            if (!jsonObject["success"].GetBoolean())
            {
                Helper.AfficherMessage(IpFreebox + " (Ls " + directory + ") : " + jsonObject["msg"].GetString());
                return null;
            }
            var result = jsonObject["result"];

            return result.GetArray().Select(t => t.GetObject()["name"].GetString()).ToList();
        }

        public async Task<string> Download(string magnetUrl, string directory)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var pathDir = PathVideo;

            foreach (var s in directory.Split('\\', '/').Where(s => !String.IsNullOrEmpty(s)))
            {
                await CreerDossier(s, pathDir);
                pathDir += "/" + s;
            }

            var path = WebUtility.UrlEncode(magnetUrl);
            var content = "download_url=" + path + "\r\n&download_dir=" + Helper.EncodeTo64(pathDir);
            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/downloads/add/", WebMethod.Post,
                                         "application/x-www-form-urlencoded", content,
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var jsonObject = JsonObject.Parse(json);
            if (!jsonObject["success"].GetBoolean())
            {
                Helper.AfficherMessage(IpFreebox + " (Download magnet ) : " + jsonObject["msg"].GetString());
                return null;
            }

            return ((int)jsonObject["result"].GetObject()["id"].GetNumber()).ToString();
        }

        public async Task<string> DownloadFile(FileInfo torrentURL, string directory, bool isRelativeDir)
        {
            return await DownloadFile(File.ReadAllBytes(torrentURL.FullName), torrentURL.Name, directory, isRelativeDir);
        }

        public async Task<string> DownloadFile(string urlFichier, string directory, bool isRelativeDir)
        {
            string lien = urlFichier;
            Uri uri = new Uri(lien);
            string nomFichier = uri.Segments[uri.Segments.Length - 1];
            WebRequest request = HttpWebRequest.Create(lien);

            request.ContentType = "application/x-bittorrent";

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    if (null != stream)
                    {
                        return await DownloadFile(ReadFully(stream), nomFichier, directory, isRelativeDir);
                    }
                }
            }
            return null;
        }

        public async Task<string> DownloadFile(Stream filchier, string nomFichier, string directory, bool isRelativeDir)
        {
            return await DownloadFile(ReadFully(filchier), nomFichier, directory, isRelativeDir);
        }

        public async Task<string> DownloadFile(byte[] fichier, string nomFichier, string directory, bool isRelativeDir)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            string pathDir;
            if (isRelativeDir)
            {
                pathDir = PathVideo;
                foreach (var s in directory.Split('\\', '/').Where(s => !String.IsNullOrEmpty(s)))
                {
                    await CreerDossier(s, pathDir);
                    pathDir += "/" + s;
                }
            }
            else
            {
                pathDir = directory;
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("download_dir", Helper.EncodeTo64(pathDir));
            parameters.Add("archive_password", "");
            parameters.Add("download_file", new FormUpload.FileParameter(fichier, nomFichier, "application/x-bittorrent"));

            string strReponse = "";
            var formDataPost = await FormUpload.MultipartFormDataPost("http://" + IpFreebox + "/api/v1/downloads/add"
                , "Me"
                , parameters
                , new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var response = formDataPost.GetResponseStream();
            using (var streamReader = new StreamReader(response))
            {
                strReponse = streamReader.ReadToEnd();
            }

            var jsonObject = JsonObject.Parse(strReponse);
            if (!jsonObject["success"].GetBoolean())
            {
                Helper.AfficherMessage(IpFreebox + " (DownloadFile " + nomFichier + " ) : " + jsonObject["msg"].GetString());
                return null;
            }

            return ((int)jsonObject["result"].GetObject()["id"].GetNumber()).ToString();
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public async Task<string> UploadFile(string inputFile, string outputDir, string outputFileName)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var pathDir = PathVideo;

            foreach (var s in outputDir.Split('\\', '/').Where(s => !String.IsNullOrEmpty(s)))
            {
                await CreerDossier(s, pathDir);
                pathDir += "/" + s;
            }

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/upload/", WebMethod.Post, "application/json",
                                         new JsonObject
                                         {
                                             { "dirname", JsonValue.CreateStringValue(Helper.EncodeTo64(pathDir)) },
                                             { "upload_name", JsonValue.CreateStringValue(outputFileName) }
                                         }.ToString(), null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            if (json == null)
            {
                Helper.AfficherMessage(IpFreebox + " (UploadFile " + inputFile + " ) : Erreur lors de l'appel à la freebox ");
                return null;
            }

            try
            {
                var jobj = JsonObject.Parse(json);
                if (!jobj["success"].GetBoolean() && jobj["error_code"].GetString() == "conflict")
                {
                    await DeleteFile(pathDir + "/" + outputFileName);
                    json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/upload/", WebMethod.Post, "application/json",
                                         new JsonObject
                                         {
                                             { "dirname",JsonValue.CreateStringValue( Helper.EncodeTo64(pathDir)) },
                                             { "upload_name", JsonValue.CreateStringValue(outputFileName) }
                                         }.ToString(), null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
                    jobj = JsonObject.Parse(json);
                }

                if (!jobj["success"].GetBoolean())
                {
                    Helper.AfficherMessage(IpFreebox + " (UploadFile " + inputFile + " ) : " + jobj["msg"].ToString());
                    return null;
                }
                var id = jobj["result"].GetObject()["id"].GetNumber();

                string text = File.ReadAllText(inputFile, Encoding.UTF8);

                const string boundary = "----WebKitFormBoundary0Qvwx7fycAF2CWmh";

                json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/upload/" + id + "/send", WebMethod.Post, "multipart/form-data; boundary=" + boundary,
                                         "--" + boundary + Environment.NewLine +
                                         "Content-Disposition: form-data; name=\"" + outputFileName + "\"; filename=\"" + outputFileName + "\"" + Environment.NewLine +
                                         "Content-Type: text/plain" + Environment.NewLine + Environment.NewLine + text + Environment.NewLine +
                                         "--" + boundary + "--",
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            }
            catch (Exception ex)
            {
                Helper.AfficherMessage(IpFreebox + " (UploadFile " + inputFile + " ) : " + ex.Message);
                return null;
            }

            return JsonObject.Parse(json).ToString();
        }

        public async Task<string> DeleteFile(string filePath)
        {
            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/fs/rm/", WebMethod.Post,
                                            "application/json",
                                             new JsonObject
                                                 {
                                                      {"files", new JsonArray() { JsonValue.CreateStringValue(Helper.EncodeTo64(filePath)) } }
                                                }.ToString(),
                                            headers: new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) }
                                            );
            return JsonObject.Parse(json).ToString();
        }

        public async Task<string> CleanUpload()
        {
            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/upload/clean", WebMethod.DELETE,
                                           headers: new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            return JsonObject.Parse(json).ToString();
        }

        public async Task<string> GetFileNameDownloaded(string idDownload)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/downloads/" + idDownload, WebMethod.Get,
                                         "application/x-www-form-urlencoded", "",
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var jsonObject = JsonObject.Parse(json);
            if (!jsonObject["success"].GetBoolean())
            {
                Helper.AfficherMessage(IpFreebox + " (GetFileNameDownloaded " + idDownload + " ) : " + jsonObject["msg"].GetString());
                return null;
            }
            return (jsonObject["result"].GetObject()["name"].GetString());
        }

        public async Task<UserFreebox> GetInfosFreebox()
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/storage/disk/", WebMethod.Get,
                                         "application/x-www-form-urlencoded", null, null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            if (json == null)
            {
                Helper.AfficherMessage(IpFreebox + " (GetInfosFreebox ) : la requête n'a pas retournée de résultat");
                return null;
            }
            var jsonObject = JsonObject.Parse(json);

            if (!jsonObject["success"].GetBoolean())
            {
                Helper.AfficherMessage(IpFreebox + " (GetInfosFreebox ) : " + jsonObject["msg"].GetString());
                return null;
            }
            var userFreebox = new UserFreebox(this);
            userFreebox.FreeSpace = (long)jsonObject["result"].GetArray()[0].GetObject()["partitions"].GetArray()[0].GetObject()["free_bytes"].GetNumber();
            userFreebox.Ratio = 100.0 - userFreebox.FreeSpace / jsonObject["result"].GetArray()[0].GetObject()["total_bytes"].GetNumber() * 100;

            json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/downloads/", WebMethod.Get,
                                         "application/x-www-form-urlencoded", null, null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            jsonObject = JsonObject.Parse(json);

            if (!jsonObject["success"].GetBoolean())
            {
                Helper.AfficherMessage(IpFreebox + " (GetInfosFreebox) : " + jsonObject["msg"].GetString());
                return null;
            }

            try
            {
                foreach (var item in jsonObject["result"].GetArray())
                {
                    var obj = item.GetObject();
                    var di = new DownloadItem
                    {
                        Pourcentage = obj["rx_pct"].GetNumber(),
                        Name = obj["name"].GetString(),
                        Status = obj["status"].GetString(),
                        RxPourcentage = obj["tx_pct"].GetNumber(),
                    };
                    userFreebox.Downloads.Add(di);
                }
            }
            catch (Exception ex)
            {
            }

            return userFreebox;
        }
    }

    public class UserFreebox
    {
        private readonly Freebox _fb;

        public UserFreebox(Freebox fb)
        {
            _fb = fb;
            Downloads = new List<DownloadItem>();
            Movies = new ObservableCollection<OMDb>();
        }

        public long FreeSpace { get; set; }
        public double Ratio { get; set; }
        public List<DownloadItem> Downloads { get; set; }

        public ObservableCollection<OMDb> Movies { get; set; }
        public string PathFilm { get; private set; }

        public async void LoadMovies()
        {
            foreach (var item in await _fb.Ls(PathFilm, false))
            {
                var nom = await GuessIt.GuessNom(item);
                var omdb = await OMDb.GetNote(nom, item);
                Movies.Add(omdb);
            }
        }
    }

    public class DownloadItem
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public double Pourcentage { get; set; }
        public double RxPourcentage { get; set; }
    }
}