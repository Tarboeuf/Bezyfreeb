// Créer par : pepinat
// Le : 03-06-2014

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Threading.Tasks;
using BezyFB_UWP.Lib.Helpers;

namespace BezyFB_UWP.Lib.BetaSerie
{
    public class BetaSerie
    {
        private readonly string _login;
        private readonly string _password;

        public BetaSerie(string login, string password)
        {
            _login = login;
            _password = password;
        }

        private const string ApiAdresse = "http://api.betaseries.com";

        private const string EnteteArgs = "?v=2.2&key=d0256f2444ab";

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
        private const string Note = "/note";
        private const string Shows = "/shows";

        //private const string Timeline = "/timeline";

        public string Error { get; private set; }

        public EpisodeRoot Root { get; private set; }

        private string Token { get; set; }

        public async System.Threading.Tasks.Task<bool> GenereToken(bool force = false)
        {
            if (force || String.IsNullOrEmpty(Token))
            {
                try
                {
                    string link = ApiAdresse + Members + "/auth.xml" + EnteteArgs;
                    link += "&login=" + _login + "&password=" + _password;
                    Error = await ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

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

        public async System.Threading.Tasks.Task<EpisodeRoot> GetListeNouveauxEpisodesTest()
        {
            if (!await GenereToken())
                return null;

            Error = "";
            try
            {
                string link = ApiAdresse + Episodes + "/list" + EnteteArgs;
                link += "&userid=" + _login + "&token=" + Token;
                var xml = await ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

                var serializer = new XmlSerializer(typeof(EpisodeRoot), new XmlRootAttribute("root"));
                using (var reader = GenerateStreamFromString(xml))
                {
                    if (null == xml)
                    {
                        Error = "Erreur lors de la récupération des nouveaux épisodes.";
                        return null;
                    }
                    var rt = (EpisodeRoot)serializer.Deserialize(reader);

                    Root = rt;
                    return rt;
                }
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

        public async Task<SousTitreRoot> GetPathSousTitre(string episode)
        {
            if (!await GenereToken())
                return null;

            Error = "";
            try
            {
                string link = ApiAdresse + Subtitles + "/episode" + EnteteArgs;
                link += "&id=" + episode + "&language=vf&token=" + Token;
                var xml = await ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

                var serializer = new XmlSerializer(typeof(SousTitreRoot), new XmlRootAttribute("root"));
                SousTitreRoot rt;
                using (var reader = GenerateStreamFromString(xml))
                {
                    rt = (SousTitreRoot)serializer.Deserialize(reader);
                }

                if (!rt.subtitles.Any())
                {
                    link = ApiAdresse + Subtitles + "/episode" + EnteteArgs;
                    link += "&id=" + episode + "&token=" + Token;
                    xml = await ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

                    serializer = new XmlSerializer(typeof(SousTitreRoot), new XmlRootAttribute("root"));
                    using (var reader = GenerateStreamFromString(xml))
                        rt = (SousTitreRoot)serializer.Deserialize(reader);
                }
                return rt;
            }
            catch (Exception e)
            {
                Error = "GetPathSousTitre : " + e.Message;
            }
            return null;
        }

        public async Task SetEpisodeDownnloaded(Episode episode)
        {
            if (!await GenereToken())
                return;

            Error = "";
            try
            {
                string link = ApiAdresse + Episodes + "/downloaded" + EnteteArgs;
                link += "&id=" + episode.id + "&token=" + Token;
                await ApiConnector.Call(link, WebMethod.Post, null, null, "text/xml");
                episode.user[0].downloaded = "1";
            }
            catch (Exception e)
            {
                Error = "GetPathSousTitre : " + e.Message;
            }
        }

        public async Task SetEpisodeSeen(Episode episode)
        {
            if (!await GenereToken())
                return;

            Error = "";
            try
            {
                string link = ApiAdresse + Episodes + Watched + EnteteArgs;
                link += "&id=" + episode.id + "&token=" + Token;
                await ApiConnector.Call(link, WebMethod.Post, null, null, "text/xml");
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

        public async void SetEpisodeUnSeen(Episode episode)
        {
            if (!await GenereToken())
                return;

            Error = "";
            try
            {
                string link = ApiAdresse + Episodes + Watched + EnteteArgs;
                link += "&id=" + episode.id + "&token=" + Token;
                await ApiConnector.Call(link, WebMethod.DELETE, null, null, "text/xml");
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

        public async void NoterEpisode(Episode episode, int note)
        {
            string link = ApiAdresse + Episodes + Note + EnteteArgs;
            link += "&id=" + episode.id + "&note=" + note + "&token=" + Token;
            await ApiConnector.Call(link, WebMethod.Post, null, null, "text/xml");
        }

        public async Task<List<Episode>> GetShowEpisode(rootShowsShow show)
        {
            if (!await GenereToken())
                return null;

            Error = "";
            try
            {
                string link = ApiAdresse + Shows + Episodes + EnteteArgs;
                link += "&id=" + show.id + "&token=" + Token;
                var xml = await ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

                var serializer = new XmlSerializer(typeof(EpisodeList), new XmlRootAttribute("root"));
                using (var reader = GenerateStreamFromString(xml))
                {
                    if (null == xml)
                    {
                        Error = "Erreur lors de la récupération des nouveaux épisodes.";
                        return null;
                    }
                    var rt = (EpisodeList)serializer.Deserialize(reader);
                    return rt.episodes.ToList();
                }
            }
            catch (Exception e)
            {
                Error = "GetListeNouveauxEpisodesTest : " + e.Message;
            }
            return null;
        }
    }
}