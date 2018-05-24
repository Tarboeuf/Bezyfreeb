using BezyFB_UWP.Lib;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BezyFB_UWP.Lib.T411;

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
            if (await ClientContext.Current.Freebox.ConnectNewFreebox())
                Settings.Current.FreeboxIp = ClientContext.Current.Freebox.IpFreebox;
        }

        private async void TesterBetaseries(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(pwdBetaSerie.Password))
                return;
            try
            {   
                Settings.PwdBetaSerie = pwdBetaSerie.Password;
                ClientContext.Current.ResetBetaserie();
                var client = ClientContext.Current.BetaSerie;
                
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
                Settings.Current.PassT411 = pwdT411.Password;
                T411Client client = await T411Client.New(Settings.Current.LoginT411, Settings.Current.PassT411);
                if (client.IsTokenCreated)
                {
                    await ClientContext.Current.MessageDialogService.AfficherMessage("La connexion s'est réalisé avec succés");
                    ClientContext.Current.ResetT411();
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
