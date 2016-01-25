using BezyFB_UWP.Lib;
using BezyFB_UWP.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace BezyFB_UWP
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class PageSettings : Page
    {
        public Settings Settings
        {
            get { return (Settings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Settings.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(Settings), typeof(PageSettings), new PropertyMetadata(null));

        public PageSettings()
        {
            this.InitializeComponent();
            Settings = Settings.Current;
            DataContext = Settings;
        }

        private async void Freebox_Click(object sender, RoutedEventArgs e)
        {
            if (await Settings.Freebox.ConnectNewFreebox())
                Settings.Current.FreeboxIp = Settings.Freebox.IpFreebox;
        }

        private async void TesterBetaseries(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(pwdBetaSerie.Password))
                return;
            try
            {   
                Settings.PwdBetaSerie = pwdBetaSerie.Password;
                Settings.ResetBetaserie();
                var client = Settings.BetaSerie;
                
                if (await client.GenereToken(true))
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
                //T411Client client = T411Client.New(LoginT411, PassT411).Result;
                //if (client.IsTokenCreated)
                //{
                    Helper.AfficherMessage("La connexion s'est réalisé avec succés");
                //}
                //else
                //{
                //    Helper.AfficherMessage("Impossible de se connecter à T411 :\r\nLe token n'est pas créé");
                //}
            }
            catch (Exception ex)
            {
                Helper.AfficherMessage("Impossible de se connecter à T411 :\r\n" + ex.Message);
            }
        }
        
    }
}
