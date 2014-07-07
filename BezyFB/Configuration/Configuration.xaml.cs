using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows;
using BezyFB.Annotations;
using BezyFB.Helpers;
using BezyFB.Properties;

namespace BezyFB.Configuration
{
    /// <summary>
    /// Logique d'interaction pour Configuration.xaml
    /// </summary>
    public sealed partial class Configuration : Window, INotifyPropertyChanged
    {
        private readonly BetaSerie.BetaSerie _bs;

        public Configuration(BetaSerie.BetaSerie bs)
        {
            InitializeComponent();
            DataContext = this;
            _bs = bs;
        }

        public String FreeboxIp
        {
            get { return Settings.Default.IpFreebox; }
            set
            {
                Settings.Default.IpFreebox = value;
                OnPropertyChanged("FreeboxIp");
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

        public string LoginBetaSerie
        {
            get { return Settings.Default.LoginBetaSerie; }
            set
            {
                Settings.Default.LoginBetaSerie = value;
                OnPropertyChanged("LoginBetaSerie");
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Freebox.Freebox.TestToken(true);
            FreeboxIp = Settings.Default.IpFreebox;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            _bs.GenereToken(true);
            if (String.IsNullOrEmpty(Settings.Default.TokenFreebox))
                Button_Click(null, null);
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            Close();
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