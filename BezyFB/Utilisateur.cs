// Créer par : pepinat
// Le : 22-06-2014

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using BezyFB.Properties;

namespace BezyFB
{
    public class Utilisateur
    {
        public static Utilisateur Current()
        {
            return new Utilisateur();
        }

        private List<ShowConfiguration> ShowConfigurations { get; set; }

        public readonly Dictionary<string, string> SeriePath;
        public readonly Dictionary<string, string> EztvPath;

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

            SeriePath = new Dictionary<string, string>
            {
                {"17", "\\\\192.168.2.254\\Disque dur\\Vidéos\\Californication\\"},
                {"1275", "\\\\192.168.2.254\\Disque dur\\Vidéos\\Walking dead\\"}
            };

            EztvPath = new Dictionary<string, string>
            {
                {"17", "40"},
                {"1275", "428"}
            };
        }
    }
}