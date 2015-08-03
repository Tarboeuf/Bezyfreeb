// Créer par : pepinat
// Le : 03-06-2014

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using BezyFB.Helpers;
using BezyFB.Properties;

namespace BezyFB.BetaSerie
{
    public class BetaSerie
    {
        private const string ApiAdresse = "http://api.betaseries.com";

        private const string EnteteArgs = "?v=2.2&key=d0256f2444ab";

        //private const string Login = "Tarboeuf";
        //private const string Password = "55fc47e4665c3df4047618f941c054e5";

        //private const string _Login = "haiecapique";
        //private const string _Password = "d9fb8a057fb2af1c9c9557e49eee7dd4";

        //private const string _EnteteArgs = "?v=2.2&key=3c15b9796654";

        //private const string Comments = "/comments";
        private const string Episodes = "/episodes";

        //private const string Friends = "/friends";
        private const string Members = "/members";

        //private const string Messages = "/messages";
        //private const string Movies = "/movies";
        //private const string Pictures = "/pictures";
        //private const string Planning = "/planning";
        //private const string Shows = "/shows";
        private const string Subtitles = "/subtitles";
        private const string Watched = "/watched";

        //private const string Timeline = "/timeline";

        public string Error { get; private set; }

        public EpisodeRoot Root { get; private set; }

        private string Token { get; set; }

        public bool GenereToken(bool force = false)
        {
            if (force || String.IsNullOrEmpty(Token))
            {
                try
                {
                    string link = ApiAdresse + Members + "/auth.xml" + EnteteArgs;
                    link += "&login=" + Settings.Default.LoginBetaSerie + "&password=" + Settings.Default.PwdBetaSerie;
                    Error = ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

                    XDocument xdoc = XDocument.Parse(Error);
                    var t = from lv1 in xdoc.Descendants("token")
                            select lv1.Value;
                    if (t.Any())
                        Token = t.First();

                    if (string.IsNullOrEmpty(Token))
                    {
                        var tError = from lv1 in xdoc.Descendants("content")
                                     select lv1.Value;
                        Error = tError.First();
                        return false;
                    }
                    Error = Token;
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return false;
                }
            }
            return true;
        }

        public EpisodeRoot GetListeNouveauxEpisodesTest()
        {
            if (!GenereToken())
                return null;

            Error = "";
            try
            {
                string link = ApiAdresse + Episodes + "/list" + EnteteArgs;
                link += "&userid=" + Settings.Default.LoginBetaSerie + "&token=" + Token;
                var xml = ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

                var serializer = new XmlSerializer(typeof (EpisodeRoot), new XmlRootAttribute("root"));
                var reader = GenerateStreamFromString(xml);
                if (null == xml)
                {
                    Error = "Erreur lors de la récupération des nouveaux épisodes.";
                    return null;
                }
                var rt = (EpisodeRoot) serializer.Deserialize(reader);
                reader.Close();
                Root = rt;
                return rt;
            }
            catch (Exception e)
            {
                Error = "GetListeNouveauxEpisodesTest : " + e.Message;
            }
            Root = null;
            return null;
        }

        private Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public SousTitreRoot GetPathSousTitre(string episode)
        {
            if (!GenereToken())
                return null;

            Error = "";
            try
            {
                string link = ApiAdresse + Subtitles + "/episode" + EnteteArgs;
                link += "&id=" + episode + "&language=vf&token=" + Token;
                var xml = ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

                var serializer = new XmlSerializer(typeof (SousTitreRoot), new XmlRootAttribute("root"));
                var reader = GenerateStreamFromString(xml);
                var rt = (SousTitreRoot) serializer.Deserialize(reader);
                reader.Close();

                if (!rt.subtitles.Any())
                {
                    link = ApiAdresse + Subtitles + "/episode" + EnteteArgs;
                    link += "&id=" + episode + "&token=" + Token;
                    xml = ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

                    serializer = new XmlSerializer(typeof(SousTitreRoot), new XmlRootAttribute("root"));
                    reader = GenerateStreamFromString(xml);
                    rt = (SousTitreRoot)serializer.Deserialize(reader);
                    reader.Close();
                }
                return rt;
            }
            catch (Exception e)
            {
                Error = "GetPathSousTitre : " + e.Message;
            }
            return null;
        }

        public void SetEpisodeDownnloaded(Episode episode)
        {
            if (!GenereToken())
                return;

            Error = "";
            try
            {
                string link = ApiAdresse + Episodes + "/downloaded" + EnteteArgs;
                link += "&id=" + episode.id + "&token=" + Token;
                ApiConnector.Call(link, WebMethod.Post, null, null, "text/xml");
                episode.user[0].downloaded = "1";
            }
            catch (Exception e)
            {
                Error = "GetPathSousTitre : " + e.Message;
            }
        }

        public void SetEpisodeSeen(Episode episode)
        {
            if (!GenereToken())
                return;

            Error = "";
            try
            {
                string link = ApiAdresse + Episodes + Watched + EnteteArgs;
                link += "&id=" + episode.id + "&token=" + Token;
                ApiConnector.Call(link, WebMethod.Post, null, null, "text/xml");
                foreach (var rootShowsShow in Root.shows)
                {
                    if (rootShowsShow.unseen.Contains(episode))
                        rootShowsShow.unseen = rootShowsShow.unseen.Where(e => e != episode).ToArray();
                }
            }
            catch (Exception e)
            {
                Error = "GetPathSousTitre : " + e.Message;
            }
        }
        public void SetEpisodeUnSeen(Episode episode)
        {
            if (!GenereToken())
                return;

            Error = "";
            try
            {
                string link = ApiAdresse + Episodes + Watched + EnteteArgs;
                link += "&id=" + episode.id + "&token=" + Token;
                ApiConnector.Call(link, WebMethod.DELETE, null, null, "text/xml");
                foreach (var rootShowsShow in Root.shows)
                {
                    if (rootShowsShow.unseen.Contains(episode))
                        rootShowsShow.unseen = rootShowsShow.unseen.Where(e => e != episode).ToArray();
                }
            }
            catch (Exception e)
            {
                Error = "GetPathSousTitre : " + e.Message;
            }
        }
    }
}