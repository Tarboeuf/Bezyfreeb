using BezyFB.Configuration;
using BezyFB.Helpers;
using BezyFB.Properties;
using BezyFB.T411;
using FreeboxPortableLib;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.XPath;
using WpfTemplateBaseLib;
using WpfTemplateLib;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetStatusText("Veuillez choisir votre catégorie");
            gridButton.Visibility = Visibility.Visible;
            T411Client.BaseAddress = Settings.Default.T411Address;
            CheckConfiguration();
        }

        public void SetStatusText(string text)
        {
            var window = Notification.FindAncestor<Window>();
            if (window == null) return;
            Notification.AddNotification(text);
        }

        private async void MainWindow_OnClosed(object sender, EventArgs e)
        {
            await ClientContext.Current.Freebox.Deconnexion();
        }

        private async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tc.SelectedIndex == 1 && T411UserControl.lv.ItemsSource == null)
            {
                pb.Visibility = Visibility.Visible;
                SetStatusText("Chargement depuis T411");
                await T411UserControl.LoadT411();
                SetStatusText("T411 chargé");
                pb.Visibility = Visibility.Collapsed;
            }
            if (tc.SelectedIndex == 2)
            {
                if (!(TabFreebox.DataContext is UserFreebox))
                {
                    try
                    {
                        var uf = await ClientContext.Current.Freebox.GetInfosFreebox();
                        TabFreebox.DataContext = uf;

                        //await uf.LoadMovies(Dispatcher);
                    }
                    catch (Exception ex)
                    {
                        await ClientContext.Current.MessageDialogService.AfficherMessage("Erreur lors de la récupération des infos freebox : \r\n" + ex.Message);
                    }
                }
            }
        }
        private void Quitter_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        

        private async void Betaseries_OnClick(object sender, RoutedEventArgs e)
        {
            tc.SelectedIndex = 0;
            await BetaSerieUserControl.LoadBetaseries();
        }

        private void T411_OnClick(object sender, RoutedEventArgs e)
        {
            tc.SelectedIndex = 1;
        }

        private void Freebox_OnClick(object sender, RoutedEventArgs e)
        {
            gridButton.Visibility = Visibility.Collapsed;
            tc.SelectedIndex = 2;
        }

        private void Configuration_Click(object sender, RoutedEventArgs e)
        {
            var c = new Configuration.Configuration();
            c.Owner = Application.Current.MainWindow;
            c.ShowDialog();
            BetaSerieUserControl.InitialiseElements();
            CheckConfiguration();
        }

        private void ExporterConfiguration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Fichier config (*.confbz)|*.confbz";
                if (sfd.ShowDialog(this) ?? false)
                {
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                    config.SaveAs(sfd.FileName);
                }
            }
            catch (Exception ex)
            {
                ClientContext.Current.MessageDialogService.AfficherMessage(ex.Message + ex.StackTrace);
            }
        }

        private void ImporterConfiguration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Multiselect = false;
                ofd.Filter = "Fichier config (*.confbz)|*.confbz";
                if (ofd.ShowDialog(this) ?? false)
                {
                    try
                    {
                        // Open settings file as XML
                        var import = XDocument.Load(ofd.FileName);
                        // Get the <setting> elements
                        var settings = import.XPathSelectElements("//setting");
                        foreach (var setting in settings)
                        {
                            string name = setting.Attribute("name").Value;
                            string value = setting.XPathSelectElement("value").FirstNode.ToString();

                            try
                            {
                                Settings.Default[name] = value; // throws SettingsPropertyNotFoundException
                            }
                            catch (SettingsPropertyNotFoundException spnfe)
                            {
                                //_logger.WarnException("An imported setting ({0}) did not match an existing setting.".FormatString(name), spnfe);
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        //_logger.ErrorException("Could not import settings.", exc);
                        Settings.Default.Reload(); // from last set saved, not defaults
                    }
                }
            }
            catch (Exception ex)
            {
                ClientContext.Current.MessageDialogService.AfficherMessage(ex.Message + ex.StackTrace);
            }
        }

        private void CheckConfiguration()
        {
            ButtonBetaserie.IsEnabled = !string.IsNullOrEmpty(Settings.Default.LoginBetaSerie) &&
                                        !string.IsNullOrEmpty(Settings.Default.PwdBetaSerie);
            ButtonT411.IsEnabled = !string.IsNullOrEmpty(Settings.Default.LoginT411) &&
                                        !string.IsNullOrEmpty(Settings.Default.PassT411);
            ButtonFreebox.IsEnabled = !string.IsNullOrEmpty(Settings.Default.IpFreebox) &&
                                        !Settings.Default.IpFreebox.Contains("freebox");

            LabelAvertissement.Visibility = ButtonBetaserie.IsEnabled && ButtonT411.IsEnabled && ButtonBetaserie.IsEnabled
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

    }
}