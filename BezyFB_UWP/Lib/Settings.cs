using BezyFB_UWP.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using BezyFB_UWP.Lib.EzTv;
using BezyFB_UWP.Lib.T411;

namespace BezyFB_UWP.Lib
{
    public class Settings : INotifyPropertyChanged
    {
        private Settings()
        {

        }

        public const string AppName = "BezyFreeb";
        public const string AppVersion = "UWP";
        public const string AppId = "fr.freebox.bezyfreeb";

        private static Lazy<Settings> _current = new Lazy<Settings>(() => new Settings());
        private Lazy<BetaSerie.BetaSerie> _betaserie = new Lazy<BetaSerie.BetaSerie>(() => new BetaSerie.BetaSerie(Current.LoginBetaSerie, Current.PwdBetaSerie));
        private static readonly Lazy<Freebox.Freebox> _freebox = new Lazy<Freebox.Freebox>(() => new Freebox.Freebox(Current));
        private static readonly Lazy<Eztv> _eztv = new Lazy<Eztv>(() => new Eztv());
        private static readonly AsyncLazy<T411Client> _t411Client = new AsyncLazy<T411Client>(() => T411Client.New(Current.LoginT411, Current.PassT411));

        public static Settings Current
        {
            get { return _current.Value; }
        }

        public T411Client T411 => _t411Client.GetAwaiter().GetResult();
        public Freebox.Freebox Freebox => _freebox.Value;

        public BetaSerie.BetaSerie BetaSerie => _betaserie.Value;

        public Eztv Eztv => _eztv.Value;

        public void ResetBetaserie()
        {
            _betaserie = new Lazy<Lib.BetaSerie.BetaSerie>(() => new BetaSerie.BetaSerie(Current.LoginBetaSerie, Current.PwdBetaSerie));
        }
        
        public String FreeboxIp
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
                ApplicationData.Current.LocalSettings.Values["PwdBetaSerie"] = Helper.GetMd5Hash(value);
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
            get { return ApplicationData.Current.LocalSettings.Values["ShowConfigurationList"] as string; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["ShowConfigurationList"] = value;
                OnPropertyChanged("ShowConfigurationList");
            }
        }

        private Lazy<Utilisateur> _users = new Lazy<Utilisateur>(() => Utilisateur.Current());

        public Utilisateur User => _users.Value;

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
