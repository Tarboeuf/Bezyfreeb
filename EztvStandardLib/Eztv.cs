// Créer par : pepinat
// Le : 23-06-2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonStandardLib;
using HtmlAgilityPack;

namespace EztvStandardLib
{
    public sealed class Eztv
    {
        private const string Url = "https://eztv.ag/";

        private List<Show> _shows = null;

        private static Dictionary<string, string> _PagesSeries = new Dictionary<string, string>();

        public IApiConnectorService ApiConnector { get; set; }

        public async Task<string> GetMagnetSerieEpisode(string serie, string episode)
        {
            if (serie == null)
                return null;

            string html;
            if (_PagesSeries.ContainsKey(serie))
            {
                html = _PagesSeries[serie];
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
            if (_PagesSeries.ContainsKey(serie))
            {
                html = _PagesSeries[serie];
            }
            else
            {
                html = await ApiConnector.Call(Url + "shows/" + serie + "/", WebMethod.Get, null, null, "text/xml");
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.Descendants("//a[@class]");

            var collection = nodes.Where(n => n.Attributes["href"].Value.Contains(episode) && n.Attributes["href"].Value.Contains(".torrent") && n.Attributes["class"].Value.StartsWith("download_"))
                                    .Select(link => link.Attributes["href"].Value).ToList();

            foreach (var link in collection.Where(h => !h.Contains("720p") && !h.Contains("1080p")).Union(collection))
            {
                return link;
            }
            return null;
        }

        public async Task<List<Show>> GetListShow()
        {
            if (_shows == null)
            {
                string html = await ApiConnector.Call(Url + "showlist/", WebMethod.Get, null, null, "text/xml");

                if (string.IsNullOrEmpty(html))
                    return new List<Show>();

                var lines =
                    html.Split(new [] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries)
                        .Where(l => l.Contains("thread_link"));
                
                _shows = lines.Select(s => GetShow(s.Trim())).Where(s => s != null).ToList();
            }

            return _shows;
        }

        private Show GetShow(string str)
        {
            string name = str.Substring(str.IndexOf("\"thread_link\">", StringComparison.Ordinal) + 14);
            name = name.Substring(0, name.IndexOf("</a>", StringComparison.Ordinal));
            string id = str.Substring(str.IndexOf("/shows/", StringComparison.Ordinal)+7);
            id = id.Substring(0, id.IndexOf("/", StringComparison.Ordinal));
            return new Show
            {
                Id = id,
                Name = name
            };
        }

        public sealed class Show
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }
    }
}