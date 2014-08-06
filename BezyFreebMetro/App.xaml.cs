using BezyFreebMetro.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;
using System.ComponentModel;

// Pour plus d'informations sur le modèle Application grille, consultez la page http://go.microsoft.com/fwlink/?LinkId=234226

namespace BezyFreebMetro
{
    /// <summary>
    /// Fournit un comportement spécifique à l'application afin de compléter la classe Application par défaut.
    /// </summary>
    sealed partial class App : Application
    {
        public AppSettings Settings = new AppSettings();

        /// <summary>
        /// Initialise l'objet Application singleton.  Il s'agit de la première ligne de code créé
        /// à être exécutée. Elle correspond donc à l'équivalent logique de main() ou WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoqué lorsque l'application est lancée normalement par l'utilisateur final.  D'autres points d'entrée
        /// seront utilisés par exemple au moment du lancement de l'application pour l'ouverture d'un fichier spécifique.
        /// </summary>
        /// <param name="e">Détails concernant la requête et le processus de lancement.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            // Affichez des informations de profilage graphique lors du débogage.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Afficher les compteurs de fréquence des trames actuels
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Ne répétez pas l'initialisation de l'application lorsque la fenêtre comporte déjà du contenu,
            // assurez-vous juste que la fenêtre est active

            if (rootFrame == null)
            {
                // Créez un Frame utilisable comme contexte de navigation et naviguez jusqu'à la première page
                rootFrame = new Frame();
                //Associez au frame une clé SuspensionManager                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");
                // Définir la page par défaut
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restaure l'état de session enregistré uniquement lorsque cela est approprié
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Un problème est survenu lors de la restauration de l'état.
                        //Partez du principe que l'état est absent et continuez
                    }
                }

                // Placez le frame dans la fenêtre active
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // Quand la pile de navigation n'est pas restaurée, accède à la première page,
                // puis configurez la nouvelle page en transmettant les informations requises en tant que
                // paramètre
                rootFrame.Navigate(typeof(GroupedItemsPage), e.Arguments);
            }
            // Vérifiez que la fenêtre actuelle est active
            Window.Current.Activate();
        }

        /// <summary>
        /// Appelé lorsque la navigation vers une page donnée échoue
        /// </summary>
        /// <param name="sender">Frame à l'origine de l'échec de navigation.</param>
        /// <param name="e">Détails relatifs à l'échec de navigation</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Appelé lorsque l'exécution de l'application est suspendue.  L'état de l'application est enregistré
        /// sans savoir si l'application pourra se fermer ou reprendre sans endommager
        /// le contenu de la mémoire.
        /// </summary>
        /// <param name="sender">Source de la requête de suspension.</param>
        /// <param name="e">Détails de la requête de suspension.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
        }

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {

            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "Paramètres", "Paramètres", (handler) => ShowCustomSettingFlyout()));
        }

        public void ShowCustomSettingFlyout()
        {
            Settings CustomSettingFlyout = new Settings();
            CustomSettingFlyout.Show();
        }

    }

    public class AppSettings : INotifyPropertyChanged
    {
        public static AppSettings Default;

        public AppSettings()
        {
            Default = this;
        }

        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        private bool _showFormatBar;
        public bool ShowFormatBar
        {
            get
            {
                if (localSettings.Values["showFormatBar"] == null)
                    localSettings.Values["showFormatBar"] = true;

                _showFormatBar = (bool)localSettings.Values["showFormatBar"];
                return _showFormatBar;
            }
            set
            {
                _showFormatBar = value;
                localSettings.Values["showFormatBar"] = _showFormatBar;
                NotifyPropertyChanged("ShowFormatBar");
                NotifyPropertyChanged("FormatBarVisibility");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string ShowConfigurationList;


        public string LoginBetaSerie { get { return "Tarboeuf"; } set { } }

        public string PwdBetaSerie { get { return "55fc47e4665c3df4047618f941c054e5"; } set { } }

        public string IpFreebox { get; set; }

        public string TokenFreebox { get; set; }

        public string AppId { get; set; }

        public string AppName { get; set; }

        public string AppVersion { get; set; }

        public string PathVideo { get; set; }
    }
}
