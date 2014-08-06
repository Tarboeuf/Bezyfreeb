using BezyFB.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BezyFreebMetro.BezyFreeb.IMDB
{
    public static class ImdbAPI
    {
        private const string TheTvDBAPI = "1A4ADC2761788221";

        public static async Task<string> GetImagePath(string id)
        {
            var xml = await ApiConnector.Call("http://thetvdb.com/api/" + TheTvDBAPI + "/series/" + id + "/banners.xml", WebMethod.Get);
            var xdom = XDocument.Parse(xml);
            try
            {
                var relativePath = xdom.Root.Elements("Banner").First().Element("ThumbnailPath").Value as string;

                return "http://thetvdb.com/banners/" + relativePath;
            }
            catch (Exception)
            {

            }
            return null;
        }
    }
}
