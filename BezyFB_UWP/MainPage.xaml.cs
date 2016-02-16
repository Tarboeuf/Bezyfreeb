using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using BezyFB_UWP.Annotations;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BezyFB_UWP
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            ProgressBar.DataContext = ProgressBarDC.Current;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void BetaSerie_Click(object sender, RoutedEventArgs e)
        {
            var frame = this.DataContext as Frame;
            Page page = frame?.Content as Page;
            if (page?.GetType() != typeof(PageBetaserie))
            {
                frame.Navigate(typeof(PageBetaserie));
            }
        }

        private void T411_Click(object sender, RoutedEventArgs e)
        {
            var frame = this.DataContext as Frame;
            Page page = frame?.Content as Page;
            if (page?.GetType() != typeof(PageT411))
            {
                frame.Navigate(typeof(PageT411));
            }
        }

        private void Freebox_Click(object sender, RoutedEventArgs e)
        {
            var frame = this.DataContext as Frame;
            Page page = frame?.Content as Page;
            if (page?.GetType() != typeof(PageFreebox))
            {
                frame.Navigate(typeof(PageFreebox));
            }
        }

        private void MenuButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            var frame = this.DataContext as Frame;
            Page page = frame?.Content as Page;
            if (page?.GetType() != typeof(PageSettings))
            {
                frame.Navigate(typeof(PageSettings));
            }
        }
    }

    public class ProgressBarDC : INotifyPropertyChanged
    {
        private static readonly Lazy<ProgressBarDC> _current = new Lazy<ProgressBarDC>();

        public static ProgressBarDC Current => _current.Value;

        private bool _isProgress;

        public bool IsProgress
        {
            get { return _isProgress; }
            set
            {
                _isProgress = value; 
                OnPropertyChanged(); 
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(Visibility));
            }
        }

        public Visibility Visibility => IsProgress ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
