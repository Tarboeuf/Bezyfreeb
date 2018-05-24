using System.Threading.Tasks;
using CommonStandardLib;
using Newtonsoft.Json.Linq;

namespace BetaseriesStandardLib
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
            var obj = jobj["title"] ?? jobj["series"];
            string nom = "";
            if (null != obj)
                nom = obj.ToString();
            return nom;
        }
    }
}