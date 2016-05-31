// Créer par : pepinat
// Le : 23-06-2014

using BezyFB_UWP.Lib.Helpers;
using CommonLib;
using CommonPortableLib;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BezyFB_UWP.Lib.EzTv
{
    public sealed class Eztv : IEztv
    {
        private const string Url = "https://eztv.ag/";
        //private const string Url = "http://eztv.it/";
        //private const string Url = "http://eztv-proxy.net/";
        //private const string Url = https://eztv.ch/";

        private static readonly Dictionary<string, string> PagesSeries = new Dictionary<string, string>();
        private static List<Show> _shows = null;

        public IApiConnectorService ApiConnector { get; set; }

        public async Task<string> GetMagnetSerieEpisode(string serie, string episode)
        {
            if (serie == null)
                return null;

            string html;
            if (PagesSeries.ContainsKey(serie))
            {
                html = PagesSeries[serie];
            }
            else
            {
                html = await ApiConnector.Call(Url + "shows/" + serie + "/", WebMethod.Get, null, null, "text/xml");
            }

            if (html != null)
            {
                var reg = new Regex(@"magnet:\?xt=urn:[^""]*");

                var collec = reg.Matches(html);
                foreach (Match match in collec)
                {
                    if (match.Value.Contains(episode) && !match.Value.Contains("720p") && !match.Value.Contains("1080p"))
                        return match.Value;
                }

                foreach (Match match in collec)
                {
                    if (match.Value.Contains(episode))
                        return match.Value;
                }
            }

            return null;
        }

        public async Task<string> GetTorrentSerieEpisode(string serie, string episode)
        {
            string html;
            if (PagesSeries.ContainsKey(serie))
            {
                html = PagesSeries[serie];
            }
            else
            {
                html = await ApiConnector.Call(Url + "shows/" + serie + "/", WebMethod.Get, null, null, "text/xml");
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var collection = doc.DocumentNode.Elements("//a[@class]")
                .Where(n => n.Attributes["href"].Value.Contains(episode) && n.Attributes["href"].Value.Contains(".torrent") && n.Attributes["class"].Value.StartsWith("download_"))
                .Select(link => link.Attributes["href"].Value).ToList();
            foreach (var link in collection.Where(h => !h.Contains("720p") && !h.Contains("1080p")).Union(collection))
            {
                return link;
            }
            return null;
        }

        public async Task<List<Show>> GetListShow()
        {
            if (null != _shows)
            {
                return _shows;
            }
            string html = await ApiConnector.Call(Url + "search/", WebMethod.Get, null, null, "text/xml");

            if (string.IsNullOrEmpty(html))
                return new List<Show>();

            html = html.Split(new[] { "<select name=\"SearchString\">" }, StringSplitOptions.RemoveEmptyEntries)[1];
            html = html.Split(new[] { "</select>" }, StringSplitOptions.RemoveEmptyEntries)[0];

            html = html.Replace("<option value=\"", "");
            html = html.Replace("\r\n", "");

            var series = html.Split(new[] { "</option>" }, StringSplitOptions.RemoveEmptyEntries);

            _shows = series.Skip(1).Select(s => GetShow(s.Trim())).Where(s => s != null).ToList();

            return _shows;
        }

        private Show GetShow(string str)
        {
            if (str.IndexOf('"') == -1 || str.IndexOf('>') == -1)
                return null;

            return new Show
            {
                Id = str.Substring(0, str.IndexOf('"')),
                Name = str.Substring(str.IndexOf('>') + 1)
            };
        }

        public sealed class Show
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }
    }

    public interface IEztv
    {
        Task<string> GetMagnetSerieEpisode(string idEztv, string code);
        Task<string> GetTorrentSerieEpisode(string idEztv, string code);
    }
}