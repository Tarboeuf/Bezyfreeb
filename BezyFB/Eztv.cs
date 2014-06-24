// Créer par : pepinat
// Le : 23-06-2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BezyFB
{
    public class Eztv
    {
        private const string URL = "http://eztv.it/";

        public string GetMagnetSerieEpisode(string serie, string episode)
        {
            string html = Helper.LireRequetePOST(URL, "", "shows/" + serie + "/", "", false);

            Regex reg = new Regex(@"magnet:\?xt=urn:[^""]*");

            var collec = reg.Matches(html);
            foreach (Match match in collec)
            {
                if (match.Value.Contains(episode) && !match.Value.Contains("720p"))
                    return match.Value;
            }

            foreach (Match match in collec)
            {
                if (match.Value.Contains(episode))
                    return match.Value;
            }

            return html;
        }

        public IEnumerable<Show> GetListShow()
        {
            string html = Helper.LireRequetePOST(URL, "", "search/", "", false);

            html = html.Split(new[] { "<select name=\"SearchString\">" }, StringSplitOptions.RemoveEmptyEntries)[1];
            html = html.Split(new[] { "</select>" }, StringSplitOptions.RemoveEmptyEntries)[0];

            html = html.Replace("<option value=\"", "");

            var series = html.Split(new string[] { "</option>" }, StringSplitOptions.RemoveEmptyEntries);

            IEnumerable<Show> shows = series.Skip(1).Select(GetShow).Where(s => s != null);

            return shows;
        }

        private Show GetShow(string str)
        {
            if (str.IndexOf('"') != -1 && str.IndexOf('>') != -1)
            {
                Show s = new Show();
                s.Id = str.Substring(0, str.IndexOf('"'));
                s.Name = str.Substring(str.IndexOf('>') + 1);
                return s;
            }
            return null;
        }

        public sealed class Show
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }
    }
}