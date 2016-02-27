using BezyFB.Annotations;
using BezyFB.Helpers;
using BezyFB.Properties;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Forms;
using BezyFB.T411;

namespace BezyFB.Configuration
{
    /// <summary>
    /// Logique d'interaction pour Configuration.xaml
    /// </summary>
    public sealed partial class Configuration : INotifyPropertyChanged
    {
        private readonly BetaSerie.BetaSerie _bs;
        private readonly Freebox.Freebox _freeboxApi;

        public Configuration(BetaSerie.BetaSerie bs, Freebox.Freebox freeboxApi)
        {
            InitializeComponent();
            DataContext = this;
            _bs = bs;
            _freeboxApi = freeboxApi;
        }

        public String FreeboxIp
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_freeboxApi.ConnectNewFreebox())
                FreeboxIp = Settings.Default.IpFreebox;
            else
                Settings.Default.Reload();
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

        private void password_Click(object sender, RoutedEventArgs e)
        {
            var passForm = new PasswordForm();
            passForm.Owner = this;
            passForm.ShowDialog();
            if (null != passForm.Pwd)
                PwdBetaSerie = passForm.Pwd;
        }

        private void passwordT411_Click(object sender, RoutedEventArgs e)
        {
            var passForm = new PasswordForm();
            passForm.Owner = this;
            passForm.ShowDialog();
            if (null != passForm.Pwd)
                PassT411 = passForm.Pwd;
        }

        private void pathLocalclick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog diag = new FolderBrowserDialog();
            diag.SelectedPath = Settings.Default.PathNonReseau;
            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PathLocal = diag.SelectedPath;
            }
        }

        private void TesterBetaseries(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new BetaSerie.BetaSerie(LoginBetaSerie, PwdBetaSerie);
                
                if (client.GenereToken(true))
                {
                    Helper.AfficherMessage("La connexion s'est réalisé avec succés");
                }
                else
                {
                    Helper.AfficherMessage("Impossible de se connecter à BetaSeries :\r\n" + client.Error);
                }
            }
            catch (Exception ex)
            {
                Helper.AfficherMessage("Impossible de se connecter à T411 :\r\n" + ex.Message);
            }
        }

        private void TesterT411(object sender, RoutedEventArgs e)
        {
            try
            {
                T411Client client = T411Client.New(LoginT411, PassT411).Result;
                if (client.IsTokenCreated)
                {
                    Helper.AfficherMessage("La connexion s'est réalisé avec succés");
                }
                else
                {
                    Helper.AfficherMessage("Impossible de se connecter à T411 :\r\nLe token n'est pas créé");
                }
            }
            catch (Exception ex)
            {
                Helper.AfficherMessage("Impossible de se connecter à T411 :\r\n" + ex.Message);
            }
        }
    }
}