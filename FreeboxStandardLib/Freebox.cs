// Créer par : tkahn
// Le : 24-06-2014

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommonStandardLib;
using Newtonsoft.Json.Linq;

namespace FreeboxStandardLib
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
            MachineName = current.Hostname;
        }

        private string SessionToken { get; set; }
        public string IpFreebox { get; set; }
        public string AppId { get; private set; }
        public string AppName { get; private set; }
        public string AppVersion { get; private set; }
        public string MachineName { get; private set; }

        private const string _DOSSIER_FINIT = "__finit";

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

            if (!string.IsNullOrEmpty(SessionToken))
            {
                // on vérifie que la connexion est toujours valable

                var jobject = await CallJson("/downloads/stats", WebMethod.Get);

                if ((bool)jobject["success"])
                    return true;
            }

            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v2/login/", WebMethod.Get);
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
            string result;
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
                if (string.IsNullOrEmpty(SessionToken))
                    return null;

                var json = await CallJson("/login/logout/");

                return json.ToString();
            }
            catch (Exception ex)
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (Deconnexion) : " + ex.Message);
                return null;
            }
        }

        public async Task<string> CreerDossier(string directory, string parent)
        {
            await GenererSessionToken();

            var json = await CallJson("/fs/mkdir/", WebMethod.Post,
                                         "application/x-www-form-urlencoded", new JObject
                                         {
                                             { "parent", Crypto.EncodeTo64(parent) },
                                             { "dirname", directory}
                                         }.ToString());

            return json.ToString();
        }

        public async Task<List<string>> Ls(string directory, bool onlyFolder, bool supprimerDossierSystem)
        {
            await GenererSessionToken();

            var jobject = await CallJson("/fs/ls/" + Crypto.EncodeTo64(directory) + "?onlyFolder=" + (onlyFolder ? 1 : 0),
                                         WebMethod.Get, "application/x-www-form-urlencoded");

            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (Ls " + directory + ") : " + (string)jobject["msg"]);
                return null;
            }
            var result = jobject["result"];

            var list = result.Select(t => (string)t["name"]);
            if (supprimerDossierSystem)
            {
                list = list.Where(f => f != "." && f != "..");
            }

            return list.ToList();
        }

        public async Task<string> Download(string magnetUrl, string directory, bool isRelativeDir)
        {
            await GenererSessionToken();

            string pathDir;
            if (isRelativeDir)
            {
                pathDir = _current.PathVideo;
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

            var path = WebUtility.UrlEncode(magnetUrl);
            var content = "download_url=" + path + "\r\n&download_dir=" + Crypto.EncodeTo64(pathDir);
            var jobject = await CallJson("/downloads/add/", WebMethod.Post,
                                         "application/x-www-form-urlencoded", content);

            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (Download magnet ) : " + (string)jobject["msg"]);
                return null;
            }

            return ((int)jobject["result"]["id"]).ToString();
        }

        public async Task<string> DownloadFile(string urlFichier, string directory, bool isRelativeDir)
        {
            string lien = urlFichier;
            Uri uri = new Uri(lien);
            string nomFichier = uri.Segments[uri.Segments.Length - 1];

            if (uri.IsFile)
            {
                FileInfo fi = new FileInfo(lien);
                using (Stream s = fi.OpenRead())
                {
                    return await DownloadFile(s, nomFichier, directory, isRelativeDir);
                }
            }

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
            await GenererSessionToken();

            string pathDir;
            if (isRelativeDir)
            {
                pathDir = _current.PathVideo;
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

        public async Task<JObject> UploadFile(string inputFile, string outputDir, string outputFileName)
        {
            await GenererSessionToken();

            var pathDir = _current.PathVideo;

            foreach (var s in outputDir.Split('\\', '/').Where(s => !string.IsNullOrEmpty(s)))
            {
                await CreerDossier(s, pathDir);
                pathDir += "/" + s;
            }

            var id = await GetIdUpload(pathDir, outputFileName);

            if (id == -1)
            {
                await
                    MessageDialogService.AfficherMessage(IpFreebox + " (UploadFile " + inputFile + " ) : Erreur lors de l'appel à la freebox ");
                return null;
            }
            try
            {
                string text = File.ReadAllText(inputFile, Encoding.UTF8);

                const string boundary = "----WebKitFormBoundary0Qvwx7fycAF2CWmh";

                var jobject = await CallJson("/upload/" + id + "/send", WebMethod.Post, "multipart/form-data; boundary=" + boundary,
                                         "--" + boundary + Environment.NewLine +
                                         "Content-Disposition: form-data; name=\"" + outputFileName + "\"; filename=\"" + outputFileName + "\"" + Environment.NewLine +
                                         "Content-Type: text/plain" + Environment.NewLine + Environment.NewLine + text + Environment.NewLine +
                                         "--" + boundary + "--");

                return jobject;
            }
            catch (Exception ex)
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (UploadFile " + inputFile + " ) : " + ex.Message);
                return null;
            }
        }

        private async Task<int> GetIdUpload(string pathDir, string outputFileName)
        {
            if (await IsFileExists(pathDir, outputFileName))
            {
                await DeleteFile(pathDir + "/" + outputFileName);
            }

            var jobj = await GetJsonIdUpload(pathDir, outputFileName);

            if (jobj != null)
            {
                if (!(bool)jobj["success"])
                {
                    return -1;
                }
                return (int)jobj["result"]["id"];
            }

            // tentative de récupération d'un upload en cours : 
            jobj = await CallJson("/upload/", WebMethod.Get);
            if (null != jobj)
            {
                if ((bool)jobj["success"])
                {
                    var token = jobj["result"].FirstOrDefault(j => (string)j["status"] == "authorized");
                    if (null != token)
                    {
                        return (int)token["id"];
                    }
                }
            }

            return -1;
        }

        private async Task<JObject> GetJsonIdUpload(string pathDir, string outputFileName)
        {
            try
            {
                var json = await CallJson("/upload/", WebMethod.Post, "application/json",
                    new JObject { { "dirname", Crypto.EncodeTo64(pathDir) }, { "upload_name", outputFileName } }.ToString());
                return json;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<bool> IsFileExists(string pathDir, string outputFileName)
        {
            try
            {
                var job = await CallJson("/fs/info/" + Crypto.EncodeTo64(pathDir + "/" + outputFileName), WebMethod.Get);
                if (null == job)
                {
                    return false;
                }
                return (bool)job["success"];
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task DeleteEmptyFolder()
        {
            await GenererSessionToken();

            await DeleteEmptyFolder("/Disque dur/Vidéos");
        }
        private async Task<int> DeleteEmptyFolder(string currentFolderPath)
        {
            var jobject = await CallJson("/fs/ls/" + Crypto.EncodeTo64(currentFolderPath) + "?onlyFolder=1&countSubFolder=1",
                                         WebMethod.Get, "application/x-www-form-urlencoded");

            int nbDeletedFolder = 0;
            bool subFolderDeleted = false;

            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (Ls Vidéos) : " + (string)jobject["msg"]);
                return 0;
            }

            var result = jobject["result"];

            foreach (var item in result)
            {
                int fileCount = (int)item["filecount"];
                int folderCount = (int)item["foldercount"];
                var name = (string)item["name"];

                if(name == "." || name == "..")
                {
                    continue;
                }

                if (fileCount == 0 && folderCount == 0)
                {
                    // suppression du dossier
                    await DeleteFile(currentFolderPath + "/" + name);
                    nbDeletedFolder++;
                }

                if (fileCount == 0 && folderCount != 0)
                {
                    // on parcourt le dossier enfant pour le vider si besoin
                    var nbSubfolderDeleted = await DeleteEmptyFolder(currentFolderPath + "/" + name);
                    nbDeletedFolder += nbSubfolderDeleted;
                    if(nbSubfolderDeleted > 0)
                    {
                        subFolderDeleted = true;
                    }
                }
            }

            if (!subFolderDeleted)
                return nbDeletedFolder;

            // on repasse si jamais des dossiers doivent maintenant être supprimé
            jobject = await CallJson("/fs/ls/" + Crypto.EncodeTo64(currentFolderPath) + "?onlyFolder=1&countSubFolder=1",
                                         WebMethod.Get, "application/x-www-form-urlencoded");



            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (Ls Vidéos) : " + (string)jobject["msg"]);
                return 0;
            }

            result = jobject["result"];

            foreach (var item in result)
            {
                int fileCount = (int)item["filecount"];
                int folderCount = (int)item["foldercount"];
                var name = (string)item["name"];

                if (fileCount == 0 && folderCount == 0)
                {
                    // suppression du dossier
                    await DeleteFile(currentFolderPath + "/" + name);
                    nbDeletedFolder++;
                }
            }
            return nbDeletedFolder;
        }

        private async Task<bool> Move(string[] files, string destination)
        {
            try
            {
                var job = await CallJson("/fs/mv/", WebMethod.Post, "application/json",
                    new JObject
                    {
                        {"files", new JArray(files.Select(f => Crypto.EncodeTo64(f)))},
                        {"dst", Crypto.EncodeTo64(destination)},
                        { "mode", "overwrite"}
                    }.ToString());

                if (null == job)
                {
                    return false;
                }
                return (bool)job["success"];
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> DeleteFile(string filePath)
        {
            var jobject = await CallJson("/fs/rm/", WebMethod.Post,
                                            "application/json",
                                             new JObject
                                                 {
                                                      {"files", new JArray() { Crypto.EncodeTo64(filePath) } }
                                                }.ToString());
            return jobject.ToString();
        }

        public async Task<string> CleanUpload()
        {
            var jobject = await CallJson("/upload/clean", WebMethod.DELETE);
            return jobject.ToString();
        }

        public async Task<string> GetFileNameDownloaded(string idDownload)
        {
            await GenererSessionToken();

            var jobject = await CallJson("/downloads/" + idDownload, WebMethod.Get, "application/x-www-form-urlencoded", "");

            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (GetFileNameDownloaded " + idDownload + " ) : " + (string)jobject["msg"]);
                return null;
            }
            return ((string)jobject["result"]["name"]);
        }

        public async Task<UserFreebox> GetInfosFreebox()
        {
            await GenererSessionToken();

            var jobject = await CallJson("/storage/disk/", WebMethod.Get, "application/x-www-form-urlencoded");

            if (jobject == null)
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (GetInfosFreebox ) : la requête n'a pas retournée de résultat");
                return null;
            }
            if (!(bool)jobject["success"])
            {
                await MessageDialogService.AfficherMessage(IpFreebox + " (GetInfosFreebox ) : " + (string)jobject["msg"]);
                return null;
            }
            var userFreebox = new UserFreebox(this);
            userFreebox.FreeSpace = (long)jobject["result"][0]["partitions"][0]["free_bytes"];
            userFreebox.Ratio = 100.0 - userFreebox.FreeSpace / (double)jobject["result"][0]["total_bytes"] * 100;

            jobject = await CallJson("/downloads/", WebMethod.Get, "application/x-www-form-urlencoded");

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
                        Id = (int)obj["id"]

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

            await GenererSessionToken();

            var jsonObject = await CallJson("/fs/ls/" + Crypto.EncodeTo64(directory) + "?onlyFolder=0&countSubFolder=1",
                                         WebMethod.Get, "application/x-www-form-urlencoded");

            if (jsonObject == null)
                return null;


            if (!(bool)jsonObject["success"])
            {
                return null;
            }
            var result = jsonObject["result"];

            return result.Select(t => new FBFileInfo(t)).ToList();
        }

        public async Task DeleteTerminated(int id)
        {
            await CallJson("/downloads/" + id, WebMethod.DELETE);

        }

        public async Task<List<string>> GetTelechargementFini()
        {
            var fichiers = await Ls(_current.PathFilm, false, true);
            var user = await GetInfosFreebox();
            var fichierDownloaded = new List<string>();
            foreach (var downloadItem in user.Downloads)
            {
                fichierDownloaded.Add(await GetFileNameDownloaded(downloadItem.Id.ToString()));
            }
            var fichiersNonTrouved = fichiers.Where(fichier => fichier != _DOSSIER_FINIT).Where(fichier => !fichierDownloaded.Contains(fichier)).ToList();
            return fichiersNonTrouved;
        }

        public async Task DeplacerTelechargementFini()
        {
            var list = await GetTelechargementFini();
            await CreerDossier(_DOSSIER_FINIT, _current.PathFilm);
            await Move(list.Select(f => _current.PathFilm + f).ToArray(), _current.PathFilm + _DOSSIER_FINIT);
        }

        private async Task<JObject> CallJson(string finUrl, WebMethod method = WebMethod.Post, string contentType = null,
            string content = null,
            string headerAccept = null,
            Encoding encoding = null)
        {
            var json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3" + finUrl, method, contentType, content, headerAccept,
                        new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) }, encoding);

            if (string.IsNullOrEmpty(json))
                return null;

            var jobject = JObject.Parse(json);

            //if (!(bool)jobject["success"] && (string)jobject["error_code"] == "invalid_token")
            //{
            //    SessionToken = null;
            //    await GenererSessionToken();

            //    json = await ApiConnector.Call("http://" + IpFreebox + "/api/v3/" + finUrl, method, contentType, content, headerAccept,
            //                new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) }, encoding);

            //    if (string.IsNullOrEmpty(json))
            //        return null;

            //    return JObject.Parse(json);
            //}
            return jobject;

        }
    }
}