using BezyFB.Helpers;
using CommonPortableLib;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BezyFB.BetaSerieLib
{
    public class GuessIt
    {
        public IApiConnectorService ApiConnector { get; set; }

        public async Task<string> GuessNom(string name)
        {
            var jsonGuessit = await ApiConnector.Call("http://guessit.io/guess?filename=" + name.Replace(" ", "%20") + ".avi", WebMethod.Get);
            if (string.IsNullOrEmpty(jsonGuessit))
                return name;

            var jobj = JObject.Parse(jsonGuessit);
            var obj = jobj["title"];
            string nom = "";
            if (null != obj)
                nom = obj.ToString();
            return nom;
        }
    }
}