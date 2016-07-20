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
using BezyFB_UWP.Lib;
using BezyFB_UWP.Lib.Helpers;
using BezyFB_UWP.Lib.T411;
using CommonLib;
using CommonPortableLib;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BezyFB_UWP
{
    public sealed partial class T411UserControl : UserControl
    {
        public T411UserControl()
        {
            this.InitializeComponent();
        }



        public Torrent Item
        {
            get { return (Torrent)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(Torrent), typeof(T411UserControl), new PropertyMetadata(0));



        private async void Download_OnClick(object sender, RoutedEventArgs e)
        {
            if (null != Item)
            {
                if (await ClientContext.Current.MessageDialogService.ShowYesNoDialog("Êtes-vous sûr de vouloir télécharger ce film ?\r\n" + Item.Name) == DialogResult.Yes)
                {
                    ProgressBarDC.Current.IsProgress = true;
                    using (var stream = ClientContext.Current.T411.DownloadTorrent(Item.Id))
                    {
                        try
                        {
                            await ClientContext.Current.Freebox.DownloadFile(stream, Item.Name + ".torrent", Settings.Current.PathFilm, false);
                            await ClientContext.Current.MessageDialogService.AfficherMessage("Le téléchargement a été rajouté");
                        }
                        catch (Exception)
                        {
                            await ClientContext.Current.MessageDialogService.AfficherMessage("Une erreur est survenue lors de l'ajout du téléchargement");
                        }
                    }
                }
                ProgressBarDC.Current.IsProgress = false;
            }
        }
    }
}
