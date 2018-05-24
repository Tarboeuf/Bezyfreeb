using BezyFB_UWP.Lib;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FreeboxStandardLib;

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
            _infos = await ClientContext.Current.Freebox.GetInfosFreebox();
            DataContext = _infos;
            ProgressBarDC.Current.IsProgress = false;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }
    }
}
