using BezyFB.Annotations;
using BezyFB.Helpers;
using BezyFB.Properties;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Forms;
using BezyFB.T411;
using FreeboxPortableLib;
using BezyFB.BetaSerieLib;

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
                MySettings.Current.FreeboxIp = Settings.Default.IpFreebox;
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

        private async void TesterT411(object sender, RoutedEventArgs e)
        {
            try
            {
                T411Client client = await T411Client.New(MySettings.Current.LoginT411, MySettings.Current.PassT411);
                if (client.IsTokenCreated)
                {
                    await ClientContext.Current.MessageDialogService.AfficherMessage("La connexion s'est réalisé avec succés");
                }
                else
                {
                    await ClientContext.Current.MessageDialogService.AfficherMessage("Impossible de se connecter à T411 :\r\nLe token n'est pas créé");
                }
            }
            catch (Exception ex)
            {
                await ClientContext.Current.MessageDialogService.AfficherMessage("Impossible de se connecter à T411 :\r\n" + ex.Message);
            }
        }
    }
}