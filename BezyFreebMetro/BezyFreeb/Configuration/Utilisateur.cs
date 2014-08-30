// Créer par : pepinat
// Le : 22-06-2014

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using BezyFB.EzTv;
using BezyFreebMetro;
using System.Threading.Tasks;


namespace BezyFB.Configuration
{
    public class Utilisateur
    {
        public static async Task<Utilisateur> Current()
        {
            Utilisateur user = new Utilisateur();
            await user.Initialize();
            return user;
        }

        public void SerializeElement()
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<ShowConfiguration>));

            StringWriter writer = new StringWriter();
            try
            {

                ser.Serialize(writer, Shows);
                AppSettings.Default.ShowConfigurationList = writer.ToString();
            }
            catch (Exception e)
            {

                throw;
            }
            //AppSettings.Save();
            //writer.Close();
        }

        public List<ShowConfiguration> Shows { get; set; }

        private async Task Initialize()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<ShowConfiguration>));

            XmlReaderSettings settings = new XmlReaderSettings();

            var shows = await Eztv.GetListShow();
            

            // No settings need modifying here
            if (!string.IsNullOrEmpty(AppSettings.Default.ShowConfigurationList))
            {
                using (StringReader textReader = new StringReader(AppSettings.Default.ShowConfigurationList))
                {
                    using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                    {
                        Shows = (List<ShowConfiguration>)serializer.Deserialize(xmlReader);
                        Shows.ForEach(s => s.Utilisateur = this);
                        Shows.ForEach(s => s.Shows = shows.ToList());
                    }
                }
            }
            else
                Shows = new List<ShowConfiguration>();
        }

        private Utilisateur()
        {
        }

        private async Task<ShowConfiguration> GetSerie(String idBetaSerie, String nomSerie)
        {
            var showConfiguration = Shows.FirstOrDefault(s => s.IdBetaSerie == idBetaSerie);

            if (showConfiguration != null)
                return showConfiguration;

            var eztv = new Eztv();
            var shows = await Eztv.GetListShow();
            var show = new ShowConfiguration
            {
                HasSubtitle = true,
                IdBetaSerie = idBetaSerie,
                IsDownloadable = true,
                ManageSeasonFolder = true,
                PathFreebox = nomSerie,
                ShowName = nomSerie,
                IdEztv = shows.Where(c => String.Equals(nomSerie, c.Name)).Select(c => c.Id).FirstOrDefault(),
                Shows = shows.ToList(),
                Utilisateur = this
            };

            Shows.Add(show);
            return show;
        }

        public async Task<ShowConfiguration> GetSerie(Episode episode)
        {
            return await GetSerie(episode.show_id, episode.show_title);
        }

        public async Task<object> GetSerie(rootShowsShow rootShow)
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

        [XmlIgnore]
        public Utilisateur Utilisateur { get; set; }

        [XmlIgnore]
        public List<Eztv.Show> Shows
        {
            get;
            set;
        }

        public string IdEztv
        {
            get { return _IdEztv; }
            set
            {
                _IdEztv = value;
                if (null != Utilisateur)
                    Utilisateur.SerializeElement();
                OnPropertyChanged("IdEztv");
                OnPropertyChanged("EztvShow");
            }
        }

        public string IdBetaSerie
        {
            get { return _IdBetaSerie; }
            set
            {
                _IdBetaSerie = value;
                OnPropertyChanged("IdBetaSerie");
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
                if (null != Utilisateur)
                    Utilisateur.SerializeElement();
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

    public static class ListExtension
    {
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
        }
    }
}