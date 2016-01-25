using BezyFB_UWP.Lib.Helpers;
using System;
using Windows.Data.Json;

namespace BezyFB_UWP.Lib.BetaSerie
{
    public class OMDb
    {
        public static async System.Threading.Tasks.Task<OMDb> GetNote(string nom, string fileName = null)
        {
            var jsonOmdb = await ApiConnector.Call("http://www.omdbapi.com/?t=" + nom, WebMethod.Get);
            if (null == jsonOmdb)
                return new OMDb();
            var jobj = JsonObject.Parse(jsonOmdb);
            if (jobj["Response"].GetBoolean())
                return new OMDb
                {
                    Note = GetNote(jobj["imdbRating"]),
                    Title = jobj["Title"].GetString(),
                    Year = jobj["Year"].GetString(),
                    Resume = jobj["Plot"].GetString(),
                    Poster = jobj["Poster"].GetString(),
                    FileName = fileName ?? nom,
                };
            return new OMDb { FileName = fileName ?? nom, };
        }

        private static double GetNote(IJsonValue token)
        {
            try
            {
                return token.GetNumber();
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