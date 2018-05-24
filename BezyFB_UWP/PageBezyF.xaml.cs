using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace BezyFB_UWP
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class PageBezyF : Page
    {
        public PageBezyF()
        {
            this.InitializeComponent();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
