using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BetaseriesPortableLib;

namespace BezyFB_UWP.Lib
{
    public class Utilisateur
    {
        private static readonly Lazy<Utilisateur> _current = new Lazy<Utilisateur>(() => new Utilisateur());
        public static Utilisateur Current()
        {
            return _current.Value;
        }

        public void SerializeElement()
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<ShowConfiguration>));

            using (StringWriter writer = new StringWriter())
            {
                ser.Serialize(writer, Shows);
                Settings.Current.ShowConfigurationList = writer.ToString();
            }
        }

        public List<ShowConfiguration> Shows { get; set; }

        private Utilisateur()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<ShowConfiguration>));

            XmlReaderSettings settings = new XmlReaderSettings();

            // No settings need modifying here
            if (!string.IsNullOrEmpty(Settings.Current.ShowConfigurationList))
            {
                using (StringReader textReader = new StringReader(Settings.Current.ShowConfigurationList))
                {
                    using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                    {
                        Shows = (List<ShowConfiguration>)serializer.Deserialize(xmlReader);
                    }
                }
            }
            else
                Shows = new List<ShowConfiguration>();
        }

        private async Task<ShowConfiguration> GetSerie(String idBetaSerie, String nomSerie)
        {
            var showConfiguration = Shows.FirstOrDefault(s => s.IdBetaSerie == idBetaSerie);

            if (showConfiguration != null)
                return showConfiguration;

            var listShow = await ClientContext.Current.Eztv.GetListShow();
            var show = new ShowConfiguration
            {
                HasSubtitle = true,
                IdBetaSerie = idBetaSerie,
                IsDownloadable = true,
                ManageSeasonFolder = true,
                PathFreebox = nomSerie,
                ShowName = nomSerie,
                IdEztv = listShow.FirstOrDefault(c => string.Equals(nomSerie, c.Name))?.Id
            };

            Shows.Add(show);
            return show;
        }

        public async Task<ShowConfiguration> GetSerie(Episode episode)
        {
            return await GetSerie(episode.show_id, episode.show_title);
        }

        public async Task<ShowConfiguration> GetSerie(rootShowsShow rootShow)
        {
            return await GetSerie(rootShow.id, rootShow.title);
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
