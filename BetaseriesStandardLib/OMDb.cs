using System;
using System.Threading.Tasks;
using CommonStandardLib;
using Newtonsoft.Json.Linq;

namespace BetaseriesStandardLib
{
    public class OMDb
    {
        public static async Task<OMDb> GetNote(string nom, IApiConnectorService apiConnector, string fileName = null)
        {
            var jsonOmdb = await apiConnector.Call("http://www.omdbapi.com/?t=" + nom, WebMethod.Get);
            if (null == jsonOmdb)
                return new OMDb();
            try
            {
                var jobj = JObject.Parse(jsonOmdb);
                if ((bool)jobj["Response"])
                    return new OMDb
                    {
                        Note = GetNote(jobj["imdbRating"]),
                        Title = (string)jobj["Title"],
                        Year = (string)jobj["Year"],
                        Resume = (string)jobj["Plot"],
                        Poster = (string)jobj["Poster"],
                        FileName = fileName ?? nom,
                    };
            }
            catch (Exception)
            {

            }
            return new OMDb { FileName = fileName ?? nom, };
        }

        private static double GetNote(JToken token)
        {
            try
            {
                return (double)token;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public string Title { get; set; }
        public string Year { get; set; }
        public double Note { get; set; }
        public string Resume { get; set; }
        public string FileName { get; set; }
        public string Poster { get; set; }
    }
}