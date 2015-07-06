using BezyFB.Helpers;
using Newtonsoft.Json.Linq;

namespace BezyFB.BetaSerie
{
    public static class OMDb
    {

        public static double GetNote(string nom)
        {
            var jsonOmdb = ApiConnector.Call("http://www.omdbapi.com/?t=" + nom, WebMethod.Get);
            var jobj = JObject.Parse(jsonOmdb);
            return (double)jobj["imdbRating"];
        }
    }
}