using BezyFB_UWP.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using EztvPortableLib;
using BezyFB_UWP.Lib.T411;
using CommonLib;
using FreeboxPortableLib;
using CommonPortableLib;
using Windows.Networking.Connectivity;
using System.IO;

namespace BezyFB_UWP.Lib
{
    public class Settings : INotifyPropertyChanged, ISettingsFreebox
    {
        private Settings()
        {

        }

        public string AppName { get; } = "BezyFreeb";
        public string AppVersion { get; } = "UWP";
        public string AppId { get; } = "fr.freebox.bezyfreeb";

        private static Lazy<Settings> _current = new Lazy<Settings>(() => new Settings());
        private Lazy<Utilisateur> _users = new Lazy<Utilisateur>(() => Utilisateur.Current());

        public Utilisateur User => _users.Value;

        public ICryptographic Crypto { get; set; }

        public static Settings Current
        {
            get { return _current.Value; }
        }

        public string FreeboxIp
        {
            get { return ApplicationData.Current.LocalSettings.Values["IpFreebox"] as string; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["IpFreebox"] = value;
                OnPropertyChanged("FreeboxIp");
            }
        }

        public string PathVideo
        {
            get { return ApplicationData.Current.LocalSettings.Values["PathVideo"] as string ?? "/Disque dur/Vidéos"; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["PathVideo"] = value;
                OnPropertyChanged("PathVideo");
            }
        }

        public string PathFilm
        {
            get { return ApplicationData.Current.LocalSettings.Values["PathFilm"] as string; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["PathFilm"] = value;
                OnPropertyChanged("PathFilm");
            }
        }

        public string LoginBetaSerie
        {
            get { return ApplicationData.Current.LocalSettings.Values["LoginBetaSerie"] as string; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["LoginBetaSerie"] = value;
                OnPropertyChanged("LoginBetaSerie");
            }
        }

        public string LoginT411
        {
            get { return ApplicationData.Current.LocalSettings.Values["LoginT411"] as string; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["LoginT411"] = value;
                OnPropertyChanged("LoginT411");
            }
        }

        public string PathLocal
        {
            get { return ApplicationData.Current.LocalSettings.Values["PathNonReseau"] as string; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["PathNonReseau"] = value;
                OnPropertyChanged("PathLocal");
            }
        }

        public string PwdBetaSerie
        {
            get { return ApplicationData.Current.LocalSettings.Values["PwdBetaSerie"] as string; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["PwdBetaSerie"] = Crypto.GetMd5Hash(value);
                OnPropertyChanged("PwdBetaSerie");
            }
        }

        public string PassT411
        {
            get { return ApplicationData.Current.LocalSettings.Values["PassT411"] as string; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["PassT411"] = value;
                OnPropertyChanged("PassT411");
            }
        }

        public string T411Address
        {
            get { return ApplicationData.Current.LocalSettings.Values["T411Address"] as string ?? "https://api.t411.in/"; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["T411Address"] = value;
                OnPropertyChanged("T411Address");
            }
        }

        public string ShowConfigurationList
        {
            get
            {
                var file = ApplicationData.Current.LocalFolder.TryGetItemAsync("ShowConfigurationList").GetAwaiter().GetResult() as StorageFile;
                if (file == null)
                {
                    return null;
                }
                using (var stream = file.OpenAsync(FileAccessMode.Read).GetAwaiter().GetResult())
                {
                    using (StreamReader sr = new StreamReader(stream.AsStream()))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            set
            {
                var file = ApplicationData.Current.LocalFolder.TryGetItemAsync("ShowConfigurationList").GetAwaiter().GetResult() as StorageFile;
                if (file == null)
                {
                    file = ApplicationData.Current.LocalFolder.CreateFileAsync("ShowConfigurationList").GetAwaiter().GetResult();
                }
                using (var stream = file.OpenStreamForWriteAsync().GetAwaiter().GetResult())
                {
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        sw.Write(value);
                    }
                }

                OnPropertyChanged("ShowConfigurationList");
            }
        }

        public string Hostname
        {
            get
            {
                return NetworkInformation.GetHostNames().FirstOrDefault(ni => ni.Type == Windows.Networking.HostNameType.DomainName)?.DisplayName;
            }
        }

        public string TokenFreebox
        {
            get { return ApplicationData.Current.LocalSettings.Values["TokenFreebox"] as string; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["TokenFreebox"] = value;
                OnPropertyChanged("TokenFreebox");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
