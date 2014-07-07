// Créer par : pepinat
// Le : 22-06-2014

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using BezyFB.EzTv;
using BezyFB.Properties;

namespace BezyFB.Configuration
{
    public class Utilisateur
    {
        public static Utilisateur Current()
        {
            Utilisateur user = new Utilisateur();

            return user;
        }

        public void SerializeElement()
        {
            XmlSerializer ser = new XmlSerializer(typeof (List<ShowConfiguration>));

            StringWriter writer = new StringWriter();
            ser.Serialize(writer, Shows);
            Settings.Default.ShowConfigurationList = writer.ToString();
            Settings.Default.Save();
            writer.Close();
        }

        public List<ShowConfiguration> Shows { get; set; }

        private Utilisateur()
        {
            XmlSerializer serializer = new XmlSerializer(typeof (List<ShowConfiguration>));

            XmlReaderSettings settings = new XmlReaderSettings();

            // No settings need modifying here
            if (!string.IsNullOrEmpty(Settings.Default.ShowConfigurationList))
            {
                using (StringReader textReader = new StringReader(Settings.Default.ShowConfigurationList))
                {
                    using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                    {
                        Shows = (List<ShowConfiguration>) serializer.Deserialize(xmlReader);
                    }
                }
            }
            else
                Shows = new List<ShowConfiguration>();
        }

        public ShowConfiguration GetSerie(string showId, rootShowsShow showBS = null)
        {
            foreach (var showConfiguration in Shows)
            {
                if (showConfiguration.IdBetaSerie == showId)
                    return showConfiguration;
            }
            if (null == showBS)
                return null;

            ShowConfiguration show = new ShowConfiguration()
                {
                    HasSubtitle = true,
                    IdBetaSerie = showId,
                    IsDownloadable = true,
                    ManageSeasonFolder = true,
                    PathFreebox = showBS.title,
                    ShowName = showBS.title
                };
            Shows.Add(show);
            return show;
        }
    }

    public sealed class ShowConfiguration : INotifyPropertyChanged
    {
        private string _IdEztv;
        private string _IdBetaSerie;
        private string _ShowName;
        private bool _IsDownloadable;
        private bool _HasSubtitle;
        private string _PathReseau;
        private bool _ManageSeasonFolder;
        private string _PathFreebox;

        public string IdBetaSerie
        {
            get { return _IdBetaSerie; }
            set
            {
                _IdBetaSerie = value;
                OnPropertyChanged("IdBetaSerie");
            }
        }

        public string IdEztv
        {
            get { return _IdEztv; }
            set
            {
                _IdEztv = value;
                OnPropertyChanged("IdEztv");
            }
        }

        public string ShowName
        {
            get { return _ShowName; }
            set
            {
                _ShowName = value;
                OnPropertyChanged("ShowName");
            }
        }

        public string PathFreebox
        {
            get { return _PathFreebox; }
            set
            {
                _PathFreebox = value;
                OnPropertyChanged("PathFreebox");
            }
        }

        public bool ManageSeasonFolder
        {
            get { return _ManageSeasonFolder; }
            set
            {
                _ManageSeasonFolder = value;
                OnPropertyChanged("ManageSeasonFolder");
            }
        }

        public string PathReseau
        {
            get { return _PathReseau; }
            set
            {
                _PathReseau = value;
                OnPropertyChanged("PathReseau");
            }
        }

        public bool HasSubtitle
        {
            get { return _HasSubtitle; }
            set
            {
                _HasSubtitle = value;
                OnPropertyChanged("HasSubtitle");
            }
        }

        public bool IsDownloadable
        {
            get { return _IsDownloadable; }
            set
            {
                _IsDownloadable = value;
                OnPropertyChanged("IsDownloadable");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}