using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
    }
}
