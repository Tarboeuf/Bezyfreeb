// Créer par : tkahn
// Le : 24-06-2014

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommonPortableLib;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace FreeboxPortableLib
{
    public class Freebox
    {
        private readonly ISettingsFreebox _current;
        public IApiConnectorService ApiConnector { get; set; }
        public ICryptographic Crypto { get; set; }
        public IMessageDialogService MessageDialogService { get; set; }
        public IFormUploadService FormUploadService { get; set; }

        public Freebox(ISettingsFreebox current)
        {
            this._current = current;
            IpFreebox = current.FreeboxIp;
            AppId = current.AppId;
            AppName = current.AppName;
            AppVersion = current.AppVersion;
            PathVideo = current.PathVideo;
            MachineName = current.Hostname;
        }

        private string SessionToken { get; set; }
        public string IpFreebox { get; set; }
        public string AppId { get; private set; }
        public string AppName { get; private set; }
        public string AppVersion { get; private set; }
        public string PathVideo { get; private set; }
        public string MachineName { get; private set; }

        public async Task<bool> ConnectNewFreebox()
        {
            IpFreebox = "mafreebox.freebox.fr";

            if (!await TesterConnexionFreebox())
                return false;

            if (!await GenererAppToken())
                return false;

            if (!await GenererSessionToken())
                return false;

            var json = await ApiConnector.Call("http://mafreebox.freebox.fr/api/v3/connection/config/", WebMethod.Get, "application/json", null, null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var j = JObject.Parse(json);

            IpFreebox = j["result"]["remote_access_ip"] + ":" + j["result"]["remote_access_port"];

            return true;
        }

        private async Task<bool> TesterConnexionFreebox()
        {
            var url = "http://google.com";
            var client = new HttpClient();
            var data = await client.GetAsync(url);

            return data.IsSuccessStatusCode;
        }

        private async Task<bool> GenererSessionToken()
        {
            if (IpFreebox.StartsWith("http:"))
                return false;

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/login/", WebMethod.Get);
            if (json == null)
                return false;
            var challenge = (string)JObject.Parse(json)["result"]["challenge"];

            var password = Crypto.Encode(challenge, _current.TokenFreebox);

            json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/login/session/", WebMethod.Post, "application/json",
                                     new JObject() { { "password", password }, { "app_id", AppId } }.ToString());
            var session = JObject.Parse(json);

            if (session == null)
                return false;

            SessionToken = session["result"]["session_token"].ToString();

            return true;
        }

        private async Task<bool> GenererAppToken()
        {
            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/login/authorize/", WebMethod.Post, "application/json",
                                         new JObject
                                             {
                                                 {"app_id", AppId}, {"app_name", AppName},
                                                 {"app_version", AppVersion}, {"device_name", MachineName}
                                             }.ToString());

            var apptokenrequest = JObject.Parse(json);

            var appToken = (string)apptokenrequest["result"]["app_token"];
            var trackId = (double)apptokenrequest["result"]["track_id"];
            String result;
            do
            {
                json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/login/authorize/" + trackId, WebMethod.Get);
                var apptokenstatus = JObject.Parse(json);
                result = (string)apptokenstatus["result"]["status"];
                await Task.Delay(TimeSpan.FromSeconds(0.5));
            } while (result == "pending");

            if (result != "granted")
                return false;

            _current.TokenFreebox = appToken;
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

                return JObject.Parse(json).ToString();
            }
            catch (Exception ex)
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (Deconnexion) : " + ex.Message);
                return null;
            }
        }

        public async Task<string> CreerDossier(string directory, string parent)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/fs/mkdir/", WebMethod.Post,
                                         "application/x-www-form-urlencoded", new JObject
                                         {
                                             { "parent", Crypto.EncodeTo64(parent) },
                                             { "dirname", directory}
                                         }.ToString(),
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            return JObject.Parse(json).ToString();
        }

        public async Task<List<string>> Ls(string directory, bool onlyFolder)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/fs/ls/" + Crypto.EncodeTo64(directory) + "?onlyFolder=" + (onlyFolder ? 1 : 0),
                                         WebMethod.Get, "application/x-www-form-urlencoded", null, null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            if (json == null)
                return new List<string>();

            var jobject = JObject.Parse(json);

            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (Ls " + directory + ") : " + (string)jobject["msg"]);
                return null;
            }
            var result = jobject["result"];

            return result.Select(t => (string)t["name"]).ToList();
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
            var content = "download_url=" + path + "\r\n&download_dir=" + Crypto.EncodeTo64(pathDir);
            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/downloads/add/", WebMethod.Post,
                                         "application/x-www-form-urlencoded", content,
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var jobject = JObject.Parse(json);
            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (Download magnet ) : " + (string)jobject["msg"]);
                return null;
            }

            return ((int)jobject["result"]["id"]).ToString();
        }

        //public async Task<string> DownloadFile(FileInfo torrentURL, string directory, bool isRelativeDir)
        //{
        //    return await DownloadFile(File.ReadAllBytes(torrentURL.FullName), torrentURL.Name, directory, isRelativeDir);
        //}

        public async Task<string> DownloadFile(string urlFichier, string directory, bool isRelativeDir)
        {
            string lien = urlFichier;
            Uri uri = new Uri(lien);
            string nomFichier = uri.Segments[uri.Segments.Length - 1];

            var content = await ApiConnector.GetResponse(lien, "application/x-bittorrent");
            if (null != content)
            {
                return await DownloadFile(content, nomFichier, directory, isRelativeDir);
            }
            return null;
        }

        public async Task<string> DownloadFile(Stream fichier, string nomFichier, string directory, bool isRelativeDir)
        {
            return await DownloadFile(ApiConnectorHelper.ReadFully(fichier), nomFichier, directory, isRelativeDir);
        }

        public async Task<string> DownloadFile(byte[] fichier, string nomFichier, string directory, bool isRelativeDir)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            string pathDir;
            if (isRelativeDir)
            {
                pathDir = PathVideo;
                foreach (var s in directory.Split('\\', '/').Where(s => !string.IsNullOrEmpty(s)))
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
            parameters.Add("download_dir", Crypto.EncodeTo64(pathDir));
            parameters.Add("archive_password", "");
            parameters.Add("download_file", new FileParameter(fichier, nomFichier, "application/x-bittorrent"));

            string strReponse = await FormUploadService.MultipartFormDataPost("http://" + IpFreebox + "/api/v1/downloads/add"
                , "Me"
                , parameters
                , new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });


            var jobject = JObject.Parse(strReponse);
            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (DownloadFile " + nomFichier + " ) : " + (string)jobject["msg"]);
                return null;
            }

            return ((int)jobject["result"]["id"]).ToString();
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
                                         new JObject
                                         {
                                             { "dirname", Crypto.EncodeTo64(pathDir) },
                                             { "upload_name", outputFileName }
                                         }.ToString(), null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            if (json == null)
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (UploadFile " + inputFile + " ) : Erreur lors de l'appel à la freebox ");
                return null;
            }

            try
            {
                var jobj = JObject.Parse(json);
                if (!(bool)jobj["success"] && (string)jobj["error_code"] == "conflict")
                {
                    await DeleteFile(pathDir + "/" + outputFileName);
                    json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/upload/", WebMethod.Post, "application/json",
                                         new JObject
                                         {
                                             { "dirname", Crypto.EncodeTo64(pathDir) },
                                             { "upload_name", outputFileName }
                                         }.ToString(), null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
                    jobj = JObject.Parse(json);
                }

                if (!(bool)jobj["success"])
                {
                    await MessageDialogService.AfficherMessage(IpFreebox + " (UploadFile " + inputFile + " ) : " + jobj["msg"].ToString());
                    return null;
                }
                var id = (int)jobj["result"]["id"];

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
                await MessageDialogService.AfficherMessage(IpFreebox + " (UploadFile " + inputFile + " ) : " + ex.Message);
                return null;
            }

            return JObject.Parse(json).ToString();
        }

        public async Task<string> DeleteFile(string filePath)
        {
            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/fs/rm/", WebMethod.Post,
                                            "application/json",
                                             new JObject
                                                 {
                                                      {"files", new JArray() { Crypto.EncodeTo64(filePath) } }
                                                }.ToString(),
                                            headers: new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) }
                                            );
            return JObject.Parse(json).ToString();
        }

        public async Task<string> CleanUpload()
        {
            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/upload/clean", WebMethod.DELETE,
                                           headers: new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            return JObject.Parse(json).ToString();
        }

        public async Task<string> GetFileNameDownloaded(string idDownload)
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/downloads/" + idDownload, WebMethod.Get,
                                         "application/x-www-form-urlencoded", "",
                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
            var jobject = JObject.Parse(json);
            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (GetFileNameDownloaded " + idDownload + " ) : " + (string)jobject["msg"]);
                return null;
            }
            return ((string)jobject["result"]["name"]);
        }

        public async Task<UserFreebox> GetInfosFreebox()
        {
            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/storage/disk/", WebMethod.Get,
                                         "application/x-www-form-urlencoded", null, null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            if (json == null)
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (GetInfosFreebox ) : la requête n'a pas retournée de résultat");
                return null;
            }
            var jobject = JObject.Parse(json);

            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (GetInfosFreebox ) : " + (string)jobject["msg"]);
                return null;
            }
            var userFreebox = new UserFreebox(this);
            userFreebox.FreeSpace = (long)jobject["result"][0]["partitions"][0]["free_bytes"];
            userFreebox.Ratio = 100.0 - userFreebox.FreeSpace / (double)jobject["result"][0]["total_bytes"] * 100;

            json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/downloads/", WebMethod.Get,
                                         "application/x-www-form-urlencoded", null, null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            jobject = JObject.Parse(json);

            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (GetInfosFreebox) : " + (string)jobject["msg"]);
                return null;
            }

            try
            {
                foreach (var item in jobject["result"])
                {
                    var obj = item;
                    var di = new DownloadItem
                    {
                        Pourcentage = (double)obj["rx_pct"],
                        Name = (string)obj["name"],
                        Status = (string)obj["status"],
                        RxPourcentage = (double)obj["tx_pct"],
                    };
                    userFreebox.Downloads.Add(di);
                }
            }
            catch (Exception)
            {
            }

            return userFreebox;
        }

        public async Task<List<FBFileInfo>> LsFileInfo(string directory)
        {
            if (directory.EndsWith(".")) return null;

            if (String.IsNullOrEmpty(SessionToken))
                await GenererSessionToken();

            var json = await ApiConnector.Call("http://" + _current.FreeboxIp + "/api/v3/fs/ls/" + Crypto.EncodeTo64(directory) + "?onlyFolder=0&countSubFolder=1",
                                         WebMethod.Get, "application/x-www-form-urlencoded", null, null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            if (json == null)
                return null;

            var jsonObject = JObject.Parse(json);

            if (!(bool)jsonObject["success"])
            {
                return null;
            }
            var result = jsonObject["result"];

            return result.Select(t => new FBFileInfo(t)).ToList();
        }
    }

    public class UserFreebox
    {
        private readonly Freebox _fb;

        public UserFreebox(Freebox fb)
        {
            _fb = fb;
            Downloads = new List<DownloadItem>();
            //Movies = new ObservableCollection<OMDb>();
        }

        public long FreeSpace { get; set; }
        public double Ratio { get; set; }
        public List<DownloadItem> Downloads { get; set; }

        //public ObservableCollection<OMDb> Movies { get; set; }
        public string PathFilm { get; private set; }

        //public async void LoadMovies()
        //{
        //    foreach (var item in await _fb.Ls(PathFilm, false))
        //    {
        //        var nom = await GuessIt.GuessNom(item);
        //        var omdb = await OMDb.GetNote(nom, item);
        //        Movies.Add(omdb);
        //    }
        //}
    }

    public class DownloadItem
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public double Pourcentage { get; set; }
        public double RxPourcentage { get; set; }
    }
}