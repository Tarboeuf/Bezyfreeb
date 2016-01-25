using BezyFB_UWP.Lib.Helpers;
using Windows.Data.Json;

namespace BezyFB_UWP.Lib.BetaSerie
{
    public static class GuessIt
    {
        public static async System.Threading.Tasks.Task<string> GuessNom(string name)
        {
            var jsonGuessit = await ApiConnector.Call("http://guessit.io/guess?filename=" + name.Replace(" ", "%20") + ".avi", WebMethod.Get);
            if (string.IsNullOrEmpty(jsonGuessit))
                return name;

            var jobj = JsonObject.Parse(jsonGuessit);
            var obj = jobj["title"];
            string nom = "";
            if (null != obj)
                nom = obj.ToString();
            return nom;
        }
    }
}