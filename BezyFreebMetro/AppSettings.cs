using System.ComponentModel;

namespace BezyFreebMetro
{
    public sealed class AppSettings : INotifyPropertyChanged
    {
        public static AppSettings Default;

        public AppSettings()
        {
            Default = this;
        }

        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        private bool _showFormatBar;
        public bool ShowFormatBar
        {
            get
            {
                if (localSettings.Values["showFormatBar"] == null)
                    localSettings.Values["showFormatBar"] = true;

                _showFormatBar = (bool)localSettings.Values["showFormatBar"];
                return _showFormatBar;
            }
            set
            {
                _showFormatBar = value;
                localSettings.Values["showFormatBar"] = _showFormatBar;
                NotifyPropertyChanged("ShowFormatBar");
                NotifyPropertyChanged("FormatBarVisibility");
            }
        }

        #region ShowConfigurationList

        private string _ShowConfigurationList;
        public string ShowConfigurationList
        {
            get
            {
                if (localSettings.Values["ShowConfigurationList"] == null)
                    localSettings.Values["ShowConfigurationList"] = "";

                _ShowConfigurationList = (string)localSettings.Values["ShowConfigurationList"];
                return _ShowConfigurationList;
            }
            set
            {
                _ShowConfigurationList = value;
                localSettings.Values["ShowConfigurationList"] = _ShowConfigurationList;
                NotifyPropertyChanged("ShowConfigurationList");
            }
        }
        #endregion

        #region LoginBetaSerie

        private string _LoginBetaSerie;
        public string LoginBetaSerie
        {
            get
            {
                if (localSettings.Values["LoginBetaSerie"] == null)
                    localSettings.Values["LoginBetaSerie"] = "Tarboeuf";

                _LoginBetaSerie = (string)localSettings.Values["LoginBetaSerie"];
                return _LoginBetaSerie;
            }
            set
            {
                _LoginBetaSerie = value;
                localSettings.Values["LoginBetaSerie"] = _LoginBetaSerie;
                NotifyPropertyChanged("LoginBetaSerie");
            }
        }
        #endregion

        #region PwdBetaSerie

        private string _PwdBetaSerie;
        public string PwdBetaSerie
        {
            get
            {
                if (localSettings.Values["PwdBetaSerie"] == null)
                    localSettings.Values["PwdBetaSerie"] = "55fc47e4665c3df4047618f941c054e5";

                _PwdBetaSerie = (string)localSettings.Values["PwdBetaSerie"];
                return _PwdBetaSerie;
            }
            set
            {
                _PwdBetaSerie = value;
                localSettings.Values["PwdBetaSerie"] = _PwdBetaSerie;
                NotifyPropertyChanged("PwdBetaSerie");
            }
        }
        #endregion

        #region IpFreebox

        private string _IpFreebox;
        public string IpFreebox
        {
            get
            {
                if (localSettings.Values["IpFreebox"] == null)
                    localSettings.Values["IpFreebox"] = "http://mafreebox.freebox.fr";

                _IpFreebox = (string)localSettings.Values["IpFreebox"];
                return _IpFreebox;
            }
            set
            {
                _IpFreebox = value;
                localSettings.Values["IpFreebox"] = _IpFreebox;
                NotifyPropertyChanged("IpFreebox");
            }
        }
        #endregion

        #region TokenFreebox

        private string _TokenFreebox;
        public string TokenFreebox
        {
            get
            {
                if (localSettings.Values["TokenFreebox"] == null)
                    localSettings.Values["TokenFreebox"] = "";

                _TokenFreebox = (string)localSettings.Values["TokenFreebox"];
                return _TokenFreebox;
            }
            set
            {
                _TokenFreebox = value;
                localSettings.Values["TokenFreebox"] = _TokenFreebox;
                NotifyPropertyChanged("TokenFreebox");
            }
        }
        #endregion

        #region AppId

        private string _AppId;
        public string AppId
        {
            get
            {
                if (localSettings.Values["AppId"] == null)
                    localSettings.Values["AppId"] = "BezyFreeb";

                _AppId = (string)localSettings.Values["AppId"];
                return _AppId;
            }
            set
            {
                _AppId = value;
                localSettings.Values["AppId"] = _AppId;
                NotifyPropertyChanged("AppId");
            }
        }
        #endregion

        #region AppName

        private string _AppName;
        public string AppName
        {
            get
            {
                if (localSettings.Values["AppName"] == null)
                    localSettings.Values["AppName"] = "BezyFreeb";

                _AppName = (string)localSettings.Values["AppName"];
                return _AppName;
            }
            set
            {
                _AppName = value;
                localSettings.Values["AppName"] = _AppName;
                NotifyPropertyChanged("AppName");
            }
        }
        #endregion

        #region AppVersion

        private string _AppVersion;
        public string AppVersion
        {
            get
            {
                if (localSettings.Values["AppVersion"] == null)
                    localSettings.Values["AppVersion"] = "0.0.Beta";

                _AppVersion = (string)localSettings.Values["AppVersion"];
                return _AppVersion;
            }
            set
            {
                _AppVersion = value;
                localSettings.Values["AppVersion"] = _AppVersion;
                NotifyPropertyChanged("AppVersion");
            }
        }
        #endregion

        #region PathVideo

        private string _PathVideo;
        public string PathVideo
        {
            get
            {
                if (localSettings.Values["PathVideo"] == null)
                    localSettings.Values["PathVideo"] = "Disque dur\\Vidéos";

                _PathVideo = (string)localSettings.Values["PathVideo"];
                return _PathVideo;
            }
            set
            {
                _PathVideo = value;
                localSettings.Values["PathVideo"] = _PathVideo;
                NotifyPropertyChanged("PathVideo");
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}