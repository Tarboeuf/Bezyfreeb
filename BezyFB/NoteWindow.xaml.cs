using System.Windows;
using System.Windows.Controls;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour NoteWindow.xaml
    /// </summary>
    public partial class NoteWindow : Window
    {
        private readonly BetaSerie.BetaSerie _bs;
        private readonly Episode _episode;

        public NoteWindow(BetaSerie.BetaSerie bs, Episode episode)
        {
            _bs = bs;
            _episode = episode;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var content = ((Button)sender).Content;
            if (content != null)
                _bs.NoterEpisode(_episode, int.Parse(content as string));
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}