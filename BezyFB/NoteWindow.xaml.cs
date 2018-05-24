using System.Windows;
using System.Windows.Controls;
using BetaseriesStandardLib;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour NoteWindow.xaml
    /// </summary>
    public partial class NoteWindow : Window
    {
        private readonly BetaSerie _bs;
        private readonly Episode _episode;

        public NoteWindow(BetaSerie bs, Episode episode)
        {
            _bs = bs;
            _episode = episode;
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var content = ((Button)sender).Content;
            if (content != null)
                await _bs.NoterEpisode(_episode, int.Parse(content as string));
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}