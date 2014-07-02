// Créer par : pepinat
// Le : 22-06-2014

using System.Collections.Generic;
using System.Linq;
using BezyFB.EzTv;

namespace BezyFB.Configuration
{
    public class Utilisateur
    {
        public static Utilisateur Current()
        {
            return new Utilisateur();
        }

        private readonly Dictionary<string, string> _seriePath;
        private readonly Dictionary<string, string> _eztvPath;

        public string GetSeriePath(string idBetaserie, string nomSerie)
        {
            if (_seriePath.ContainsKey(idBetaserie))
                return _seriePath[idBetaserie];

            return nomSerie + "\\";
        }

        public string GetIdEztv(string idBetaserie, string nomSerie)
        {
            if (_eztvPath.ContainsKey(idBetaserie))
                return _eztvPath[idBetaserie];

            var ez = new Eztv();
            var show = ez.GetListShow().FirstOrDefault(s => s.Name == nomSerie);
            if (null != show)
            {
                _eztvPath.Add(idBetaserie, show.Id);
                return show.Id;
            }

            return null;
        }

        private Utilisateur()
        {
            _seriePath = new Dictionary<string, string>
            {
                /*{"17", "Californication/"},
                {"1275", "Walking dead/"},
                {"5579", "Devious Maid/"}*/
            };

            _eztvPath = new Dictionary<string, string>
                {
                    /*{"17", "40"},
                    {"1275", "428"},
                    {"5579", "854"}*/
                };
        }
    }
}