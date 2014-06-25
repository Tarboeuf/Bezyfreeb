// Créer par : pepinat
// Le : 22-06-2014

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using BezyFB.Properties;

namespace BezyFB
{
    public class Utilisateur
    {
        private const string _PATH_DEFAUT_FB = "\\\\192.168.2.254\\";
        private const string _PATH_VIDEOS = "Disque dur/Vidéos/";

        public static Utilisateur Current()
        {
            return new Utilisateur();
        }

        public List<ShowConfiguration> ShowConfigurations { get; set; }

        private readonly Dictionary<string, string> SeriePath;
        private readonly Dictionary<string, string> EztvPath;

        public string GetSeriePath(string IdBetaserie, string nomSerie)
        {
            if (SeriePath.ContainsKey(IdBetaserie))
                return SeriePath[IdBetaserie];

            return _PATH_DEFAUT_FB + _PATH_VIDEOS + nomSerie + "\\";
        }

        public string GetIdEztv(string IdBetaserie, string nomSerie)
        {
            if (EztvPath.ContainsKey(IdBetaserie))
                return EztvPath[IdBetaserie];

            Eztv ez = new Eztv();
            var show = ez.GetListShow().FirstOrDefault(s => s.Name == nomSerie);
            if (null != show)
            {
                EztvPath.Add(IdBetaserie, show.Id);
                return show.Id;
            }

            return null;
        }

        private Utilisateur()
        {
            StringCollection showsCollection = Settings.Default.ShowConfigurationList;
            if (null != showsCollection)
            {
                ShowConfigurations = new List<ShowConfiguration>();
                foreach (string elem in showsCollection)
                {
                    ShowConfigurations.Add(new ShowConfiguration(elem));
                }
            }

            SeriePath = new Dictionary<string, string>();
            SeriePath.Add("17", "Californication\\");
            SeriePath.Add("1275", "Walking dead\\");
            SeriePath.Add("1275", "Devious Maid\\");

            EztvPath = new Dictionary<string, string>();
            EztvPath.Add("17", "40");
            EztvPath.Add("1275", "428");
            EztvPath.Add("1275", "854");
        }
    }
}