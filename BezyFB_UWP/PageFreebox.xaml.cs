using BezyFB_UWP.Lib;
using BezyFB_UWP.Lib.BetaSerie;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BezyFB_UWP.Lib.Freebox;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace BezyFB_UWP
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class PageFreebox : Page
    {
        private UserFreebox _infos;

        public PageFreebox()
        {
            this.InitializeComponent();
        }


        private async void PageFreebox_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_infos == null)
            {
                await Refresh();
            }
        }

        private async Task Refresh()
        {
            ProgressBarDC.Current.IsProgress = true;
            _infos = await Settings.Current.Freebox.GetInfosFreebox();
            DataContext = _infos;
            ProgressBarDC.Current.IsProgress = false;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }
    }
}
