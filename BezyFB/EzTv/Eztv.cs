﻿// Créer par : pepinat
// Le : 23-06-2014

using BezyFB.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BezyFB.EzTv
{
    public sealed class Eztv
    {
        private const string Url = "https://eztv.it/";   //     private const string Url = "http://eztv.it/"; //"http://eztv-proxy.net/"; https://eztv.ch/

        public static string GetMagnetSerieEpisode(string serie, string episode)
        {
            string html = ApiConnector.Call(Url + "shows/" + serie + "/", WebMethod.Get, null, null, "text/xml");

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

            return html;
        }

        public IEnumerable<Show> GetListShow()
        {
            string html = ApiConnector.Call(Url + "search/", WebMethod.Get, null, null, "text/xml");

            if (string.IsNullOrEmpty(html))
                return new List<Show>();

            html = html.Split(new[] { "<select name=\"SearchString\">" }, StringSplitOptions.RemoveEmptyEntries)[1];
            html = html.Split(new[] { "</select>" }, StringSplitOptions.RemoveEmptyEntries)[0];

            html = html.Replace("<option value=\"", "");
            html = html.Replace("\r\n", "");

            var series = html.Split(new[] { "</option>" }, StringSplitOptions.RemoveEmptyEntries);

            IEnumerable<Show> shows = series.Skip(1).Select(s => GetShow(s.Trim())).Where(s => s != null);

            return shows;
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
}