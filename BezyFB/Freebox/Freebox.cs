// Créer par : tkahn
// Le : 24-06-2014

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
                new JObject { { "app_id", Settings.Default.AppId }, { "app_name", Settings.Default.AppName },
                { "app_version", Settings.Default.AppVersion }, { "device_name", Environment.MachineName } }.ToString());

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
²            try
            {
                if (String.IsNullOrEmpty(SessionToken))
                    GenererSessionToken();

                var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v2/login/logout/", WebMethod.Post, null, null,
                    null, new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

                return JObject.Parse(json).ToString();
            }
            catch (Exception)
            {
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

        public string Ls(string directory)
        {
            if (String.IsNullOrEmpty(SessionToken))
                GenererSessionToken();

            var json = ApiConnector.Call("http://" + Settings.Default.IpFreebox + "/api/v2/fs/ls/" + Helper.EncodeTo64(directory),
                WebMethod.Get, "application/x-www-form-urlencoded", null, null,
                new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", SessionToken) });

            return JObject.Parse(json).ToString();
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

            return JObject.Parse(json).ToString();
        }
    }
}