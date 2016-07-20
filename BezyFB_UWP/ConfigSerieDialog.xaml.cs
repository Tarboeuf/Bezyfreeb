using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using BezyFB_UWP.Lib;
using EztvPortableLib;

// Pour plus d'informations sur le modèle d'élément Boîte de dialogue de contenu, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace BezyFB_UWP
{
    public sealed partial class ConfigSerieDialog : ContentDialog
    {
        private readonly Eztv _eztv;
        private readonly ShowConfiguration _showConfiguration;
        private FilteredCollection<Eztv.Show> _list;


        public ConfigSerieDialog(ShowConfiguration showConfiguration)
        {
            _eztv = ClientContext.Current.Eztv;
            _showConfiguration = showConfiguration;
            this.InitializeComponent();
            this.Loaded += ConfigSerieDialog_Loaded;
        }

        private async void ConfigSerieDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressBarDC.Current.IsProgress = true;
            _list = new FilteredCollection<Eztv.Show>((await _eztv.GetListShow()).ToList());
            ListView.ItemsSource = _list;

            var texte = _showConfiguration.ShowName
                .Replace("(", "")
                .Replace(")", "").Split(' ')
                .OrderByDescending(n => n.Length).FirstOrDefault();
            TextBox.Text = texte;
            ProgressBarDC.Current.IsProgress = false;
        }

        private void RefreshList()
        {
            _list.RefreshView(n => n.Name.ToLower().Contains(TextBox.Text.ToLower()));
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var show = ListView.SelectedItem as Eztv.Show;
            if (show != null)
            {
                _showConfiguration.IdEztv = show.Id;
                Utilisateur.Current().SerializeElement();
            }
            Hide();
        }


        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshList();
        }
    }

    internal class FilteredCollection<T> : List<T>, INotifyCollectionChanged
    {
        private List<T> _view;
        
        /// <summary>
        /// Create a new FilteredTaskStore based on an existing TaskStore
        /// </summary>
        /// <param name="values">The TaskStore to base this filter on</param>
        public FilteredCollection(IList<T> values) : base(values)
        {
            _view = new List<T>(values);
        }
        
        public void RefreshView(Func<T, bool> criteria)
        {
            var newList = _view.Where(criteria);

            Clear();
            AddRange(newList);
            
            // Call the event handler for the updated list.
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
