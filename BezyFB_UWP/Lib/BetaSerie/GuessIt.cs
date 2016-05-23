using BezyFB_UWP.Lib.Helpers;
using CommonLib;
using CommonPortableLib;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace BezyFB_UWP.Lib.BetaSerie
{
    public class GuessIt : IGuessIt
    {
        public IApiConnectorService ApiConnector { get; set; }
        
        public async Task<string> GuessNom(string name)
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
    public interface IGuessIt
    {
        Task<string> GuessNom(string name);
    }
}