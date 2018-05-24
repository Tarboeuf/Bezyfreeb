using BezyFB.Properties;
using System;
using System.Windows;
using System.Windows.Forms;
using BetaseriesStandardLib;
using FreeboxStandardLib;

namespace BezyFB.Configuration
{
    /// <summary>
    /// Logique d'interaction pour Configuration.xaml
    /// </summary>
    public sealed partial class Configuration
    {
        private readonly BetaSerie _bs;
        private readonly Freebox _freeboxApi;

        public Configuration()
        {
            InitializeComponent();
            DataContext = MySettings.Current;
            _bs = ClientContext.Current.BetaSerie;
            _freeboxApi = ClientContext.Current.Freebox;
        }


        private async void ButtonSynchroniser_Click(object sender, RoutedEventArgs e)
        {
            if (await _freeboxApi.ConnectNewFreebox())
                MySettings.Current.FreeboxIp = _freeboxApi.IpFreebox;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            Close();
        }

        private void password_Click(object sender, RoutedEventArgs e)
        {
            var passForm = new PasswordForm();
            passForm.Owner = this;
            passForm.ShowDialog();
            if (null != passForm.Pwd)
                MySettings.Current.PwdBetaSerie = passForm.Pwd;
        }

        private void passwordT411_Click(object sender, RoutedEventArgs e)
        {
            var passForm = new PasswordForm();
            passForm.Owner = this;
            passForm.ShowDialog();
            if (null != passForm.Pwd)
                MySettings.Current.PassT411 = passForm.Pwd;
        }

        private void pathLocalclick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog diag = new FolderBrowserDialog();
            diag.SelectedPath = Settings.Default.PathNonReseau;
            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MySettings.Current.PathLocal = diag.SelectedPath;
            }
        }

        private async void TesterBetaseries(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new BetaSerie(MySettings.Current.LoginBetaSerie, MySettings.Current.PwdBetaSerie);
                
                if (await client.GenereToken(true))
                {
                    await ClientContext.Current.MessageDialogService.AfficherMessage("La connexion s'est réalisé avec succés");
                }
                else
                {
                    await ClientContext.Current.MessageDialogService.AfficherMessage("Impossible de se connecter à BetaSeries :\r\n" + client.Error);
                }
            }
            catch (Exception ex)
            {
                await ClientContext.Current.MessageDialogService.AfficherMessage("Impossible de se connecter à T411 :\r\n" + ex.Message);
            }
        }
    }
}