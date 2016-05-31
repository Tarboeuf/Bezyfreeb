//// Créer par : tkahn
//// Le : 24-06-2014

//using BezyFB.BetaSerie;
//using BezyFB.Helpers;
//using BezyFB.Properties;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Threading;

//namespace BezyFB.Freebox
//{
//    public class FreeboxOld
//    {
//        private String SessionToken { get; set; }

//        public bool ConnectNewFreebox()
//        {
//            Settings.Default.IpFreebox = "mafreebox.freebox.fr";

//            if (!TesterConnexionFreebox())
//                return false;

//            if (!GenererAppToken())
//                return false;

//            if (!GenererSessionToken())
//                return false;

//            var json = ApiConnector.Call("http://mafreebox.freebox.fr/api/v3/connection/config/", WebMethod.Get, "application/json", null, null,
//                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
//            var j = JObject.Parse(json);

//            Settings.Default.IpFreebox = (string)j["result"]["remote_access_ip"] + ":" + (string)j["result"]["remote_access_port"];

//            return true;
//        }

//        private bool TesterConnexionFreebox()
//        {
//            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

//            try
//            {
//                s.Connect(Settings.Default.IpFreebox, 80);
//            }
//            catch (Exception ex)
//            {
//                Helper.AfficherMessage(Settings.Default.IpFreebox + " (TesterConnexionFreebox) : " + ex.Message);
//                return false;
//            }
//            return true;
//        }

//        private bool GenererSessionToken()
//        {
//            if (Settings.Default.IpFreebox.StartsWith("http:"))
//                return false;

//            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/login/", WebMethod.Get);
//            if (json == null)
//                return false;
//            var challenge = (string)JObject.Parse(json)["result"]["challenge"];

//            var password = Helper.Encode(challenge, Settings.Default.TokenFreebox);

//            json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/login/session/", WebMethod.Post, "application/json",
//                                     new JObject { { "password", password }, { "app_id", Settings.Default.AppId } }.ToString());
//            var session = JObject.Parse(json);

//            if (session == null)
//                return false;

//            SessionToken = (string)session["result"]["session_token"];

//            return true;
//        }

//        private bool GenererAppToken()
//        {
//            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/login/authorize/", WebMethod.Post, "application/json",
//                                         new JObject
//                                             {
//                                                 {"app_id", Settings.Default.AppId}, {"app_name", Settings.Default.AppName},
//                                                 {"app_version", Settings.Default.AppVersion}, {"device_name", Environment.MachineName}
//                                             }.ToString());

//            var apptokenrequest = JObject.Parse(json);

//            var appToken = (string)apptokenrequest["result"]["app_token"];
//            var trackId = (string)apptokenrequest["result"]["track_id"];
//            String result;
//            do
//            {
//                json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/login/authorize/" + trackId, WebMethod.Get);
//                var apptokenstatus = JObject.Parse(json);
//                result = (string)apptokenstatus["result"]["status"];
//                Thread.Sleep(500);
//            } while (result == "pending");

//            if (result != "granted")
//                return false;

//            Settings.Default.TokenFreebox = appToken;
//            return true;
//        }

//        public string Deconnexion()
//        {
//            try
//            {
//                if (String.IsNullOrEmpty(SessionToken))
//                    return null;

//                var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/login/logout/", WebMethod.Post, null, null,
//                                             null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

//                if (json == null) return null;
//                return JObject.Parse(json).ToString();
//            }
//            catch (Exception ex)
//            {
//                Helper.AfficherMessage(Settings.Default.IpFreebox + " (Deconnexion) : " + ex.Message);
//                return null;
//            }
//        }

//        public string CreerDossier(string directory, string parent)
//        {
//            if (String.IsNullOrEmpty(SessionToken))
//                GenererSessionToken();

//            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/fs/mkdir/", WebMethod.Post,
//                                         "application/x-www-form-urlencoded", new JObject { { "parent", Helper.EncodeTo64(parent) }, { "dirname", directory } }.ToString(),
//                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

//            return JObject.Parse(json).ToString();
//        }

//        public List<string> Ls(string directory, bool onlyFolder)
//        {
//            if (String.IsNullOrEmpty(SessionToken))
//                GenererSessionToken();

//            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/fs/ls/" + Helper.EncodeTo64(directory) + "?onlyFolder=" + (onlyFolder ? 1 : 0),
//                                         WebMethod.Get, "application/x-www-form-urlencoded", null, null,
//                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

//            if (json == null)
//                return new List<string>();

//            var jsonObject = JObject.Parse(json);

//            if (!(bool)jsonObject["success"])
//            {
//                Helper.AfficherMessage(Settings.Default.IpFreebox + " (Ls " + directory + ") : " + (string)jsonObject["msg"]);
//                return null;
//            }
//            var result = jsonObject["result"];

//            return result.Select(t => t["name"].ToString()).ToList();
//        }

//        public string Download(String magnetUrl, string directory)
//        {
//            if (String.IsNullOrEmpty(SessionToken))
//                GenererSessionToken();

//            var pathDir = Settings.Default.PathVideo;

//            foreach (var s in directory.Split('\\', '/').Where(s => !String.IsNullOrEmpty(s)))
//            {
//                CreerDossier(s, pathDir);
//                pathDir += "/" + s;
//            }

//            var path = System.Web.HttpUtility.UrlEncode(magnetUrl);
//            var content = "download_url=" + path + "\r\n&download_dir=" + Helper.EncodeTo64(pathDir);
//            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/downloads/add/", WebMethod.Post,
//                                         "application/x-www-form-urlencoded", content,
//                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
//            var jsonObject = JObject.Parse(json);
//            if (!(bool)jsonObject["success"])
//            {
//                Helper.AfficherMessage(Settings.Default.IpFreebox + " (Download magnet ) : " + (string)jsonObject["msg"]);
//                return null;
//            }

//            return ((int)jsonObject["result"]["id"]).ToString();
//        }

//        public string DownloadFile(FileInfo torrentURL, string directory, bool isRelativeDir)
//        {
//            return DownloadFile(File.ReadAllBytes(torrentURL.FullName), torrentURL.Name, directory, isRelativeDir);
//        }

//        public string DownloadFile(string urlFichier, string directory, bool isRelativeDir)
//        {
//            string lien = urlFichier;
//            Uri uri = new Uri(lien);
//            string nomFichier = uri.Segments[uri.Segments.Length - 1];
//            WebRequest request = HttpWebRequest.Create(lien);

//            request.ContentType = "application/x-bittorrent";

//            using (WebResponse response = request.GetResponse())
//            {
//                using (var stream = response.GetResponseStream())
//                {
//                    if (null != stream)
//                    {
//                        return DownloadFile(ReadFully(stream), nomFichier, directory, isRelativeDir);
//                    }
//                }
//            }
//            return null;
//        }

//        public string DownloadFile(Stream filchier, string nomFichier, string directory, bool isRelativeDir)
//        {
//            return DownloadFile(ReadFully(filchier), nomFichier, directory, isRelativeDir);
//        }

//        public string DownloadFile(byte[] fichier, string nomFichier, string directory, bool isRelativeDir)
//        {
//            if (String.IsNullOrEmpty(SessionToken))
//                GenererSessionToken();

//            string pathDir;
//            if (isRelativeDir)
//            {
//                pathDir = Settings.Default.PathVideo;
//                foreach (var s in directory.Split('\\', '/').Where(s => !String.IsNullOrEmpty(s)))
//                {
//                    CreerDossier(s, pathDir);
//                    pathDir += "/" + s;
//                }
//            }
//            else
//            {
//                pathDir = directory;
//            }

//            Dictionary<string, object> parameters = new Dictionary<string, object>();
//            parameters.Add("download_dir", Helper.EncodeTo64(pathDir));
//            parameters.Add("archive_password", "");
//            parameters.Add("download_file", new FormUpload.FileParameter(fichier, nomFichier, "application/x-bittorrent"));

//            string strReponse = "";
//            strReponse = FormUpload.MultipartFormDataPost("http://" + Settings.Default.IpFreebox + "/api/v1/downloads/add", "Me", parameters, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

//            var jsonObject = JObject.Parse(strReponse);
//            if (!(bool)jsonObject["success"])
//            {
//                Helper.AfficherMessage(Settings.Default.IpFreebox + " (DownloadFile " + nomFichier + " ) : " + (string)jsonObject["msg"]);
//                return null;
//            }

//            return ((int)jsonObject["result"]["id"]).ToString();
//        }

//        public static byte[] ReadFully(Stream input)
//        {
//            byte[] buffer = new byte[16 * 1024];
//            using (MemoryStream ms = new MemoryStream())
//            {
//                int read;
//                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
//                {
//                    ms.Write(buffer, 0, read);
//                }
//                return ms.ToArray();
//            }
//        }

//        public string UploadFile(string inputFile, string outputDir, string outputFileName)
//        {
//            if (String.IsNullOrEmpty(SessionToken))
//                GenererSessionToken();

//            var pathDir = Settings.Default.PathVideo;

//            foreach (var s in outputDir.Split('\\', '/').Where(s => !String.IsNullOrEmpty(s)))
//            {
//                CreerDossier(s, pathDir);
//                pathDir += "/" + s;
//            }

//            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/upload/", WebMethod.Post, "application/json",
//                                         new JObject { { "dirname", Helper.EncodeTo64(pathDir) }, { "upload_name", outputFileName } }.ToString(), null,
//                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
//            if (json == null)
//            {
//                Helper.AfficherMessage(Settings.Default.IpFreebox + " (UploadFile " + inputFile + " ) : Erreur lors de l'appel à la freebox ");
//                return null;
//            }

//            try
//            {
//                var jobj = JObject.Parse(json);
//                if (!(bool)jobj["success"] && (string)jobj["error_code"] == "conflict")
//                {
//                    DeleteFile(pathDir + "/" + outputFileName);
//                    json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/upload/", WebMethod.Post, "application/json",
//                                         new JObject { { "dirname", Helper.EncodeTo64(pathDir) }, { "upload_name", outputFileName } }.ToString(), null,
//                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
//                    jobj = JObject.Parse(json);
//                }

//                if (!(bool)jobj["success"])
//                {
//                    Helper.AfficherMessage(Settings.Default.IpFreebox + " (UploadFile " + inputFile + " ) : " + jobj["msg"].ToString());
//                    return null;
//                }
//                var id = (string)jobj["result"]["id"];

//                string text = File.ReadAllText(inputFile, Encoding.UTF8);

//                const string boundary = "----WebKitFormBoundary0Qvwx7fycAF2CWmh";

//                json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/upload/" + id + "/send", WebMethod.Post, "multipart/form-data; boundary=" + boundary,
//                                         "--" + boundary + Environment.NewLine +
//                                         "Content-Disposition: form-data; name=\"" + outputFileName + "\"; filename=\"" + outputFileName + "\"" + Environment.NewLine +
//                                         "Content-Type: text/plain" + Environment.NewLine + Environment.NewLine + text + Environment.NewLine +
//                                         "--" + boundary + "--",
//                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
//            }
//            catch (Exception ex)
//            {
//                Helper.AfficherMessage(Settings.Default.IpFreebox + " (UploadFile " + inputFile + " ) : " + ex.Message);
//                return null;
//            }

//            return JObject.Parse(json).ToString();
//        }

//        public string DeleteFile(string filePath)
//        {
//            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/fs/rm/", WebMethod.Post,
//                                            "application/json",
//                                             new JObject
//                                                 {
//                                                      {"files", new JArray { Helper.EncodeTo64(filePath) } }
//                                                }.ToString(),
//                                            headers: new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) }
//                                            );
//            return JObject.Parse(json).ToString();
//        }

//        public string CleanUpload()
//        {
//            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/upload/clean", WebMethod.DELETE,
//                                           headers: new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
//            return JObject.Parse(json).ToString();
//        }

//        public string GetFileNameDownloaded(string idDownload)
//        {
//            if (String.IsNullOrEmpty(SessionToken))
//                GenererSessionToken();

//            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/downloads/" + idDownload, WebMethod.Get,
//                                         "application/x-www-form-urlencoded", "",
//                                         null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });
//            var jsonObject = JObject.Parse(json);
//            if (!(bool)jsonObject["success"])
//            {
//                Helper.AfficherMessage(Settings.Default.IpFreebox + " (GetFileNameDownloaded " + idDownload + " ) : " + (string)jsonObject["msg"]);
//                return null;
//            }
//            return ((string)jsonObject["result"]["name"]);
//        }

//        public UserFreebox GetInfosFreebox()
//        {
//            if (String.IsNullOrEmpty(SessionToken))
//                GenererSessionToken();

//            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/storage/disk/", WebMethod.Get,
//                                         "application/x-www-form-urlencoded", null, null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

//            if (json == null)
//            {
//                Helper.AfficherMessage(Settings.Default.IpFreebox + " (GetInfosFreebox ) : la requête n'a pas retournée de résultat");
//                return null;
//            }
//            var jsonObject = JObject.Parse(json);

//            if (!(bool)jsonObject["success"])
//            {
//                Helper.AfficherMessage(Settings.Default.IpFreebox + " (GetInfosFreebox ) : " + (string)jsonObject["msg"]);
//                return null;
//            }
//            var userFreebox = new UserFreebox(this);
//            userFreebox.FreeSpace = (long)jsonObject["result"][0]["partitions"][0]["free_bytes"];
//            userFreebox.Ratio = 100.0 - (double)userFreebox.FreeSpace / (long)jsonObject["result"][0]["total_bytes"] * 100;

//            json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v3/downloads/", WebMethod.Get,
//                                         "application/x-www-form-urlencoded", null, null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

//            jsonObject = JObject.Parse(json);

//            if (!(bool)jsonObject["success"])
//            {
//                Helper.AfficherMessage(Settings.Default.IpFreebox + " (GetInfosFreebox) : " + (string)jsonObject["msg"]);
//                return null;
//            }

//            try
//            {
//                foreach (var item in jsonObject["result"])
//                {
//                    var di = new DownloadItem
//                    {
//                        Pourcentage = (double)item["rx_pct"],
//                        Name = (string)item["name"],
//                        Status = (string)item["status"],
//                        RxPourcentage = (double)item["tx_pct"],
//                    };
//                    userFreebox.Downloads.Add(di);
//                }
//            }
//            catch (Exception ex)
//            {
//            }

//            return userFreebox;
//        }

//    }

//    //public class UserFreebox
//    //{
//    //    private readonly Freebox _fb;

//    //    public UserFreebox(Freebox fb)
//    //    {
//    //        _fb = fb;
//    //        Downloads = new List<DownloadItem>();
//    //        Movies = new ObservableCollection<OMDb>();
//    //    }

//    //    public long FreeSpace { get; set; }
//    //    public double Ratio { get; set; }
//    //    public List<DownloadItem> Downloads { get; set; }

//    //    public ObservableCollection<OMDb> Movies { get; set; }

//    //    public void LoadMovies(Dispatcher dispatcher)
//    //    {
//    //        new Task(() =>
//    //        {
//    //            foreach (var item in _fb.Ls(Settings.Default.PathFilm, false))
//    //            {
//    //                var nom = GuessIt.GuessNom(item);
//    //                var omdb = OMDb.GetNote(nom, item);

//    //                dispatcher.BeginInvoke(new Action(() => Movies.Add(omdb)));
//    //            }
//    //        }).Start();
//    //    }
//    //}

//    //public class DownloadItem
//    //{
//    //    public string Name { get; set; }
//    //    public string Status { get; set; }
//    //    public double Pourcentage { get; set; }
//    //    public double RxPourcentage { get; set; }
//    //}
//}