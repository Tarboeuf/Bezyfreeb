using BezyFB.Helpers;
using Newtonsoft.Json.Linq;

namespace BezyFB.BetaSerie
{
    public static class GuessIt
    {

        public static string GuessNom(string name)
        {
            var jsonGuessit = ApiConnector.Call("http://guessit.io/guess?filename=" + name.Replace(" ", "%20"), WebMethod.Get);
            var jobj = JObject.Parse(jsonGuessit);
            var obj = jobj["title"];
            string nom = "";
            if (null != obj)
                nom = obj.ToString();
            return nom;
        }
    }
}