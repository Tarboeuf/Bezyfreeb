using BezyFB.Freebox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément du menu volant des paramètres, consultez la page http://go.microsoft.com/fwlink/?LinkId=273769
using BezyFB.Helpers;

namespace BezyFreebMetro
{
    public sealed partial class Settings : SettingsFlyout
    {
        public Settings()
        {
            this.InitializeComponent();
            DataContext = AppSettings.Default;
        }

        private async void SeConnecterFreebox_Click(object sender, RoutedEventArgs e)
        {
            Freebox fb = new Freebox();
            await fb.ConnectNewFreebox();
        }

        private void ChangerPassWord(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = "";
            StackPanelPassword.Visibility = Visibility.Visible;
        }

        private void ValiderPassword(object sender, RoutedEventArgs e)
        {
            AppSettings.Default.PwdBetaSerie = Helper.GetMd5Hash(PasswordBox.Password);
            StackPanelPassword.Visibility = Visibility.Collapsed;
        }
    }
}
