using BezyFB.Annotations;
using BezyFB.Helpers;
using BezyFB.Properties;
using FreeboxPortableLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BezyFB.Configuration
{
    public class MySettings : ISettingsFreebox, INotifyPropertyChanged
    {
        public static MySettings Current => new MySettings();

        public string FreeboxIp
        {
            get { return Settings.Default.IpFreebox; }
            set
            {
                Settings.Default.IpFreebox = value;
                OnPropertyChanged(nameof(FreeboxIp));
            }
        }

        public string PathVideo
        {
            get { return Settings.Default.PathVideo; }
            set
            {
                Settings.Default.PathVideo = value;
                OnPropertyChanged("PathVideo");
            }
        }

        public string PathFilm
        {
            get { return Settings.Default.PathFilm; }
            set
            {
                Settings.Default.PathFilm = value;
                OnPropertyChanged("PathFilm");
            }
        }

        public string LoginBetaSerie
        {
            get { return Settings.Default.LoginBetaSerie; }
            set
            {
                Settings.Default.LoginBetaSerie = value;
                OnPropertyChanged("LoginBetaSerie");
            }
        }

        public string LoginT411
        {
            get { return Settings.Default.LoginT411; }
            set
            {
                Settings.Default.LoginT411 = value;
                OnPropertyChanged("LoginT411");
            }
        }

        public string T411Address
        {
            get { return Settings.Default.T411Address; }
            set
            {
                Settings.Default.T411Address = value;
                OnPropertyChanged(nameof(T411Address));
            }
        }

        public string PathLocal
        {
            get { return Settings.Default.PathNonReseau; }
            set
            {
                Settings.Default.PathNonReseau = value;
                OnPropertyChanged("PathLocal");
            }
        }

        public bool AffichageErreurMessageBox
        {
            get { return Settings.Default.AffichageErreurMessageBox; }
            set
            {
                Settings.Default.AffichageErreurMessageBox = value;
                OnPropertyChanged("AffichageErreurMessageBox");
            }
        }

        public string PwdBetaSerie
        {
            get { return Settings.Default.PwdBetaSerie; }
            set
            {
                Settings.Default.PwdBetaSerie = Helper.GetMd5Hash(MD5.Create(), value);
                OnPropertyChanged("PwdBetaSerie");
            }
        }

        public string PassT411
        {
            get { return Settings.Default.PassT411; }
            set
            {
                Settings.Default.PassT411 = value;
                OnPropertyChanged("PassT411");
            }
        }

        public string AppId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string AppName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string AppVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Hostname
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string TokenFreebox
        {
            get
            {
                return Settings.Default.TokenFreebox;
            }

            set
            {
                Settings.Default.TokenFreebox = value;
                OnPropertyChanged(nameof(TokenFreebox));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
